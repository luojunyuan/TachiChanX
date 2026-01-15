using R3;
using System.Windows;
using TouchChanX.WPF.Menu;
using TouchChanX.WPF.Touch;

namespace TouchChanX.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Touch.Clicked.Subscribe(touchRect =>
        {
            Menu.FakeTouchDockAnchor = TouchDockAnchor.SnapFromRect(new(this.ActualWidth, this.ActualHeight), touchRect);
            Menu.Visibility = Visibility.Visible;
        });

        Menu.Closed
            .Prepend(Unit.Default)
            .Subscribe(_ => Menu.Visibility = Visibility.Collapsed);

        // 订阅执行任何动画期间都禁止整个页面再次交互
        Observable.Merge(TouchControl.AnimationRunning)
            .Subscribe(running => this.IsHitTestVisible = !running);
    }
}