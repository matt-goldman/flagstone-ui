using System.Globalization;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Maps Bootstrap variables to Flagstone UI tokens
/// </summary>
public class BootstrapMapper
{
	/// <summary>
	/// Map Bootstrap variables to Flagstone tokens
	/// </summary>
	/// <param name="variables">Parsed Bootstrap variables</param>
	/// <param name="options">Conversion options</param>
	/// <returns>Flagstone tokens</returns>
	public FlagstoneTokens MapToFlagstoneTokens(BootstrapVariables variables, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();

		var tokens = new FlagstoneTokens();

		// Map colors
		MapColors(variables.Colors, tokens, options);

		// Map typography
		MapTypography(variables.Typography, tokens);

		// Map spacing
		MapSpacing(variables.Spacing, tokens);

		// Map borders
		MapBorders(variables.Borders, tokens);

		// Map shadows from Other variables
		MapShadowVariables(variables.Other, tokens);

		return tokens;
	}

	private void MapColors(Dictionary<string, string> bootstrapColors, FlagstoneTokens tokens, ConversionOptions options)
	{
		// Primary semantic colors
		MapColor(bootstrapColors, tokens, "primary", "Color.Primary", "Primary brand color", options);
		MapColor(bootstrapColors, tokens, "secondary", "Color.Secondary", "Secondary brand color", options);
		MapColor(bootstrapColors, tokens, "success", "Color.Success", "Success state color", options);
		MapColor(bootstrapColors, tokens, "danger", "Color.Error", "Error/danger state color", options);
		MapColor(bootstrapColors, tokens, "warning", "Color.Warning", "Warning state color", options);
		MapColor(bootstrapColors, tokens, "info", "Color.Info", "Info state color", options);

		// Surface colors
		MapColor(bootstrapColors, tokens, "light", "Color.Surface", "Light surface color", options);
		MapColor(bootstrapColors, tokens, "dark", "Color.SurfaceVariant.Dark", "Dark surface variant", options);

		// Convenience aliases for Bootstrap's semantic "light" / "dark" button variants
		MapColor(bootstrapColors, tokens, "light", "Color.Light", "Light semantic color (Bootstrap light)", options);
		MapColor(bootstrapColors, tokens, "dark", "Color.Dark", "Dark semantic color (Bootstrap dark)", options);

		// Background and text colors
		MapColor(bootstrapColors, tokens, "body-bg", "Color.Background", "Body background color", options);
		MapColor(bootstrapColors, tokens, "body-color", "Color.OnBackground", "Body text color", options);
		MapColor(bootstrapColors, tokens, "border-color", "Color.Outline", "Border color", options);
	}

	private void MapColor(Dictionary<string, string> bootstrapColors, FlagstoneTokens tokens,
		string bootstrapKey, string flagstoneKey, string purpose, ConversionOptions options)
	{
		if (!bootstrapColors.TryGetValue(bootstrapKey, out var value))
			return;

		var normalizedValue = NormalizeColorValue(value);

		tokens.Colors[flagstoneKey] = new ColorToken
		{
			Key = flagstoneKey,
			Value = normalizedValue,
			DarkValue = options.DarkModeStrategy == DarkModeStrategy.Auto
				? GenerateDarkModeColor(normalizedValue)
				: null,
			Purpose = purpose
		};

		// Auto-generate an accessible on-color for semantic colors if it's not already provided.
		// This helps avoid hard-coded fallbacks in generated styles.
		if (flagstoneKey.StartsWith("Color.", StringComparison.Ordinal) &&
			!flagstoneKey.StartsWith("Color.On", StringComparison.Ordinal))
		{
			var baseName = flagstoneKey.Substring("Color.".Length);
			var onKey = $"Color.On{baseName}";
			if (!tokens.Colors.ContainsKey(onKey))
			{
				var onValue = TryGenerateOnColor(normalizedValue);
				if (!string.IsNullOrWhiteSpace(onValue))
				{
					tokens.Colors[onKey] = new ColorToken
					{
						Key = onKey,
						Value = onValue,
						Purpose = $"Auto-generated on-color for {flagstoneKey}"
					};
				}
			}
		}
	}

