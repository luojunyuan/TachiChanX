using Avalonia.Controls;

namespace TouchChanX.Ava.Menu.Pages;

public partial class DevicePage : PageBase
{
    protected override Grid ContentGrid => this.DeviceGrid;
    
    public DevicePage()
    {
        InitializeComponent();
        
        this.AddBackItemToGrid();
    }
}