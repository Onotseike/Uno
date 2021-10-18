using SQLite;

using SQLiteNetExtensions.Attributes;

using System;

namespace DemoApp.Database.Entities
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
