using Demo.Database.Entities;
using Demo.Database.Enums;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Demo.Database
{
    public static class MockData
    {

        public static Account UserAccount = new Account
        {
            Holder = "Jane Doe",
            BankName = "Bank of the World",
            Number = "1234567890",
            Iban = "NIL",
            RoutingNumber = "NIL",
            IsUser = true,
            Currency = "EUR",
            Address = new Address
            {
                AddressOne = "House 123, Street 456, Off 789 Crescent",
                AddressTwo = "NIL",
                City = "Earth's City",
                State = "Earth's State",
                Country = "Earth's Country",
                PostalCode = "Earth's POST/ZIP Code",
                IsUser = true
            }    
        };

        public static ItemBlob itemA = new ItemBlob
        {
            ItemType = "Service",
            Description = "Web Application Development",
            Price = 500
        };

        public static ItemBlob itemB = new ItemBlob
        {
            ItemType = "Product",
            Description = "Web Application Development",
            Price = 500
        };

        public static Client clientA = new Client
        {
            Type = ClientType.Individual,
            Name = "Tessel Fabrics",
            Communication = new Communication
            {
                WorkEmail = "jane.fabrics@fabby.com",
                WorkPhone = "+2342774566262626"
            },
            BankAccount = new Account
            {
                BankName = "GTB Bank",
                Holder = "Tessel Fabrics LTD",
                Number = "6012532365",
                Iban = "NGGT 1000 1110 2222 4444",
                Currency = "NLD"
            },
            BillingAddress = new Address
            {
                AddressOne = "Industrial Avenue",
                City = "Tessel",
                State = "Zuid Hold",
                Country = "Netherlads",
                PostalCode = "562234",
                Type = AddressType.Both
            }
        };

        public static Client clientB = new Client
        {
            Type = ClientType.Individual,
            Name = "John Comma",
            Communication = new Communication
            {
                WorkEmail = "info@johncomma.co.uk",
                WorkPhone = "+23484747372718"
            },
            BankAccount = new Account
            {
                BankName = "Mugu Bank",
                Holder = "John Comma LTD",
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

        public static Invoice invoiceA = new Invoice
        {
            Currency = "EUR",
            IssueDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(30),
            Status = InvoiceStatus.Due,
            Client = clientA,
            FullName = UserAccount.Holder,
            UserAddress = UserAccount.Address,
            Items = new ObservableCollection<ItemBlob> { itemA, itemB, itemA, itemB }
        };


        public static Invoice invoiceB = new Invoice
        {
            Currency = "GBP",
            IssueDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(60),
            Status = InvoiceStatus.Due,
            Client = clientB,
            FullName = UserAccount.Holder,
            UserAddress = UserAccount.Address,
            Items = new ObservableCollection<ItemBlob> { itemA, itemB, itemA, itemB }
        };

    }
}
