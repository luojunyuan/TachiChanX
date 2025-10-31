using Avalonia.Controls;
using TouchChanX.Ava.Menu.Pages.Components;

namespace TouchChanX.Ava.Menu.Pages;

public partial class MainPage : UserControl
{
    public IEnumerable<MenuButton> MenuButtons => 
        MainPageGrid.Children.OfType<MenuButton>();

    public MainPage()
    {
        InitializeComponent();
    }
}