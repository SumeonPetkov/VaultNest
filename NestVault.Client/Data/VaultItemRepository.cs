using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NestVault.Client.Models;
using NestVault.Client.Services;

namespace NestVault.Client.Data
{
    /// <summary>
    /// Repository for vault items. Password and Notes columns are stored
    /// AES-GCM encrypted; all other columns are plaintext for searching.
    /// </summary>
    public class VaultItemRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;
        private readonly VaultCryptoService _crypto;

        public VaultItemRepository(VaultCryptoService crypto, ILogger<VaultItemRepository> logger)
        {
            _crypto = crypto;
            _logger = logger;
        }

        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS VaultItem (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                Url TEXT NOT NULL,
                Notes TEXT NOT NULL,
                IsFavorite INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL
            );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating VaultItem table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<VaultItem>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM VaultItem ORDER BY IsFavorite DESC, Name COLLATE NOCASE";
            var items = new List<VaultItem>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(await ReadItemAsync(reader));
            }

            return items;
        }

        public async Task<VaultItem?> GetAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM VaultItem WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return await ReadItemAsync(reader);
            }

            return null;
        }

        public async Task<int> SaveItemAsync(VaultItem item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            item.UpdatedAtUtc = DateTime.UtcNow;
            if (item.ID == 0)
                item.CreatedAtUtc = item.UpdatedAtUtc;

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO VaultItem (Name, Username, Password, Url, Notes, IsFavorite, CreatedAtUtc, UpdatedAtUtc)
                VALUES (@Name, @Username, @Password, @Url, @Notes, @IsFavorite, @CreatedAtUtc, @UpdatedAtUtc);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE VaultItem SET Name = @Name, Username = @Username, Password = @Password,
                    Url = @Url, Notes = @Notes, IsFavorite = @IsFavorite, UpdatedAtUtc = @UpdatedAtUtc
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@Username", item.Username);
            saveCmd.Parameters.AddWithValue("@Password", await _crypto.EncryptAsync(item.Password));
            saveCmd.Parameters.AddWithValue("@Url", item.Url);
            saveCmd.Parameters.AddWithValue("@Notes", await _crypto.EncryptAsync(item.Notes));
            saveCmd.Parameters.AddWithValue("@IsFavorite", item.IsFavorite ? 1 : 0);
            saveCmd.Parameters.AddWithValue("@CreatedAtUtc", item.CreatedAtUtc.ToString("O"));
            saveCmd.Parameters.AddWithValue("@UpdatedAtUtc", item.UpdatedAtUtc.ToString("O"));

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }

        public async Task<int> DeleteItemAsync(VaultItem item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM VaultItem WHERE ID = @id";
            deleteCmd.Parameters.AddWithValue("@id", item.ID);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        private async Task<VaultItem> ReadItemAsync(SqliteDataReader reader)
        {
            return new VaultItem
            {
                ID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Username = reader.GetString(2),
                Password = await _crypto.DecryptAsync(reader.GetString(3)),
                Url = reader.GetString(4),
                Notes = await _crypto.DecryptAsync(reader.GetString(5)),
                IsFavorite = reader.GetInt32(6) == 1,
                CreatedAtUtc = DateTime.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind),
                UpdatedAtUtc = DateTime.Parse(reader.GetString(8), null, System.Globalization.DateTimeStyles.RoundtripKind)
            };
        }
    }
}
