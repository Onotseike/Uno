using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Demo.CloudProvider
{
    public interface ICloudProvider
    {
        Task<bool> SignInAsync();
        Task<bool> SignOutAsync();
        Task<bool> Restore(string databaseName);
        Task<bool> BackUp(string databaseName);
    }
}
