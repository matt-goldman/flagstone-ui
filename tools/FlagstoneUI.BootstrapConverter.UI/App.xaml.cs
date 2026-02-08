namespace FlagstoneUI.BootstrapConverter.UI;

public partial class App : Application
{
	//readonly MainPage _page;

	public App()//MainPage page)
	{
		InitializeComponent();
		//_page = page;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());// _page);
	}

	public App(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}
}
