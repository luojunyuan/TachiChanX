using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TouchChanX.Ava.Menu.Pages;

public partial class GamePage : PageBase
{
    protected override Grid ContentGrid => this.GamePageGrid;

    public GamePage()
    {
        InitializeComponent();
        
        this.AddBackItemToGrid();
    }
}