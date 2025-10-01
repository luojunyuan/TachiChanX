using Avalonia.Animation;
using Avalonia.Controls;

namespace TouchChanX.Ava.Menu;

public class Storyboard
{
    public required List<(Control, Animation)> Animations { get; init; }

    public Task PlayAsync()
    {
        var tasks = new List<Task>();
        foreach (var (control, animation) in Animations)
        {
            var t = animation.RunAsync(control);
            tasks.Add(t);
        }

        return Task.WhenAll(tasks);
    }
}