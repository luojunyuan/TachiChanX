using Avalonia.Animation;
using Avalonia.Controls;

namespace TouchChanX.Ava.Menu;

// 在 Menu 下执行动画只使用 Storyboard 而不用 Animation
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

    public static Task PlayMultiAsync(params Storyboard[] storyboards) => 
        Task.WhenAll(storyboards.Select(storyboard => storyboard.PlayAsync()).ToList());
}

public static class StoryboardExtensions
{
    public static Storyboard AsStoryboard(this Animation animation, Control control) =>
        new()
        {
            Animations = [(control, animation)]
        };
}