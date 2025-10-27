using Avalonia;
using Avalonia.Controls;
using R3;
using R3.ObservableEvents;
using TouchChanX.Ava.Menu.Pages;
using TouchChanX.Ava.Menu.Pages.Components;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl : UserControl
{
    private const int TouchSpacing = Shared.Constants.TouchSpacing;
    
    private const double TouchSize = 80;

    public event EventHandler? Closed;
    
    public Shared.TouchDockAnchor FakeTouchDockAnchor { get; set; } = Shared.TouchDockAnchor.Default;
    
    public MenuControl()
    {
        InitializeComponent();

        MainPage.MenuButtons
            .Select(item => item.Clicked.Select(_ => item))
            .Merge()
            .SubscribeAwait(async (item, _) => await GoToInnerPageAsync(item));

        BackgroundLayer.Events().Tapped
            .SubscribeAwait(async (_, _) => await CloseMenuAsync());
    }
    
    private Point AnchorPoint(Shared.TouchDockAnchor anchor, Size? window = null)
    {
        var width = window?.Width ?? this.Bounds.Width;
        var height = window?.Height ?? this.Bounds.Height;
        var alignRight = width - TouchSize - TouchSpacing;
        var alignBottom = height - TouchSize - TouchSpacing;

        return anchor switch
        {
            { IsTopLeft: true } => new Point(TouchSpacing, TouchSpacing),
            { IsTopRight: true } => new Point(alignRight, TouchSpacing),
            { IsBottomLeft: true } => new Point(TouchSpacing, alignBottom),
            { IsBottomRight: true } => new Point(alignRight, alignBottom),
            Shared.TouchDockAnchor.Left x => new Point(TouchSpacing, x.Scale * height - TouchSize / 2 - TouchSpacing),
            Shared.TouchDockAnchor.Top x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, TouchSpacing),
            Shared.TouchDockAnchor.Right x => new Point(alignRight, x.Scale * height - TouchSize / 2 - TouchSpacing),
            Shared.TouchDockAnchor.Bottom x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, alignBottom),
            _ => default,
        };
    }
    
    public async Task ShowMenuAsync(Size window)
    {
        var pos = AnchorPoint(FakeTouchDockAnchor, window);
        await PlayShowMenuStoryboardAsync(pos, window);
        
        Menu.Classes.Add("expanded");
        Menu.RenderTransform = null;
    }

    public async Task CloseMenuAsync()
    {
        Menu.Classes.Remove("expanded");

        var pos = AnchorPoint(FakeTouchDockAnchor);
        await PlayCloseMenuStoryboardAsync(pos);

        if (InnerPageHost.Content is not null)
        {
            InnerPageHost.Content = null;
            MainPage.Opacity = 1d;
            MainPage.IsEnabled =  true;
        }
    
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private async Task GoToInnerPageAsync(object? sender)
    {
        PageBase innerPage = sender switch
        {
            MenuButton { Tag: "Device" } entryItem => new DevicePage { EntryCell = entryItem.Cell },
            MenuButton { Tag: "Game" } entryItem => new GamePage { EntryCell = entryItem.Cell },
            _ => throw new NotSupportedException(),
        };

        innerPage.BackRequested.SubscribeAwait(async (_, _) => await ReturnToMainPageAsync(innerPage));

        InnerPageHost.Content = innerPage;
        
        await PlayTransitionInnerPageStoryboardAsync(innerPage);
    }

    private async Task ReturnToMainPageAsync(object? sender)
    {
        if (sender is not PageBase innerPage) 
            return;

        await PlayTransitionMainPageStoryboardAsync(innerPage);

        InnerPageHost.Content = null;
    }
}