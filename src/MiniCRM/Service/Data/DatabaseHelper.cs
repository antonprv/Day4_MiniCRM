using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using MiniCRM.Core.Models;

namespace MiniCRM.Service.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            SQLitePCL.Batteries.Init();

            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Clients (
                        Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName  TEXT    NOT NULL,
                        Phone     TEXT,
                        Email     TEXT,
                        Company   TEXT,
                        Status    INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT    NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public List<Client> GetAll()
        {
            var result = new List<Client>();
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Clients ORDER BY CreatedAt DESC;";
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(MapReader(reader));
                }
            }
            return result;
        }

        public Client GetById(int id)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Clients WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? MapReader(reader) : null;
                }
            }
        }

        public int Insert(Client client)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Clients (FullName, Phone, Email, Company, Status, CreatedAt)
                    VALUES (@name, @phone, @email, @company, @status, @created);
                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@name", client.FullName);
                cmd.Parameters.AddWithValue("@phone", client.Phone ?? "");
                cmd.Parameters.AddWithValue("@email", client.Email ?? "");
                cmd.Parameters.AddWithValue("@company", client.Company ?? "");
                cmd.Parameters.AddWithValue("@status", (int)client.Status);
                cmd.Parameters.AddWithValue("@created", client.CreatedAt.ToString("o"));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Client client)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Clients SET
                        FullName  = @name,
                        Phone     = @phone,
                        Email     = @email,
                        Company   = @company,
                        Status    = @status
                    WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@name", client.FullName);
                cmd.Parameters.AddWithValue("@phone", client.Phone ?? "");
                cmd.Parameters.AddWithValue("@email", client.Email ?? "");
                cmd.Parameters.AddWithValue("@company", client.Company ?? "");
                cmd.Parameters.AddWithValue("@status", (int)client.Status);
                cmd.Parameters.AddWithValue("@id", client.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Clients WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Client> Search(string query)
        {
            var result = new List<Client>();
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT * FROM Clients
                    WHERE FullName  LIKE @q
                       OR Phone     LIKE @q
                       OR Email     LIKE @q
                       OR Company   LIKE @q
                    ORDER BY CreatedAt DESC;";
                cmd.Parameters.AddWithValue("@q", $"%{query}%");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(MapReader(reader));
                }
            }
            return result;
        }

        private Client MapReader(SqliteDataReader reader)
        {
            return new Client
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Phone = reader.GetString(2),
                Email = reader.GetString(3),
                Company = reader.GetString(4),
                Status = (ClientStatus)reader.GetInt32(5),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            };
        }
    }
}