using System;
using System.IO;
using System.Security.Cryptography;

namespace Lyralabs.TempMailServer
{
    // Adapted from https://docs.microsoft.com/de-de/dotnet/api/system.security.cryptography.aes?view=net-5.0
    public sealed class SymmetricCryptoService
    {
        public (string key, string iv) GenerateKey()
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            aes.GenerateKey();

            return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
        }

        public string EncryptString(string plainText, string key, string iv)
        {
            if (plainText is null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            if (String.IsNullOrWhiteSpace(key) == true)
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            if (String.IsNullOrWhiteSpace(iv) == true)
            {
                throw new ArgumentException($"'{nameof(iv)}' cannot be null or whitespace.", nameof(iv));
            }

            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            var encrypted = memoryStream.ToArray();
            return Convert.ToBase64String(encrypted);
        }

        public string DecryptString(string cipherText, string key, string iv)
        {
            if (String.IsNullOrWhiteSpace(cipherText) == true)
            {
                throw new ArgumentException($"'{nameof(cipherText)}' cannot be null or whitespace.", nameof(cipherText));
            }

            if (String.IsNullOrWhiteSpace(key) == true)
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            if (String.IsNullOrWhiteSpace(iv) == true)
            {
                throw new ArgumentException($"'{nameof(iv)}' cannot be null or whitespace.", nameof(iv));
            }

            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherText));
            memoryStream.Seek(0, SeekOrigin.Begin);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
