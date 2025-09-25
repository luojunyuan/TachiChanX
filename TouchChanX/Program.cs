using Avalonia;
using TouchChanX.Ava;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

var app = AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();

app.StartWithClassicDesktopLifetime(args);
