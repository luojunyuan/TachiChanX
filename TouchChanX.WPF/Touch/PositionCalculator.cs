using System.Diagnostics.Contracts;
using System.Windows;

namespace TouchChanX.WPF.Touch;

public static class PositionCalculator
{
    /// <summary>
    /// 判断新的移动坐标是否超出规定边界
    /// </summary>
    [Pure]
    public static bool IsBeyondBoundary(Size container, Rect touchRect)
    {
        var oneThirdDistance = touchRect.Width / 3;
        var twoThirdDistance = oneThirdDistance * 2;

        return touchRect.X < -oneThirdDistance ||
               touchRect.Y < -oneThirdDistance ||
               touchRect.X > container.Width - twoThirdDistance ||
               touchRect.Y > container.Height - twoThirdDistance;
    }

    /// <summary>
    /// 计算 Touch 最终停靠位置
    /// </summary>
    [Pure]
    public static Point CalculateTouchFinalPosition(Size container, Rect touch, int spacing)
    {
        var (left, top) = (touch.X, touch.Y);
        var touchSize = touch.Width;
        var xMidline = container.Width / 2;
        var right = container.Width - left - touchSize;
        var bottom = container.Height - top - touchSize;

        var hSnapLimit = touchSize / 2;
        var vSnapLimit = touchSize / 3 * 2;

        var centerToLeft = left + hSnapLimit;

        var hCloseLeft = left < hSnapLimit;
        var hCloseRight = right < hSnapLimit;
        var vCloseTop = top < vSnapLimit;
        var vCloseBottom = bottom < vSnapLimit;

        var alignRight = container.Width - touchSize - spacing;
        var alignBottom = container.Height - touchSize - spacing;

        return
            hCloseLeft  && vCloseTop    ? new Point(spacing, spacing) :
            hCloseRight && vCloseTop    ? new Point(alignRight, spacing) :
            hCloseLeft  && vCloseBottom ? new Point(spacing, alignBottom) :
            hCloseRight && vCloseBottom ? new Point(alignRight, alignBottom) :
            vCloseTop    ? new Point(left, spacing) :
            vCloseBottom ? new Point(left, alignBottom) :
            centerToLeft < xMidline     ? new Point(spacing, top) :
            /* centerToLeft >= xMidline */   new Point(alignRight, top);
    }
    
    /// <summary>
    /// 更新 touch 的停靠位置。根据旧的信息计算新的 touch 应该处于的位置和大小
    /// </summary>
    public static Rect CalculateNewDockedPosition(Size oldSize, Rect touchRect, Size newSize, int spacing)
    {
        // 保证了 touch 位置一定是一个有效的值
        var legalPosition = CalculateTouchFinalPosition(oldSize, touchRect, spacing);
        
        // right, bottom 本地变量在这个函数里表示吸附在窗口右下角时 TranslateTransform X Y 应该的值
        var right = oldSize.Width - spacing - touchRect.Width;
        var bottom = oldSize.Height - spacing - touchRect.Height;
        
        var touchSize = touchRect.Size;
        var newRight = newSize.Width - spacing - touchRect.Width;
        var newBottom = newSize.Height - spacing - touchRect.Height;
        
        var scaleY = (touchRect.Y + spacing + touchRect.Height / 2) / oldSize.Height;
        var scaleX = (touchRect.X + spacing + (touchRect.Width / 2)) / oldSize.Width;
        var newY = newSize.Height * scaleY - spacing - touchRect.Height / 2;
        var newX = newSize.Width * scaleX - spacing - touchRect.Width / 2;
        
        return (legalPosition.X, legalPosition.Y) switch
        {
            var (x, y) when IsSnapped(x, spacing) && IsSnapped(y, spacing) => 
                new Rect(new Point(spacing, spacing)   , touchSize),
            var (x, y) when IsSnapped(x, spacing) && IsSnapped(y, bottom) => 
                new Rect(new Point(spacing, newBottom) , touchSize),
            var (x, y) when IsSnapped(x, right)   && IsSnapped(y, spacing) => 
                new Rect(new Point(newRight, spacing)  , touchSize),
            var (x, y) when IsSnapped(x, right)   && IsSnapped(y, bottom) => 
                new Rect(new Point(newRight, newBottom), touchSize),
            
            var (x, _) when IsSnapped(x, spacing) => 
                new Rect(new Point(spacing, newY), touchSize),
            var (_, y) when IsSnapped(y, spacing) => 
                new Rect(new Point(newX, spacing), touchSize),
            var (x, _) when IsSnapped(x, right) => 
                new Rect(new Point(newRight, newY), touchSize),
            var (_, y) when IsSnapped(y, bottom) => 
                new Rect(new Point(newX, newBottom), touchSize),
            
            _ => default,
        };
        
        static bool IsSnapped(double value, double target, double tolerance = 0.01d) => 
            Math.Abs(value - target) <= tolerance;
    }
}
