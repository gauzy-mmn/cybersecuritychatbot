using CybersecurityChatbot;
using MySql.Data.MySqlClient;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Handles all MySQL database operations for the Task Assistant.
    ///
    /// NuGet package required: MySql.Data
    ///
    /// Run this SQL once before starting the app:
    ///   CREATE DATABASE IF NOT EXISTS cybersecbot;
    ///   USE cybersecbot;
    ///   CREATE TABLE IF NOT EXISTS Tasks (
    ///       Id           INT AUTO_INCREMENT PRIMARY KEY,
    ///       Title        VARCHAR(255) NOT NULL,
    ///       Description  TEXT,
    ///       IsCompleted  TINYINT(1)   NOT NULL DEFAULT 0,
    ///       ReminderDate DATETIME     NULL,
    ///       CreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
    ///   );
    /// </summary>
    public static class DatabaseManager
    {
        // Update these to match your MySQL server
        private const string Host = "localhost";
        private const string Database = "cybersecbot";
        private const string User = "root";
        private const string Password = "yourpassword";

        private static string ConnectionString =>
            $"Server={Host};Database={Database};Uid={User};Pwd={Password};";

        public static bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        public static void EnsureTableExists()
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id           INT AUTO_INCREMENT PRIMARY KEY,
                    Title        VARCHAR(255) NOT NULL,
                    Description  TEXT,
                    IsCompleted  TINYINT(1)   NOT NULL DEFAULT 0,
                    ReminderDate DATETIME     NULL,
                    CreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                        cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DB] {ex.Message}"); }
        }

        public static int AddTask(string title, string description, DateTime? reminderDate)
        {
            string sql = @"INSERT INTO Tasks (Title, Description, IsCompleted, ReminderDate, CreatedAt)
                           VALUES (@title, @desc, 0, @reminder, @created);
                           SELECT LAST_INSERT_ID();";
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@desc", description ?? "");
                        cmd.Parameters.AddWithValue("@reminder", reminderDate.HasValue
                            ? (object)reminderDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@created", DateTime.Now);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DB] AddTask: {ex.Message}"); return -1; }
        }

        public static List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();
            string sql = "SELECT Id, Title, Description, IsCompleted, ReminderDate, CreatedAt FROM Tasks ORDER BY CreatedAt DESC;";
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            tasks.Add(new CyberTask
                            {
                                Id = r.GetInt32("Id"),
                                Title = r.GetString("Title"),
                                Description = r.IsDBNull(r.GetOrdinal("Description")) ? "" : r.GetString("Description"),
                                IsCompleted = r.GetBoolean("IsCompleted"),
                                ReminderDate = r.IsDBNull(r.GetOrdinal("ReminderDate")) ? (DateTime?)null : r.GetDateTime("ReminderDate"),
                                CreatedAt = r.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DB] GetAllTasks: {ex.Message}"); }
            return tasks;
        }

        public static bool MarkCompleted(int id)
        {
            return Execute("UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id;", ("@id", (object)id));
        }

        public static bool DeleteTask(int id)
        {
            return Execute("DELETE FROM Tasks WHERE Id = @id;", ("@id", (object)id));
        }

        public static bool UpdateReminder(int id, DateTime reminderDate)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE Tasks SET ReminderDate = @r WHERE Id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@r", reminderDate);
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DB] UpdateReminder: {ex.Message}"); return false; }
        }

        private static bool Execute(string sql, params (string name, object value)[] parms)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        foreach (var (n, v) in parms) cmd.Parameters.AddWithValue(n, v);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DB] Execute: {ex.Message}"); return false; }
        }
    }
}
