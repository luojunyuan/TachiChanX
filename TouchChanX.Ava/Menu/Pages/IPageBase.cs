using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava.Menu.Pages;

public interface IPageBase
{
    protected Grid ContentGrid { get; }
    
    public static event EventHandler? BackRequested;

    public void AddBackItemToGrid()
    {
        var backItem = new MenuItem { Symbol = Symbol.ArrowLeft };
        Grid.SetRow(backItem, 1);
        Grid.SetColumn(backItem, 1);
        ContentGrid.Children.Add(backItem);
        backItem.Events().PointerPressed
            .Select(_ => Unit.Default)
            .Subscribe(_ => BackRequested?.Invoke(this, EventArgs.Empty));
    }
    
    private static readonly TimeSpan PageTransitionInDuration = TimeSpan.FromMilliseconds(400);
    
    private static MenuCell? CurrentEntryCell;
    
    public Storyboard BuildPageEnterStoryboard(MenuCell entry, double menuWidth)
    {
        CurrentEntryCell = entry;
        var animations = new List<(Control, Animation)>();

        var entryItemPos = GetItemPosition(entry, menuWidth);
        foreach (var item in this.GetGridItems())
        {
            var itemPos = GetItemPosition(item.Cell, menuWidth);

            // 设置 item 的初始位置为入口点 item 位置
            var offsetX = entryItemPos.X - itemPos.X;
            var offsetY = entryItemPos.Y - itemPos.Y;
            item.RenderTransform = new TranslateTransform(offsetX, offsetY);

            var anim = new Animation
            {
                Duration = PageTransitionInDuration,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, offsetX),
                            new Setter(TranslateTransform.YProperty, offsetY),
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(TranslateTransform.YProperty, 0d),
                        }
                    }
                }
            };
            animations.Add((item, anim));
        }

        return new Storyboard { Animations = animations };
    }

    public Storyboard BuildPageExitStoryboard(double menuWidth, TimeSpan duration)
    {
        var animations = new List<(Control, Animation)>();
        var exitItemPos = GetItemPosition(CurrentEntryCell!, menuWidth);

        foreach (var item in this.GetGridItems())
        {
            var itemPos = GetItemPosition(item.Cell, menuWidth);
            var offsetX = exitItemPos.X - itemPos.X;
            var offsetY = exitItemPos.Y - itemPos.Y;

            var anim = new Animation
            {
                Duration = duration,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(TranslateTransform.YProperty, 0d),
                            new Setter(Visual.OpacityProperty, 1d),
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, offsetX),
                            new Setter(TranslateTransform.YProperty, offsetY),
                            new Setter(Visual.OpacityProperty, 0d),
                        }
                    }
                }
            };
            animations.Add((item, anim));
        }

        return new Storyboard { Animations = animations };
    }
    
    private IEnumerable<MenuItem> GetGridItems()
    {
        foreach (var child in ContentGrid.Children)
        {
            if (child is MenuItem ctrl)
                yield return ctrl;
        }
    }
    
    /// <summary>
    /// 以 Menu 中心为坐标轴原点计算对应 cell 的位置
    /// </summary>
    private static Point GetItemPosition(MenuCell cell, double menuWidth)
    {
        var cellSize = menuWidth / 3;
        var x = (cell.Col - 1) * cellSize;
        var y = (cell.Row - 1) * cellSize;
        return new Point(x, y);
    }
}