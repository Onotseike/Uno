using System;
using System.Collections.Generic;
using System.Text;

using DemoApp.Database.Enums;

using SQLite;
using SQLiteNetExtensions.Attributes;

namespace DemoApp.Database.Entities
{
    [Table("Item")]
    public class Item
    {
        #region Column(s)

        /// <summary>
        /// Unique Identifier of Item record. (Required)
        /// </summary>
        [PrimaryKey, AutoIncrement]
        [Column("id"), NotNull]
        public Guid Id { get; set; }

        /// <summary>
        /// Type of item - Product or Service. (Required)
        /// </summary>
        [Column("item_type"), NotNull]
        public ItemType Type { get; set; }

        /// <summary>
        /// Description of Item record. (Required)
        /// </summary>
        [Column("description"), NotNull]
        public string Description { get; set; }

        /// <summary>
        /// Currency of Item record. (Required)
        /// </summary>
        [Column("currency"), NotNull]
        [MaxLength(4)]
        public string Currency { get; set; }

        /// <summary>
        /// Price of Item record. (Required)
        /// </summary>
        [Column("price"), NotNull]
        public double Price { get; set; }

        /// <summary>
        /// Discount attached to Item record.
        /// </summary>
        [Column("isDeleted")]
        public bool IsDeleted { get; set; }

        #endregion


    }
}
