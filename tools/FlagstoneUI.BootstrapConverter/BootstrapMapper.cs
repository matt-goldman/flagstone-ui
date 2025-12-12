using FlagstoneUI.BootstrapConverter.Models;
using System.Globalization;

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
