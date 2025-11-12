using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

namespace FlagstoneUI.Themes.Material.Tests;

/// <summary>
/// Base class for MAUI UI component tests.
/// Ensures a MAUI application context is initialized before tests run,
/// providing the dispatcher and application infrastructure needed for
/// BindableObject-derived components.
/// </summary>
public abstract class MauiTestBase : IDisposable
{
	private static MauiApp? _mauiApp;
	private static Application? _application;
	private static readonly object _lock = new();

	/// <summary>
	/// Initializes the MAUI application context if not already initialized.
	/// This is called before each test that inherits from this class.
	/// </summary>
	protected MauiTestBase()
	{
		InitializeMauiApp();
	}

	/// <summary>
	/// Initializes the MAUI application singleton.
	/// Thread-safe initialization ensures only one instance is created.
	/// </summary>
	private static void InitializeMauiApp()
	{
		if (_mauiApp is not null)
			return;

		lock (_lock)
		{
			if (_mauiApp is not null)
				return;

			_mauiApp = MauiTestApplicationBuilder.CreateMauiApp();
			_application = _mauiApp.Services.GetRequiredService<IApplication>() as Application;

			// Set as current application so BindableObjects can find the dispatcher
			if (_application is not null)
			{
				Application.SetCurrentApplication(_application);
			}
		}
	}

	/// <summary>
	/// Disposes of test resources. Override if your test class needs cleanup.
	/// </summary>
	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
