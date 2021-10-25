using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Database.Entities
{
    public class ItemBlob
    {
        public string ItemType { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
    }
}
