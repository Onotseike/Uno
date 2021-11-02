using Demo.Database.Entities;
using Demo.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Demo.ViewModels
{
    public class HomeVM : BaseNotifyClass
    {
        #region Properties

        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<Invoice> Invoice { get; set; }

        #endregion
    }
}
