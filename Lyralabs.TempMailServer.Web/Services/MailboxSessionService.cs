using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Lyralabs.TempMailServer.Data;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Lyralabs.TempMailServer.Web.Services
{
    public class MailboxSessionService : IRecipient<MailReceivedMessage>
    {
        private readonly UserState userState;
        private readonly ILocalStorageService localStorage;
        private readonly AsymmetricCryptoService cryptoService;
        private readonly MailboxService mailboxService;

        public List<MailModel> Mails { get; private set; } = new List<MailModel>();

        public MailboxSessionService(UserState userState, ILocalStorageService LocalStorage, AsymmetricCryptoService cryptoService, MailboxService mailboxService)
        {
            this.userState = userState;
            this.localStorage = LocalStorage;
            this.cryptoService = cryptoService;
            this.mailboxService = mailboxService;
        }

        public void TestEmail()
        {
            var client = new SmtpClient("127.0.0.1");
            client.Timeout = 1000;

            var from = new MailAddress("steve@contoso.com", "Steve Ballmer");
            var to = new MailAddress(this.userState.CurrentMailbox, "Steve Jobs");
            var msg = new MailMessage(from, to);
            msg.Subject = "Hi, wie gehts?";
            msg.Body = $"body blubb \r\n{Guid.NewGuid()}";

            client.Send(msg);
        }

        public async Task<UserSecret> GetOrCreateUserSecret()
        {
            if (await this.localStorage.ContainKeyAsync("secret") == true)
            {
                var secretFromStorage = await this.localStorage.GetItemAsync<UserSecret>("secret");

                if (secretFromStorage.PrivateKey != null && secretFromStorage.PublicKey != null && secretFromStorage.Password != null)
                {
                    return secretFromStorage;
                }
            }

            var secret = this.cryptoService.GenerateUserSecret();

            await this.localStorage.SetItemAsync("secret", secret);

            return secret;
        }

        public async Task Refresh()
        {
            this.Mails = await this.mailboxService.GetDecryptedMailsAsync(this.userState.CurrentMailbox, this.userState.Secret.Value.PrivateKey);
        }

        public void Receive(MailReceivedMessage message)
        {
            this.Mails.Insert(0, message.Mail);
        }

        public async Task DeleteMail(MailModel currentMail)
        {
            await this.mailboxService.DeleteMail(currentMail.Id);
            this.Mails.Remove(currentMail);
        }
    }
}
