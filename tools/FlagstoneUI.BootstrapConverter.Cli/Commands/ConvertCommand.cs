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
		// Enable logging if debug is requested
		if (debug)
		{
			ConverterLogger.IsEnabled = true;
			ConverterLogger.Info("Debug logging enabled");
		}

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
			Console.WriteLine($"Input files: {string.Join(", ", inputs)}");
			Console.WriteLine($"Output: {output}");
			Console.WriteLine($"Format: {format}");
			Console.WriteLine($"Dark Mode: {darkMode}");
			Console.WriteLine($"Namespace: {ns}");
			Console.WriteLine($"Comments: {includeComments}");
			Console.WriteLine($"Analysis Mode: {analysisMode}");
			Console.WriteLine();
		}

		FlagstoneTokens tokens;
		var options = new ConversionOptions
		{
			DarkModeStrategy = darkMode,
			IncludeComments = includeComments,
			Namespace = ns
		};

		// Determine analysis strategy
		var useVariableAnalysis = analysisMode.ToLowerInvariant() is "variables" or "hybrid";
		var useCssAnalysis = analysisMode.ToLowerInvariant() is "css" or "hybrid";

		if (useCssAnalysis)
		{
			// New top-down approach: analyze CSS classes
			Console.Write("Analyzing Bootstrap CSS classes... ");
			
			// Read CSS content
			var cssContents = new List<string>();
			foreach (var input in inputs)
			{
				string content;
				if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
				{
					using var client = new HttpClient();
					content = await client.GetStringAsync(uri);
				}
				else if (File.Exists(input))
				{
					content = await File.ReadAllTextAsync(input);
				}
				else
				{
					throw new FileNotFoundException($"Input file not found: {input}");
				}
				cssContents.Add(content);
			}
			
			var analyzer = new BootstrapCssAnalyzer();
			var componentStyles = cssContents.Count == 1 
				? analyzer.AnalyzeComponents(cssContents[0])
				: analyzer.AnalyzeMultipleFiles(cssContents.ToArray());
			
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓");
			Console.ResetColor();

			if (verbose)
			{
				var styleCount = typeof(BootstrapComponentStyles).GetProperties()
					.Count(p => p.GetValue(componentStyles) != null);
				Console.WriteLine($"  Component styles extracted: {styleCount}");
			}

			// Map computed styles to tokens
			Console.Write("Extracting tokens from component styles... ");
			var mapper = new BootstrapMapper();
			tokens = mapper.MapComponentStylesToTokens(componentStyles, options);
			
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓");
			Console.ResetColor();
		}
		else
		{
			tokens = new FlagstoneTokens();
		}

		// Optionally supplement with variable-based analysis
		if (useVariableAnalysis)
		{
			Console.Write($"Parsing Bootstrap {(useCssAnalysis ? "variables (supplemental)" : "variables")}... ");
			var parser = new BootstrapParser();
			
			BootstrapVariables variables;
			if (inputs.Length == 1)
			{
				var input = inputs[0];
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
			}
			else
			{
				variables = await parser.ParseMultipleFilesAsync(inputs, format);
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

			// Map variables to tokens (merge with CSS-based tokens if hybrid mode)
			Console.Write($"Mapping variables to tokens{(useCssAnalysis ? " (merging)" : "")}... ");
			var mapper = new BootstrapMapper();
			var variableTokens = mapper.MapToFlagstoneTokens(variables, options);
			
			// Merge tokens (CSS-based tokens take precedence)
			if (useCssAnalysis)
			{
				MergeTokens(tokens, variableTokens);
			}
			else
			{
				tokens = variableTokens;
			}
			
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓");
			Console.ResetColor();
		}

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
		
		// Extract theme name from first input file or use default
		var firstInput = inputs[0];
		var themeName = Path.GetFileNameWithoutExtension(firstInput);
		if (string.IsNullOrWhiteSpace(themeName) || Uri.TryCreate(firstInput, UriKind.Absolute, out _))
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

	/// <summary>
	/// Merge tokens from variable analysis into CSS-based tokens
	/// CSS-based tokens take precedence (only add missing tokens from variables)
	/// </summary>
	private static void MergeTokens(FlagstoneTokens target, FlagstoneTokens source)
	{
		// Merge colors (don't override CSS-extracted colors)
		foreach (var (key, value) in source.Colors)
		{
			if (!target.Colors.ContainsKey(key))
			{
				target.Colors[key] = value;
			}
		}

		// Merge typography
		foreach (var (key, value) in source.Typography)
		{
			if (!target.Typography.ContainsKey(key))
			{
				target.Typography[key] = value;
			}
		}

		// Merge spacing
		foreach (var (key, value) in source.Spacing)
		{
			if (!target.Spacing.ContainsKey(key))
			{
				target.Spacing[key] = value;
			}
		}

		// Merge border radius
		foreach (var (key, value) in source.BorderRadius)
		{
			if (!target.BorderRadius.ContainsKey(key))
			{
				target.BorderRadius[key] = value;
			}
		}

		// Merge border width
		foreach (var (key, value) in source.BorderWidth)
		{
			if (!target.BorderWidth.ContainsKey(key))
			{
				target.BorderWidth[key] = value;
			}
		}
	}
}
