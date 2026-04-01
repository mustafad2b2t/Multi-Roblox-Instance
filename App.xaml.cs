using System.Windows;

namespace RobloxMultiLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Set working directory to the executable location
            // so accounts.json / settings.json are always in the same folder.
            System.IO.Directory.SetCurrentDirectory(
                System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location)
                ?? System.IO.Directory.GetCurrentDirectory());
        }
    }
}
