using Demo.Database.Entities;
using Demo.Database.Services;
using Demo.Helpers;

using SQLite;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Demo.ViewModels
{
    public class MainVM : BaseNotifyClass
    {
        #region Properties

        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<Invoice> Invoices { get; set; }
        public Account UserAccount { get; set; }
        public Address UserAddress { get; set; }        

        private readonly string databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Demo.db");

        #endregion

        #region OneDrive Properties

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

        #region DBServices

        public ClientDBService ClientDBService { get; set; }
        public InvoiceDBService InvoiceDBService { get; set; }
        public AccountDBService AccountDBService { get; set; }
        public AddressDBService AddressDBService { get; set; }
        
        #endregion

        #region Constructor(s)

        public MainVM()
        {
            InitializeDatabase();
            ClientDBService = new ClientDBService();
            InvoiceDBService = new InvoiceDBService();
            AccountDBService = new AccountDBService();
            AddressDBService = new AddressDBService();
        }

        private async void InitializeDatabase()
        {
            await Windows.Storage.StorageFolder.GetFolderFromPathAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path);

            var dbExists = File.Exists(databasePath);
            if (!dbExists)
            {
                using (var connection = new SQLiteConnection(databasePath))
                {
                    connection.CreateTable<Account>();
                    connection.CreateTable<Address>();
                    connection.CreateTable<Client>();
                    connection.CreateTable<Communication>();
                    connection.CreateTable<Invoice>();
                }
            }
        }

        private async void LoadEntities()
        {
            var fetchClients = ClientDBService.GetEntities();
            var fetchInvoices = InvoiceDBService.GetEntities();
            var fetchAccount = AccountDBService.GetUserEntities();
            var fetchAddress = AddressDBService.GetUserEntities();

            Clients = new ObservableCollection<Client>(fetchClients.entities);
            Invoices = new ObservableCollection<Invoice>(fetchInvoices.entities);
            UserAccount = fetch
        }
        #endregion
    }
}
