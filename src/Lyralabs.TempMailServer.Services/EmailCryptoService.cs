using System;
using System.Linq;
using System.Reflection;
using Lyralabs.TempMailServer.Data;

namespace Lyralabs.TempMailServer
{
    public sealed class EmailCryptoService
    {
        private readonly AsymmetricCryptoService cryptoService;

        public EmailCryptoService(AsymmetricCryptoService cryptoService)
        {
            this.cryptoService = cryptoService;
        }

        public MailModel Encrypt(MailModel mail, string publicKey)
        {
            var dto = mail.Clone();
            this.ForEachString(mail, dto, x => this.cryptoService.Encrypt(x, publicKey));
            return dto;
        }

        public MailModel Decrypt(MailModel mail, string privateKey)
        {
            var dto = mail.Clone();
            this.ForEachString(mail, dto, x => this.cryptoService.Decrypt(x, privateKey));
            return dto;
        }

        private void ForEachString(MailModel source, MailModel destination, Func<string, string> memberFunc)
        {
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.PropertyType == typeof(string))
                .ToList();

            properties.ForEach(x => x.SetValue(destination, memberFunc((string)x.GetValue(source))));
        }
    }
}
