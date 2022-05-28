using Bogus;

using Demo.Models;

using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.MockData
{
    internal class MockData
    {

        public Faker<Author> Author { get; set; }
        public Faker<Book> Book { get; set; }
        
        public MockData()
        {
            Author = new Faker<Author>()
                .RuleFor(a => a.Id, Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Person.FullName);

            Book = new Faker<Book>()
                .RuleFor(b => b.Id, Guid.NewGuid())
                .RuleFor(b => b.Title, f => f.Commerce.ProductName())
                .RuleFor(b => b.Author, f => f.PickRandom(Author.Generate()))
                .RuleFor(b => b.Genre, f => f.G);
        }
    }
}
