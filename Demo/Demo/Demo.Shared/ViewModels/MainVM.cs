using Demo.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.ViewModels
{
    public class MainVM : BaseNotifyClass
    {
        #region Properties

        public ClientsVM ClientsVM { get; private set; }
        public HomeVM HomeVM { get; private set; }
        public InvoicesVM InvoicesVM { get; private set; }
        public SettingsVM SettingsVM { get; private set; }

        #endregion

        #region Constructor(s)

        public MainVM()
        {
            ClientsVM = new ClientsVM();
            HomeVM = new HomeVM();
            InvoicesVM = new InvoicesVM();
            SettingsVM = new SettingsVM();
        }


        #endregion
    }
}
