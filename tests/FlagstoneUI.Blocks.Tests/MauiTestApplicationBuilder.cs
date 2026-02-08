using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;

namespace FlagstoneUI.Blocks.Tests;

/// <summary>
/// Builder for creating a MAUI application instance for unit testing.
/// This provides the necessary MAUI infrastructure (dispatcher, application context)
/// for testing UI components that inherit from BindableObject.
/// </summary>
public static class MauiTestApplicationBuilder
{
	/// <summary>
	/// Creates and initializes a minimal MAUI application for testing purposes.
	/// </summary>
	/// <returns>A configured MAUI application instance.</returns>
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder.UseMauiApp<TestApplication>();

		// Configure a test dispatcher provider
		builder.Services.AddSingleton<IDispatcherProvider>(sp => new TestDispatcherProvider());

		return builder.Build();
	}

	/// <summary>
	/// Test application class used for unit testing.
	/// Provides minimal MAUI application infrastructure without actual UI.
	/// </summary>
	private class TestApplication : Application
	{
		protected override Window CreateWindow(IActivationState? activationState)
		{
			// Return a minimal window with empty content page - tests don't need actual UI
			return new Window(new ContentPage());
		}
	}

	/// <summary>
	/// Test dispatcher provider that provides a synchronous dispatcher for unit tests.
	/// </summary>
	private class TestDispatcherProvider : IDispatcherProvider
	{
		private readonly TestDispatcher _dispatcher = new();

		public IDispatcher GetForCurrentThread() => _dispatcher;
	}

	/// <summary>
	/// Synchronous test dispatcher that executes actions immediately on the current thread.
	/// </summary>
	private class TestDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public IDispatcherTimer CreateTimer() => throw new NotImplementedException();

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotImplementedException();
	}
}
