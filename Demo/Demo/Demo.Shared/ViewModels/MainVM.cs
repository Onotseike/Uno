using Demo.CloudProvider;
using Demo.Database.Entities;
using Demo.Database.Enums;
using Demo.Database.Services;
using Demo.Helpers;

using Microsoft.Identity.Client;

using Newtonsoft.Json.Linq;

using SQLite;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModels
{
    public class MainVM : BaseNotifyClass
    {
        #region Properties

        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<Invoice> Invoices { get; set; }
        public Account UserAccount { get; set; }
        public Address UserAddress { get; set; }

        private readonly string databasePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Demo.db"));
        //Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Demo.db");

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

        #region Summary Properties

        public List<(InvoiceStatus status, double amount, string currency)> GroupedByStatus { get; set; }
        public List<(Client client, List<(InvoiceStatus status, double amount, string currency)> statusAmount)> GroupedByClient { get; set; }

        public List<(Client client, double amount)> TopPaid { get; set; }
        public List<(Client client, double amount)> TopDue { get; set; }
        public List<(Client client, double amount)> TopVoid { get; set; }

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
            InitializeServices();
            LoadEntities();
            Summarize();
            Debug.WriteLine(databasePath);
            

            //OneDriveSetupAsync().ConfigureAwait(false);
        }

        #endregion

        #region DB Services

        private void InitializeDatabase()
        {
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

        private void InitializeServices()
        {
            ClientDBService = new ClientDBService();
            InvoiceDBService = new InvoiceDBService();
            AccountDBService = new AccountDBService();
            AddressDBService = new AddressDBService();
        }

        private void LoadEntities()
        {
            var fetchClients = ClientDBService.GetEntities();
            var fetchInvoices = InvoiceDBService.GetEntities();
            var fetchAccount = AccountDBService.GetUserEntities();
            var fetchAddress = AddressDBService.GetUserEntities();

            Clients = new ObservableCollection<Client>(fetchClients.entities);
            Invoices = new ObservableCollection<Invoice>(fetchInvoices.entities);
            UserAccount = fetchAccount.entities.FirstOrDefault();
            UserAddress = fetchAddress.entities.FirstOrDefault();

            if (UserAccount == null)
            {
                UserAccount = new Account();
                UserAccount.IsUser = true;
            }
            if (UserAddress == null)
            {
                UserAddress = new Address();
                UserAddress.IsUser = true;
            }
        }

        public async Task SaveAccount(Account account)
        {
            if (account.BankName != null && account.Holder != null && account.Currency != null)
            {
                var result = AccountDBService.UpdateEntity(account);
                if (!result.isSuccessful)
                {
                    await Dialogs.GenericDialogAsync("Account Save Failed", result.operationMessage, "OK");
                }
                await Dialogs.GenericDialogAsync($"Account Saved", result.operationMessage, "OK");
            }
        }

        public async Task SaveAddress(Address address)
        {
            if (address.AddressOne != null && address.City != null && address.Country != null && address.PostalCode != null)
            {
                var result = AddressDBService.UpdateEntity(address);
                if (!result.isSuccessful)
                {
                    await Dialogs.GenericDialogAsync("Account Save Failed", result.operationMessage, "OK");
                }
                await Dialogs.GenericDialogAsync($"Account Saved", result.operationMessage, "OK");
            }
        }

        #endregion

        #region Helper method(s)

        private void Summarize()
        {
            if (Invoices != null)
            {
                GroupedByStatus = Invoices.GroupBy(invoice => invoice.Status, (_status, invoices) =>
                 (
                     status: _status,
                     amount :invoices.Select(invoice => invoice.Items.Sum(item => item.Price)).Sum(),
                     currency : invoices.Select(invoice => invoice.Currency).FirstOrDefault()
                )).ToList();

                GroupedByClient = Invoices.GroupBy(invoice => invoice.Client, (_client, invoices) =>
                (
                    client: _client,
                    statusAmount: invoices.GroupBy(invoice => invoice.Status, (_status, _invoices) => (
                       _status: _status,
                       amount : _invoices.Select(invoice => invoice.Items.Sum(item => item.Price)).Sum(),
                       currency : _invoices.Select(invoice => invoice.Currency).FirstOrDefault()
                   )).ToList()
                )).ToList(); 

                var topPaid = GroupedByClient.Select(group => new { client = group.client, amount = group.statusAmount.FirstOrDefault(item => item.status == InvoiceStatus.Paid).amount });
                var topDue = GroupedByClient.Select(group => new { client = group.client, amount = group.statusAmount.FirstOrDefault(item => item.status == InvoiceStatus.Due).amount });
                var topVoid = GroupedByClient.Select(group => new { client = group.client, amount = group.statusAmount.FirstOrDefault(item => item.status == InvoiceStatus.Void).amount });

                TopPaid = topPaid.Select(item => (item.client, item.amount)).ToList();
                TopDue = topDue.Select(item => (item.client, item.amount)).ToList();
                TopVoid = topVoid.Select(item => (item.client, item.amount)).ToList();
            }
        }

        #endregion

        #region OneDrive Methods

        public async Task OneDriveSetupAsync()
        {
            try
            {
                OneDrive.BuildPublicClientApplication();
                var result = await OneDrive.InitializeWithSilentProviderAsync();
                if (result == null)
                {
                    result = await OneDrive.InitializeWithInteractiveProviderAsync();
                }
                await OneDrive.InitializeGraphClientAsync(result);
                var user  = await OneDrive.AccountInfo();
                Username = user.Value.username;
                UserEmail = user.Value.email;
                IsSignedIn = user.Value.isSignedIn;
            }            
            catch (Exception exception)
            {
                await Dialogs.ExceptionDialogAsync(exception);
            }
        }

        public async Task LogOutAsync() => await OneDrive.RemoveAccountsAsync();

        public async Task Restore()
        {
            try
            {
                await OneDrive.Restore("Demo.db");
                LoadEntities();
                await Dialogs.GenericDialogAsync($"Databased Restored", "Database was successfully restored.", "OK");
            }
            catch (Exception exception)
            {
                await Dialogs.ExceptionDialogAsync(exception);
            }
        }

        public async Task BackUp()
        {
            try
            {
                await OneDrive.BackUp("Demo.db");
                await Dialogs.GenericDialogAsync($"Databased Backed up", "Database was successfully backed up.", "OK");
            }
            catch (Exception exception)
            {
                await Dialogs.ExceptionDialogAsync(exception);
            }
        }
        #endregion
    }
}
