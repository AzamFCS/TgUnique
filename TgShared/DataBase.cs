using Microsoft.Data.Sqlite;
namespace TgShared
{
    public static class Database
    {
        private static string _connectionString;

        public static void Initialize(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                var dir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            _connectionString = $"Data Source={dbPath}";
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS UserConsents (
                UserId INTEGER PRIMARY KEY,
                PhoneNumber TEXT NOT NULL,
                ConsentTime TEXT NOT NULL
            );";
            string checkColumnQuery = "PRAGMA table_info(UserConsents);";
            using var checkCommand = new SqliteCommand(checkColumnQuery, connection);
            using var reader = checkCommand.ExecuteReader();

            bool hasPhoneNumberColumn = false;
            while (reader.Read())
            {
                if (reader.GetString(1).Equals("PhoneNumber", StringComparison.OrdinalIgnoreCase))
                {
                    hasPhoneNumberColumn = true;
                    break;
                }
            }

            if (!hasPhoneNumberColumn)
            {
                string alterTableQuery = "ALTER TABLE UserConsents ADD COLUMN PhoneNumber TEXT NOT NULL DEFAULT '';";
                using var alterCommand = new SqliteCommand(alterTableQuery, connection);
                alterCommand.ExecuteNonQuery();
            }

            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();

            Console.WriteLine("✔ БД инициализирована по пути: " + Path.GetFullPath(dbPath));
        }

        public static void SaveConsent(long userId, string phoneNumber, DateTime consentTime)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand("INSERT OR REPLACE INTO UserConsents (UserId, PhoneNumber, ConsentTime) VALUES (@UserId, @PhoneNumber, @ConsentTime)", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
            command.Parameters.AddWithValue("@ConsentTime", consentTime.ToString("o")); // ISO 8601 формат
            command.ExecuteNonQuery();
        }
    }
}
