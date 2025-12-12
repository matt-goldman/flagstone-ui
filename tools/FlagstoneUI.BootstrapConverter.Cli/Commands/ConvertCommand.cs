using System.CommandLine;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Cli.Commands;

/// <summary>
/// Convert command - converts Bootstrap CSS/SCSS to Flagstone UI XAML
/// </summary>
internal static class ConvertCommand
{
	public static Command Create()
	{
		var inputOption = new Option<string[]>(
			aliases: ["--input", "-i"],
			description: "Path(s) to Bootstrap CSS/SCSS file(s) or URL(s). Multiple files will be merged.")
		{
			IsRequired = true,
			AllowMultipleArgumentsPerToken = true
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

		var debugOption = new Option<bool>(
			aliases: ["--debug"],
			description: "Enable debug logging (shows all discovered variables)");

		var analysisMode = new Option<string>(
			aliases: ["--analysis-mode", "-a"],
			description: "Analysis mode: css (top-down CSS class analysis), variables (bottom-up variable mapping), or hybrid (both, default)",
			getDefaultValue: () => "hybrid");

		var command = new Command("convert", "Convert Bootstrap theme to Flagstone UI XAML")
		{
			inputOption,
			outputOption,
			formatOption,
			darkModeOption,
			namespaceOption,
			commentsOption,
			verboseOption,
			debugOption,
			analysisMode
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
			var debug = context.ParseResult.GetValueForOption(debugOption);
			var mode = context.ParseResult.GetValueForOption(analysisMode)!;

			try
			{
				await ExecuteConvertAsync(input, output, format, darkMode, ns, comments, verbose, debug, mode);
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
				
				if (verbose)
				{
					Console.Error.WriteLine(ex.StackTrace);
				}
				
				context.ExitCode = 1;
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
		string[] inputs,
		string output,
		string formatStr,
		string darkModeStr,
		string ns,
		bool includeComments,
		bool verbose,
		bool debug,
		string analysisMode)
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

		// Parse analysis strategy
		var strategy = analysisMode.ToLowerInvariant() switch
		{
			"css" => BootstrapConverterService.AnalysisStrategy.CssOnly,
			"variables" => BootstrapConverterService.AnalysisStrategy.VariablesOnly,
			_ => BootstrapConverterService.AnalysisStrategy.Hybrid
		};

		if (verbose)
		{
			Console.WriteLine($"Input files: {string.Join(", ", inputs)}");
			Console.WriteLine($"Output: {output}");
			Console.WriteLine($"Format: {format}");
			Console.WriteLine($"Dark Mode: {darkMode}");
			Console.WriteLine($"Namespace: {ns}");
			Console.WriteLine($"Comments: {includeComments}");
			Console.WriteLine($"Analysis Mode: {strategy}");
			Console.WriteLine();
		}

		// Create conversion request
		var request = new BootstrapConverterService.ConversionRequest
		{
			Inputs = inputs,
			Format = format,
			Strategy = strategy,
			EnableDebugLogging = debug,
			Options = new ConversionOptions
			{
				DarkModeStrategy = darkMode,
				IncludeComments = includeComments,
				Namespace = ns
			}
		};

		// Execute conversion using the service
		var service = new BootstrapConverterService();
		
		Console.Write("Converting Bootstrap theme... ");
		var result = await service.ConvertAsync(request);
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ResetColor();

		if (verbose)
		{
			Console.WriteLine($"  Color tokens: {result.Statistics.ColorTokens}");
			Console.WriteLine($"  Typography tokens: {result.Statistics.TypographyTokens}");
			Console.WriteLine($"  Spacing tokens: {result.Statistics.SpacingTokens}");
			Console.WriteLine($"  Border radius tokens: {result.Statistics.BorderRadiusTokens}");
			Console.WriteLine($"  Border width tokens: {result.Statistics.BorderWidthTokens}");
			if (result.Statistics.ComponentStylesExtracted > 0)
			{
				Console.WriteLine($"  Component styles extracted: {result.Statistics.ComponentStylesExtracted}");
			}
			if (result.Statistics.VariablesParsed > 0)
			{
				Console.WriteLine($"  Variables parsed: {result.Statistics.VariablesParsed}");
			}
		}

		// Generate XAML files
		Console.Write("Generating XAML files... ");
		var generator = new XamlThemeGenerator();
		
		Directory.CreateDirectory(output);
		
		await generator.GenerateFilesAsync(
			result.Tokens, 
			result.ThemeName, 
			output, 
			request.Options!, 
			result.ComponentStyles);
		
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
		Console.WriteLine($"  Styles.xaml: {Path.Combine(output, "Styles.xaml")}");
	}
}
