using Avalonia.Controls;

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