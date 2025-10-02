using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace TouchChanX.Ava.Menu.Pages;

public partial class PageBase
{
    private static readonly TimeSpan PageShowDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan PageBackDuration = TimeSpan.FromMilliseconds(250);
    
    public Storyboard BuildPageTranslateStoryboard(double menuWidth, bool reverse = false)
    {
        var entryItemPos = GetItemPosition(EntryCell, menuWidth);
        var animations = 
            (from item in this.GetGridItems() 
                let itemPos = GetItemPosition(item.Cell, menuWidth) 
                let anim = new Animation
                {
                    Duration = reverse ? PageBackDuration : PageShowDuration, 
                    FillMode = FillMode.Forward,
                    Easing = reverse ? new LinearEasing() : new CubicEaseOut(),
                    PlaybackDirection = reverse ? PlaybackDirection.Reverse : PlaybackDirection.Normal,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0d), 
                            Setters =
                            {
                                // 起始位置在入口点 cell
                                new Setter(TranslateTransform.XProperty, entryItemPos.X - itemPos.X), 
                                new Setter(TranslateTransform.YProperty, entryItemPos.Y - itemPos.Y),
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
                } 
                select (item, anim)).Select(dummy => ((Control, Animation))dummy).ToList();

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