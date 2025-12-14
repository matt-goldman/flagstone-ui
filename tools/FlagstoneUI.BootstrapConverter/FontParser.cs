using FlagstoneUI.BootstrapConverter.Models;
using System.Text.RegularExpressions;

namespace FlagstoneUI.BootstrapConverter;

/// <summary>
/// Parses font information from Bootstrap CSS/SCSS files
/// </summary>
public partial class FontParser
{
	/// <summary>
	/// Extract font information from Bootstrap content
	/// </summary>
	public FontInformation ParseFonts(string[] inputs, string[] contents)
	{
		var fontInfo = new FontInformation();

		foreach (var content in contents)
		{
			// Parse @import statements for Google Fonts
			ParseGoogleFontImports(content, fontInfo);

			// Parse @font-face rules for local fonts
			ParseFontFaceRules(content, fontInfo);

			// Parse font-family variables
			ParseFontFamilyVariables(content, fontInfo);
		}

		// Deduplicate families by name
		fontInfo.Families = fontInfo.Families
			.GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
			.Select(g => g.First())
			.ToList();

		return fontInfo;
	}

	private void ParseGoogleFontImports(string content, FontInformation fontInfo)
	{
		// Match: @import url('https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap');
		var matches = GoogleFontImportRegex().Matches(content);

		foreach (Match match in matches)
		{
			var url = match.Groups[1].Value;
			fontInfo.DownloadUrls.Add(url);

			// Extract font family name and weights from URL
			var fontMatch = GoogleFontUrlRegex().Match(url);
			if (fontMatch.Success)
			{
				var familyName = fontMatch.Groups[1].Value.Replace("+", " ", StringComparison.Ordinal);
				var weightsStr = fontMatch.Groups[2].Value;

				var family = new FontFamily
				{
					Name = familyName,
					Source = FontSource.GoogleFonts
				};

				// Parse weights (e.g., "400;700" or "400")
				if (!string.IsNullOrWhiteSpace(weightsStr))
				{
					var weightParts = weightsStr.Split(';', ',');
					var validWeights = weightParts
						.Where(w => int.TryParse(w, out _))
						.Select(w => int.Parse(w));
					
					foreach (var weightValue in validWeights)
					{
						family.Weights.Add(weightValue);
					}
				}

				// Check for italic
				if (url.Contains("ital", StringComparison.OrdinalIgnoreCase))
				{
					family.HasItalic = true;
				}

				fontInfo.Families.Add(family);
			}
		}
	}

