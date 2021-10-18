using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DemoApp.Database.Entities;
using DemoApp.Database.Exceptions;

using SQLite;

using SQLiteNetExtensions.Extensions;

namespace DemoApp.Database.Services
{
    public class ItemDBService : BaseDBService<Item>
    {
        #region CreateOperation(s)

        public override (bool isSuccessful, string operationMessage, object errorObject) AddEntity(Item entity)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    connection.Insert(entity);
                    return (true, $"Insert of type : {nameof(Item)} with Children was Successful.", null);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Insert of type : {nameof(Item)} with Children was not Successful. \n Exception Details: {exception} ", entity);
                }
            }
        }

        public override (bool isSuccessful, string operationMessage, object errorObject) AddEntities(Item[] entities)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    connection.InsertAll(entities);
                    return (true, $"Insert of a collection of type : {nameof(Item)} with Children was Successful.", null);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Insert of a collection of type : {nameof(Item)} with Children was not Successful. \n Exception Details: {exception} ", entities);
                }
            }
        }

        #endregion

        #region ReadOperation(s)

        public override (bool isSuccessful, string operationMessage, Item entity) GetEntity(Guid itemId)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    var item = connection.Get<Item>(itemId);
                    return (true, $"Read of entity of type : {nameof(Item)} was Successful.", item);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Read of entity of type : {nameof(Item)} was not Successful. \n Exception Details: {exception}.\n ITEM ID: {itemId}.", null);
                }
            }
        }

        public override (bool isSuccessful, string operationMessage, List<Item> entities) GetEntities(Guid[] itemIds = null)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    var items = itemIds == null ? connection.GetAllWithChildren<Item>() : connection.GetAllWithChildren<Item>(item => itemIds.Contains(item.Id), true);
                    return (true, $"Read of collection entity of type : {nameof(Item)} was Successful.", items);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Read of collection entity of type : {nameof(Item)} was not Successful. \n Exception Details: {exception}.\n ITEM IDs: {itemIds}.", null);
                }
            }
        }

        #endregion

        #region DeleteOperation(s)

        public override (bool isSuccessful, string operationMessage, object errorObject) DeleteEntity(Item item)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    //item.IsDeleted = true;
                    //sconnection.InsertOrReplaceWithChildren(item);
                    connection.Delete(item);
                    return (true, $"Delete of entity of type : {nameof(Item)} was Successful", null);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Delete of entity of type : {nameof(Item)} was not Successful. \n Exception Details: {exception}.\n ITEM IDs: {item.Id}.", null);
                }
            }
        }

        public override (bool isSuccessful, string operationMessage, object errorObject) DeleteEntities(Item[] itemIds)
        {
            using (var connection = new SQLiteConnection("./database/SoloDB.db"))
            {
                try
                {
                    //foreach (var item in itemIds)
                    //{
                    //    item.IsDeleted = true;
                    //    connection.InsertOrReplaceWithChildren(item);
                    //}
                    connection.DeleteAll(itemIds, true);
                    return (true, $"Delete of collection of entity of type : {nameof(itemIds)} was Successful", null);
                }
                catch (DatabaseException exception)
                {
                    return (false, $"Delete of collection of entity of type : {nameof(Item)} was not Successful. \n Exception Details: {exception}.\n ITEM IDs: {itemIds}.", null);
                }
            }
        }

        #endregion
    }
}
