namespace DAL.Infrastructure;

public interface ISqlConnectionStringProvider
{
    string ConnectionString { get; }
}
