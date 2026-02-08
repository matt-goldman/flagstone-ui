using System.CommandLine;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.Cli.Commands;

/// <summary>
/// Convert command - converts Bootstrap CSS/SCSS to FlagstoneUI XAML
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

		var outputFormatOption = new Option<string>(
			aliases: ["--output-format"],
			description: "Output format: xaml or csharp (default: xaml)",
			getDefaultValue: () => "xaml");

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

		var includeFontsOption = new Option<bool>(
			aliases: ["--include-fonts"],
			description: "Include font information in conversion result",
			getDefaultValue: () => false);

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

		var command = new Command("convert", "Convert Bootstrap theme to FlagstoneUI XAML")
		{
			inputOption,
			outputOption,
			formatOption,
			outputFormatOption,
			darkModeOption,
			namespaceOption,
			commentsOption,
			includeFontsOption,
			verboseOption,
			debugOption,
			analysisMode
		};

		command.SetHandler(async (context) =>
		{
			var input = context.ParseResult.GetValueForOption(inputOption)!;
			var output = context.ParseResult.GetValueForOption(outputOption)!;
			var format = context.ParseResult.GetValueForOption(formatOption)!;
			var outputFormat = context.ParseResult.GetValueForOption(outputFormatOption)!;
			var darkMode = context.ParseResult.GetValueForOption(darkModeOption)!;
			var ns = context.ParseResult.GetValueForOption(namespaceOption)!;
			var comments = context.ParseResult.GetValueForOption(commentsOption);
			var includeFonts = context.ParseResult.GetValueForOption(includeFontsOption);
			var verbose = context.ParseResult.GetValueForOption(verboseOption);
			var debug = context.ParseResult.GetValueForOption(debugOption);
			var mode = context.ParseResult.GetValueForOption(analysisMode)!;

			try
			{
				await ExecuteConvertAsync(input, output, format, outputFormat, darkMode, ns, comments, includeFonts, verbose, debug, mode);
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
		string outputFormatStr,
		string darkModeStr,
		string ns,
		bool includeComments,
		bool includeFonts,
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

		// Parse output format
		var outputFormat = outputFormatStr.ToLowerInvariant() switch
		{
			"csharp" or "cs" => ResourceDictionaryFormat.CSharp,
			_ => ResourceDictionaryFormat.Xaml
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
			"css" => AnalysisStrategy.CssOnly,
			"variables" => AnalysisStrategy.VariablesOnly,
			_ => AnalysisStrategy.Hybrid
		};

		if (verbose)
		{
			Console.WriteLine($"Input files: {string.Join(", ", inputs)}");
			Console.WriteLine($"Output: {output}");
			Console.WriteLine($"Format: {format}");
			Console.WriteLine($"Output Format: {outputFormat}");
			Console.WriteLine($"Dark Mode: {darkMode}");
			Console.WriteLine($"Namespace: {ns}");
			Console.WriteLine($"Comments: {includeComments}");
			Console.WriteLine($"Analysis Mode: {strategy}");
			Console.WriteLine();
		}

		// Create conversion request
		var request = new ConversionRequest
		{
			Inputs				= inputs,
			Format				= format,
			Strategy			= strategy,
			EnableDebugLogging	= debug,
			Options				= new ConversionOptions
			{
				DarkModeStrategy	= darkMode,
				IncludeComments		= includeComments,
				Namespace			= ns,
				OutputFormat		= outputFormat,
				IncludeFonts		= includeFonts
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

		// Generate files
		var fileType = outputFormat == ResourceDictionaryFormat.CSharp ? "C# files" : "XAML files";
		Console.Write($"Generating {fileType}... ");

		Directory.CreateDirectory(output);

		if (outputFormat == ResourceDictionaryFormat.CSharp)
		{
			var generator = new CSharpThemeGenerator();
			var tokensCs = generator.GenerateTokensCs(result.Tokens, request.Options);
			var themeCs = generator.GenerateThemeCs(result.Tokens, result.ThemeName, request.Options);
			var stylesCs = generator.GenerateStylesCs(result.Tokens, result.ThemeName, request.Options);

			await File.WriteAllTextAsync(Path.Combine(output, "Tokens.cs"), tokensCs);
			await File.WriteAllTextAsync(Path.Combine(output, "Theme.cs"), themeCs);
			await File.WriteAllTextAsync(Path.Combine(output, "Styles.cs"), stylesCs);
		}
		else
		{
			var generator = new XamlThemeGenerator();
			var tokensXaml = generator.GenerateTokensXaml(result.Tokens, request.Options);
			var themeXaml = generator.GenerateThemeXaml(result.Tokens, result.ThemeName, request.Options);
			var stylesXaml = generator.GenerateStylesXaml(result.Tokens, result.ThemeName, result.ComponentStyles, request.Options);
			var themeCodeBehind = generator.GenerateCodeBehind($"{request.Options!.Namespace}.{SanitizeThemeName(result.ThemeName)}", result.ThemeName);
			var stylesCodeBehind = generator.GenerateCodeBehind($"{request.Options.Namespace}.{SanitizeThemeName(result.ThemeName)}Styles", $"{result.ThemeName} Styles");

			await File.WriteAllTextAsync(Path.Combine(output, "Tokens.xaml"), tokensXaml);
			await File.WriteAllTextAsync(Path.Combine(output, "Theme.xaml"), themeXaml);
			await File.WriteAllTextAsync(Path.Combine(output, "Theme.xaml.cs"), themeCodeBehind);
			await File.WriteAllTextAsync(Path.Combine(output, "Styles.xaml"), stylesXaml);
			await File.WriteAllTextAsync(Path.Combine(output, "Styles.xaml.cs"), stylesCodeBehind);
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓");
		Console.ResetColor();

		// Success summary
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine("Conversion complete!");
		Console.ResetColor();

		var fileExtension = outputFormat == ResourceDictionaryFormat.CSharp ? "cs" : "xaml";
		Console.WriteLine($"  Tokens.{fileExtension}: {Path.Combine(output, $"Tokens.{fileExtension}")}");
		Console.WriteLine($"  Theme.{fileExtension}:  {Path.Combine(output, $"Theme.{fileExtension}")}");
		Console.WriteLine($"  Styles.{fileExtension}: {Path.Combine(output, $"Styles.{fileExtension}")}");

		// Display font information if requested
		if (includeFonts && result.Fonts != null && result.Fonts.HasFonts)
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("⚠ Font Setup Required");
			Console.ResetColor();

			foreach (var family in result.Fonts.Families)
			{
				Console.WriteLine();
				Console.WriteLine($"Font: {family.Name}");
				Console.WriteLine($"  Source: {family.Source}");

				if (family.Weights.Count > 0)
				{
					Console.WriteLine($"  Weights: {string.Join(", ", family.Weights.OrderBy(w => w))}");
				}

				if (family.HasItalic)
				{
					Console.WriteLine($"  Italic: Yes");
				}

				Console.WriteLine($"  Suggested Alias: \"{family.SuggestedAlias}\"");
			}

			// Display download URLs
			if (result.Fonts.DownloadUrls.Count > 0)
			{
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Download fonts from:");
				Console.ResetColor();
				foreach (var url in result.Fonts.DownloadUrls)
				{
					Console.WriteLine($"  {url}");
				}
			}

			// Display registration instructions
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Registration Instructions:");
			Console.ResetColor();
			Console.WriteLine("1. Download font files (.ttf or .otf format)");
			Console.WriteLine("2. Add fonts to your project (e.g., Resources/Fonts/)");
			Console.WriteLine("3. Register in MauiProgram.cs:");
			Console.WriteLine();
			Console.WriteLine("   builder.ConfigureFonts(fonts =>");
			Console.WriteLine("   {");

			foreach (var family in result.Fonts.Families.Where(f => f.Source != FontSource.System))
			{
				var fileName = $"{family.SuggestedAlias}-Regular.ttf";
				Console.WriteLine($"       fonts.AddFont(\"{fileName}\", \"{family.SuggestedAlias}\");");
			}

			Console.WriteLine("   });");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("⚠ Always verify font licenses before using downloaded fonts in your application.");
			Console.ResetColor();
		}
	}

	private static string SanitizeThemeName(string themeName)
	{
		if (string.IsNullOrWhiteSpace(themeName))
			return "Theme";

		var sanitized = new System.Text.StringBuilder();
		var needsCapital = true;

		foreach (var ch in themeName)
		{
			if (char.IsLetterOrDigit(ch))
			{
				sanitized.Append(needsCapital ? char.ToUpper(ch) : ch);
				needsCapital = false;
			}
			else if (ch == '_')
			{
				sanitized.Append('_');
				needsCapital = false;
			}
			else
			{
				// Skip invalid characters and capitalize next letter
				needsCapital = true;
			}
		}

		var result = sanitized.ToString();

		// Ensure it starts with a letter or underscore
		if (result.Length > 0 && char.IsDigit(result[0]))
			result = "_" + result;

		return string.IsNullOrEmpty(result) ? "Theme" : result;
	}
}
