using TouchChanX.Entry.Islands;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

var app = new App();
app.Initialize();
var win = new TouchChanX.WinUI.MainWindow()
{
};
win.Activate();
app.Run();
