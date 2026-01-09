using R3;
using System.Windows;
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

        // Touch.Click -> Set MenuFakeTouch, Touch.Visibility = Collapsed
        Touch.PreviewMouseLeftButtonUp += (_, _) => Touch.Visibility = Visibility.Collapsed;

        // 订阅执行任何动画期间都禁止整个页面再次交互
        Observable.Merge(TouchControl.AnimationRunning)
            .Subscribe(running => this.IsHitTestVisible = !running);
    }
}