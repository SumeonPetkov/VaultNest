using System.Security.Cryptography;

namespace NestVault.Client.Services
{
    /// <summary>
    /// Generates cryptographically random passwords, guaranteeing at least one
    /// character from every enabled character set.
    /// </summary>
    public class PasswordGeneratorService
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?";

        public string Generate(int length, bool useLowercase, bool useUppercase, bool useDigits, bool useSymbols)
        {
            var sets = new List<string>();
            if (useLowercase) sets.Add(Lowercase);
            if (useUppercase) sets.Add(Uppercase);
            if (useDigits) sets.Add(Digits);
            if (useSymbols) sets.Add(Symbols);

            if (sets.Count == 0)
                sets.Add(Lowercase);

            length = Math.Max(length, sets.Count);

            var allChars = string.Concat(sets);
            var result = new char[length];

            // One character from each enabled set, the rest from the combined pool.
            for (int i = 0; i < sets.Count; i++)
                result[i] = sets[i][RandomNumberGenerator.GetInt32(sets[i].Length)];

            for (int i = sets.Count; i < length; i++)
                result[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];

            // Fisher–Yates shuffle so the guaranteed characters aren't always first.
            for (int i = result.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return new string(result);
        }
    }
}
