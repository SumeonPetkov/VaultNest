using System.Security.Cryptography;

namespace NestVault.Client.Services
{
    /// <summary>
    /// Encrypts and decrypts vault secrets with AES-GCM. The 256-bit vault key
    /// is generated on first use and kept in platform secure storage
    /// (DPAPI on Windows), so the SQLite database never contains plaintext secrets.
    /// </summary>
    public class VaultCryptoService
    {
        private const string KeyStorageName = "nestvault_vault_key";
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private byte[]? _key;
        private readonly SemaphoreSlim _keyLock = new(1, 1);

        private async Task<byte[]> GetKeyAsync()
        {
            if (_key is not null)
                return _key;

            await _keyLock.WaitAsync();
            try
            {
                if (_key is not null)
                    return _key;

                var stored = await SecureStorage.Default.GetAsync(KeyStorageName);
                if (stored is null)
                {
                    var key = RandomNumberGenerator.GetBytes(32);
                    await SecureStorage.Default.SetAsync(KeyStorageName, Convert.ToBase64String(key));
                    _key = key;
                }
                else
                {
                    _key = Convert.FromBase64String(stored);
                }

                return _key;
            }
            finally
            {
                _keyLock.Release();
            }
        }

        /// <summary>
        /// Encrypts plaintext to a base64 payload laid out as nonce || tag || ciphertext.
        /// </summary>
        public async Task<string> EncryptAsync(string plaintext)
        {
            var key = await GetKeyAsync();
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);

            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var tag = new byte[TagSize];
            var cipherBytes = new byte[plainBytes.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var payload = new byte[NonceSize + TagSize + cipherBytes.Length];
            nonce.CopyTo(payload, 0);
            tag.CopyTo(payload, NonceSize);
            cipherBytes.CopyTo(payload, NonceSize + TagSize);

            return Convert.ToBase64String(payload);
        }

        public async Task<string> DecryptAsync(string payloadBase64)
        {
            if (string.IsNullOrEmpty(payloadBase64))
                return string.Empty;

            var key = await GetKeyAsync();
            var payload = Convert.FromBase64String(payloadBase64);

            var nonce = payload.AsSpan(0, NonceSize);
            var tag = payload.AsSpan(NonceSize, TagSize);
            var cipherBytes = payload.AsSpan(NonceSize + TagSize);
            var plainBytes = new byte[cipherBytes.Length];

            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return System.Text.Encoding.UTF8.GetString(plainBytes);
        }
    }
}
