using System.Diagnostics.Contracts;
using Avalonia;

namespace TouchChanX.Ava.Touch;

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
    public static Point CalculateTouchFinalPosition(Size container, Rect touch)
    {
        const int TouchSpace = 2;

        var (left, top) = new Point(touch.X, touch.Y);
        var touchSize = touch.Width;
        var xMidline = container.Width / 2;
        var right = container.Width - left - touchSize;
        var bottom = container.Height - top - touchSize;

        var hSnapLimit = touchSize / 2;
        var vSnapLimit = touchSize / 3 * 2;

        var centerToLeft = left + hSnapLimit;

        return
            HCloseTo(left) && VCloseTo(top) ? new Point(TouchSpace, TouchSpace) :
            HCloseTo(right) && VCloseTo(top) ? new Point(AlignToRight(), TouchSpace) :
            HCloseTo(left) && VCloseTo(bottom) ? new Point(TouchSpace, AlignToBottom()) :
            HCloseTo(right) && VCloseTo(bottom) ? new Point(AlignToRight(), AlignToBottom()) :
                               VCloseTo(top) ? new Point(left, TouchSpace) :
                               VCloseTo(bottom) ? new Point(left, AlignToBottom()) :
            centerToLeft < xMidline ? new Point(TouchSpace, top) :
         /* centerToLeft >= xMidline */           new Point(AlignToRight(), top);

        double AlignToBottom() => container.Height - touchSize - TouchSpace;
        double AlignToRight() => container.Width - touchSize - TouchSpace;
        bool HCloseTo(double distance) => distance < hSnapLimit;
        bool VCloseTo(double distance) => distance < vSnapLimit;
    }
}
