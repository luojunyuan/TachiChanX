using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TouchChanX.Entry.Islands
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    public sealed partial class App : CoreIsland.Application
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
        }

    }
}
