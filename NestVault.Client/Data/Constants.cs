namespace NestVault.Client.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "NestVault.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        /// <summary>
        /// Base address of the NestVault API. Android emulators reach the host
        /// machine through 10.0.2.2 instead of localhost.
        /// </summary>
        public static string ApiBaseUrl =>
            DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5035"
                : "http://localhost:5035";
    }
}
