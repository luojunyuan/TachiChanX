using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TouchChanX.Ava.Menu.Pages;

public partial class GamePage : UserControl, IPageBase
{
    public Grid ContentGrid => this.GamePageGrid;

    public GamePage()
    {
        InitializeComponent();
        (this as IPageBase).AddBackItemToGrid();
    }
}