using Tizen.Applications;

using Uno.UI.Runtime.Skia;

namespace Demo.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new Demo.App());
            host.Run();
        }
    }
}