	private static string? TryGenerateOnColor(string color)
	{
		// Only handle #RRGGBB for now.
		if (!color.StartsWith('#') || color.Length != 7)
			return null;

		try
		{
			var r = int.Parse(color.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			var g = int.Parse(color.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			var b = int.Parse(color.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			// Relative luminance approximation (0..255)
			var brightness = (r * 299 + g * 587 + b * 114) / 1000;
			return brightness >= 128 ? "#000000" : "#FFFFFF";
		}
		catch
		{
			return null;
		}
	}

	private void MapTypography(Dictionary<string, string> bootstrapTypography, FlagstoneTokens tokens)
	{
		// Font families
		if (bootstrapTypography.TryGetValue("font-family-base", out var fontFamily))
		{
			tokens.Typography["FontFamily.Default"] = new TypographyToken
			{
				Key = "FontFamily.Default",
				Value = ConvertFontFamily(fontFamily),
				Purpose = "Default font family"
			};
		}
		// Fallback to headings-font-family if font-family-base not found
		else if (bootstrapTypography.TryGetValue("headings-font-family", out var headingsFont))
		{
			tokens.Typography["FontFamily.Default"] = new TypographyToken
			{
				Key = "FontFamily.Default",
				Value = ConvertFontFamily(headingsFont),
				Purpose = "Default font family (from headings)"
			};
		}

		if (bootstrapTypography.TryGetValue("font-family-monospace", out var monoFont))
		{
			tokens.Typography["FontFamily.Monospace"] = new TypographyToken
			{
				Key = "FontFamily.Monospace",
				Value = ConvertFontFamily(monoFont),
				Purpose = "Monospace font family"
			};
		}

		// Font sizes
		if (bootstrapTypography.TryGetValue("font-size-base", out var fontSize))
		{
			var sizeInPx = ConvertToPixels(fontSize, 16.0); // 1rem = 16px default
			tokens.Typography["FontSize.Body"] = new TypographyToken
			{
				Key = "FontSize.Body",
				Value = sizeInPx.ToString(CultureInfo.InvariantCulture),
				Unit = "px",
				Purpose = "Base body font size"
			};
		}

		// Line height
		if (bootstrapTypography.TryGetValue("line-height-base", out var lineHeight))
		{
			tokens.Typography["LineHeight.Default"] = new TypographyToken
			{
				Key = "LineHeight.Default",
				Value = lineHeight,
				Purpose = "Default line height"
			};
		}
	}

	private void MapSpacing(Dictionary<string, string> bootstrapSpacing, FlagstoneTokens tokens)
	{
		// Base spacer
		if (bootstrapSpacing.TryGetValue("spacer", out var spacer))
		{
			var baseValue = ConvertToPixels(spacer, 16.0);

			// Generate spacing scale based on Bootstrap's pattern
			tokens.Spacing["Spacing.ExtraSmall"] = CreateNumericToken("Spacing.ExtraSmall", baseValue * 0.25, "Extra small spacing");
			tokens.Spacing["Spacing.Small"] = CreateNumericToken("Spacing.Small", baseValue * 0.5, "Small spacing");
			tokens.Spacing["Spacing.Medium"] = CreateNumericToken("Spacing.Medium", baseValue, "Medium spacing (base)");
			tokens.Spacing["Spacing.Large"] = CreateNumericToken("Spacing.Large", baseValue * 1.5, "Large spacing");
			tokens.Spacing["Spacing.ExtraLarge"] = CreateNumericToken("Spacing.ExtraLarge", baseValue * 3, "Extra large spacing");
		}
	}

	private void MapBorders(Dictionary<string, string> bootstrapBorders, FlagstoneTokens tokens)
	{
		// Border radius - prefer button-specific values if available
		if (bootstrapBorders.TryGetValue("btn-border-radius", out var btnRadius))
		{
			var radiusValue = ConvertToPixels(btnRadius, 16.0);
			tokens.BorderRadius["Radius.Medium"] = CreateNumericToken("Radius.Medium", radiusValue, "Medium corner radius (from button)");
		}
		else if (bootstrapBorders.TryGetValue("border-radius", out var radius))
		{
			var radiusValue = ConvertToPixels(radius, 16.0);
			tokens.BorderRadius["Radius.Medium"] = CreateNumericToken("Radius.Medium", radiusValue, "Medium corner radius");
		}

		if (bootstrapBorders.TryGetValue("btn-border-radius-sm", out var btnRadiusSm))
		{
			var radiusValue = ConvertToPixels(btnRadiusSm, 16.0);
			tokens.BorderRadius["Radius.Small"] = CreateNumericToken("Radius.Small", radiusValue, "Small corner radius (from button)");
		}
		else if (bootstrapBorders.TryGetValue("border-radius-sm", out var radiusSm))
		{
			var radiusValue = ConvertToPixels(radiusSm, 16.0);
			tokens.BorderRadius["Radius.Small"] = CreateNumericToken("Radius.Small", radiusValue, "Small corner radius");
		}

		if (bootstrapBorders.TryGetValue("btn-border-radius-lg", out var btnRadiusLg))
		{
			var radiusValue = ConvertToPixels(btnRadiusLg, 16.0);
			tokens.BorderRadius["Radius.Large"] = CreateNumericToken("Radius.Large", radiusValue, "Large corner radius (from button)");
		}
		else if (bootstrapBorders.TryGetValue("border-radius-lg", out var radiusLg))
		{
			var radiusValue = ConvertToPixels(radiusLg, 16.0);
			tokens.BorderRadius["Radius.Large"] = CreateNumericToken("Radius.Large", radiusValue, "Large corner radius");
		}

		// Extract any other border radius values found in the theme
		foreach (var (key, value) in bootstrapBorders)
		{
			if (key.Contains("border-radius", StringComparison.OrdinalIgnoreCase) &&
				!tokens.BorderRadius.Values.Any(t => Math.Abs(t.Value - ConvertToPixels(value, 16.0)) < 0.1))
			{
				var radiusValue = ConvertToPixels(value, 16.0);
				var tokenKey = GenerateRadiusTokenKey(key);
				var purpose = $"Corner radius from {key}";
				tokens.BorderRadius[tokenKey] = CreateNumericToken(tokenKey, radiusValue, purpose);
			}
		}

		// Border width
		if (bootstrapBorders.TryGetValue("border-width", out var width))
		{
			var widthValue = ConvertToPixels(width, 16.0);
			tokens.BorderWidth["BorderWidth.Default"] = CreateNumericToken("BorderWidth.Default", widthValue, "Default border width");
		}

		// Per-edge border widths
		MapPerEdgeBorders(bootstrapBorders, tokens);
	}

	/// <summary>
	/// Map shadow variables from Bootstrap Other variables to Flagstone shadow tokens
	/// </summary>
	private void MapShadowVariables(Dictionary<string, string> otherVariables, FlagstoneTokens tokens)
	{
		ConverterLogger.Debug($"Mapping shadow variables from {otherVariables.Count} other variables...");

		// Look for shadow-related variables
		foreach (var (key, value) in otherVariables)
		{
			// Check for box-shadow or shadow-related variables
			if (key.Contains("box-shadow", StringComparison.OrdinalIgnoreCase) ||
				key.Contains("-shadow", StringComparison.OrdinalIgnoreCase))
			{
				ConverterLogger.Debug($"  Found shadow variable: {key} = {value}");
				var shadow = ParseBoxShadow(value);
				if (shadow != null)
				{
					// Generate a meaningful key from the variable name
					var tokenKey = GenerateShadowTokenKey(key);
					shadow.Key = tokenKey;
					shadow.Purpose = $"Shadow from Bootstrap variable {key}";
					tokens.Shadows[tokenKey] = shadow;
					ConverterLogger.Debug($"  Mapped to {tokenKey}");
				}
				else
				{
					ConverterLogger.Debug($"  Failed to parse shadow value: {value}");
				}
			}
		}

		ConverterLogger.Debug($"Total shadow tokens mapped: {tokens.Shadows.Count}");
	}

	/// <summary>
	/// Generate a shadow token key from a Bootstrap variable name
	/// </summary>
	private static string GenerateShadowTokenKey(string variableName)
	{
		// Convert "btn-box-shadow" -> "Shadow.Button"
		// Convert "box-shadow" -> "Shadow.Default"
		// Convert "box-shadow-sm" -> "Shadow.Small"
		// Convert "box-shadow-lg" -> "Shadow.Large"

		var normalized = variableName
			.Replace("btn-box-shadow", "Button", StringComparison.OrdinalIgnoreCase)
			.Replace("box-shadow-lg", "Large", StringComparison.OrdinalIgnoreCase)
			.Replace("box-shadow-sm", "Small", StringComparison.OrdinalIgnoreCase)
			.Replace("box-shadow", "Default", StringComparison.OrdinalIgnoreCase)
			.Replace("-shadow", "", StringComparison.OrdinalIgnoreCase)
			.Replace("_", "", StringComparison.Ordinal)
			.Replace("-", ".", StringComparison.Ordinal);

		// Clean up and ensure proper casing
		var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
		var capitalizedParts = parts
			.Where(p => !string.IsNullOrEmpty(p))
			.Select(p => char.ToUpper(p[0], CultureInfo.InvariantCulture) + p.Substring(1).ToLower(CultureInfo.InvariantCulture));
		var result = string.Join(".", capitalizedParts);

		return result.StartsWith("Shadow.", StringComparison.Ordinal) ? result : $"Shadow.{result}";
	}

	/// <summary>
	/// Map per-edge border properties from Bootstrap to Flagstone tokens
	/// </summary>
	private void MapPerEdgeBorders(Dictionary<string, string> bootstrapBorders, FlagstoneTokens tokens, string? componentName = null)
	{
		var suffix = string.IsNullOrEmpty(componentName) ? "Default" : componentName;

		// Check for multi-value border-width (e.g., "2px 0 0 0" for top-only border)
		if (bootstrapBorders.TryGetValue("border-width", out var borderWidth) && borderWidth.Contains(' ', StringComparison.Ordinal))
		{
			var parts = borderWidth.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 4)
			{
				// CSS order: top right bottom left
				var topWidthValue = ConvertToPixels(parts[0], 16.0);
				var rightWidthValue = ConvertToPixels(parts[1], 16.0);
				var bottomWidthValue = ConvertToPixels(parts[2], 16.0);
				var leftWidthValue = ConvertToPixels(parts[3], 16.0);

				if (topWidthValue > 0)
					tokens.BorderTopWidth[$"BorderTopWidth.{suffix}"] = CreateNumericToken($"BorderTopWidth.{suffix}", topWidthValue, $"Top border width for {suffix}");
				if (rightWidthValue > 0)
					tokens.BorderRightWidth[$"BorderRightWidth.{suffix}"] = CreateNumericToken($"BorderRightWidth.{suffix}", rightWidthValue, $"Right border width for {suffix}");
				if (bottomWidthValue > 0)
					tokens.BorderBottomWidth[$"BorderBottomWidth.{suffix}"] = CreateNumericToken($"BorderBottomWidth.{suffix}", bottomWidthValue, $"Bottom border width for {suffix}");
				if (leftWidthValue > 0)
					tokens.BorderLeftWidth[$"BorderLeftWidth.{suffix}"] = CreateNumericToken($"BorderLeftWidth.{suffix}", leftWidthValue, $"Left border width for {suffix}");
			}
		}

		// Individual edge properties
		if (bootstrapBorders.TryGetValue("border-top-width", out var topWidth))
		{
			var value = ConvertToPixels(topWidth, 16.0);
			tokens.BorderTopWidth[$"BorderTopWidth.{suffix}"] = CreateNumericToken($"BorderTopWidth.{suffix}", value, $"Top border width for {suffix}");
		}

		if (bootstrapBorders.TryGetValue("border-right-width", out var rightWidth))
		{
			var value = ConvertToPixels(rightWidth, 16.0);
			tokens.BorderRightWidth[$"BorderRightWidth.{suffix}"] = CreateNumericToken($"BorderRightWidth.{suffix}", value, $"Right border width for {suffix}");
		}

		if (bootstrapBorders.TryGetValue("border-bottom-width", out var bottomWidth))
		{
			var value = ConvertToPixels(bottomWidth, 16.0);
			tokens.BorderBottomWidth[$"BorderBottomWidth.{suffix}"] = CreateNumericToken($"BorderBottomWidth.{suffix}", value, $"Bottom border width for {suffix}");
		}

		if (bootstrapBorders.TryGetValue("border-left-width", out var leftWidth))
		{
			var value = ConvertToPixels(leftWidth, 16.0);
			tokens.BorderLeftWidth[$"BorderLeftWidth.{suffix}"] = CreateNumericToken($"BorderLeftWidth.{suffix}", value, $"Left border width for {suffix}");
		}
	}

	/// <summary>
	/// Parse box-shadow value and create shadow tokens
	/// </summary>
	public void MapShadows(Dictionary<string, string> shadowProps, FlagstoneTokens tokens, string componentName)
	{
		// Try Bootstrap custom properties first, then fall back to box-shadow
		var boxShadow = shadowProps.GetValueOrDefault("--bs-btn-box-shadow")
			?? shadowProps.GetValueOrDefault("--bs-box-shadow")
			?? shadowProps.GetValueOrDefault("box-shadow");

		if (string.IsNullOrWhiteSpace(boxShadow))
			return;

		var shadow = ParseBoxShadow(boxShadow);
		if (shadow != null)
		{
			shadow.Key = $"Shadow.{componentName}";
			shadow.Purpose = $"{componentName} shadow from Bootstrap box-shadow";
			tokens.Shadows[shadow.Key] = shadow;
		}
	}

	/// <summary>
	/// Parse a CSS box-shadow value into a ShadowToken
	/// </summary>
	private ShadowToken? ParseBoxShadow(string boxShadow)
	{
		if (string.IsNullOrWhiteSpace(boxShadow) || boxShadow == "none")
			return null;

		try
		{
			// Handle multiple shadows (comma-separated)
			// For now, take the first non-inset shadow
			var shadows = boxShadow.Split(',');
			foreach (var shadow in shadows)
			{
				var trimmedShadow = shadow.Trim();
				
				// Skip inset shadows
				if (trimmedShadow.StartsWith("inset", StringComparison.OrdinalIgnoreCase))
					continue;

				var result = ParseSingleBoxShadow(trimmedShadow);
				if (result != null)
					return result;
			}

			return null;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Parse a single CSS box-shadow value (not comma-separated)
	/// </summary>
	private ShadowToken? ParseSingleBoxShadow(string boxShadow)
	{
		try
		{
			// Simple parser for common box-shadow patterns:
			// "3px 3px 0 0 #000"
			// "0 0.5rem 1rem rgba(0, 0, 0, 0.15)"

			var opacity = 1.0;
			var color = "#000"; // Default fallback color

			// First, extract any rgba/rgb color to avoid splitting issues
			var rgbaMatch = System.Text.RegularExpressions.Regex.Match(boxShadow, @"rgba?\([^)]+\)");
			if (rgbaMatch.Success)
			{
				var colorValue = rgbaMatch.Value;
				
				// Extract opacity from rgba
				var alphaMatch = System.Text.RegularExpressions.Regex.Match(colorValue, @"rgba\([^,]+,\s*[^,]+,\s*[^,]+,\s*([0-9.]+)\)");
				if (alphaMatch.Success && double.TryParse(alphaMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
				{
					opacity = alpha;
				}

				// Extract RGB components
				var rgbMatch = System.Text.RegularExpressions.Regex.Match(colorValue, @"rgba?\(\s*([0-9]+)\s*,\s*([0-9]+)\s*,\s*([0-9]+)");
				if (rgbMatch.Success)
				{
					var r = rgbMatch.Groups[1].Value;
					var g = rgbMatch.Groups[2].Value;
					var b = rgbMatch.Groups[3].Value;
					color = $"rgb({r},{g},{b})";
				}
				else
				{
					// rgba with variables or invalid format - skip this shadow
					ConverterLogger.Debug($"Skipping shadow with CSS variable or invalid rgba: {colorValue}");
					return null;
				}

				// Remove the color from the string for further parsing
				boxShadow = boxShadow.Replace(rgbaMatch.Value, "", StringComparison.Ordinal).Trim();
			}
			else if (boxShadow.Contains("#", StringComparison.Ordinal))
			{
				// Extract hex color
				var hexMatch = System.Text.RegularExpressions.Regex.Match(boxShadow, @"#[0-9a-fA-F]{3,8}");
				if (hexMatch.Success)
				{
					color = hexMatch.Value;
					boxShadow = boxShadow.Replace(hexMatch.Value, "", StringComparison.Ordinal).Trim();
				}
			}
			else if (boxShadow.Contains("rgb(", StringComparison.Ordinal))
			{
				// Handle rgb() without alpha
				var rgbMatch = System.Text.RegularExpressions.Regex.Match(boxShadow, @"rgb\(\s*([0-9]+)\s*,\s*([0-9]+)\s*,\s*([0-9]+)\s*\)");
				if (rgbMatch.Success)
				{
					color = rgbMatch.Value;
					boxShadow = boxShadow.Replace(rgbMatch.Value, "", StringComparison.Ordinal).Trim();
				}
			}

			// Now parse the numeric values
			var parts = boxShadow.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 3)
				return null;

			var offsetX = ConvertToPixels(parts[0], 16.0);
			var offsetY = ConvertToPixels(parts[1], 16.0);
			var blur = ConvertToPixels(parts[2], 16.0);

			return new ShadowToken
			{
				Key = "Shadow.Temp", // Will be set by caller
				OffsetX = offsetX,
				OffsetY = offsetY,
				Radius = blur,
				Color = NormalizeColorValue(color),
				Opacity = opacity
			};
		}
		catch
		{
			return null;
		}
	}

	private static NumericToken CreateNumericToken(string key, double value, string purpose)
	{
		return new NumericToken
		{
			Key = key,
			Value = value,
			Unit = "px",
			Purpose = purpose
		};
	}

	private static string NormalizeColorValue(string value)
	{
		// Remove whitespace and convert to uppercase for hex values
		value = value.Trim();

		// If it's a hex color, ensure it starts with #
		if (value.StartsWith('#'))
			return value.ToUpperInvariant();

		// Handle rgb/rgba
		if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
			return value;

		// Handle named colors (leave as-is for now)
		return value;
	}

	private static string? GenerateDarkModeColor(string lightColor)
	{
		// Simple dark mode generation: darken light colors, lighten dark colors
		// This is a placeholder - could use a color manipulation library for better results

		if (!lightColor.StartsWith('#') || lightColor.Length != 7)
			return null; // Only handle hex colors for now

		try
		{
			var r = int.Parse(lightColor.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber);
			var g = int.Parse(lightColor.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber);
			var b = int.Parse(lightColor.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber);

			// Calculate brightness (perceived luminance)
			var brightness = (r * 299 + g * 587 + b * 114) / 1000;

			// If bright color, darken it; if dark color, lighten it
			var factor = brightness > 128 ? 0.7 : 1.3;

			r = Math.Clamp((int)(r * factor), 0, 255);
			g = Math.Clamp((int)(g * factor), 0, 255);
			b = Math.Clamp((int)(b * factor), 0, 255);

			return $"#{r:X2}{g:X2}{b:X2}";
		}
		catch (FormatException)
		{
			return null;
		}
		catch (OverflowException)
		{
			return null;
		}
	}

	private static string ConvertFontFamily(string cssFont)
	{
		// Extract first font from CSS font stack
		// Remove quotes and get first font
		var fonts = cssFont.Split(',');
		var firstFont = fonts[0].Trim().Trim('\'', '"');

		// Map common web fonts to MAUI fonts
		return firstFont switch
		{
			"-apple-system" or "system-ui" => "System",
			"BlinkMacSystemFont" => "System",
			"Segoe UI" => "Segoe UI",
			_ => firstFont
		};
	}

	private static double ConvertToPixels(string value, double baseFontSize)
	{
		value = value.Trim().ToLowerInvariant();

		// Already in pixels
		if (value.EndsWith("px", StringComparison.Ordinal))
		{
			return double.Parse(value.Replace("px", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture);
		}

		// Convert rem to pixels
		if (value.EndsWith("rem", StringComparison.Ordinal))
		{
			var rem = double.Parse(value.Replace("rem", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture);
			return rem * baseFontSize;
		}

		// Convert em to pixels (assume base font size)
		if (value.EndsWith("em", StringComparison.Ordinal))
		{
			var em = double.Parse(value.Replace("em", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture);
			return em * baseFontSize;
		}

		// Try to parse as unitless number (assume pixels)
		if (double.TryParse(value, CultureInfo.InvariantCulture, out var number))
		{
			return number;
		}

		return 0;
	}

	private static string GenerateRadiusTokenKey(string bootstrapKey)
	{
		// Generate appropriate Flagstone token key from Bootstrap variable name
		return bootstrapKey.ToLowerInvariant() switch
		{
			var key when key.Contains("breadcrumb", StringComparison.OrdinalIgnoreCase) => "Radius.Breadcrumb",
			var key when key.Contains("card", StringComparison.OrdinalIgnoreCase) => "Radius.Card",
			var key when key.Contains("btn", StringComparison.OrdinalIgnoreCase) && key.Contains("sm", StringComparison.OrdinalIgnoreCase) => "Radius.ButtonSmall",
			var key when key.Contains("btn", StringComparison.OrdinalIgnoreCase) && key.Contains("lg", StringComparison.OrdinalIgnoreCase) => "Radius.ButtonLarge",
			var key when key.Contains("btn", StringComparison.OrdinalIgnoreCase) => "Radius.Button",
			var key when key.Contains("sm", StringComparison.OrdinalIgnoreCase) => "Radius.Small",
			var key when key.Contains("lg", StringComparison.OrdinalIgnoreCase) => "Radius.Large",
			_ => "Radius.Default"
		};
	}

	/// <summary>
	/// Map Bootstrap component styles to Flagstone tokens (top-down approach)
	/// Extracts tokens from computed CSS styles rather than variables
	/// </summary>
	/// <param name="componentStyles">Computed Bootstrap component styles</param>
	/// <param name="options">Conversion options</param>
	/// <returns>Flagstone tokens extracted from component styles</returns>
	public FlagstoneTokens MapComponentStylesToTokens(BootstrapComponentStyles componentStyles, ConversionOptions? options = null)
	{
		options ??= new ConversionOptions();
		var tokens = new FlagstoneTokens();

		ConverterLogger.Info("Mapping computed component styles to Flagstone tokens...");

		// Extract tokens from button primary (our reference component)
		if (componentStyles.ButtonPrimary != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonPrimary, tokens, "Primary", options);
		}

		// Extract from other button variants
		if (componentStyles.ButtonSecondary != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonSecondary, tokens, "Secondary", options);
		}

		if (componentStyles.ButtonSuccess != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonSuccess, tokens, "Success", options);
		}

		if (componentStyles.ButtonDanger != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonDanger, tokens, "Error", options);
		}

		if (componentStyles.ButtonWarning != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonWarning, tokens, "Warning", options);
		}

		if (componentStyles.ButtonInfo != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonInfo, tokens, "Info", options);
		}

		if (componentStyles.ButtonLight != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonLight, tokens, "Light", options);
		}

		if (componentStyles.ButtonDark != null)
		{
			ExtractTokensFromButton(componentStyles.ButtonDark, tokens, "Dark", options);
		}

		// Extract base spacing and borders from button base
		if (componentStyles.ButtonBase != null)
		{
			ExtractSpacingTokens(componentStyles.ButtonBase, tokens);
			ExtractBorderTokens(componentStyles.ButtonBase, tokens);
			ExtractTypographyTokens(componentStyles.ButtonBase, tokens);
		}

		ConverterLogger.Debug($"Extracted {tokens.Colors.Count} color tokens, {tokens.Typography.Count} typography tokens, {tokens.Spacing.Count} spacing tokens");

		return tokens;
	}

	private void ExtractTokensFromButton(ComputedStyle buttonStyle, FlagstoneTokens tokens, string colorName, ConversionOptions options)
	{
		// Bootstrap 5 uses CSS custom properties (--bs-btn-bg, --bs-btn-color, etc.)
		// Try CSS custom properties first, fall back to regular properties
		var bgColor = buttonStyle.GetProperty("--bs-btn-bg") ?? buttonStyle.GetProperty("background-color");
		var textColor = buttonStyle.GetProperty("--bs-btn-color") ?? buttonStyle.GetProperty("color");
		var borderColor = buttonStyle.GetProperty("--bs-btn-border-color") ?? buttonStyle.GetProperty("border-color");

		if (!string.IsNullOrWhiteSpace(bgColor))
		{
			tokens.Colors[$"Color.{colorName}"] = new ColorToken
			{
				Key = $"Color.{colorName}",
				Value = NormalizeColorValue(bgColor),
				DarkValue = options.DarkModeStrategy == DarkModeStrategy.Auto ? GenerateDarkModeColor(NormalizeColorValue(bgColor)) : null,
				Purpose = $"{colorName} color from Bootstrap .btn-{colorName.ToLowerInvariant()}"
			};
		}

		if (!string.IsNullOrWhiteSpace(textColor))
		{
			tokens.Colors[$"Color.On{colorName}"] = new ColorToken
			{
				Key = $"Color.On{colorName}",
				Value = NormalizeColorValue(textColor),
				Purpose = $"Text color on {colorName} background"
			};
		}
		else
		{
			// If Bootstrap doesn't provide an explicit text color, derive one from the background.
			if (!string.IsNullOrWhiteSpace(bgColor))
			{
				var derivedOn = TryGenerateOnColor(NormalizeColorValue(bgColor));
				if (!string.IsNullOrWhiteSpace(derivedOn))
				{
					tokens.Colors[$"Color.On{colorName}"] = new ColorToken
					{
						Key = $"Color.On{colorName}",
						Value = derivedOn,
						Purpose = $"Auto-generated text color on {colorName} background"
					};
				}
			}
		}

		// Extract border color if present
		if (!string.IsNullOrWhiteSpace(borderColor))
		{
			tokens.Colors[$"Color.{colorName}Border"] = new ColorToken
			{
				Key = $"Color.{colorName}Border",
				Value = NormalizeColorValue(borderColor),
				DarkValue = options.DarkModeStrategy == DarkModeStrategy.Auto ? GenerateDarkModeColor(NormalizeColorValue(borderColor)) : null,
				Purpose = $"Border color for {colorName} variant"
			};
		}

		// Extract shadows from button style
		var analyzer = new BootstrapCssAnalyzer();
		var shadowProps = analyzer.ExtractShadows(buttonStyle);
		MapShadows(shadowProps, tokens, $"Button.{colorName}");
	}

	private void ExtractSpacingTokens(ComputedStyle style, FlagstoneTokens tokens)
	{
		// Try CSS custom properties first
		var padding = style.GetProperty("--bs-btn-padding-y") ?? style.GetProperty("padding");
		if (!string.IsNullOrWhiteSpace(padding))
		{
			// Parse padding (e.g., "0.375rem 0.75rem" or "0.375rem")
			var paddingValue = ConvertToPixels(padding.Split(' ')[0], 16.0);

			tokens.Spacing["Spacing.Button"] = new NumericToken
			{
				Key = "Spacing.Button",
				Value = paddingValue,
				Unit = "px",
				Purpose = "Button padding from Bootstrap .btn"
			};
		}
	}

	private void ExtractBorderTokens(ComputedStyle style, FlagstoneTokens tokens)
	{
		var borderRadius = style.GetProperty("--bs-btn-border-radius") ?? style.GetProperty("border-radius");
		if (!string.IsNullOrWhiteSpace(borderRadius))
		{
			var radiusValue = ConvertToPixels(borderRadius, 16.0);

			tokens.BorderRadius["Radius.Button"] = new NumericToken
			{
				Key = "Radius.Button",
				Value = radiusValue,
				Unit = "px",
				Purpose = "Button border radius from Bootstrap .btn"
			};
		}

		var borderWidth = style.GetProperty("--bs-btn-border-width") ?? style.GetProperty("border-width");
		if (!string.IsNullOrWhiteSpace(borderWidth))
		{
			var widthValue = ConvertToPixels(borderWidth, 16.0);

			tokens.BorderWidth["BorderWidth.Button"] = new NumericToken
			{
				Key = "BorderWidth.Button",
				Value = widthValue,
				Unit = "px",
				Purpose = "Button border width from Bootstrap .btn"
			};
		}

		// Extract per-edge borders
		var analyzer = new BootstrapCssAnalyzer();
		var borderProps = analyzer.ExtractBorders(style);
		MapPerEdgeBorders(borderProps, tokens, "Button");
	}

	private void ExtractTypographyTokens(ComputedStyle style, FlagstoneTokens tokens)
	{
		var fontSize = style.GetProperty("--bs-btn-font-size") ?? style.GetProperty("font-size");
		if (!string.IsNullOrWhiteSpace(fontSize))
		{
			var sizeValue = ConvertToPixels(fontSize, 16.0);

			tokens.Typography["FontSize.Button"] = new TypographyToken
			{
				Key = "FontSize.Button",
				Value = sizeValue.ToString(CultureInfo.InvariantCulture),
				Unit = "px",
				Purpose = "Button font size from Bootstrap .btn"
			};
		}

		var fontWeight = style.GetProperty("--bs-btn-font-weight") ?? style.GetProperty("font-weight");
		if (!string.IsNullOrWhiteSpace(fontWeight))
		{
			tokens.Typography["FontWeight.Button"] = new TypographyToken
			{
				Key = "FontWeight.Button",
				Value = fontWeight,
				Purpose = "Button font weight from Bootstrap .btn"
			};
		}
	}
}
