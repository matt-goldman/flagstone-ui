using System.Text.Json;
using System.Xml.Linq;

namespace FlagstoneUI.TokenGenerator;

/// <summary>
/// Validates themes against design system contracts.
/// Supports contract inheritance (extends) and multiple contract layers.
/// </summary>
public class ContractValidator
{
	private readonly Dictionary<string, JsonElement> _loadedContracts = new();
	private readonly string _contractsDirectory;

	public ContractValidator(string? contractsDirectory = null)
	{
		_contractsDirectory = contractsDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "docs", "contracts");
	}

	/// <summary>
	/// Validates a theme XAML file against a design system contract.
	/// </summary>
	public async Task<ContractValidationResult> ValidateThemeAsync(string themePath, string contractPath)
	{
		var result = new ContractValidationResult();

		try
		{
			// Load and resolve the contract (including inheritance)
			var contract = await LoadContractWithInheritanceAsync(contractPath);
			result.ContractName = contract.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "unknown" : "unknown";
			result.ContractLayer = contract.TryGetProperty("layer", out var layerEl) ? layerEl.GetString() ?? "unknown" : "unknown";

			// Load the theme XAML
			if (!File.Exists(themePath))
			{
				result.AddError("file_not_found", $"Theme file not found: {themePath}");
				return result;
			}

			var themeDoc = XDocument.Load(themePath);
			var root = themeDoc.Root;
			if (root == null)
			{
				result.AddError("invalid_xaml", "Theme XAML has no root element");
				return result;
			}

			// Validate required styles
			if (contract.TryGetProperty("requiredStyles", out var requiredStyles))
			{
				ValidateRequiredStyles(root, requiredStyles, result);
			}

			// Validate token definitions against schema
			if (contract.TryGetProperty("tokenSchema", out var tokenSchema))
			{
				ValidateTokenSchema(root, tokenSchema, result);
			}

			// Validate styling surface coverage (if minimal/theme layer)
			if (contract.TryGetProperty("stylingSurface", out var stylingSurface))
			{
				ValidateStylingSurface(root, stylingSurface, result);
			}

			result.IsValid = result.Errors.Count == 0;
		}
		catch (JsonException ex)
		{
			result.AddError("contract_parse_error", $"Failed to parse contract: {ex.Message}");
		}
		catch (System.Xml.XmlException ex)
		{
			result.AddError("theme_parse_error", $"Failed to parse theme XAML: {ex.Message}");
		}
		catch (IOException ex)
		{
			result.AddError("io_error", $"File operation failed: {ex.Message}");
		}

		return result;
	}

	/// <summary>
	/// Loads a contract and resolves any inheritance chain.
	/// </summary>
	private async Task<JsonElement> LoadContractWithInheritanceAsync(string contractPath)
	{
		var contract = await LoadContractAsync(contractPath);

		// Check for inheritance
		if (contract.TryGetProperty("extends", out var extendsEl))
		{
			var parentName = extendsEl.GetString();
			if (!string.IsNullOrEmpty(parentName))
			{
				var parentPath = ResolveContractPath(parentName);
				var parentContract = await LoadContractWithInheritanceAsync(parentPath);

				// Merge parent into current (current overrides parent)
				contract = MergeContracts(parentContract, contract);
			}
		}

		return contract;
	}

	private async Task<JsonElement> LoadContractAsync(string path)
	{
		if (_loadedContracts.TryGetValue(path, out var cached))
		{
			return cached;
		}

		var json = await File.ReadAllTextAsync(path);
		var doc = JsonDocument.Parse(json);
		_loadedContracts[path] = doc.RootElement.Clone();
		return _loadedContracts[path];
	}

	private string ResolveContractPath(string contractName)
	{
		// Try multiple resolution strategies:
		// 1. Built-in contracts in docs/contracts/
		// 2. Relative to current contract
		// 3. Absolute path

		var builtInPath = Path.Combine(_contractsDirectory, $"{contractName}.json");
		if (File.Exists(builtInPath))
		{
			return builtInPath;
		}

		var generatedPath = Path.Combine(_contractsDirectory, $"{contractName}-generated.json");
		if (File.Exists(generatedPath))
		{
			return generatedPath;
		}

		// Could extend this to support NuGet packages, URLs, etc.
		throw new FileNotFoundException($"Contract not found: {contractName}");
	}

	private JsonElement MergeContracts(JsonElement parent, JsonElement child)
	{
		// For now, simple merge strategy: child properties override parent
		// More sophisticated merging could be implemented (deep merge for arrays, etc.)
		using var stream = new MemoryStream();
		using var writer = new Utf8JsonWriter(stream);

		writer.WriteStartObject();

		// Copy parent properties
		foreach (var prop in parent.EnumerateObject().Where(p => p.Name != "extends"))
		{
			prop.WriteTo(writer);
		}

		// Override/add child properties
		foreach (var prop in child.EnumerateObject())
		{
			// For requiredStyles, merge instead of replace
			if (prop.Name == "requiredStyles" && parent.TryGetProperty("requiredStyles", out var parentStyles))
			{
				writer.WritePropertyName("requiredStyles");
				MergeRequiredStyles(writer, parentStyles, prop.Value);
			}
			else if (prop.Name != "extends")
			{
				prop.WriteTo(writer);
			}
		}

		writer.WriteEndObject();
		writer.Flush();

		stream.Position = 0;
		return JsonDocument.Parse(stream).RootElement.Clone();
	}

	private static void MergeRequiredStyles(Utf8JsonWriter writer, JsonElement parent, JsonElement child)
	{
		writer.WriteStartObject();

		var processedControls = new HashSet<string>(StringComparer.Ordinal);

		// Add/merge child styles
		foreach (var control in child.EnumerateObject())
		{
			processedControls.Add(control.Name);

			if (parent.TryGetProperty(control.Name, out var parentControl))
			{
				// Merge: combine named styles
				writer.WritePropertyName(control.Name);
				writer.WriteStartObject();

				// Write implicit requirement (child takes precedence)
				if (control.Value.TryGetProperty("implicit", out var implicitVal))
				{
					writer.WriteBoolean("implicit", implicitVal.GetBoolean());
				}
				else if (parentControl.TryGetProperty("implicit", out var parentImplicit))
				{
					writer.WriteBoolean("implicit", parentImplicit.GetBoolean());
				}

				// Merge named styles
				var namedStyles = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

				if (parentControl.TryGetProperty("named", out var parentNamed))
				{
					foreach (var style in parentNamed.EnumerateArray())
					{
						var styleName = style.GetProperty("name").GetString() ?? "";
						namedStyles[styleName] = style;
					}
				}

				if (control.Value.TryGetProperty("named", out var childNamed))
				{
					foreach (var style in childNamed.EnumerateArray())
					{
						var styleName = style.GetProperty("name").GetString() ?? "";
						namedStyles[styleName] = style; // Override with child
					}
				}

				if (namedStyles.Count > 0)
				{
					writer.WritePropertyName("named");
					writer.WriteStartArray();
					foreach (var style in namedStyles.Values)
					{
						style.WriteTo(writer);
					}
					writer.WriteEndArray();
				}

				writer.WriteEndObject();
			}
			else
			{
				control.WriteTo(writer);
			}
		}

		// Add parent-only controls
		foreach (var control in parent.EnumerateObject().Where(c => !processedControls.Contains(c.Name)))
		{
			control.WriteTo(writer);
		}

		writer.WriteEndObject();
	}

	private void ValidateRequiredStyles(XElement root, JsonElement requiredStyles, ContractValidationResult result)
	{
		var mauiNs = root.Name.Namespace;
		var xNs = root.GetNamespaceOfPrefix("x");
		var coreNs = root.GetNamespaceOfPrefix("core") ?? root.GetNamespaceOfPrefix("fs");

		// Get all Style elements
		var styles = root.Descendants(mauiNs + "Style").ToList();

		foreach (var controlReq in requiredStyles.EnumerateObject())
		{
			var controlName = controlReq.Name;
			var requirements = controlReq.Value;

			// Check implicit style requirement
			if (requirements.TryGetProperty("implicit", out var implicitReq) && implicitReq.GetBoolean())
			{
				var hasImplicit = styles.Any(s =>
				{
					var targetType = s.Attribute("TargetType")?.Value ?? "";
					var key = s.Attribute(xNs + "Key")?.Value;
					return IsTargetTypeMatch(targetType, controlName, coreNs) && key == null;
				});

				if (!hasImplicit)
				{
					result.AddError($"missing_implicit_style",
						$"Contract requires implicit style for {controlName} but none found");
				}
				else
				{
					result.ImplicitStylesFound.Add(controlName);
				}
			}

			// Check named style requirements
			if (requirements.TryGetProperty("named", out var namedReq))
			{
				foreach (var styleReq in namedReq.EnumerateArray())
				{
					var styleName = styleReq.GetProperty("name").GetString() ?? "";

					var hasNamed = styles.Any(s =>
					{
						var targetType = s.Attribute("TargetType")?.Value ?? "";
						var key = s.Attribute(xNs + "Key")?.Value;
						return IsTargetTypeMatch(targetType, controlName, coreNs) && key == styleName;
					});

					if (!hasNamed)
					{
						result.AddError($"missing_named_style",
							$"Contract requires named style '{styleName}' for {controlName} but none found");
					}
					else
					{
						result.NamedStylesFound.Add($"{controlName}.{styleName}");
					}
				}
			}
		}
	}

	private static bool IsTargetTypeMatch(string targetType, string controlName, XNamespace? coreNs)
	{
		// Handle various TargetType formats:
		// - "core:FsButton"
		// - "fs:FsButton"
		// - "FsButton"
		// - "FlagstoneUI.Core.Controls.FsButton"

		if (string.IsNullOrEmpty(targetType))
			return false;

		// Check for namespace prefix format
		if (targetType.Contains(':', StringComparison.Ordinal))
		{
			var parts = targetType.Split(':');
			return parts.Length == 2 && parts[1] == controlName;
		}

		// Check for full namespace format
		if (targetType.Contains('.', StringComparison.Ordinal))
		{
			return targetType.EndsWith($".{controlName}", StringComparison.Ordinal);
		}

		// Simple name match
		return targetType == controlName;
	}

	private void ValidateTokenSchema(XElement root, JsonElement tokenSchema, ContractValidationResult result)
	{
		var xNs = root.GetNamespaceOfPrefix("x");

		// Check for required token categories
		if (tokenSchema.TryGetProperty("requiredCategories", out var categories))
		{
			foreach (var category in categories.EnumerateArray().Select(c => c.GetString() ?? "").Where(c => !string.IsNullOrEmpty(c)))
			{
				var prefix = GetCategoryPrefix(category);

				// Look for any tokens with this prefix
				var hasTokens = root.Descendants()
					.Any(el => el.Attribute(xNs + "Key")?.Value?.StartsWith(prefix, StringComparison.Ordinal) == true);

				if (!hasTokens)
				{
					result.AddWarning($"missing_category",
						$"Contract recommends tokens in category '{category}' (prefix: {prefix}) but none found");
				}
			}
		}

		// Validate specific token definitions
		if (tokenSchema.TryGetProperty("definitions", out var definitions))
		{
			foreach (var tokenDef in definitions.EnumerateObject())
			{
				var tokenName = tokenDef.Name;
				var tokenReq = tokenDef.Value;
				var isRequired = tokenReq.TryGetProperty("required", out var reqProp) && reqProp.GetBoolean();

				var tokenElement = root.Descendants()
					.FirstOrDefault(el => el.Attribute(xNs + "Key")?.Value == tokenName);

				if (tokenElement == null && isRequired)
				{
					result.AddError($"missing_token", $"Required token '{tokenName}' not found");
				}
				else if (tokenElement != null)
				{
					result.TokensFound.Add(tokenName);
				}
			}
		}
	}

	private static string GetCategoryPrefix(string category)
	{
		return category switch
		{
			"colors" => "Color.",
			"spacing" => "Space.",
			"typography" => "FontSize.",
			"borderRadius" => "Radius.",
			"borderWidth" => "BorderWidth.",
			"elevation" => "Elevation.",
			"padding" => "Padding.",
			"opacity" => "Opacity.",
			_ => category + "."
		};
	}

	private void ValidateStylingSurface(XElement root, JsonElement stylingSurface, ContractValidationResult result)
	{
		// This validates that the theme provides implicit styles that cover
		// the essential properties for each control in the styling surface.
		// It's more of a coverage check than strict validation.

		if (!stylingSurface.TryGetProperty("controls", out var controls))
		{
			return;
		}

		var mauiNs = root.Name.Namespace;
		var xNs = root.GetNamespaceOfPrefix("x");
		var coreNs = root.GetNamespaceOfPrefix("core") ?? root.GetNamespaceOfPrefix("fs");

		var styles = root.Descendants(mauiNs + "Style").ToList();

		foreach (var control in controls.EnumerateObject())
		{
			var controlName = control.Name;
			var controlDef = control.Value;

			// Find the implicit style for this control
			var implicitStyle = styles.FirstOrDefault(s =>
			{
				var targetType = s.Attribute("TargetType")?.Value ?? "";
				var key = s.Attribute(xNs + "Key")?.Value;
				return IsTargetTypeMatch(targetType, controlName, coreNs) && key == null;
			});

			if (implicitStyle == null)
			{
				// Already reported in ValidateRequiredStyles if it's required
				continue;
			}

			// Check coverage of essential properties
			if (controlDef.TryGetProperty("styledProperties", out var properties))
			{
				var setters = implicitStyle.Descendants(mauiNs + "Setter").ToList();
				var coveredProps = setters
					.Select(s => s.Attribute("Property")?.Value ?? "")
					.Where(p => !string.IsNullOrEmpty(p))
					.ToHashSet(StringComparer.Ordinal);

				foreach (var prop in properties.EnumerateArray())
				{
					var propName = prop.GetProperty("name").GetString() ?? "";
					var isEssential = prop.TryGetProperty("essential", out var essentialProp) && essentialProp.GetBoolean();

					if (isEssential && !coveredProps.Contains(propName))
					{
						result.AddWarning($"uncovered_property",
							$"Essential property '{propName}' on {controlName} is not set in implicit style");
					}
				}
			}
		}
	}
}

