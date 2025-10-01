using Avalonia.Controls;

namespace TouchChanX.Ava.Menu.Pages;

public partial class DevicePage : UserControl, IPageBase
{
    public Grid ContentGrid => this.DeviceGrid;

    public DevicePage()
    {
        InitializeComponent();
        (this as IPageBase).AddBackItemToGrid();
    }
}