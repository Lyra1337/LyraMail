using System.Net.Mail;
using Blazored.LocalStorage;
using CommunityToolkit.Mvvm.Messaging;
using Lyralabs.TempMailServer.Data;

namespace Lyralabs.TempMailServer.Web.Services
{
    public class MailboxSessionService : IRecipient<MailReceivedMessage>, IDisposable
    {
        public event EventHandler MailReceived;

        private readonly UserState userState;
        private readonly ILocalStorageService localStorage;
        private readonly AsymmetricCryptoService cryptoService;
        private readonly MailboxService mailboxService;
        private readonly IMessenger messenger;
        private readonly ILogger<MailboxSessionService> logger;

        public List<MailPreviewDto> Mails { get; private set; } = [];

        public MailboxSessionService(
            UserState userState,
            ILocalStorageService LocalStorage,
            AsymmetricCryptoService cryptoService,
            MailboxService mailboxService,
            IMessenger messenger,
            ILogger<MailboxSessionService> logger)
        {
            this.userState = userState;
            this.localStorage = LocalStorage;
            this.cryptoService = cryptoService;
            this.mailboxService = mailboxService;
            this.messenger = messenger;
            this.logger = logger;
        }

        public void TestEmail()
        {
            try
            {
                var client = new SmtpClient("127.0.0.1");
                client.Timeout = 1000;

                var from = new MailAddress("steve@contoso.com", "Steve Ballmer");
                var to = new MailAddress(this.userState.CurrentMailbox, "Steve Jobs");
                var msg = new MailMessage(from, to);
                msg.Subject = "Test Mail?";
                msg.Body = $"Test Mail issued at {DateTime.UtcNow} (UTC)\r\n{Guid.NewGuid()}";

                client.Send(msg);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "failed to send test-email");
            }
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
            this.MailReceived?.Invoke(this, EventArgs.Empty);
        }

        public void Receive(MailReceivedMessage message)
        {
            this.Mails.Insert(0, this.mailboxService.ConvertToPreview(message.Mail));
            this.MailReceived?.Invoke(this, EventArgs.Empty);
        }

        public async Task DeleteMail(int mailid)
        {
            await this.mailboxService.DeleteMail(mailid);
            this.Mails.Remove(this.Mails.Single(m => m.Id == mailid));
        }

        public async Task GetMailbox(bool forceNew = false)
        {
            var userSecret = this.userState.Secret.Value;

            if (forceNew == true)
            {
                this.userState.CurrentMailbox = await this.mailboxService.GenerateNewMailbox(userSecret.PublicKey, userSecret.Password);
            }
            else
            {
                this.userState.CurrentMailbox = await this.mailboxService.GetOrCreateMailboxAsync(userSecret.PrivateKey, userSecret.Password);
            }

            this.messenger.Register(this, this.userState.CurrentMailbox);

            await this.Refresh();
        }

        internal async Task<MailModel> GetMailByIdAsync(int mailId)
        {
            var mail = await this.mailboxService.GetDecryptedMailById(this.userState.CurrentMailbox, mailId, this.userState.Secret.Value.PrivateKey);
            return mail;
        }

        public void Dispose()
        {
            this.messenger.UnregisterAll(this);
            this.Mails.Clear();
        }
    }
}
