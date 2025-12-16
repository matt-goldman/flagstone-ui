using ExCSS;
using FlagstoneUI.BootstrapConverter.Models;
using System.Text.RegularExpressions;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Analyzes Bootstrap CSS to extract computed styles for components
/// Uses top-down approach: looks for CSS classes, not variables
/// </summary>
public class BootstrapCssAnalyzer
{
	/// <summary>
	/// Regex pattern for matching CSS theme blocks with light mode properties.
	/// Matches :root or [data-bs-theme=light] selectors and captures the content within curly braces.
	/// Pattern structure: (?:^|\n)\s*(?::root|(?:\:root,)?\[data-bs-theme\s*=\s*['""]?light['""]?\])[^{]*\{([^}]*)\}
	/// - (?:^|\n)\s*: Match start of line or newline followed by optional whitespace
	/// - (?::root|(?:\:root,)?\[data-bs-theme\s*=\s*['""]?light['""]?\]): Match :root or [data-bs-theme=light] selector
	/// - [^{]*\{: Match any characters up to opening brace
	/// - ([^}]*): Capture group 1 - the declarations block content
	/// - \}: Match closing brace
	/// </summary>
	private static readonly Regex ThemeLightPattern = new(
		@"(?:^|\n)\s*(?::root|(?:\:root,)?\[data-bs-theme\s*=\s*['""]?light['""]?\])[^{]*\{([^}]*)\}",
		RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	/// Regex pattern for matching CSS theme blocks with dark mode properties.
	/// Matches [data-bs-theme=dark] selectors and captures the content within curly braces.
	/// Pattern structure: (?:^|\n)\s*\[data-bs-theme\s*=\s*['""]?dark['""]?\][^{]*\{([^}]*)\}
	/// - (?:^|\n)\s*: Match start of line or newline followed by optional whitespace
	/// - \[data-bs-theme\s*=\s*['""]?dark['""]?\]: Match [data-bs-theme=dark] selector
	/// - [^{]*\{: Match any characters up to opening brace
	/// - ([^}]*): Capture group 1 - the declarations block content
	/// - \}: Match closing brace
	/// </summary>
	private static readonly Regex ThemeDarkPattern = new(
		@"(?:^|\n)\s*\[data-bs-theme\s*=\s*['""]?dark['""]?\][^{]*\{([^}]*)\}",
		RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	/// Regex pattern for matching CSS custom property declarations.
	/// Matches --property-name: value; syntax and captures both the property name and value.
	/// Pattern structure: (--[a-z0-9-]+)\s*:\s*([^;]+);
	/// - (--[a-z0-9-]+): Capture group 1 - custom property name (starts with --, followed by lowercase letters, digits, or hyphens)
	/// - \s*:\s*: Match colon with optional surrounding whitespace
	/// - ([^;]+): Capture group 2 - property value (any characters except semicolon)
	/// - ;: Match trailing semicolon
	/// </summary>
	private static readonly Regex CustomPropertyPattern = new(
		@"(--[a-z0-9-]+)\s*:\s*([^;]+);",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	/// <summary>
	/// Analyze Bootstrap CSS and extract component styles
	/// </summary>
	/// <param name="cssContent">Bootstrap CSS content</param>
	/// <returns>Computed styles for Bootstrap components</returns>
	public BootstrapComponentStyles AnalyzeComponents(string cssContent)
	{
		ConverterLogger.Info("Analyzing Bootstrap CSS for component styles...");

		var parser = new StylesheetParser();
		var stylesheet = parser.Parse(cssContent);

		var styles = new BootstrapComponentStyles
		{
			// Base button
			ButtonBase = ExtractStyle(stylesheet, ".btn"),

			// Button variants (solid)
			ButtonPrimary = ExtractStyle(stylesheet, ".btn-primary"),
			ButtonSecondary = ExtractStyle(stylesheet, ".btn-secondary"),
			ButtonSuccess = ExtractStyle(stylesheet, ".btn-success"),
			ButtonDanger = ExtractStyle(stylesheet, ".btn-danger"),
			ButtonWarning = ExtractStyle(stylesheet, ".btn-warning"),
			ButtonInfo = ExtractStyle(stylesheet, ".btn-info"),
			ButtonLight = ExtractStyle(stylesheet, ".btn-light"),
			ButtonDark = ExtractStyle(stylesheet, ".btn-dark"),

			// Outline variants
			ButtonOutlinePrimary = ExtractStyle(stylesheet, ".btn-outline-primary"),
			ButtonOutlineSecondary = ExtractStyle(stylesheet, ".btn-outline-secondary"),
			ButtonOutlineSuccess = ExtractStyle(stylesheet, ".btn-outline-success"),
			ButtonOutlineDanger = ExtractStyle(stylesheet, ".btn-outline-danger"),
			ButtonOutlineWarning = ExtractStyle(stylesheet, ".btn-outline-warning"),
			ButtonOutlineInfo = ExtractStyle(stylesheet, ".btn-outline-info"),
			ButtonOutlineLight = ExtractStyle(stylesheet, ".btn-outline-light"),
			ButtonOutlineDark = ExtractStyle(stylesheet, ".btn-outline-dark"),

			// Button sizes
			ButtonLarge = ExtractStyle(stylesheet, ".btn-lg"),
			ButtonSmall = ExtractStyle(stylesheet, ".btn-sm"),

			// Form controls
			FormControl = ExtractStyle(stylesheet, ".form-control"),
			FormControlFocus = ExtractStyle(stylesheet, ".form-control:focus"),
			FormControlPlaceholder = ExtractStyle(stylesheet, ".form-control::placeholder"),

			// Validation states
			FormControlValid = ExtractStyle(stylesheet, ".form-control.is-valid"),
			FormControlInvalid = ExtractStyle(stylesheet, ".form-control.is-invalid"),
			FormControlValidFocus = ExtractStyle(stylesheet, ".form-control.is-valid:focus"),
			FormControlInvalidFocus = ExtractStyle(stylesheet, ".form-control.is-invalid:focus"),

			// Cards
			Card = ExtractStyle(stylesheet, ".card"),
			CardBody = ExtractStyle(stylesheet, ".card-body"),
			CardHeader = ExtractStyle(stylesheet, ".card-header"),
			CardFooter = ExtractStyle(stylesheet, ".card-footer")
		};

		ConverterLogger.Debug($"Extracted {CountNonNullStyles(styles)} component styles");

		return styles;
	}

	/// <summary>
	/// Extract computed style for a specific CSS selector
	/// Handles CSS cascade, specificity, and inheritance
	/// </summary>
	/// <param name="stylesheet">Parsed stylesheet</param>
	/// <param name="selector">CSS selector to match (e.g., ".btn-primary")</param>
	/// <returns>Computed style with aggregated properties</returns>
	private ComputedStyle? ExtractStyle(Stylesheet stylesheet, string selector)
	{
		ConverterLogger.Debug($"Extracting style for selector: {selector}");

		// Find all rules that match or contain this selector
		var matchingRules = stylesheet.StyleRules
			.Where(rule => RuleMatchesSelector(rule, selector))
			.ToList();

		if (matchingRules.Count == 0)
		{
			ConverterLogger.Debug($"  No rules found for {selector}");
			return null;
		}

		ConverterLogger.Debug($"  Found {matchingRules.Count} matching rule(s)");

		// Aggregate properties from all matching rules
		// Later rules override earlier ones (CSS cascade)
		var computedStyle = new ComputedStyle
		{
			Selector = selector,
			Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		};

		foreach (var rule in matchingRules)
		{
			if (rule.Style == null)
			{
				ConverterLogger.Debug($"    Rule has null Style property");
				continue;
			}
			
			ConverterLogger.Debug($"    Rule has {rule.Style.Length} declarations");
			
			foreach (var declaration in rule.Style)
			{
				var propertyName = declaration.Name;
				var propertyValue = declaration.Value ?? string.Empty;

				// Override previous value (cascade)
				computedStyle.Properties[propertyName] = propertyValue;

				ConverterLogger.Debug($"    {propertyName}: {propertyValue}");
			}
		}

		return computedStyle;
	}

	/// <summary>
	/// Check if a CSS rule matches the target selector
	/// Supports exact matches and compound selectors
	/// </summary>
	private bool RuleMatchesSelector(IStyleRule rule, string targetSelector)
	{
		if (rule.Selector == null)
			return false;

		var selectorText = rule.Selector.Text;

		// Exact match
		if (selectorText.Equals(targetSelector, StringComparison.OrdinalIgnoreCase))
			return true;

		// Compound selector match (e.g., ".btn.btn-primary" matches ".btn-primary")
		// Split by comma for grouped selectors
		var selectors = selectorText.Split(',', StringSplitOptions.TrimEntries);

		// Filter selectors that contain the target selector
		return selectors.Any(sel => sel.Contains(targetSelector, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Merge multiple CSS files and analyze them as one stylesheet
	/// Useful for Bootswatch themes with multiple partials
	/// </summary>
	/// <param name="cssContents">Array of CSS file contents</param>
	/// <returns>Computed styles from merged stylesheets</returns>
	public BootstrapComponentStyles AnalyzeMultipleFiles(string[] cssContents)
	{
		ConverterLogger.Info($"Analyzing {cssContents.Length} CSS file(s)...");

		// Merge all CSS content
		var mergedCss = string.Join("\n\n", cssContents);

		return AnalyzeComponents(mergedCss);
	}

	/// <summary>
	/// Count non-null styles in component collection
	/// </summary>
	private static int CountNonNullStyles(BootstrapComponentStyles styles)
	{
		var properties = typeof(BootstrapComponentStyles).GetProperties();
		return properties.Count(p => p.GetValue(styles) != null);
	}

	/// <summary>
	/// Extract all color values from a computed style
	/// Useful for token generation
	/// </summary>
	public Dictionary<string, string> ExtractColors(ComputedStyle style)
	{
		var colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Common color properties
		string[] colorProperties = [
			"color",
			"background-color",
			"border-color",
			"border-top-color",
			"border-right-color",
			"border-bottom-color",
			"border-left-color",
			"outline-color"
		];

		foreach (var prop in colorProperties)
		{
			var value = style.GetProperty(prop);
			if (!string.IsNullOrWhiteSpace(value))
			{
				colors[prop] = value;
			}
		}

		return colors;
	}

	/// <summary>
	/// Extract spacing values (padding, margin) from a computed style
	/// </summary>
	public Dictionary<string, string> ExtractSpacing(ComputedStyle style)
	{
		var spacing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Spacing properties
		string[] spacingProperties = [
			"padding",
			"padding-top",
			"padding-right",
			"padding-bottom",
			"padding-left",
			"margin",
			"margin-top",
			"margin-right",
			"margin-bottom",
			"margin-left"
		];

		foreach (var prop in spacingProperties)
		{
			var value = style.GetProperty(prop);
			if (!string.IsNullOrWhiteSpace(value))
			{
				spacing[prop] = value;
			}
		}

		return spacing;
	}

	/// <summary>
	/// Extract border properties from a computed style
	/// </summary>
	public Dictionary<string, string> ExtractBorders(ComputedStyle style)
	{
		var borders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Border properties
		string[] borderProperties = [
			"border",
			"border-width",
			"border-style",
			"border-color",
			"border-radius",
			"border-top-left-radius",
			"border-top-right-radius",
			"border-bottom-right-radius",
			"border-bottom-left-radius",
			// Per-edge border properties
			"border-top",
			"border-right",
			"border-bottom",
			"border-left",
			"border-top-width",
			"border-right-width",
			"border-bottom-width",
			"border-left-width",
			"border-top-color",
			"border-right-color",
			"border-bottom-color",
			"border-left-color",
			"border-top-style",
			"border-right-style",
			"border-bottom-style",
			"border-left-style"
		];

		foreach (var prop in borderProperties)
		{
			var value = style.GetProperty(prop);
			if (!string.IsNullOrWhiteSpace(value))
			{
				borders[prop] = value;
			}
		}

		return borders;
	}

	/// <summary>
	/// Extract shadow properties from a computed style
	/// </summary>
	public Dictionary<string, string> ExtractShadows(ComputedStyle style)
	{
		var shadows = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Shadow properties
		string[] shadowProperties = [
			"box-shadow",
			"--bs-btn-box-shadow",
			"--bs-box-shadow",
			"text-shadow"
		];

		foreach (var prop in shadowProperties)
		{
			var value = style.GetProperty(prop);
			if (!string.IsNullOrWhiteSpace(value))
			{
				shadows[prop] = value;
			}
		}

		return shadows;
	}

	/// <summary>
	/// Extract typography properties from a computed style
	/// </summary>
	public Dictionary<string, string> ExtractTypography(ComputedStyle style)
	{
		var typography = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Typography properties
		string[] typographyProperties = [
			"font-family",
			"font-size",
			"font-weight",
			"font-style",
			"line-height",
			"letter-spacing",
			"text-align",
			"text-decoration",
			"text-transform"
		];

		foreach (var prop in typographyProperties)
		{
			var value = style.GetProperty(prop);
			if (!string.IsNullOrWhiteSpace(value))
			{
				typography[prop] = value;
			}
		}

		return typography;
	}

	/// <summary>
	/// Extract CSS custom properties from :root and theme-specific blocks
	/// </summary>
	/// <param name="cssContent">Bootstrap CSS content</param>
	/// <returns>Dictionary with 'light' and 'dark' theme custom properties</returns>
	/// <remarks>
	/// NOTE: ExCSS 4.2.3 does not parse CSS custom properties (--*), so we use manual regex parsing
	/// </remarks>
	public Dictionary<string, Dictionary<string, string>> ExtractThemeCustomProperties(string cssContent)
	{
		ConverterLogger.Info("Extracting theme-specific CSS custom properties...");

		var result = new Dictionary<string, Dictionary<string, string>>
		{
			["light"] = [],
			["dark"] = []
		};

		// Manual parsing since ExCSS doesn't support CSS custom properties
		// Extract light mode properties
		foreach (Match match in ThemeLightPattern.Matches(cssContent))
		{
			var declarations = match.Groups[1].Value;
			ParseCustomProperties(declarations, result["light"]);
		}

		// Extract dark mode properties
		foreach (Match match in ThemeDarkPattern.Matches(cssContent))
		{
			var declarations = match.Groups[1].Value;
			ParseCustomProperties(declarations, result["dark"]);
		}

		ConverterLogger.Info($"Extracted {result["light"].Count} light mode properties, {result["dark"].Count} dark mode properties");
		return result;
	}

	/// <summary>
	/// Parse CSS custom properties from a declarations block
	/// </summary>
	private static void ParseCustomProperties(string declarationsBlock, Dictionary<string, string> target)
	{
		foreach (Match match in CustomPropertyPattern.Matches(declarationsBlock))
		{
			var propertyName = match.Groups[1].Value.Trim();
			var propertyValue = match.Groups[2].Value.Trim();
			
			target[propertyName] = propertyValue;
			ConverterLogger.Debug($"    {propertyName}: {propertyValue}");
		}
	}
}
