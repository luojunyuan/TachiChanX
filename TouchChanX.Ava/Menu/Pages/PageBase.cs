using Avalonia.Controls;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava.Menu.Pages;

public abstract partial class PageBase : UserControl
{
    protected abstract Grid ContentGrid { get; }
    
    public required MenuCell EntryCell { get; init; }

    public static event EventHandler? BackRequested;

    protected void AddBackItemToGrid()
    {
        var backItem = new MenuItem { Symbol = Symbol.ArrowLeft };
        Grid.SetRow(backItem, 1);
        Grid.SetColumn(backItem, 1);
        ContentGrid.Children.Add(backItem);
        backItem.Clicked
            .Select(_ => Unit.Default)
            .Subscribe(_ => BackRequested?.Invoke(this, EventArgs.Empty));
    }
}