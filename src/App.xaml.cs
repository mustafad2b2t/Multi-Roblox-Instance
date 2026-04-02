using System.Windows;
using RobloxMultiLauncher.Core;

namespace RobloxMultiLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize Application Paths (Migrate if necessary)
            AppPaths.Init();
        }
    }
}

