namespace NestVault.Client.Models
{
    /// <summary>
    /// A single credential entry in the vault. <see cref="Password"/> and
    /// <see cref="Notes"/> are held decrypted in memory only; the repository
    /// encrypts them before they touch the database.
    /// </summary>
    public class VaultItem
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[..1].ToUpperInvariant();

        public override string ToString() => Name;
    }
}
