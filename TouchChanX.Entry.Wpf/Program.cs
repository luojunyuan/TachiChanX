Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

var app = new TouchChanX.WPF.App();
app.InitializeComponent();
app.Run(new TouchChanX.WPF.MainWindow());
