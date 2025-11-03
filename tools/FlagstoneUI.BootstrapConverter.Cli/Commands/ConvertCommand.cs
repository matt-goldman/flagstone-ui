using System.CommandLine;
using FlagstoneUI.BootstrapConverter;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Cli.Commands;

/// <summary>
/// Convert command - converts Bootstrap CSS/SCSS to Flagstone UI XAML
/// </summary>
internal static class ConvertCommand
{
	public static Command Create()
	{
		var inputOption = new Option<string>(
			aliases: ["--input", "-i"],
			description: "Path to Bootstrap CSS/SCSS file or URL")
		{
			IsRequired = true
		};

		var outputOption = new Option<string>(
			aliases: ["--output", "-o"],
			description: "Output directory for generated XAML files",
			getDefaultValue: () => Directory.GetCurrentDirectory());

		var formatOption = new Option<string>(
			aliases: ["--format", "-f"],
			description: "Input format: css, scss, or auto (default: auto)",
			getDefaultValue: () => "auto");

		var darkModeOption = new Option<string>(
			aliases: ["--dark-mode", "-d"],
			description: "Dark mode generation: auto, manual, or none (default: auto)",
			getDefaultValue: () => "auto");

		var namespaceOption = new Option<string>(
			aliases: ["--namespace", "-n"],
			description: "XAML namespace for generated resources",
			getDefaultValue: () => "FlagstoneUI.Resources");

		var commentsOption = new Option<bool>(
			aliases: ["--comments", "-c"],
			description: "Include purpose comments in generated XAML",
			getDefaultValue: () => true);

		var verboseOption = new Option<bool>(
			aliases: ["--verbose", "-v"],
			description: "Enable verbose output");

		var command = new Command("convert", "Convert Bootstrap theme to Flagstone UI XAML")
		{
			inputOption,
			outputOption,
			formatOption,
			darkModeOption,
			namespaceOption,
			commentsOption,
			verboseOption
		};

		command.SetHandler(async (context) =>
		{
			var input = context.ParseResult.GetValueForOption(inputOption)!;
			var output = context.ParseResult.GetValueForOption(outputOption)!;
			var format = context.ParseResult.GetValueForOption(formatOption)!;
			var darkMode = context.ParseResult.GetValueForOption(darkModeOption)!;
			var ns = context.ParseResult.GetValueForOption(namespaceOption)!;
			var comments = context.ParseResult.GetValueForOption(commentsOption);
			var verbose = context.ParseResult.GetValueForOption(verboseOption);

			try
			{
				await ExecuteConvertAsync(input, output, format, darkMode, ns, comments, verbose);
				context.ExitCode = 0;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine($"Error: {ex.Message}");
				Console.ResetColor();
				
				if (verbose)
				{
					Console.Error.WriteLine(ex.StackTrace);
				}
				
				context.ExitCode = 1;
			}
		});

		return command;
	}

	private static async Task ExecuteConvertAsync(
		string input,
		string output,
		string formatStr,
		string darkModeStr,
		string ns,
		bool includeComments,
		bool verbose)
	{
		// Parse format
		var format = formatStr.ToLowerInvariant() switch
		{
			"css" => BootstrapFormat.Css,
			"scss" => BootstrapFormat.Scss,
			_ => BootstrapFormat.Auto
		};

		// Parse dark mode strategy
		var darkMode = darkModeStr.ToLowerInvariant() switch
		{
			"manual" => DarkModeStrategy.Manual,
			"none" => DarkModeStrategy.None,
			_ => DarkModeStrategy.Auto
		};

		if (verbose)
		{
			Console.WriteLine($"Input: {input}");
			Console.WriteLine($"Output: {output}");
			Console.WriteLine($"Format: {format}");
			Console.WriteLine($"Dark Mode: {darkMode}");
			Console.WriteLine($"Namespace: {ns}");
			Console.WriteLine($"Comments: {includeComments}");
			Console.WriteLine();
		}

		// Step 1: Parse Bootstrap theme
		Console.Write("Parsing Bootstrap theme... ");
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
		
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ResetColor();

		if (verbose)
		{
			Console.WriteLine($"  Colors: {variables.Colors.Count}");
			Console.WriteLine($"  Typography: {variables.Typography.Count}");
			Console.WriteLine($"  Spacing: {variables.Spacing.Count}");
			Console.WriteLine($"  Borders: {variables.Borders.Count}");
			Console.WriteLine($"  Other: {variables.Other.Count}");
		}

		// Step 2: Map to Flagstone tokens
		Console.Write("Mapping to Flagstone UI tokens... ");
		var mapper = new BootstrapMapper();
		var options = new ConversionOptions
		{
			DarkModeStrategy = darkMode,
			IncludeComments = includeComments,
			Namespace = ns
		};
		
		var tokens = mapper.MapToFlagstoneTokens(variables, options);
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ResetColor();

		if (verbose)
		{
			Console.WriteLine($"  Color tokens: {tokens.Colors.Count}");
			Console.WriteLine($"  Typography tokens: {tokens.Typography.Count}");
			Console.WriteLine($"  Spacing tokens: {tokens.Spacing.Count}");
			Console.WriteLine($"  Border radius tokens: {tokens.BorderRadius.Count}");
			Console.WriteLine($"  Border width tokens: {tokens.BorderWidth.Count}");
		}

		// Step 3: Generate XAML files
		Console.Write("Generating XAML files... ");
		var generator = new XamlThemeGenerator();
		
		// Ensure output directory exists
		Directory.CreateDirectory(output);
		
		// Extract theme name from input file or use default
		var themeName = Path.GetFileNameWithoutExtension(input);
		if (string.IsNullOrWhiteSpace(themeName) || Uri.TryCreate(input, UriKind.Absolute, out _))
		{
			themeName = "Bootstrap";
		}
		
		await generator.GenerateFilesAsync(tokens, themeName, output, options);
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ResetColor();

		// Success summary
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Conversion complete!");
		Console.ResetColor();
		Console.WriteLine($"  Tokens.xaml: {Path.Combine(output, "Tokens.xaml")}");
		Console.WriteLine($"  Theme.xaml:  {Path.Combine(output, "Theme.xaml")}");
	}
}
