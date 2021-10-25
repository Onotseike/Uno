using Demo.Helpers;

namespace Demo.CloudProvider
{
    public class BaseCloudProvider : BaseNotifyClass
    {
        #region INotifyPropertyChanged Implementation(s)

        private bool isSignedIn;
        public bool IsSignedIn
        {
            get => isSignedIn;
            set => SetProperty(ref isSignedIn, value);
        }

        private bool isBackedUp;
        public bool IsBackedUp
        {
            get => isBackedUp;
            set => SetProperty(ref isBackedUp, value);
        }

        // The user's display name
        private string userName;
        public string Username
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        // The user's email
        private string userEmail;
        public string UserEmail
        {
            get => userEmail;
            set => SetProperty(ref userEmail, value);
        }

        #endregion
    }
}
