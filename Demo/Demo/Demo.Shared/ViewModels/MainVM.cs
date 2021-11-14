using Demo.CloudProvider;
using Demo.Database;
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
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModels
{
    public class MainVM : BaseNotifyClass
    {
        #region Properties

        public ClientsVM ClientsVM { get; set; }
        public InvoicesVM InvoicesVM { get; set; }
        public SettingsVM SettingsVM { get; set; }

        public ObservableCollection<Invoice> Invoices { get; set; }
        
        private readonly string databasePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Demo.db"));
        

        #endregion
                
        #region Summary Properties

        public List<(InvoiceStatus status, double amount, string currency)> GroupedByStatus { get; set; }
        public List<(Client client, List<(InvoiceStatus status, double amount, string currency)> statusAmount)> GroupedByClient { get; set; }

        public List<(Client client, double amount)> TopPaid { get; set; }
        public List<(Client client, double amount)> TopDue { get; set; }
        public List<(Client client, double amount)> TopVoid { get; set; }

        #endregion

        #region Constructor(s)

        public MainVM()
        {
            InitializeDatabase();
            
            Summarize();
           
            SettingsVM = new SettingsVM();
            ClientsVM = new ClientsVM();
            InvoicesVM = new InvoicesVM();

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
                var userService = new AccountDBService();
                userService.AddEntity(MockData.UserAccount);
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

       
    }
}
