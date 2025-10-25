using System.Drawing;

namespace TouchChanX.Ava;

public static class Shared
{
    public static class Constants
    {
        public const int TouchSpacing = 2;
    }
    
    public abstract record TouchDockAnchor(TouchDockAnchor.Tags Tag)
    {
        public enum Tags
        {
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        public static TouchDockAnchor NewLeft(double scale) => new Left(scale);
        public static TouchDockAnchor NewTop(double scale) => new Top(scale);
        public static TouchDockAnchor NewRight(double scale) => new Right(scale);
        public static TouchDockAnchor NewBottom(double scale) => new Bottom(scale);
        public static TouchDockAnchor NewTopLeft() => new TopLeft();
        public static TouchDockAnchor NewTopRight() => new TopRight();
        public static TouchDockAnchor NewBottomLeft() => new BottomLeft();
        public static TouchDockAnchor NewBottomRight() => new BottomRight();

        public record Left(double Scale) : TouchDockAnchor(Tags.Left);
        public record Top(double Scale) : TouchDockAnchor(Tags.Top);
        public record Right(double Scale) : TouchDockAnchor(Tags.Right);
        public record Bottom(double Scale) : TouchDockAnchor(Tags.Bottom);
        public record TopLeft() : TouchDockAnchor(Tags.TopLeft);
        public record TopRight() : TouchDockAnchor(Tags.TopRight);
        public record BottomLeft() : TouchDockAnchor(Tags.BottomLeft);
        public record BottomRight() : TouchDockAnchor(Tags.BottomRight);

        public bool IsTopLeft => Tag == Tags.TopLeft;
        public bool IsTopRight => Tag == Tags.TopRight;
        public bool IsBottomLeft => Tag == Tags.BottomLeft;
        public bool IsBottomRight => Tag == Tags.BottomRight;

        public static TouchDockAnchor Default { get; } = new Left(0.5);
        
        public static TouchDockAnchor FromRect(Size containerSize, Rectangle touchRect)
        {
            const int spacing = Constants.TouchSpacing;
        
            var right = containerSize.Width - spacing - touchRect.Width;
            var bottom = containerSize.Height - spacing - touchRect.Height;
        
            var x = touchRect.X;
            var y = touchRect.Y;
        
            return (x, y) switch
            {
                // Corners
                (spacing, spacing) => NewTopLeft(),
                (spacing, var py) when py == bottom => NewBottomLeft(),
                var (px, py) when px == right && py == spacing => NewTopRight(),
                var (px, py) when px == right && py == bottom => NewBottomRight(),
            
                // Edges
                (spacing, var py) => 
                    NewLeft((py + spacing + touchRect.Height / 2.0) / containerSize.Height),
                (var px, spacing) => 
                    NewTop((px + spacing + touchRect.Width / 2.0) / containerSize.Width),
                var (px, py) when px == right => 
                    NewRight((py + spacing + touchRect.Height / 2.0) / containerSize.Height),
                var (px, py) when py == bottom => 
                    NewBottom((px + spacing + touchRect.Width / 2.0) / containerSize.Width),
            
                _ => Default
            };
        }
    }
}