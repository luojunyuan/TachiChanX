using FluentIcons.Common;
using R3;
using R3.ObservableEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TouchChanX.WPF.Controls;

/// <summary>
/// MenuButton.xaml 的交互逻辑
/// </summary>
public partial class MenuButton : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register(nameof(Symbol), typeof(Symbol), typeof(MenuButton), new PropertyMetadata(Symbol.Emoji));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public Symbol Symbol
    {
        get { return (Symbol)GetValue(SymbolProperty); }
        set { SetValue(SymbolProperty, value); }
    }

    public Observable<Unit> Clicked => field ??= 
        this.Events().MouseUp
        .Select(_ => Unit.Default)
        .Share();

    public MenuButton()
    {
        InitializeComponent();
        InitializeVSMTransition();
    }

    private void InitializeVSMTransition()
    {
        this.Events().TouchEnter.Select(_ => Unit.Default)
            .Merge(this.Events().MouseDown.Select(_ => Unit.Default))
            .Merge(this.Events().MouseEnter.Where(e => e.LeftButton == MouseButtonState.Pressed).Select(_ => Unit.Default))
            .Subscribe(_ =>
                VisualStateManager.GoToState(this, nameof(PressedState), false));

        this.Events().TouchLeave.Select(_ => Unit.Default)
            .Merge(this.Events().MouseUp.Select(_ => Unit.Default))
            .Merge(this.Events().MouseLeave.Select(_ => Unit.Default))
            .Subscribe(_ =>
                VisualStateManager.GoToState(this, nameof(NormalState), false));
    }
}
