namespace DAL.Infrastructure;

public sealed class SqlConnectionStringProvider : ISqlConnectionStringProvider
{
    public SqlConnectionStringProvider(string connectionString)
    {
        ConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString))
            : connectionString;
    }

    public string ConnectionString { get; }
}
