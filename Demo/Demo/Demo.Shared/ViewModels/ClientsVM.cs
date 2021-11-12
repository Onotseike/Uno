using Demo.Database.Entities;
using Demo.Database.Enums;
using Demo.Database.Services;
using Demo.Helpers;
using Demo.Pages;

using SQLite;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Demo.ViewModels
{
    public class ClientsVM : BaseNotifyClass
    {
        #region Properties

        public ObservableCollection<Client> Clients { get; set; }

        #endregion

        #region DBServices

        public ClientDBService ClientDBService { get; set; }

        #endregion

        #region DB Services        

        private void InitializeServices()
        {
            ClientDBService = new ClientDBService();
        }

        private void LoadEntities()
        {
            var fetchClients = ClientDBService.GetEntities();
            Clients = new ObservableCollection<Client>(fetchClients.entities);            
            var clientA = new Client
            {
                Type = ClientType.Individual,
                Name = "Anne Fabrics",
                Communication = new Communication
                {
                    WorkEmail = "anne.fabrics@fabby.com",
                    WorkPhone = "+2348033116328"
                },
                BankAccount = new Account
                {
                    BankName = "GTB Bank",
                    Holder = "Anne Fabrics LTD",
                    Number = "6012532365",
                    Iban = "NGGT 1000 1110 2222 4444",
                    Currency = "NGN"
                },
                BillingAddress = new Address
                {
                    AddressOne = "Industrial Avenue",
                    City = "Federal Capital Territory",
                    State = "Abuja",
                    Country = "Nigeria",
                    PostalCode = "9000108",
                    Type = AddressType.Both
                }
            };

            var clientB = new Client
            {
                Type = ClientType.Individual,
                Name = "Mio Communications",
                Communication = new Communication
                {
                    WorkEmail = "info@miocommunication.co.uk",
                    WorkPhone = "+2348044116328"
                },
                BankAccount = new Account
                {
                    BankName = "Monzo Bank",
                    Holder = "Mio Communication LTD",
                    Number = "4089456377",
                    Iban = "GBMZ 2000 3110 3322 4444",
                    Currency = "GBP"
                },
                BillingAddress = new Address
                {
                    AddressOne = "High Street Avenue",
                    City = "London",
                    Country = "United Kingdom",
                    PostalCode = "L12 3Dt",
                    Type = AddressType.Billing
                }
            };

            Clients.Add(clientA);
            Clients.Add(clientB);
            Clients.Add(clientB);
            Clients.Add(clientB);
            Clients.Add(clientB);
            Clients.Add(clientB);
        }

        #endregion

        public ClientsVM()
        {
            InitializeServices();
            LoadEntities();
        }
    }
}
