using Avalonia.Controls;

namespace TouchChanX.Ava;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Opened += (_, _) => Menu.IsVisible = false;
        Menu.Closed += (_, _) => Menu.IsVisible = false;
    }

    // ReSharper disable once AsyncVoidMethod 不用考虑不会发生的事情
    private async void TouchControlOnClicked(object? sender, Shared.TouchDockAnchor dockAnchor)
    {
        Menu.FakeTouchDockAnchor = dockAnchor;
        Menu.IsVisible = true;
        // NOTE: 因为 Menu.IsVisible <> false，一直感知不到窗体的大小变化，所以打开它时基于父容器的 bounds 计算
        await Menu.ShowMenuAsync(this.Bounds);
    }
}
