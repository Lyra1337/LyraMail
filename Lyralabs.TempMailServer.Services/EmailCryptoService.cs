using System;
using System.Linq;
using System.Reflection;
using Lyralabs.TempMailServer.Data;

namespace Lyralabs.TempMailServer
{
    public sealed class EmailCryptoService
    {
        private readonly AsymmetricCryptoService asymmetricCryptoService;
        private readonly SymmetricCryptoService symmetricCryptoService;

        public EmailCryptoService(AsymmetricCryptoService asymmetricCryptoService, SymmetricCryptoService symmetricCryptoService)
        {
            this.asymmetricCryptoService = asymmetricCryptoService;
            this.symmetricCryptoService = symmetricCryptoService;
        }

        public MailModel EncryptWithNewPassword(MailModel mail, string publicKey)
        {
            var dto = mail.Clone();

            var password = Guid.NewGuid().ToString(); // HACK: Use better password generator
            var (key, iv) = this.symmetricCryptoService.GenerateKeyByPassword(password);

            this.ForEachString(mail, dto, x => this.symmetricCryptoService.Encrypt(x, key, iv));

            dto.Password = this.asymmetricCryptoService.Encrypt(password, publicKey);

            return dto;
        }

        public MailModel Decrypt(MailModel mail, string privateKey)
        {
            var dto = mail.Clone();

            var password = this.asymmetricCryptoService.Decrypt(mail.Password, privateKey);
            var (key, iv) = this.symmetricCryptoService.GenerateKeyByPassword(password);

            this.ForEachString(mail, dto, x => this.symmetricCryptoService.Decrypt(x, key, iv));

            return dto;
        }

        private void ForEachString(MailModel source, MailModel destination, Func<string, string> memberFunc)
        {
            var propertyQuery = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.PropertyType == typeof(string));

            if (true)
            {
                propertyQuery = propertyQuery.Where(x => x.Name != nameof(MailModel.Password));
            }

            var properties = propertyQuery.ToList();

            properties.ForEach(x =>
            {
                if (x.GetValue(source) is string value)
                {
                    x.SetValue(destination, memberFunc(value));
                }
            });
        }
    }
}
