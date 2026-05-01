using R3;
using System.Numerics;
using TouchChanX.WinUI.Menu;
using TouchChanX.WinUI.Touch;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Windows.UI.Xaml.Controls
{
    //public partial class Grid
    //{
    //}

    public static class GridExtensions
    {
        public static void Add(this Grid grid, UIElement element) => grid.Children.Add(element);

        extension(Grid grid)
        {
            public ColumnDefinitionCollection ColumnDefinitions
            {
                set
                {
                    foreach (var column in value)
                        grid.ColumnDefinitions.Add(column);
                }
            }
        }
    }
}

namespace TouchChanX.WinUI
{
    //<Grid>
    //    <touch:TouchControl x:Name="Touch" Visibility="{x:Bind top:Converters.InvertVisible(MenuTouch.Visibility), Mode=OneWay}" />
    //    <menu:MenuControl x:Name="MenuTouch" />
    //</Grid>

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : CoreIsland.Window
    {
        private readonly TouchControl Touch = new();
        private readonly MenuControl MenuTouch = new();

        private void InitializeComponent()
        {
            var grid = new Grid();
            grid.Children.Add(Touch);
            grid.Children.Add(MenuTouch);
            Content = grid;

        }

        public MainWindow()
        {
            InitializeComponent();

            Touch.Clicked
                .Select(rect => TouchDockAnchor.SnapFromRect(this.Content.ActualSize.ToSize(), rect))
                .Subscribe(MenuTouch.ShowAt);
        }

        /// <summary>
        /// 手动激活 window 的 xbind，用于设置为子窗口后，Activated 事件不触发的情景
        /// </summary>
        //public void InitializeBindings() => this.Bindings.Initialize();
    }

    /// <summary>
    /// 用于 x:Bind 的转换器。
    /// </summary>
    public static class Converters
    {
        public static CornerRadius CircleCornerRadius(double width) => new(width / 2);

        public static Visibility InvertVisible(Visibility visible) =>
            visible == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    public static class Shared
    {
        public const int TouchSpacing = 2;

        public const double TouchSize = 80.0;
        public const double MenuSize = TouchSize * 4;

        extension(FrameworkElement element)
        {
            public Observable<bool> IsVisibleChanged => Observable.Create<bool>(observer =>
            {
                var token = element.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (_, _) =>
                    observer.OnNext(element.Visibility == Visibility.Visible));

                return Disposable.Create(() => element.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, token));
            });
        }
    }
}
