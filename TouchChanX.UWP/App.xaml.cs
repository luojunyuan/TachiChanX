using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace TouchChanX.UWP
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            var window = args.Window;
            window.Content = new MainPage();
            window.Activate();
        }
    }
}
