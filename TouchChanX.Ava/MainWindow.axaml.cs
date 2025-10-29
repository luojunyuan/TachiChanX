using Avalonia.Controls;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.Opened += (_, _) => Menu.IsVisible = false;
        Menu.Closed += (_, _) => Menu.IsVisible = false;

        BackgroundLayer.Events().PointerReleased
            .Where(_ => Menu.IsVisible)
            .SubscribeAwait(async (_, _) => await Menu.CloseMenuAsync());

        // 订阅执行任何动画期间都禁止整个页面再次交互
        Observable.Merge(Touch.AnimationRunning, Menu.AnimationRunning)
            .Subscribe(running => this.IsHitTestVisible = !running);

        Touch.Clicked.SubscribeAwait(async (dockAnchor, _) =>
        {
            Menu.FakeTouchDockAnchor = dockAnchor;
            Menu.IsVisible = true;
            // NOTE: 因为 Menu.IsVisible <> false，一直感知不到窗体的大小变化，所以打开它时基于父容器的 bounds 计算
            await Menu.ShowMenuAsync(this.Bounds.Size);
        });
    }
}
