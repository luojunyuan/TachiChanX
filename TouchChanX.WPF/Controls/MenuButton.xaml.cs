using FluentIcons.Common;
using System.Windows;
using System.Windows.Controls;
using R3;
using R3.ObservableEvents;
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

    public MenuButton()
    {
        InitializeComponent();
        
        this.Events().MouseEnter
            .Where(e => e is { LeftButton: MouseButtonState.Pressed } or { RightButton: MouseButtonState.Pressed })
            .Merge(this.Events().MouseDown.Cast<MouseButtonEventArgs, MouseEventArgs>())
            .Subscribe(_ => 
                VisualStateManager.GoToState(this, nameof(PressedState), false));

        this.Events().MouseLeave
            .Merge(this.Events().MouseUp.Cast<MouseButtonEventArgs, MouseEventArgs>())
            .Subscribe(_ => 
                VisualStateManager.GoToState(this, nameof(NormalState), false));
    }
}
