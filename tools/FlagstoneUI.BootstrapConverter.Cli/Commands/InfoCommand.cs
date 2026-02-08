using System.CommandLine;
using FlagstoneUI.BootstrapConverter;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Cli.Commands;

/// <summary>
/// Info command - displays information about a Bootstrap theme without converting
/// </summary>
internal static class InfoCommand
{
	public static Command Create()
	{
		var inputOption = new Option<string>(
			aliases: ["--input", "-i"],
			description: "Path to Bootstrap CSS/SCSS file or URL")
		{
			IsRequired = true
		};

		var formatOption = new Option<string>(
			aliases: ["--format", "-f"],
			description: "Input format: css, scss, or auto (default: auto)",
			getDefaultValue: () => "auto");

		var command = new Command("info", "Display information about a Bootstrap theme")
		{
			inputOption,
			formatOption
		};

		command.SetHandler(async (context) =>
		{
			var input = context.ParseResult.GetValueForOption(inputOption)!;
			var format = context.ParseResult.GetValueForOption(formatOption)!;

			try
			{
				await ExecuteInfoAsync(input, format);
				context.ExitCode = 0;
			}
			catch (FileNotFoundException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine($"Error: {ex.Message}");
				Console.ResetColor();
				context.ExitCode = 1;
			}
			catch (IOException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine($"Error: File operation failed - {ex.Message}");
				Console.ResetColor();
				context.ExitCode = 1;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine($"Error: {ex.Message}");
				Console.ResetColor();
				context.ExitCode = 1;
			}
		});

		return command;
	}

	private static async Task ExecuteInfoAsync(string input, string formatStr)
	{
		// Parse format
		var format = formatStr.ToLowerInvariant() switch
		{
			"css" => BootstrapFormat.Css,
			"scss" => BootstrapFormat.Scss,
			_ => BootstrapFormat.Auto
		};

		Console.WriteLine($"Analyzing Bootstrap theme: {input}");
		Console.WriteLine();

		// Parse the theme
		var parser = new BootstrapParser();
		
		BootstrapVariables variables;
		if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
		{
			variables = await parser.ParseFromUrlAsync(uri.ToString(), format);
		}
		else if (File.Exists(input))
		{
			variables = await parser.ParseFromFileAsync(input, format);
		}
		else
		{
			throw new FileNotFoundException($"Input file not found: {input}");
		}

		// Display summary
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Bootstrap Variables Summary");
		Console.ResetColor();
		Console.WriteLine(new string('=', 40));
		Console.WriteLine();

		PrintCategory("Colors", variables.Colors);
		PrintCategory("Typography", variables.Typography);
		PrintCategory("Spacing", variables.Spacing);
		PrintCategory("Borders", variables.Borders);
		PrintCategory("Other", variables.Other);

		Console.WriteLine();
		Console.WriteLine($"Total variables: {variables.Colors.Count + variables.Typography.Count + variables.Spacing.Count + variables.Borders.Count + variables.Other.Count}");
	}

	private static void PrintCategory(string name, Dictionary<string, string> items)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"{name} ({items.Count}):");
		Console.ResetColor();

		if (items.Count == 0)
		{
			Console.WriteLine("  (none)");
		}
		else
		{
			foreach (var (key, value) in items.OrderBy(kvp => kvp.Key).Take(10))
			{
				var displayValue = value.Length > 50 ? value[..47] + "..." : value;
				Console.WriteLine($"  {key,-30} = {displayValue}");
			}

			if (items.Count > 10)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine($"  ... and {items.Count - 10} more");
				Console.ResetColor();
			}
		}

		Console.WriteLine();
	}
}
