using System.Data.SqlClient;

namespace TravelAgencyAPI.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TravelAgencyDB");
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}