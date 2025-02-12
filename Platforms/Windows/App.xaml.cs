using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace losertron4000.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp()
        {


            return MauiProgram.CreateMauiApp();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            // Check if the app was launched from a toast notification
            if (!string.IsNullOrEmpty(args.Arguments))
            {
                // Parse the activation arguments
                var arguments = args.Arguments;

                // Example: Deserialize JSON arguments
                var action = System.Text.Json.JsonDocument.Parse(arguments)
                    .RootElement.GetProperty("action").GetString();

                var data = System.Text.Json.JsonDocument.Parse(arguments)
                    .RootElement.GetProperty("data").GetString();

                // Perform an action based on the arguments
                if (action == "viewDetails")
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Process.Start(Path.PhotosDirectory);
                    });
                }
            }
        }
    }
}
