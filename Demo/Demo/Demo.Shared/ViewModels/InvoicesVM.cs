using Demo.Database.Entities;
using Demo.Database.Enums;
using Demo.Database.Services;
using Demo.Helpers;

using SQLite;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Text;

namespace Demo.ViewModels
{
    public class InvoicesVM : BaseNotifyClass
    {
        #region Properties

        public ObservableCollection<Invoice> Invoices { get; set; }

        #endregion     

        #region DBServices

        public InvoiceDBService InvoiceDBService { get; set; }

        #endregion

        #region DB Services

        private void InitializeServices()
        {
            InvoiceDBService = new InvoiceDBService();
        }

        private void LoadEntities()
        {
            var fetchInvoices = InvoiceDBService.GetEntities();
            Invoices = new ObservableCollection<Invoice>(fetchInvoices.entities);

            var itemA = new ItemBlob
            {
                ItemType = "Service",
                Description = "Web Application Development",
                Price = 500
            };
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


            var invoiceA = new Invoice
            {
                Currency = "EUR",
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Status = InvoiceStatus.Due,
                Client = clientA,
                FullName = "User",
                UserAddress = new Address(),
                Items = new List<ItemBlob> { itemA, itemA, itemA, itemA}
            };

            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
            Invoices.Add(invoiceA);
        }

        #endregion

        public InvoicesVM()
        {
            InitializeServices();
            LoadEntities();
        }
    }
}
