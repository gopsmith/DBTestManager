using System;
using System.Collections.Generic;
using System.Linq;

namespace TestUtils.DbTest
{
    using System.Data.SqlClient;

    /// <summary>
    /// A manager of database test data
    /// </summary>
    public class DbTestManager : IDisposable
    {
        /// <summary>
        /// Holds the disposables for inserted data
        /// </summary>
        private readonly List<IDisposable> _inserted = new List<IDisposable>();

        /// <summary>
        /// The connection string
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbTestManager"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DbTestManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Inserts the specified data, returning primary key as decimal.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Primary key, using default type decimal</returns>
        public decimal Insert(DbTestTableData data)
        {
            return Insert<decimal>(data);
        }

        /// <summary>
        /// Inserts the specified data, returning primary key as designated type.
        /// </summary>
        /// <typeparam name="T">The type of the returned primary key</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Primary key</returns>
        public T Insert<T>(DbTestTableData data)
        {
            var dbi = new DbTestInsert<T>(_connectionString, data);
            _inserted.Add(dbi);
            return dbi.PrimaryKey;
        }

        /// <summary>
        /// Inserts a range of data, returning primary keys as decimal.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Primary keys, using default type decimal</returns>
        public IEnumerable<decimal> InsertRange(params DbTestTableData[] data)
        {
            return InsertRange<decimal>(data);
        }

        /// <summary>
        /// Inserts a range of data, returning primary keys as designated type.
        /// </summary>
        /// <typeparam name="T">The type of the returned primary keys</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Primary keys</returns>
        public IEnumerable<T> InsertRange<T>(params DbTestTableData[] data)
        {
            return InsertRange<T>((IEnumerable<DbTestTableData>)data);
        }

        /// <summary>
        /// Inserts a range of data.
        /// </summary>
        /// <typeparam name="T">The type of the returned primary keys</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Primary keys</returns>
        public T[] InsertRange<T>(IEnumerable<DbTestTableData> data)
        {
            return data.Select(Insert<T>).ToArray();
        }

        /// <summary>
        /// Adds the specified pre-existing data (with decimal key) to the _inserted list, for deletion during test cleanup.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="keyValue">The key value.</param>
        /// <param name="keyColumnName">Name of the key column (usually the primary key).</param>
        public void AddToCleanup(string tableName, decimal keyValue, string keyColumnName = null)
        {
            AddToCleanup<decimal>(tableName, keyValue, keyColumnName);
        }

        /// <summary>
        /// Adds the specified pre-existing data to the _inserted list, for deletion during test cleanup.
        /// </summary>
        /// <typeparam name="T">The type of the key value</typeparam>
        /// <param name="tableName">The table name.</param>
        /// <param name="keyValue">The key value.</param>
        /// <param name="keyColumnName">Name of the key column (usually the primary key).</param>
        public void AddToCleanup<T>(string tableName, T keyValue, string keyColumnName = null)
        {
            if (keyColumnName == null)
                keyColumnName = tableName.Substring(0, tableName.Length - 2) + "Id";

            var dbi = new DbTestInsert<T>(_connectionString, tableName, keyValue, keyColumnName);
            _inserted.Add(dbi);
        }

        /// <summary>
        /// Retrieves a row of data from a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="keyValue">The key value.</param>
        /// <param name="columnNames">The columns you want returned (null means ALL).</param>
        /// <param name="keyColumnName">Name of the key column (usually the primary key).</param>
        public DbTestTableData Retrieve(string tableName, object keyValue, IEnumerable<string> columnNames = null, string keyColumnName = null)
        {
            const string SelectTemplate = "select {0} from dbo.{1} where {2} = {3}";

            var tableData = new DbTestTableData(tableName);

            if (keyColumnName == null)
                keyColumnName = tableName.Substring(0, tableName.Length - 2) + "Id";

            var columns = columnNames == null ? "*" : string.Join(",", columnNames);
            var selectStatement = string.Format(SelectTemplate, columns, tableName, keyColumnName, keyValue);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = new SqlCommand(selectStatement, connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tableData.AddRange(
                        Enumerable.Range(0, reader.FieldCount)
                                  .Select(i => new DbTestColumnData<object>(reader.GetName(i), reader[i])));
                }

                reader.Close();
            }

            return tableData;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var ins in _inserted)
            {
                ins.Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Private nested class used to execute an insert and maintain the corresponding delete statement
        /// </summary>
        /// <typeparam name="T">Type of primary key</typeparam>
        private class DbTestInsert<T> : IDisposable
        {
            /// <summary>
            /// The delete statement template
            /// </summary>
            private const string DeleteTemplate = "delete dbo.{0} where {1} = {2}";

            /// <summary>
            /// The delete statement
            /// </summary>
            private readonly string _deleteStatement;

            /// <summary>
            /// The primary key
            /// </summary>
            private readonly T _primaryKey;

            /// <summary>
            /// The connection string
            /// </summary>
            private readonly string _connectionString;

            /// <summary>
            /// Gets the primary key.
            /// </summary>
            /// <value>
            /// The primary key.
            /// </value>
            public T PrimaryKey
            {
                get
                {
                    return _primaryKey;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DbTestInsert{T}"/> class.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            /// <param name="testData">The test data.</param>
            public DbTestInsert(string connectionString, DbTestTableData testData)
            {
                _connectionString = connectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    var cmd = testData.GenerateSqlInsertCommand();
                    cmd.Connection = connection;
                    _primaryKey = (T)cmd.ExecuteScalar();
                    _deleteStatement = string.Format(DeleteTemplate, testData.TableName, testData.PrimaryKey, _primaryKey);
                }
            }

            /// <summary>
            /// Alternate constructor for the <see cref="DbTestInsert{T}"/> class.
            /// Stores the key for a pre-existing data row that we want to delete during test cleanup.
            /// </summary>
            /// <param name="connectionString">The connection string.</param>
            /// <param name="tableName">The table name.</param>
            /// <param name="keyValue">The key value.</param>
            /// <param name="keyColumnName">Name of the key column (usually the primary key).</param>
            public DbTestInsert(string connectionString, string tableName, T keyValue, string keyColumnName)
            {
                _connectionString = connectionString;
                _deleteStatement = string.Format(DeleteTemplate, tableName, keyColumnName, keyValue);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand(_deleteStatement, connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
