using EnergyLegacyApp.Data.Models;
using System;
using System.Data;

namespace EnergyLegacyApp.Data
{
    public class UserRepository
    {
        private readonly DatabaseHelper _db;

        public UserRepository(DatabaseHelper dbHelper)
        {
            _db = dbHelper;
        }

        // ANTI-PATTERN: SQL injection vulnerabilities
        public User? GetUserByUsername(string username)
        {
            var query = $"SELECT * FROM Users WHERE Username = '{username}'";
            var dataTable = _db.ExecuteQuery(query);
            
            if (dataTable.Rows.Count > 0)
            {
                return MapRowToUser(dataTable.Rows[0]);
            }
            
            return null;
        }
        
        public bool InsertUser(User user)
        {
            var query = $@"INSERT INTO Users 
                (Username, Password, Email, Role, LastLogin, IsActive) 
                VALUES 
                ('{user.Username}', '{user.Password}', '{user.Email}', 
                '{user.Role}', '{user.LastLogin:yyyy-MM-dd HH:mm:ss}', {(user.IsActive ? 1 : 0)})";
            
            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }
        
        public bool UpdateLastLogin(string username)
        {
            var query = $"UPDATE Users SET LastLogin = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}' WHERE Username = '{username}'";
            var result = _db.ExecuteNonQuery(query);
            return result > 0;
        }
        
        private User MapRowToUser(DataRow row)
        {
            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                Username = row["Username"].ToString() ?? string.Empty,
                Password = row["Password"].ToString() ?? string.Empty,
                Email = row["Email"].ToString() ?? string.Empty,
                Role = row["Role"].ToString() ?? "User",
                LastLogin = Convert.ToDateTime(row["LastLogin"]),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
        
        // Method to create the Users table if it doesn't exist
        public bool EnsureUsersTableExists()
        {
            try
            {
                var checkTableQuery = "SHOW TABLES LIKE 'Users'";
                var dataTable = _db.ExecuteQuery(checkTableQuery);
                
                if (dataTable.Rows.Count == 0)
                {
                    var createTableQuery = @"
                        CREATE TABLE Users (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Username VARCHAR(50) NOT NULL UNIQUE,
                            Password VARCHAR(100) NOT NULL,
                            Email VARCHAR(100) NOT NULL,
                            Role VARCHAR(20) NOT NULL DEFAULT 'User',
                            LastLogin DATETIME NOT NULL,
                            IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        )";
                    
                    _db.ExecuteNonQuery(createTableQuery);
                    
                    // Create a default admin user
                    var adminUser = new User
                    {
                        Username = "admin",
                        Password = "admin123", // In a real app, this should be hashed
                        Email = "admin@example.com",
                        Role = "Admin",
                        LastLogin = DateTime.Now,
                        IsActive = true
                    };
                    
                    InsertUser(adminUser);
                    
                    return true;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring Users table exists: {ex.Message}");
                return false;
            }
        }
    }
}