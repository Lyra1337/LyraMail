using System.Net.Mail;
using Blazored.LocalStorage;
using CommunityToolkit.Mvvm.Messaging;
using Lyralabs.TempMailServer.Data;

namespace Lyralabs.TempMailServer.Web.Services
{
    public class MailboxSessionService(
        UserState userState,
        ILocalStorageService localStorage,
        AsymmetricCryptoService cryptoService,
        MailboxService mailboxService,
        MailRepository mailRepository,
        IMessenger messenger,
        ILogger<MailboxSessionService> logger) : IRecipient<MailReceivedMessage>, IDisposable
    {
        public event EventHandler MailReceived;
        public event EventHandler MailRead;

        public List<MailPreviewDto> Mails { get; private set; } = [];

        public void TestEmail()
        {
            try
            {
                var client = new SmtpClient("127.0.0.1");
                client.Timeout = 1000;

                var from = new MailAddress("steve@contoso.com", "Steve Ballmer");
                var to = new MailAddress(userState.CurrentMailbox, "Steve Jobs");
                var msg = new MailMessage(from, to);
                msg.Subject = "Test Mail";
                msg.Body = $"Test Mail issued at {DateTime.UtcNow} (UTC)\r\n{Guid.NewGuid()}";

                client.Send(msg);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to send test-email");
            }
        }

        public async Task<UserSecret> GetOrCreateUserSecret()
        {
            if (await localStorage.ContainKeyAsync("secret") == true)
            {
                var secretFromStorage = await localStorage.GetItemAsync<UserSecret>("secret");

                if (secretFromStorage.PrivateKey != null && secretFromStorage.PublicKey != null && secretFromStorage.Password != null)
                {
                    return secretFromStorage;
                }
            }

            var secret = cryptoService.GenerateUserSecret();

            await localStorage.SetItemAsync("secret", secret);

            return secret;
        }

        public async Task Refresh()
        {
            this.Mails = await mailboxService.GetDecryptedMailsAsync(userState.CurrentMailbox, userState.Secret.Value.PrivateKey);
            this.MailReceived?.Invoke(this, EventArgs.Empty);
        }

        public void Receive(MailReceivedMessage message)
        {
            this.Mails.Insert(0, mailboxService.ConvertToPreview(message.Mail));
            this.MailReceived?.Invoke(this, EventArgs.Empty);
        }

        public async Task DeleteMail(int mailid)
        {
            await mailboxService.DeleteMail(mailid);
            this.Mails.Remove(this.Mails.Single(m => m.Id == mailid));
        }

        public async Task GetMailbox(bool forceNew = false)
        {
            var userSecret = userState.Secret.Value;

            if (forceNew == true)
            {
                userState.CurrentMailbox = await mailboxService.GenerateNewMailbox(userSecret.PublicKey, userSecret.Password);
            }
            else
            {
                userState.CurrentMailbox = await mailboxService.GetOrCreateMailboxAsync(userSecret.PrivateKey, userSecret.Password);
            }

            messenger.Register(this, userState.CurrentMailbox);

            await this.Refresh();
        }

        internal async Task<MailModel> GetMailByIdAsync(int mailId)
        {
            var mail = await mailboxService.GetDecryptedMailById(userState.CurrentMailbox, mailId, userState.Secret.Value.PrivateKey);
            return mail;
        }

        public async Task SetMailReadMark(int mailId, bool isRead)
        {
            await mailRepository.SetReadMark(mailId, isRead);
            this.MailRead?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            messenger.UnregisterAll(this);
            this.Mails.Clear();
        }
    }
}
