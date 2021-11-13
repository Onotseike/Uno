using Demo.Database.Entities;
using Demo.Database.Services;
using Demo.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.ViewModels
{
    public class InvoiceVM : BaseNotifyClass
    {
        #region Properties

        public Invoice Entity { get; set; }
        public InvoiceDBService Service { get; set; }
        public string Header { get; set; }

        private bool isNew;
        public bool IsNew
        {
            get => isNew;
            set => SetProperty(ref isNew, value);
        }

        #endregion

        #region Constructor(s)

        public InvoiceVM(Invoice entity = null, bool _isNew = true)
        {
            Entity = entity != null ? entity : new Invoice();
            isNew = _isNew;
            Service = new InvoiceDBService();
            Header = isNew ? "New Invoice" : "Edit Invoice";
        }

        #endregion

        #region Method(s)

        public (bool isSuccessful, string operationMessage, object errorObject) SaveEntity()
        {
            if (isNew)
            {
                return  Service.AddEntity(Entity);
            }
            else
            {
                return Service.UpdateEntity(Entity);
            }
        }        

        #endregion
    }
}
