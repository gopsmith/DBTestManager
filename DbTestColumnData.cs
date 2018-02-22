
namespace TestUtils.DbTest
{
    /// <summary>
    /// Class that represents a single database cell
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    internal class DbTestColumnData<T> : IDbTestColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbTestColumnData{T}"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="value">The value.</param>
        public DbTestColumnData(string columnName, T value)
        {
            ColumnName = columnName;
            Value = value;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        object IDbTestColumn.Value
        {
            get
            {
                return Value;
            }
        }
    }
}
