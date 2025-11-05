using Windows.UI.Xaml.Controls;

namespace TouchChanX.UWP
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Content = "Clicked!";
            }
        }
    }
}
