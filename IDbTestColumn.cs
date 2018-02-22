namespace TestUtils.DbTest
{
    /// <summary>
    /// Interface for a simple database column description
    /// </summary>
    public interface IDbTestColumn
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        string ColumnName { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        object Value { get; }
    }
}
