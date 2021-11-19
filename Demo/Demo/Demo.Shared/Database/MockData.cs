using Demo.Database.Entities;
using Demo.Database.Enums;
using Bogus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Bogus.Extensions.UnitedKingdom;

namespace Demo.Database
{
    public static class MockData
    {
        public static Faker<Address> AddressFaker = new Faker<Address>()
            .StrictMode(true)
            .RuleFor(address => address.AddressOne, faker => faker.Address.StreetAddress())
            .RuleFor(address => address.AddressTwo, faker => faker.Address.SecondaryAddress())
            .RuleFor(address => address.City, faker => faker.Address.City())
            .RuleFor(address => address.State, faker => faker.Address.State())
            .RuleFor(address => address.Country, faker => faker.Address.Country())
            .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode())
            .RuleFor(address => address.IsUser, false)
            .RuleFor(address => address.Type, faker => faker.PickRandom<AddressType>());

        public static Faker<Account> AccountFaker = new Faker<Account>()
            .StrictMode(true)
            .RuleFor(account => account.IsUser, false)
            .RuleFor(account => account.BankName, faker => $"{faker.Company.CompanyName()} BANK")
            .RuleFor(account => account.Holder, faker => faker.Person.FullName)
            .RuleFor(account => account.Number, faker => faker.Finance.Account())
            .RuleFor(account => account.Iban, faker => faker.Finance.Iban())
            .RuleFor(account => account.RoutingNumber, faker => faker.Finance.RoutingNumber())
            .RuleFor(account => account.SwiftNumber, faker => faker.Finance.SortCode())
            .RuleFor(account => account.Currency, faker => faker.Finance.Currency().Code)
            .RuleFor(account => account.Address, AddressFaker.Generate())
            .RuleFor(account => account.Client, ClientFaker.Generate());
        
        public static Faker<Address> UserAddressFaker = new Faker<Address>()
            .StrictMode(true)
            .RuleFor(address => address.AddressOne, faker => faker.Address.StreetAddress())
            .RuleFor(address => address.AddressTwo, faker => faker.Address.SecondaryAddress())
            .RuleFor(address => address.City, faker => faker.Address.City())
            .RuleFor(address => address.State, faker => faker.Address.State())
            .RuleFor(address => address.Country, faker => faker.Address.Country())
            .RuleFor(address => address.PostalCode, faker => faker.Address.ZipCode())
            .RuleFor(address => address.IsUser, true)
            .RuleFor(address => address.Type, faker => faker.PickRandom<AddressType>());
            
        
        public static Faker<Account> UserAccountFaker = new Faker<Account>()
            .StrictMode(true)
            .RuleFor(account => account.IsUser, true)
            .RuleFor(account => account.BankName, faker => $"{faker.Company.CompanyName()} BANK")
            .RuleFor(account => account.Holder, faker => faker.Person.FullName)
            .RuleFor(account => account.Number, faker => faker.Finance.Account())
            .RuleFor(account => account.Iban, faker => faker.Finance.Iban())
            .RuleFor(account => account.RoutingNumber, faker => faker.Finance.RoutingNumber())
            .RuleFor(account => account.SwiftNumber, faker => faker.Finance.SortCode())
            .RuleFor(account => account.Currency, faker => faker.Finance.Currency().Code)
            .RuleFor(account => account.Address, UserAddressFaker.Generate());

        public static Faker<ItemBlob> ItemBlobFaker = new Faker<ItemBlob>()
            .RuleFor(itemBlob => itemBlob.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(itemBlob => itemBlob.Price, faker => Double.Parse(faker.Commerce.Price()))
            .RuleFor(itemBlob => itemBlob.ItemType, faker => faker.Commerce.ProductAdjective());

        public static Faker<Client> ClientFaker = new Faker<Client>()
            .RuleFor(client => client.Type, faker => faker.PickRandom<ClientType>())
            .RuleFor(client => client.Name, faker => faker.Company.CompanyName())
            .RuleFor(client => client.Communication, CommunicationFaker.Generate())
            .RuleFor(client => client.BankAccount, AccountFaker.Generate())
            .RuleFor(client => client.BillingAddress, AddressFaker.Generate());

        public static Faker<Communication> CommunicationFaker = new Faker<Communication>()
            .RuleFor(communication => communication.HomeEmail, faker => faker.Person.Email)
            .RuleFor(communication => communication.WorkEmail, faker => faker.Person.Email)
            .RuleFor(communication => communication.HomePhone, faker => faker.Person.Phone)
            .RuleFor(communication => communication.WorkPhone, faker => faker.Phone.PhoneNumber())
            .RuleFor(communication => communication.Website, faker => faker.Person.Website)
            .RuleFor(communication => communication.Client, ClientFaker.Generate())
            .RuleFor(communication => communication.IsUser, false);
        
        public static Faker<Communication> UserCommunicationFaker = new Faker<Communication>()
            .RuleFor(communication => communication.HomeEmail, faker => faker.Person.Email)
            .RuleFor(communication => communication.WorkEmail, faker => faker.Person.Email)
            .RuleFor(communication => communication.HomePhone, faker => faker.Person.Phone)
            .RuleFor(communication => communication.WorkPhone, faker => faker.Phone.PhoneNumber())
            .RuleFor(communication => communication.Website, faker => faker.Person.Website)
            .RuleFor(communication => communication.IsUser, true);

        public static Faker<Invoice> InvoiceFaker = new Faker<Invoice>()
            .RuleFor(invoice => invoice.Currency, faker => faker.Finance.Currency().Code)
            .RuleFor(invoice => invoice.IssueDate, faker => faker.Date.Recent())
            .RuleFor(invoice => invoice.DueDate, faker => faker.Date.Soon(30))
            .RuleFor(invoice => invoice.Status, faker => faker.PickRandom<InvoiceStatus>())
            .RuleFor(invoice => invoice.UserAddress, UserAccount.Address)
            .RuleFor(invoice => invoice.UserBankAccount, UserAccount)
            .RuleFor(invoice => invoice.Client, ClientFaker.Generate());

        public static Account UserAccount = UserAccountFaker.Generate();

        public static Communication UserCommunication = UserCommunicationFaker.Generate();
        
        

    }
}
