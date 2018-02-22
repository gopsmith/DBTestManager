using System;
using System.Collections.Generic;
using System.Linq;


namespace TestUtils.DbTest
{
    using System.Data.SqlClient;

    /// <summary>
    /// A class which represents a typed row of data not unlike a DataRow
    /// </summary>
    public class DbTestTableData : List<IDbTestColumn>
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets the primary key NAME.
        /// </summary>
        /// <value>
        /// The primary key.
        /// </value>
        public string PrimaryKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbTestTableData"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="primaryKey">The primary key.</param>
        public DbTestTableData(string tableName, string primaryKey = null)
        {
            TableName = tableName;
            PrimaryKey = primaryKey ?? tableName.Substring(0, tableName.Length - 2) + "Id";
        }

        /// <summary>
        /// Adds the specified column name and value.
        /// </summary>
        /// <typeparam name="T">Column data type</typeparam>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        public void Add<T>(string columnName, T value)
        {
            Add(new DbTestColumnData<T>(columnName, value));
        }

        /// <summary>
        /// Generates the SQL insert command.
        /// </summary>
        /// <returns>SqlCommand ready to be executed when supplied with a connection</returns>
        public SqlCommand GenerateSqlInsertCommand()
        {
            const string sqlTemplate = @"insert dbo.{0} ({1}) values ({2}); select @@IDENTITY as identityValue";

            var columnNames = this.Select(td => td.ColumnName).ToArray();

            var filledTemplate = string.Format(
                sqlTemplate,
                TableName,
                string.Join(",", columnNames),
                string.Join(",", columnNames.Select(cn => "@" + cn)));

            var cmd = new SqlCommand(filledTemplate);
            var parameters = this.Select(col => new SqlParameter(col.ColumnName, col.Value)).ToArray();
            cmd.Parameters.AddRange(parameters);

            return cmd;
        }

        /// <summary>
        /// Returns the value of the specified column name.
        /// The current object (this) is usually what was returned from a call to DbTestManager.Retrieve().
        /// If the column is a nullable int, test for a null value like this:  (_.Get<int?>("ColumnI") == null)
        /// </summary>
        /// <typeparam name="T">Column data type</typeparam>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The column's value, as type T</returns>
        public T Get<T>(string columnName)
        {
            var value = this.Single(l => l.ColumnName == columnName).Value;
            return (value == DBNull.Value) ? default(T) : (T)value;
        }
    }
}
