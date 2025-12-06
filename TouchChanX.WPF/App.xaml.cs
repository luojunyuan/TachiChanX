using System.Windows;
using System.Windows.Threading;

namespace TouchChanX.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly SynchronizationContext UISyncContext = new DispatcherSynchronizationContext(Current.Dispatcher);
    }
}
