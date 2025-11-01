using System.Text.Json;

namespace FlagstoneUI.TokenGenerator;

public class ThemeValidator
{
	public ValidationResult ValidateJson(string jsonPath)
	{
		var result = new ValidationResult();

		try
		{
			var json = File.ReadAllText(jsonPath);
			var catalog = JsonSerializer.Deserialize<JsonElement>(json);

			// Support both "tokens" and "baseTokens" as root properties
			if (!catalog.TryGetProperty("tokens", out var tokens) && 
			    !catalog.TryGetProperty("baseTokens", out tokens))
			{
				result.AddError("missing_tokens", "Root 'tokens' or 'baseTokens' property is required");
				return result;
			}

			// Validate color tokens
			if (tokens.TryGetProperty("colors", out var colors))
			{
				ValidateColorTokens(colors, result);
			}

			// Validate numeric tokens
			ValidateNumericTokens(tokens, result);

			// Check for circular dependencies
			ValidateTokenReferences(tokens, result);

			result.IsValid = result.Errors.Count == 0;
		}
		catch (IOException ex)
		{
			result.AddError("parse_error", $"Failed to read file: {ex.Message}");
		}
		catch (UnauthorizedAccessException ex)
		{
			result.AddError("parse_error", $"Access denied: {ex.Message}");
		}
		catch (JsonException ex)
		{
			result.AddError("parse_error", $"Failed to parse JSON: {ex.Message}");
		}

		return result;
	}

	public ValidationResult ValidateXaml(string xamlPath)
	{
		var result = new ValidationResult();

		try
		{
			if (!File.Exists(xamlPath))
			{
				result.AddError("file_not_found", $"XAML file not found: {xamlPath}");
				return result;
			}

			var doc = System.Xml.Linq.XDocument.Load(xamlPath);
			var root = doc.Root;

			if (root == null)
			{
				result.AddError("invalid_xaml", "XAML document has no root element");
				return result;
			}

			var xNs = root.GetNamespaceOfPrefix("x");
			var mauiNs = root.Name.Namespace;

			// Validate color tokens
			var colorElements = doc.Descendants(mauiNs + "Color").ToList();
			foreach (var element in colorElements)
			{
				var key = element.Attribute(xNs + "Key")?.Value;
				var value = element.Value;

				if (string.IsNullOrWhiteSpace(key))
				{
					result.AddWarning("missing_key", "Color element missing x:Key attribute");
					continue;
				}

				if (string.IsNullOrWhiteSpace(value))
				{
					result.AddError(key, $"Color token '{key}' has no value");
				}
				else if (!IsValidColorValue(value))
				{
					result.AddError(key, $"Color token '{key}' has invalid color value: {value}");
				}
			}

			// Validate numeric tokens
			var doubleElements = doc.Descendants(xNs + "Double").ToList();
			foreach (var element in doubleElements)
			{
				var key = element.Attribute(xNs + "Key")?.Value;
				var value = element.Value;

				if (string.IsNullOrWhiteSpace(key))
				{
					result.AddWarning("missing_key", "Double element missing x:Key attribute");
					continue;
				}

				if (string.IsNullOrWhiteSpace(value) || !double.TryParse(value, out _))
				{
					result.AddError(key, $"Numeric token '{key}' has invalid value: {value}");
				}
			}

			result.IsValid = result.Errors.Count == 0;
		}
		catch (Exception ex)
		{
			result.AddError("parse_error", $"Failed to parse XAML: {ex.Message}");
		}

		return result;
	}

	private void ValidateColorTokens(JsonElement colors, ValidationResult result)
	{
		foreach (var colorProp in colors.EnumerateObject())
		{
			var key = colorProp.Name;
			var token = colorProp.Value;

			if (!token.TryGetProperty("defaultValue", out var defaultValue))
			{
				result.AddError(key, $"Color token '{key}' missing defaultValue");
			}
			else if (!IsValidColorValue(defaultValue.GetString() ?? ""))
			{
				result.AddError(key, $"Color token '{key}' has invalid defaultValue");
			}

			// Check for dark mode value
			if (!token.TryGetProperty("darkValue", out _))
			{
				result.AddWarning(key, $"Color token '{key}' missing darkValue (recommended for theme support)");
			}
			else if (token.TryGetProperty("darkValue", out var darkValue))
			{
				var darkStr = darkValue.GetString() ?? "";
				if (!string.IsNullOrEmpty(darkStr) && !IsValidColorValue(darkStr))
				{
					result.AddError(key, $"Color token '{key}' has invalid darkValue");
				}
			}
		}
	}

	private void ValidateNumericTokens(JsonElement tokens, ValidationResult result)
	{
		var numericCategories = new[] { "spacing", "typography", "borderRadius", "borderWidth", "elevation", "padding", "opacity" };

		foreach (var category in numericCategories)
		{
			if (!tokens.TryGetProperty(category, out var categoryTokens))
			{
				continue;
			}

			foreach (var tokenProp in categoryTokens.EnumerateObject())
			{
				var key = tokenProp.Name;
				var token = tokenProp.Value;

				if (!token.TryGetProperty("value", out var value))
				{
					result.AddError(key, $"Numeric token '{key}' missing value");
				}
				else if (value.ValueKind != JsonValueKind.Number)
				{
					result.AddError(key, $"Numeric token '{key}' has non-numeric value");
				}

				// Validate specific ranges
				if (category == "opacity" && token.TryGetProperty("value", out var opacityVal))
				{
					var val = opacityVal.GetDouble();
					if (val < 0 || val > 1)
					{
						result.AddError(key, $"Opacity token '{key}' must be between 0 and 1 (got {val})");
					}
				}
			}
		}
	}

	private void ValidateTokenReferences(JsonElement tokens, ValidationResult result)
	{
		// TODO: Implement circular dependency detection
		// This would parse token values looking for {StaticResource ...} or {DynamicResource ...}
		// and build a dependency graph to detect cycles
	}

	private bool IsValidColorValue(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		// Check for hex colors (#RGB, #RRGGBB, #AARRGGBB)
		if (value.StartsWith('#'))
		{
			var hex = value[1..];
			return hex.Length == 3 || hex.Length == 6 || hex.Length == 8;
		}

		// Check for named colors (basic validation - just ensure it's not empty)
		// In a real implementation, you'd validate against known color names
		return true;
	}
}

public class ValidationResult
{
	public bool IsValid { get; set; }
	public List<ValidationError> Errors { get; } = new();
	public List<ValidationWarning> Warnings { get; } = new();

	public void AddError(string token, string message)
	{
		Errors.Add(new ValidationError { Token = token, Message = message });
		IsValid = false;
	}

	public void AddWarning(string token, string message)
	{
		Warnings.Add(new ValidationWarning { Token = token, Message = message });
	}

	public string ToJsonString()
	{
		return JsonSerializer.Serialize(new
		{
			valid = IsValid,
			errors = Errors.Select(e => new { token = e.Token, message = e.Message }),
			warnings = Warnings.Select(w => new { token = w.Token, message = w.Message })
		}, new JsonSerializerOptions { WriteIndented = true });
	}
}

public class ValidationError
{
	public string Token { get; set; } = "";
	public string Message { get; set; } = "";
}

public class ValidationWarning
{
	public string Token { get; set; } = "";
	public string Message { get; set; } = "";
}
