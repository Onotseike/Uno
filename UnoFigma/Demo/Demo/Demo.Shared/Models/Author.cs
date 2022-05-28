using System;
using System.Collections.Generic;
using System.Text;

using static Demo.Constants.Constants;

namespace Demo.Models
{
    internal class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Twitter { get; set; }        
        public ICollection<Book> AuthoredBooks { get; set; }
        


    }

    internal class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Genre Genre { get; set; }
        public Ratings Rating { get; set; }
        public string Isbn { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishedDate { get; set; }
        public Language Language { get; set; }
        public Author Author { get; set; }

    }    
}