	private void ParseFontFaceRules(string content, FontInformation fontInfo)
	{
		// Match: @font-face { font-family: 'FontName'; src: url('...'); }
		var matches = FontFaceRegex().Matches(content);

		foreach (Match match in matches)
		{
			var fontFaceContent = match.Groups[1].Value;

			// Extract font-family name
			var familyMatch = FontFamilyInFontFaceRegex().Match(fontFaceContent);
			if (!familyMatch.Success) continue;

			var familyName = familyMatch.Groups[1].Value.Trim('\'', '"');

			// Extract src URL
			var srcMatch = FontSrcRegex().Match(fontFaceContent);
			var source = FontSource.LocalFile;

			if (srcMatch.Success)
			{
				var srcUrl = srcMatch.Groups[1].Value;
				
				// Check if it's a Google Fonts or external URL
				if (srcUrl.Contains("fonts.googleapis.com", StringComparison.OrdinalIgnoreCase))
				{
					source = FontSource.GoogleFonts;
					if (!fontInfo.DownloadUrls.Contains(srcUrl))
					{
						fontInfo.DownloadUrls.Add(srcUrl);
					}
				}
			}

			// Extract font-weight
			var weightMatch = FontWeightRegex().Match(fontFaceContent);
			var weight = 400; // Default to normal
			if (weightMatch.Success && int.TryParse(weightMatch.Groups[1].Value, out var parsedWeight))
			{
				weight = parsedWeight;
			}

			// Check for italic
			var hasItalic = fontFaceContent.Contains("italic", StringComparison.OrdinalIgnoreCase);

			// Add or update font family
			var existingFamily = fontInfo.Families.FirstOrDefault(f => 
				f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));

			if (existingFamily != null)
			{
				if (!existingFamily.Weights.Contains(weight))
				{
					existingFamily.Weights.Add(weight);
				}
				existingFamily.HasItalic |= hasItalic;
			}
			else
			{
				fontInfo.Families.Add(new FontFamily
				{
					Name = familyName,
					Source = source,
					Weights = [weight],
					HasItalic = hasItalic
				});
			}
		}
	}

	private void ParseFontFamilyVariables(string content, FontInformation fontInfo)
	{
		// Match: $font-family-base: "Roboto", "Helvetica", sans-serif;
		//    or: --bs-font-family-base: "Roboto", "Helvetica", sans-serif;
		var scssMatches = FontFamilyVariableRegex().Matches(content);
		var cssMatches = CssFontFamilyVariableRegex().Matches(content);

		var allMatches = scssMatches.Cast<Match>().Concat(cssMatches.Cast<Match>());

		foreach (var match in allMatches)
		{
			var fontStack = match.Groups[1].Value;

			// Split by comma and get the first explicitly named font
			var fonts = fontStack.Split(',');
			foreach (var font in fonts)
			{
				var trimmedFont = font.Trim().Trim('\'', '"');

				// Skip generic font families
				if (IsGenericFontFamily(trimmedFont))
					continue;

				// Skip system UI aliases (we'll map these to "System")
				if (IsSystemUiAlias(trimmedFont))
				{
					EnsureSystemFontFamily(fontInfo);
					break; // Use first font only
				}

				// This is a named font - add it
				var existingFamily = fontInfo.Families.FirstOrDefault(f => 
					f.Name.Equals(trimmedFont, StringComparison.OrdinalIgnoreCase));

				if (existingFamily == null)
				{
					fontInfo.Families.Add(new FontFamily
					{
						Name = trimmedFont,
						Source = FontSource.Unknown // We don't know the source from variable alone
					});
				}

				break; // Only use the first explicitly named font
			}
		}
	}

	private static bool IsGenericFontFamily(string name)
	{
		var genericFamilies = new[] { "sans-serif", "serif", "monospace", "cursive", "fantasy", "system-ui" };
		return genericFamilies.Contains(name.ToLowerInvariant());
	}

	private static bool IsSystemUiAlias(string name)
	{
		var systemAliases = new[] { "-apple-system", "BlinkMacSystemFont", "system-ui" };
		return systemAliases.Contains(name, StringComparer.OrdinalIgnoreCase);
	}

	private static void EnsureSystemFontFamily(FontInformation fontInfo)
	{
		if (!fontInfo.Families.Any(f => f.Name == "System"))
		{
			fontInfo.Families.Add(new FontFamily
			{
				Name = "System",
				Source = FontSource.System
			});
		}
	}

	// Regex patterns
	[GeneratedRegex(@"@import\s+url\(['""]([^'""]+fonts\.googleapis\.com[^'""]+)['""]\)", RegexOptions.IgnoreCase)]
	private static partial Regex GoogleFontImportRegex();

	[GeneratedRegex(@"family=([^:&]+)(?::wght@([0-9;,]+))?", RegexOptions.IgnoreCase)]
	private static partial Regex GoogleFontUrlRegex();

	[GeneratedRegex(@"@font-face\s*\{([^}]+)\}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
	private static partial Regex FontFaceRegex();

	[GeneratedRegex(@"font-family:\s*['""]([^'""]+)['""]", RegexOptions.IgnoreCase)]
	private static partial Regex FontFamilyInFontFaceRegex();

	[GeneratedRegex(@"src:\s*url\(['""]([^'""]+)['""]", RegexOptions.IgnoreCase)]
	private static partial Regex FontSrcRegex();

	[GeneratedRegex(@"font-weight:\s*(\d+)", RegexOptions.IgnoreCase)]
	private static partial Regex FontWeightRegex();

	[GeneratedRegex(@"\$font-family[^:]*:\s*([^;]+);", RegexOptions.IgnoreCase)]
	private static partial Regex FontFamilyVariableRegex();

	[GeneratedRegex(@"--bs-font-family[^:]*:\s*([^;]+);", RegexOptions.IgnoreCase)]
	private static partial Regex CssFontFamilyVariableRegex();
}