/// <summary>
/// Result of validating a theme against a contract.
/// </summary>
public class ContractValidationResult
{
	public bool IsValid { get; set; } = true;
	public string ContractName { get; set; } = "";
	public string ContractLayer { get; set; } = "";
	public List<ValidationError> Errors { get; } = [];
	public List<ValidationWarning> Warnings { get; } = [];
	public HashSet<string> ImplicitStylesFound { get; } = new(StringComparer.Ordinal);
	public HashSet<string> NamedStylesFound { get; } = new(StringComparer.Ordinal);
	public HashSet<string> TokensFound { get; } = new(StringComparer.Ordinal);

	public void AddError(string code, string message)
	{
		Errors.Add(new ValidationError { Token = code, Message = message });
		IsValid = false;
	}

	public void AddWarning(string code, string message)
	{
		Warnings.Add(new ValidationWarning { Token = code, Message = message });
	}

	public string ToJsonString()
	{
		return JsonSerializer.Serialize(new
		{
			valid = IsValid,
			contract = new { name = ContractName, layer = ContractLayer },
			coverage = new
			{
				implicitStyles = ImplicitStylesFound.Count,
				namedStyles = NamedStylesFound.Count,
				tokens = TokensFound.Count
			},
			errors = Errors.Select(e => new { code = e.Token, message = e.Message }),
			warnings = Warnings.Select(w => new { code = w.Token, message = w.Message })
		}, new JsonSerializerOptions { WriteIndented = true });
	}

	public string ToSummaryString()
	{
		var sb = new System.Text.StringBuilder();

		sb.AppendLine($"Contract: {ContractName} ({ContractLayer} layer)");
		sb.AppendLine($"Status: {(IsValid ? "✅ Valid" : "❌ Invalid")}");
		sb.AppendLine();
		sb.AppendLine("Coverage:");
		sb.AppendLine($"  • Implicit styles: {ImplicitStylesFound.Count}");
		sb.AppendLine($"  • Named styles: {NamedStylesFound.Count}");
		sb.AppendLine($"  • Tokens: {TokensFound.Count}");

		return sb.ToString();
	}
}
