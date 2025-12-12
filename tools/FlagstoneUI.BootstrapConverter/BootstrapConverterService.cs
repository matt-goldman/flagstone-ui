using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Orchestrates the conversion of Bootstrap themes to Flagstone UI themes.
/// Encapsulates the complete conversion workflow with support for multiple analysis strategies.
/// </summary>
public class BootstrapConverterService
{
	/// <summary>
	/// Configuration for the conversion process
	/// </summary>
	public record ConversionRequest
	{
		/// <summary>
		/// Input file paths or URLs
		/// </summary>
		public required string[] Inputs { get; init; }

		/// <summary>
		/// Bootstrap format (CSS, SCSS, or Auto-detect)
		/// </summary>
		public BootstrapFormat Format { get; init; } = BootstrapFormat.Auto;

		/// <summary>
		/// Analysis strategy to use
		/// </summary>
		public AnalysisStrategy Strategy { get; init; } = AnalysisStrategy.Hybrid;

		/// <summary>
		/// Conversion options (dark mode, comments, namespace)
		/// </summary>
		public ConversionOptions? Options { get; init; }

		/// <summary>
		/// Enable debug logging
		/// </summary>
		public bool EnableDebugLogging { get; init; }
	}

	/// <summary>
	/// Result of the conversion process
	/// </summary>
	public record ConversionResult
	{
		/// <summary>
		/// Extracted/generated tokens
		/// </summary>
		public required FlagstoneTokens Tokens { get; init; }

		/// <summary>
		/// Component styles (if CSS analysis was used)
		/// </summary>
		public BootstrapComponentStyles? ComponentStyles { get; init; }

		/// <summary>
		/// Theme name extracted from input
		/// </summary>
		public required string ThemeName { get; init; }

		/// <summary>
		/// Statistics about the conversion
		/// </summary>
		public required ConversionStatistics Statistics { get; init; }
	}

	/// <summary>
	/// Statistics about the conversion process
	/// </summary>
	public record ConversionStatistics
	{
		public int ColorTokens { get; init; }
		public int TypographyTokens { get; init; }
		public int SpacingTokens { get; init; }
		public int BorderRadiusTokens { get; init; }
		public int BorderWidthTokens { get; init; }
		public int ComponentStylesExtracted { get; init; }
		public int VariablesParsed { get; init; }
	}

	/// <summary>
	/// Analysis strategy for conversion
	/// </summary>
	public enum AnalysisStrategy
	{
		/// <summary>
		/// Use only CSS class analysis (top-down)
		/// </summary>
		CssOnly,

		/// <summary>
		/// Use only variable parsing (bottom-up)
		/// </summary>
		VariablesOnly,

		/// <summary>
		/// Use both CSS and variables (recommended)
		/// </summary>
		Hybrid
	}

