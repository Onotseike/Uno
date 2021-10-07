using System.Threading.Tasks;

namespace DemoApp.Services.CloudProviders
{
    public interface ICloudProvider
    {
        Task<bool> SignInAsync();
        Task<bool> SignOutAsync();
        Task<bool> Restore(string databaseName);
        Task<bool> BackUp(string databaseName);
    }
}
