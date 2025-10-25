using Avalonia.Controls;
using R3;
using TouchChanX.Ava.Menu.Pages.Components;

namespace TouchChanX.Ava.Menu.Pages;

public abstract partial class PageBase : UserControl
{
    protected abstract Grid ContentGrid { get; }
    
    public required MenuCell EntryCell { get; init; }
    
    public Observable<Unit> BackRequested { get; private set; } = Observable.Empty<Unit>();

    protected void AddBackItemToGrid()
    {
        var backItem = new MenuButton { Symbol = Symbol.ArrowLeft };
        Grid.SetRow(backItem, 1);
        Grid.SetColumn(backItem, 1);
        ContentGrid.Children.Add(backItem);
        
        BackRequested = backItem.Clicked;
    }
}