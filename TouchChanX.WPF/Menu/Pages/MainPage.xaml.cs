using System.Windows.Controls;
using TouchChanX.WPF.Controls;

namespace TouchChanX.WPF.Menu.Pages;

/// <summary>
/// MainPage.xaml 的交互逻辑
/// </summary>
public partial class MainPage : UserControl
{
    public IEnumerable<MenuButton> MenuButtons =>
        MainPageGrid.Children.OfType<MenuButton>();

    public MainPage()
    {
        InitializeComponent();
    }
}
