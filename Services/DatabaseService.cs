using MySql.Data.MySqlClient;
using PasswordManager.Models;

namespace PasswordManager.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly ConfigurationService _config;

        public DatabaseService()
        {
            _config = new ConfigurationService();
            _connectionString = _config.GetConnectionString();
        }

        public bool InitializeDatabase()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Connected to database!");

                    // Create tables if they don't exist
                    CreateMasterPasswordTable(conn);
                    CreatePasswordsTable(conn);
                    CreateIndexes(conn);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"uhh db error: {ex.Message}"); //If you're Here then we both are not sure what happened
                return false;
            }
        }

        private void CreateMasterPasswordTable(MySqlConnection conn)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS master_password (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    password_hash VARCHAR(255) NOT NULL
                )";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("master password table ready!");
            }
        }

        private void CreatePasswordsTable(MySqlConnection conn)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS passwords (
                    Id INT PRIMARY KEY AUTO_INCREMENT,
                    Website VARCHAR(100) NOT NULL,
                    userName VARCHAR(255) NOT NULL,
                    EncryptedPwd TEXT NOT NULL,
                    Notes TEXT,
                    LastModified DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    Created_At DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("passwords table ready!"); 
            }
        }

        private void CreateIndexes(MySqlConnection conn)
        {
            // MySQL doesn't support IF NOT EXISTS for CREATE INDEX, so just try/catch (lazy way... my way :) )
            try
            {
                var sql1 = "CREATE INDEX idx_website ON passwords(Website)";
                using (var cmd = new MySqlCommand(sql1, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* ignore if exists */ }

            try
            {
                var sql2 = "CREATE INDEX idx_username ON passwords(userName)";
                using (var cmd = new MySqlCommand(sql2, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* ignore if exists */ }
        }

        public bool AddPassword(Password password)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = @"INSERT INTO passwords (Website, userName, EncryptedPwd, Notes) VALUES (@website, @username, @password, @notes)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@website", password.Website);
                        cmd.Parameters.AddWithValue("@username", password.userName);
                        cmd.Parameters.AddWithValue("@password", password.EncryptedPwd);
                        cmd.Parameters.AddWithValue("@notes", password.Notes ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't add password: {ex.Message}");
                return false;
            }
        }

        public bool UpdatePassword(Password password)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = @"UPDATE passwords SET Website=@website, userName=@username, EncryptedPwd=@password, Notes=@notes, LastModified=NOW() WHERE Id=@id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@website", password.Website);
                        cmd.Parameters.AddWithValue("@username", password.userName);
                        cmd.Parameters.AddWithValue("@password", password.EncryptedPwd);
                        cmd.Parameters.AddWithValue("@notes", password.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", password.Id);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't update password: {ex.Message}");
                return false;
            }
        }

        public List<Password> GetAllPasswords()
        {
            var passwords = new List<Password>();
            
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = "SELECT * FROM passwords ORDER BY Website";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passwords.Add(new Password
                            {
                                Id = reader.GetInt32("Id"),
                                Website = reader.IsDBNull(reader.GetOrdinal("Website")) ? "" : reader.GetString("Website"),
                                userName = reader.IsDBNull(reader.GetOrdinal("userName")) ? "" : reader.GetString("userName"),
                                EncryptedPwd = reader.IsDBNull(reader.GetOrdinal("EncryptedPwd")) ? "" : reader.GetString("EncryptedPwd"),
                                Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                    ? null
                                    : reader.GetString("Notes"),
                                LastModified = reader.IsDBNull(reader.GetOrdinal("LastModified")) ? DateTime.MinValue : reader.GetDateTime("LastModified"),
                                Created_At = reader.IsDBNull(reader.GetOrdinal("Created_At")) ? DateTime.MinValue : reader.GetDateTime("Created_At")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't get passwords: {ex.Message}"); //If you're Here then we both are not sure what happened
            }

            return passwords;
        }

        public bool SaveMasterPasswordHash(string passwordHash)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    using (var cmd = new MySqlCommand("DELETE FROM master_password", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    var sql = "INSERT INTO master_password (password_hash) VALUES (@hash)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@hash", passwordHash);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't save master password: {ex.Message}");
                return false;
            }
        }

        public string? GetMasterPasswordHash()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = "SELECT password_hash FROM master_password LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't get master password: {ex.Message}");
                return null;
            }
        }
        public bool DeletePassword(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    var sql = "DELETE FROM passwords WHERE Id = @id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't delete password: {ex.Message}");
                return false;
            }
        }
    }
}