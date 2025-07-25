using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace EnergyLegacyApp.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");
        }

        public MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        public DataTable ExecuteQuery(string query)
    {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        using var adapter = new MySqlDataAdapter(command);
        var dataTable = new DataTable();
        adapter.Fill(dataTable);
        return dataTable;
    }

    public int ExecuteNonQuery(string query)
    {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        return command.ExecuteNonQuery();
    }

    public object ExecuteScalar(string query)
    {
        using var connection = GetConnection();
        using var command = new MySqlCommand(query, connection);
        return command.ExecuteScalar() ?? DBNull.Value;
    }
}
}