	/// <summary>
	/// Convert Bootstrap theme to Flagstone tokens
	/// </summary>
	/// <param name="request">Conversion request configuration</param>
	/// <returns>Conversion result with tokens and statistics</returns>
	public async Task<ConversionResult> ConvertAsync(ConversionRequest request)
	{
		// Enable debug logging if requested
		if (request.EnableDebugLogging)
		{
			ConverterLogger.IsEnabled = true;
			ConverterLogger.Info("Debug logging enabled");
		}

		var options = request.Options ?? new ConversionOptions();
		var useCssAnalysis = request.Strategy is AnalysisStrategy.CssOnly or AnalysisStrategy.Hybrid;
		var useVariableAnalysis = request.Strategy is AnalysisStrategy.VariablesOnly or AnalysisStrategy.Hybrid;

		FlagstoneTokens tokens;
		BootstrapComponentStyles? componentStyles = null;
		int variablesParsed = 0;

		// CSS analysis (top-down)
		if (useCssAnalysis)
		{
			var cssContents = await ReadInputsAsync(request.Inputs);
			
			var analyzer = new BootstrapCssAnalyzer();
			componentStyles = cssContents.Count == 1
				? analyzer.AnalyzeComponents(cssContents[0])
				: analyzer.AnalyzeMultipleFiles([.. cssContents]);

			var mapper = new BootstrapMapper();
			tokens = mapper.MapComponentStylesToTokens(componentStyles ?? new BootstrapComponentStyles(), options);
		}
		else
		{
			tokens = new FlagstoneTokens();
		}

		// Variable analysis (bottom-up)
		if (useVariableAnalysis)
		{
			var parser = new BootstrapParser();
			BootstrapVariables variables;

			if (request.Inputs.Length == 1)
			{
				var input = request.Inputs[0];
				if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
				{
					variables = await parser.ParseFromUrlAsync(uri.ToString(), request.Format);
				}
				else if (File.Exists(input))
				{
					variables = await parser.ParseFromFileAsync(input, request.Format);
				}
				else
				{
					throw new FileNotFoundException($"Input file not found: {input}");
				}
			}
			else
			{
				variables = await parser.ParseMultipleFilesAsync(request.Inputs, request.Format);
			}

			variablesParsed = variables.Colors.Count + variables.Typography.Count +
							  variables.Spacing.Count + variables.Borders.Count + variables.Other.Count;

			var mapper = new BootstrapMapper();
			var variableTokens = mapper.MapToFlagstoneTokens(variables, options);

			// Merge if hybrid mode (CSS-based tokens take precedence)
			if (useCssAnalysis)
			{
				MergeTokens(tokens, variableTokens);
			}
			else
			{
				tokens = variableTokens;
			}
		}

		// Extract theme name
		var themeName = ExtractThemeName(request.Inputs[0]);

		// Build statistics
		var statistics = new ConversionStatistics
		{
			ColorTokens					= tokens.Colors.Count,
			TypographyTokens			= tokens.Typography.Count,
			SpacingTokens				= tokens.Spacing.Count,
			BorderRadiusTokens			= tokens.BorderRadius.Count,
			BorderWidthTokens			= tokens.BorderWidth.Count,
			ComponentStylesExtracted	= componentStyles != null
				? typeof(BootstrapComponentStyles).GetProperties().Count(p => p.GetValue(componentStyles) != null)
				: 0,
			VariablesParsed = variablesParsed
		};

		return new ConversionResult
		{
			Tokens			= tokens,
			ComponentStyles = componentStyles,
			ThemeName		= themeName,
			Statistics		= statistics
		};
	}

	/// <summary>
	/// Convert and generate XAML files
	/// </summary>
	/// <param name="request">Conversion request</param>
	/// <param name="outputDirectory">Directory to write XAML files to</param>
	public async Task ConvertAndGenerateFilesAsync(ConversionRequest request, string outputDirectory)
	{
		var result = await ConvertAsync(request);

		// Ensure output directory exists
		Directory.CreateDirectory(outputDirectory);

		// Choose generator based on format
		if (request.Options?.OutputFormat == ResourceDictionaryFormat.CSharp)
		{
			var generator = new CSharpThemeGenerator();
			await generator.GenerateFilesAsync(
				result.Tokens,
				result.ThemeName,
				outputDirectory,
				request.Options
			);
		}
		else
		{
			var generator = new XamlThemeGenerator();
			await generator.GenerateFilesAsync(
				result.Tokens,
				result.ThemeName,
				outputDirectory,
				request.Options ?? new ConversionOptions(),
				result.ComponentStyles
			);
		}
	}

	/// <summary>
	/// Read input contents from files or URLs
	/// </summary>
	private static async Task<List<string>> ReadInputsAsync(string[] inputs)
	{
		var contents = new List<string>();
		
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
			contents.Add(content);
		}

		return contents;
	}

	/// <summary>
	/// Extract theme name from input file path or URL
	/// </summary>
	private static string ExtractThemeName(string input)
	{
		var themeName = Path.GetFileNameWithoutExtension(input);
		if (string.IsNullOrWhiteSpace(themeName) || Uri.TryCreate(input, UriKind.Absolute, out _))
		{
			themeName = "Bootstrap";
		}
		return themeName;
	}

	/// <summary>
	/// Merge tokens from variable analysis into CSS-based tokens.
	/// CSS-based tokens take precedence (only add missing tokens from variables).
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
