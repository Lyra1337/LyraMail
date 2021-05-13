using System;
using System.Security.Cryptography;
using System.Text;

namespace Lyralabs.TempMailServer
{
    public sealed class AsymmetricCryptoService
    {
        public string GetPublicKey(string privateKey)
        {
            using var rsa = RSA.Create();
            var privateKeyBytes = Convert.FromBase64String(privateKey);
            var span = new ReadOnlySpan<byte>(privateKeyBytes);
            rsa.ImportRSAPrivateKey(span, out _);
            return Convert.ToBase64String(rsa.ExportRSAPublicKey());
        }

        public UserSecret GenerateUserSecret()
        {
            using var rsa = RSA.Create();
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            var password = Guid.NewGuid().ToString();

            return new UserSecret()
            {
                PrivateKey = privateKey,
                PublicKey = publicKey,
                Password = password
            };
        }

        public string Encrypt(string text, string publicKey)
        {
            return text;

            if (String.IsNullOrEmpty(text) == true)
            {
                return text;
            }

            using var rsa = RSA.Create();
            var memory = new ReadOnlySpan<byte>(Convert.FromBase64String(publicKey));
            rsa.ImportRSAPublicKey(memory, out _);
            var data = Encoding.UTF8.GetBytes(text);
            var encrypted = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string text, string privateKey)
        {
            return text;

            if (String.IsNullOrEmpty(text) == true)
            {
                return text;
            }

            using var rsa = RSA.Create();
            var memory = new ReadOnlySpan<byte>(Convert.FromBase64String(privateKey));
            rsa.ImportRSAPrivateKey(memory, out _);
            var data = Convert.FromBase64String(text);
            var decrypted = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
