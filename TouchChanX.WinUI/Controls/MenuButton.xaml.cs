using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.WinUI.Controls;

public sealed partial class MenuButton : UserControl
{
    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register(
            nameof(Symbol),
            typeof(Symbol),
            typeof(MenuButton),
            new PropertyMetadata(Symbol.Emoji2));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MenuButton),
            new PropertyMetadata(string.Empty));

    public Symbol Symbol
    {
        get => (Symbol)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Observable<Unit> Clicked => field ??=
        this.Events().PointerReleased
        .Select(_ => Unit.Default)
        .Share();

    public MenuButton()
    {
        InitializeComponent();

        this.Events().PointerPressed
            .Merge(this.Events().PointerEntered)
            .Where(e => e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            .Subscribe(_ =>
                VisualStateManager.GoToState(this, "Pressed", true));

        this.Events().PointerReleased
            .Merge(this.Events().PointerExited)
            .Subscribe(_ =>
                VisualStateManager.GoToState(this, "Normal", true));
    }
}
