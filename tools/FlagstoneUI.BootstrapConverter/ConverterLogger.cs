namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Simple logging utility for the Bootstrap converter
/// </summary>
public static class ConverterLogger
{
	private static bool _isEnabled;
	private static readonly object _lock = new();

	/// <summary>
	/// Enable or disable logging
	/// </summary>
	public static bool IsEnabled
	{
		get => _isEnabled;
		set => _isEnabled = value;
	}

	/// <summary>
	/// Log a debug message
	/// </summary>
	public static void Debug(string message)
	{
		if (!IsEnabled) return;

		lock (_lock)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine($"[DEBUG] {message}");
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Log an info message
	/// </summary>
	public static void Info(string message)
	{
		if (!IsEnabled) return;

		lock (_lock)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"[INFO] {message}");
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Log a warning message
	/// </summary>
	public static void Warning(string message)
	{
		if (!IsEnabled) return;

		lock (_lock)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"[WARN] {message}");
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Log a success message
	/// </summary>
	public static void Success(string message)
	{
		if (!IsEnabled) return;

		lock (_lock)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"[OK] {message}");
			Console.ResetColor();
		}
	}

	/// <summary>
	/// Log discovered variables
	/// </summary>
	public static void LogVariableDiscovered(string category, string name, string value)
	{
		if (!IsEnabled) return;

		var displayValue = value.Length > 60 ? value[..57] + "..." : value;
		Debug($"  [{category}] {name} = {displayValue}");
	}
}
