using Avalonia;
using System.Diagnostics.Contracts;

namespace TestAva;

public static class PositionCalculator
{
    /// <summary>
    /// 判断新的移动坐标是否超出规定边界
    /// </summary>
    [Pure]
    public static bool IsBeyondBoundary(Point newPos, double touchSize, Size container)
    {
        var oneThirdDistance = touchSize / 3;
        var twoThirdDistance = oneThirdDistance * 2;

        if (newPos.X < -oneThirdDistance ||
            newPos.Y < -oneThirdDistance ||
            newPos.X > container.Width - twoThirdDistance ||
            newPos.Y > container.Height - twoThirdDistance)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 计算 Touch 最终停靠位置
    /// </summary>
    [Pure]
    public static Point CalculateTouchFinalPosition(Size container, Rect touch)
    {
        const int TouchSpace = 2;

        var initPos = new Point(touch.X, touch.Y);
        var touchSize = touch.Width;
        var xMidline = container.Width / 2;
        var right = container.Width - initPos.X - touchSize;
        var bottom = container.Height - initPos.Y - touchSize;

        var hSnapLimit = touchSize / 2;
        var vSnapLimit = touchSize / 3 * 2;

        var centerToLeft = initPos.X + hSnapLimit;

        bool VCloseTo(double distance) => distance < vSnapLimit;
        bool HCloseTo(double distance) => distance < hSnapLimit;

        double AlignToRight() => container.Width - touchSize - TouchSpace;
        double AlignToBottom() => container.Height - touchSize - TouchSpace;

        var left = initPos.X;
        var top = initPos.Y;

        return
            HCloseTo(left) && VCloseTo(top) ? new Point(TouchSpace, TouchSpace) :
            HCloseTo(right) && VCloseTo(top) ? new Point(AlignToRight(), TouchSpace) :
            HCloseTo(left) && VCloseTo(bottom) ? new Point(TouchSpace, AlignToBottom()) :
            HCloseTo(right) && VCloseTo(bottom) ? new Point(AlignToRight(), AlignToBottom()) :
                               VCloseTo(top) ? new Point(left, TouchSpace) :
                               VCloseTo(bottom) ? new Point(left, AlignToBottom()) :
            centerToLeft < xMidline ? new Point(TouchSpace, top) :
         /* centerToLeft >= xMidline */           new Point(AlignToRight(), top);
    }
}
