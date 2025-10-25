using Avalonia;

namespace TouchChanX.Ava.Touch;

// net10 extension 
public static class Extension
{
    public static System.Drawing.Size ToSystemSize(this Size size) => new((int)size.Width, (int)size.Height);
    
    public static System.Drawing.Rectangle ToSystemRect(this Rect rect) => 
        new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
}