using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Orchestrates the conversion of Bootstrap themes to Flagstone UI themes.
/// Encapsulates the complete conversion workflow with support for multiple analysis strategies.
/// </summary>
public class BootstrapConverterService
{
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

		// Read all inputs (we'll need content for font parsing too)
		var cssContents = await ReadInputsAsync(request.Inputs);

		// CSS analysis (top-down)
		if (useCssAnalysis)
		{
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
				if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
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

		// Parse fonts if requested
		FontInformation? fontInfo = null;
		if (options.IncludeFonts)
		{
			var fontParser = new FontParser();
			fontInfo = fontParser.ParseFonts(request.Inputs, [.. cssContents]);
		}

		// Extract and apply theme-specific custom properties (light/dark mode)
		if (cssContents.Count > 0)
		{
			ApplyThemeCustomProperties(tokens, cssContents);
		}

		// Extract theme name
		var themeName = ExtractThemeName(request.Inputs[0]);

		// Build statistics
		var statistics = new ConversionStatistics
		{
			ColorTokens = tokens.Colors.Count,
			TypographyTokens = tokens.Typography.Count,
			SpacingTokens = tokens.Spacing.Count,
			BorderRadiusTokens = tokens.BorderRadius.Count,
			BorderWidthTokens = tokens.BorderWidth.Count,
			BorderTopWidthTokens = tokens.BorderTopWidth.Count,
			BorderRightWidthTokens = tokens.BorderRightWidth.Count,
			BorderBottomWidthTokens = tokens.BorderBottomWidth.Count,
			BorderLeftWidthTokens = tokens.BorderLeftWidth.Count,
			ShadowTokens = tokens.Shadows.Count,
			ComponentStylesExtracted = componentStyles != null
				? typeof(BootstrapComponentStyles).GetProperties().Count(p => p.GetValue(componentStyles) != null)
				: 0,
			VariablesParsed = variablesParsed
		};

		return new ConversionResult
		{
			Tokens = tokens,
			ComponentStyles = componentStyles,
			ThemeName = themeName,
			Statistics = statistics,
				Fonts = fontInfo
		};
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
			if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
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

		// Merge per-edge border widths
		foreach (var (key, value) in source.BorderTopWidth)
		{
			if (!target.BorderTopWidth.ContainsKey(key))
			{
				target.BorderTopWidth[key] = value;
			}
		}

		foreach (var (key, value) in source.BorderRightWidth)
		{
			if (!target.BorderRightWidth.ContainsKey(key))
			{
				target.BorderRightWidth[key] = value;
			}
		}

		foreach (var (key, value) in source.BorderBottomWidth)
		{
			if (!target.BorderBottomWidth.ContainsKey(key))
			{
				target.BorderBottomWidth[key] = value;
			}
		}

		foreach (var (key, value) in source.BorderLeftWidth)
		{
			if (!target.BorderLeftWidth.ContainsKey(key))
			{
				target.BorderLeftWidth[key] = value;
			}
		}

		// Merge shadows
		foreach (var (key, value) in source.Shadows)
		{
			if (!target.Shadows.ContainsKey(key))
			{
				target.Shadows[key] = value;
			}
		}
	}

	/// <summary>
	/// Extract theme-specific CSS custom properties and apply to tokens (light/dark mode)
	/// </summary>
	private static void ApplyThemeCustomProperties(FlagstoneTokens tokens, List<string> cssContents)
	{
		ConverterLogger.Info("Extracting theme-specific CSS custom properties for light/dark mode...");
		
		var analyzer = new BootstrapCssAnalyzer();
		var allThemeProps = new Dictionary<string, Dictionary<string, string>>
		{
			["light"] = [],
			["dark"] = []
		};

		// Extract from all CSS files
		foreach (var css in cssContents)
		{
			var themeProps = analyzer.ExtractThemeCustomProperties(css);
			
			// Merge light mode properties
			foreach (var (key, value) in themeProps["light"])
			{
				allThemeProps["light"][key] = value;
			}
			
			// Merge dark mode properties
			foreach (var (key, value) in themeProps["dark"])
			{
				allThemeProps["dark"][key] = value;
			}
		}

		// Map CSS custom properties to color tokens
		MapCustomPropertiesToColorTokens(tokens, allThemeProps);
		
		ConverterLogger.Info($"Applied {allThemeProps["light"].Count} light mode, {allThemeProps["dark"].Count} dark mode custom properties");
	}

	/// <summary>
	/// Map Bootstrap CSS custom properties to FlagstoneUI color tokens with dark mode values
	/// </summary>
	private static void MapCustomPropertiesToColorTokens(FlagstoneTokens tokens, Dictionary<string, Dictionary<string, string>> themeProps)
	{
		var lightProps = themeProps["light"];
		var darkProps = themeProps["dark"];

		// Map common color custom properties
		var colorMappings = new Dictionary<string, string>
		{
			["--bs-primary"] = "Color.Primary",
			["--bs-secondary"] = "Color.Secondary",
			["--bs-success"] = "Color.Success",
			["--bs-danger"] = "Color.Error",
			["--bs-warning"] = "Color.Warning",
			["--bs-info"] = "Color.Info",
			["--bs-light"] = "Color.Light",
			["--bs-dark"] = "Color.Dark",
			["--bs-body-bg"] = "Color.Background",
			["--bs-body-color"] = "Color.OnBackground",
			["--bs-border-color"] = "Color.Outline"
		};

		foreach (var (cssVar, tokenKey) in colorMappings)
		{
			if (lightProps.TryGetValue(cssVar, out var lightValue))
			{
				// Update existing token or create new one
				if (tokens.Colors.TryGetValue(tokenKey, out var existingToken))
				{
					// Check if dark mode value exists
					if (darkProps.TryGetValue(cssVar, out var darkValue))
					{
						existingToken.DarkValue = darkValue;
						ConverterLogger.Debug($"Updated {tokenKey} with dark mode value: {darkValue}");
					}
				}
				else
				{
					// Create new token
					tokens.Colors[tokenKey] = new ColorToken
					{
						Key = tokenKey,
						Value = lightValue,
						DarkValue = darkProps.TryGetValue(cssVar, out var darkValue) ? darkValue : null,
						Purpose = $"Extracted from {cssVar}"
					};
					ConverterLogger.Debug($"Created {tokenKey} from {cssVar}");
				}
			}
		}
	}
}

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

	/// <summary>
	/// Font information (if IncludeFonts option was enabled)
	/// </summary>
	public FontInformation? Fonts { get; init; }
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
	public int BorderTopWidthTokens { get; init; }
	public int BorderRightWidthTokens { get; init; }
	public int BorderBottomWidthTokens { get; init; }
	public int BorderLeftWidthTokens { get; init; }
	public int ShadowTokens { get; init; }
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
