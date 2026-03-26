using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.WinUI.Menu;

public sealed partial class MenuControl : UserControl
{
    public void ShowAt(TouchDockAnchor touchDock)
    {
        _lastTouchDockAnchor = touchDock;
        Visibility = Visibility.Visible;
    }

    public MenuControl()
    {
        InitializeComponent();
        InitializeCompositionVisuals();

        var isVisibleSubj = new Subject<bool>();
        RegisterPropertyChangedCallback(VisibilityProperty, (_, _) => isVisibleSubj.OnNext(Visibility == Visibility.Visible));

        isVisibleSubj
            .Where(isVisible => isVisible)
            .SelectMany(_ => this.Events().LayoutUpdated.Take(1).AsUnitObservable())
            .SubscribeAwait(async (_, _) =>
            {
                TransitionPresentationVisible(true);
                await PlayMenuTransitionAnimationAsync();
                TransitionPresentationVisible(false);
            });

        this.Events().PointerReleased
            .Where(e => e.OriginalSource.Equals(MenuBorder) || e.OriginalSource.Equals(BackgroundLayer))
            .SubscribeAwait(async (_, _) =>
            {
                TransitionPresentationVisible(true);
                await PlayMenuTransitionAnimationAsync(false);
                TransitionPresentationVisible(false);
                Visibility = Visibility.Collapsed;
            });
    }

    /// <summary>
    /// 控制动画过渡层显隐，仅在动画前后调用。
    /// </summary>
    private void TransitionPresentationVisible(bool isVisible)
    {
        this.IsHitTestVisible = !isVisible;
        MenuBorder.Opacity = isVisible ? 0 : 1;
        TransitionShellHost.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        TransitionItemsHost.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}
