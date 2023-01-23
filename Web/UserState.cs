using System.ComponentModel;
using Lyralabs.TempMailServer.Web.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lyralabs.TempMailServer.Web
{
    public class UserState : ObservableObject
    {
        private string currentMailbox;
        public string CurrentMailbox
        {
            get => this.currentMailbox;
            set => this.SetProperty(ref this.currentMailbox, value);
        }

        private UserSecret? secret;
        private readonly IMessenger messenger;

        public UserSecret? Secret
        {
            get => this.secret;
            set => this.SetProperty(ref this.secret, value);
        }

        public UserState(IMessenger messenger)
        {
            this.PropertyChanged += this.UserState_PropertyChanged;
            this.messenger = messenger;
        }

        private void UserState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.messenger.Send(new UserStateChangedMessage());
        }
    }
}
