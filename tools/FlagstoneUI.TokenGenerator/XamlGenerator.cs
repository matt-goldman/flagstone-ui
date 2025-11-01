using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace FlagstoneUI.TokenGenerator;

public class XamlGenerator
{
	public string GenerateXaml(string jsonPath)
	{
		var json = File.ReadAllText(jsonPath);
		var catalog = JsonSerializer.Deserialize<JsonElement>(json);

		// Support both "tokens" and "baseTokens" as root properties
		if (!catalog.TryGetProperty("tokens", out var tokens) && 
		    !catalog.TryGetProperty("baseTokens", out tokens))
		{
			throw new InvalidOperationException("Invalid catalog: missing 'tokens' or 'baseTokens' property");
		}

		var doc = CreateXamlDocument();
		var root = doc.Root!;

		// Add color tokens
		if (tokens.TryGetProperty("colors", out var colors))
		{
			AddColorTokens(root, colors);
		}

		// Add numeric tokens
		AddNumericTokens(root, tokens, "spacing");
		AddNumericTokens(root, tokens, "typography");
		AddNumericTokens(root, tokens, "borderRadius");
		AddNumericTokens(root, tokens, "borderWidth");
		AddNumericTokens(root, tokens, "elevation");
		AddNumericTokens(root, tokens, "padding");
		AddNumericTokens(root, tokens, "opacity");

		return FormatXamlDocument(doc);
	}

	public void GenerateXamlFile(string jsonPath, string outputPath)
	{
		var xaml = GenerateXaml(jsonPath);
		File.WriteAllText(outputPath, xaml);
	}

	private XDocument CreateXamlDocument()
	{
		var mauiNs = XNamespace.Get("http://schemas.microsoft.com/dotnet/2021/maui");
		var xNs = XNamespace.Get("http://schemas.microsoft.com/winfx/2009/xaml");

		var doc = new XDocument(
			new XDeclaration("1.0", "utf-8", null),
			new XElement(mauiNs + "ResourceDictionary",
				new XAttribute(XNamespace.Xmlns + "x", xNs.NamespaceName)
			)
		);

		return doc;
	}

	private void AddColorTokens(XElement root, JsonElement colors)
	{
		var mauiNs = root.Name.Namespace;
		var xNs = root.GetNamespaceOfPrefix("x");

		// Add comment header
		root.Add(new XComment(" ===== Color Tokens ===== "));

		foreach (var colorProp in colors.EnumerateObject())
		{
			var key = colorProp.Name;
			var token = colorProp.Value;

			if (token.TryGetProperty("defaultValue", out var defaultValue))
			{
				// Add purpose as comment if available
				if (token.TryGetProperty("purpose", out var purpose))
				{
					var purposeText = purpose.GetString() ?? "";
					if (!string.IsNullOrWhiteSpace(purposeText) && !purposeText.Contains("Design token for", StringComparison.Ordinal))
					{
						root.Add(new XComment($" {key}: {purposeText} "));
					}
				}

				var colorElement = new XElement(mauiNs + "Color",
					new XAttribute(xNs + "Key", key),
					defaultValue.GetString() ?? ""
				);

				root.Add(colorElement);

				// Add dark mode value as comment for reference
				if (token.TryGetProperty("darkValue", out var darkValue))
				{
					var darkStr = darkValue.GetString() ?? "";
					if (!string.IsNullOrWhiteSpace(darkStr))
					{
						root.Add(new XComment($" Dark mode: {darkStr} "));
					}
				}
			}
		}
	}

	private void AddNumericTokens(XElement root, JsonElement tokens, string category)
	{
		if (!tokens.TryGetProperty(category, out var categoryTokens))
		{
			return;
		}

		var mauiNs = root.Name.Namespace;
		var xNs = root.GetNamespaceOfPrefix("x");

		// Add category header
		var categoryTitle = char.ToUpper(category[0]) + category.Substring(1);
		root.Add(new XComment($" ===== {categoryTitle} Tokens ===== "));

		foreach (var tokenProp in categoryTokens.EnumerateObject())
		{
			var key = tokenProp.Name;
			var token = tokenProp.Value;

			if (token.TryGetProperty("value", out var value))
			{
				// Add purpose as comment if available
				if (token.TryGetProperty("purpose", out var purpose))
				{
					var purposeText = purpose.GetString() ?? "";
					if (!string.IsNullOrWhiteSpace(purposeText) && !purposeText.Contains("Design token for", StringComparison.Ordinal))
					{
						root.Add(new XComment($" {key}: {purposeText} "));
					}
				}

				var doubleElement = new XElement(xNs + "Double",
					new XAttribute(xNs + "Key", key),
					value.GetDouble().ToString()
				);

				root.Add(doubleElement);
			}
		}
	}

	private string FormatXamlDocument(XDocument doc)
	{
		var settings = new System.Xml.XmlWriterSettings
		{
			Indent = true,
			IndentChars = "    ",
			OmitXmlDeclaration = false,
			Encoding = System.Text.Encoding.UTF8
		};

		using var stringWriter = new System.IO.StringWriter();
		using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, settings))
		{
			doc.Save(xmlWriter);
		}

		return stringWriter.ToString();
	}
}
