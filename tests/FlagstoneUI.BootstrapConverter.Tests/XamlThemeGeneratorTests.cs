using FlagstoneUI.BootstrapConverter.Models;
using Shouldly;
using System.Xml.Linq;
using Xunit;

namespace FlagstoneUI.BootstrapConverter.Tests;

public class XamlThemeGeneratorTests
{
    private readonly XamlThemeGenerator _generator = new();

    [Fact]
    public void GenerateTokensXaml_ShouldGenerateValidXaml()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken
                {
                    Key = "Color.Primary",
                    Value = "#0D6EFD",
                    Purpose = "Primary brand color"
                }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("<?xml version");
        result.ShouldContain("ResourceDictionary");
        result.ShouldContain("Color.Primary");
        result.ShouldContain("#0D6EFD");
    }

    [Fact]
    public void GenerateTokensXaml_ShouldBeValidXmlDocument()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken
                {
                    Key = "Color.Primary",
                    Value = "#0D6EFD"
                }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);

        // Assert - Should parse as valid XML
        var doc = XDocument.Parse(result);
        doc.Root.ShouldNotBeNull();
        doc.Root!.Name.LocalName.ShouldBe("ResourceDictionary");
    }

    [Fact]
    public void GenerateTokensXaml_ShouldIncludeColorTokens()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken { Key = "Color.Primary", Value = "#0D6EFD" },
                ["Color.Secondary"] = new ColorToken { Key = "Color.Secondary", Value = "#6C757D" }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);
        var doc = XDocument.Parse(result);

        // Assert
        var colorElements = doc.Descendants().Where(e => e.Name.LocalName == "Color").ToList();
        colorElements.Count.ShouldBe(2);
        
        var primaryColor = colorElements.First(e => e.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2009/xaml") + "Key")?.Value == "Color.Primary");
        primaryColor.Value.ShouldBe("#0D6EFD");
    }

    [Fact]
    public void GenerateTokensXaml_ShouldIncludeTypographyTokens()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Typography = new Dictionary<string, TypographyToken>
            {
                ["FontSize.Body"] = new TypographyToken
                {
                    Key = "FontSize.Body",
                    Value = "16",
                    Unit = "px"
                }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);

        // Assert
        result.ShouldContain("FontSize.Body");
        result.ShouldContain("16");
    }

    [Fact]
    public void GenerateTokensXaml_ShouldIncludeSpacingTokens()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Spacing = new Dictionary<string, NumericToken>
            {
                ["Spacing.Medium"] = new NumericToken
                {
                    Key = "Spacing.Medium",
                    Value = 16,
                    Unit = "px"
                }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);

        // Assert
        result.ShouldContain("Spacing.Medium");
        result.ShouldContain("16");
    }

    [Fact]
    public void GenerateTokensXaml_WithComments_ShouldIncludePurpose()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken
                {
                    Key = "Color.Primary",
                    Value = "#0D6EFD",
                    Purpose = "Primary brand color"
                }
            }
        };
        var options = new ConversionOptions { IncludeComments = true };

        // Act
        var result = _generator.GenerateTokensXaml(tokens, options);

        // Assert
        result.ShouldContain("Primary brand color");
    }

    [Fact]
    public void GenerateTokensXaml_WithDarkMode_ShouldIncludeDarkValueComment()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken
                {
                    Key = "Color.Primary",
                    Value = "#0D6EFD",
                    DarkValue = "#0A58CA"
                }
            }
        };
        var options = new ConversionOptions { IncludeComments = true };

        // Act
        var result = _generator.GenerateTokensXaml(tokens, options);

        // Assert
        result.ShouldContain("Dark mode:");
        result.ShouldContain("#0A58CA");
    }

    [Fact]
    public void GenerateTokensXaml_WithoutComments_ShouldNotIncludePurpose()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken
                {
                    Key = "Color.Primary",
                    Value = "#0D6EFD",
                    Purpose = "Primary brand color"
                }
            }
        };
        var options = new ConversionOptions { IncludeComments = false };

        // Act
        var result = _generator.GenerateTokensXaml(tokens, options);

        // Assert
        result.ShouldNotContain("Primary brand color");
    }

    [Fact]
    public void GenerateThemeXaml_ShouldGenerateValidXaml()
    {
        // Arrange
        var tokens = new FlagstoneTokens();
        var themeName = "Bootstrap Darkly";

        // Act
        var result = _generator.GenerateThemeXaml(tokens, themeName);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("<?xml version");
        result.ShouldContain("ResourceDictionary");
        result.ShouldContain("Bootstrap Darkly");
    }

    [Fact]
    public void GenerateThemeXaml_ShouldIncludeMergedDictionaries()
    {
        // Arrange
        var tokens = new FlagstoneTokens();
        var themeName = "Bootstrap Theme";

        // Act
        var result = _generator.GenerateThemeXaml(tokens, themeName);

        // Assert
        result.ShouldContain("ResourceDictionary.MergedDictionaries");
        result.ShouldContain("Tokens.xaml");
    }

    [Fact]
    public async Task GenerateFilesAsync_ShouldCreateBothFiles()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken { Key = "Color.Primary", Value = "#0D6EFD" }
            }
        };
        var themeName = "Test Theme";
        var outputDir = Path.Combine(Path.GetTempPath(), "flagstone-test-" + Guid.NewGuid());

        try
        {
            // Act
            await _generator.GenerateFilesAsync(tokens, themeName, outputDir);

            // Assert
            var tokensPath = Path.Combine(outputDir, "Tokens.xaml");
            var themePath = Path.Combine(outputDir, "Theme.xaml");
			var stylesPath = Path.Combine(outputDir, "Styles.xaml");

            File.Exists(tokensPath).ShouldBeTrue();
            File.Exists(themePath).ShouldBeTrue();
			File.Exists(stylesPath).ShouldBeTrue();

            var tokensContent = await File.ReadAllTextAsync(tokensPath);
            tokensContent.ShouldContain("Color.Primary");

            var themeContent = await File.ReadAllTextAsync(themePath);
            themeContent.ShouldContain("Test Theme");

			var stylesContent = await File.ReadAllTextAsync(stylesPath);
			stylesContent.ShouldContain("FsButton");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }

    [Fact]
    public void GenerateStylesXaml_ShouldIncludeAllControlSections()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken { Key = "Color.Primary", Value = "#0D6EFD" },
                ["Color.OnPrimary"] = new ColorToken { Key = "Color.OnPrimary", Value = "#FFFFFF" },
                ["Color.Secondary"] = new ColorToken { Key = "Color.Secondary", Value = "#6C757D" },
                ["Color.OnSecondary"] = new ColorToken { Key = "Color.OnSecondary", Value = "#FFFFFF" },
                ["Color.Success"] = new ColorToken { Key = "Color.Success", Value = "#198754" },
                ["Color.OnSuccess"] = new ColorToken { Key = "Color.OnSuccess", Value = "#FFFFFF" },
                ["Color.Error"] = new ColorToken { Key = "Color.Error", Value = "#DC3545" },
                ["Color.OnError"] = new ColorToken { Key = "Color.OnError", Value = "#FFFFFF" },
                ["Color.Warning"] = new ColorToken { Key = "Color.Warning", Value = "#FFC107" },
                ["Color.OnWarning"] = new ColorToken { Key = "Color.OnWarning", Value = "#000000" },
                ["Color.Info"] = new ColorToken { Key = "Color.Info", Value = "#0DCAF0" },
                ["Color.OnInfo"] = new ColorToken { Key = "Color.OnInfo", Value = "#000000" },
                ["Color.Background"] = new ColorToken { Key = "Color.Background", Value = "#FFFFFF" },
                ["Color.OnBackground"] = new ColorToken { Key = "Color.OnBackground", Value = "#212529" },
                ["Color.Outline"] = new ColorToken { Key = "Color.Outline", Value = "#DEE2E6" }
            },
            BorderRadius = new Dictionary<string, NumericToken>
            {
                ["Radius.Medium"] = new NumericToken { Key = "Radius.Medium", Value = 6 }
            },
            BorderWidth = new Dictionary<string, NumericToken>
            {
                ["BorderWidth.Default"] = new NumericToken { Key = "BorderWidth.Default", Value = 1 }
            },
            Typography = new Dictionary<string, TypographyToken>
            {
                ["FontSize.Body"] = new TypographyToken { Key = "FontSize.Body", Value = "16" }
            }
        };

        // Act
        var xaml = _generator.GenerateStylesXaml(tokens, "Test");
        var doc = XDocument.Parse(xaml);

        // Assert (sections exist)
        xaml.ShouldContain("FsButton Styles");
        xaml.ShouldContain("FsEntry Styles");
        xaml.ShouldContain("FsEditor Styles");
        xaml.ShouldContain("FsCard Styles");

        // Assert (required style keys exist)
        xaml.ShouldContain("x:Key=\"OutlinedButton\"");
        xaml.ShouldContain("x:Key=\"TextButton\"");
        xaml.ShouldContain("x:Key=\"ButtonSecondary\"");
        xaml.ShouldContain("x:Key=\"ButtonOutlinePrimary\"");
        xaml.ShouldContain("x:Key=\"ButtonSmall\"");
        xaml.ShouldContain("x:Key=\"ButtonLarge\"");
        xaml.ShouldContain("x:Key=\"EntryValid\"");
        xaml.ShouldContain("x:Key=\"EntryInvalid\"");
        xaml.ShouldContain("x:Key=\"EditorValid\"");
        xaml.ShouldContain("x:Key=\"EditorInvalid\"");

        // Assert (visual states exist)
        xaml.ShouldContain("x:Name=\"Disabled\"");
        xaml.ShouldContain("x:Name=\"Pressed\"");
        xaml.ShouldContain("x:Name=\"Focused\"");

        // Assert (default TargetType styles exist)
        var styles = doc.Descendants().Where(e => e.Name.LocalName == "Style").ToList();
        styles.Any(s => (string?)s.Attribute("TargetType") == "fs:FsEntry").ShouldBeTrue();
        styles.Any(s => (string?)s.Attribute("TargetType") == "fs:FsEditor").ShouldBeTrue();
        styles.Any(s => (string?)s.Attribute("TargetType") == "fs:FsCard").ShouldBeTrue();
    }

    [Fact]
    public void GenerateTokensXaml_ShouldIncludeAllCategories()
    {
        // Arrange
        var tokens = new FlagstoneTokens
        {
            Colors = new Dictionary<string, ColorToken>
            {
                ["Color.Primary"] = new ColorToken { Key = "Color.Primary", Value = "#0D6EFD" }
            },
            Typography = new Dictionary<string, TypographyToken>
            {
                ["FontSize.Body"] = new TypographyToken { Key = "FontSize.Body", Value = "16" }
            },
            Spacing = new Dictionary<string, NumericToken>
            {
                ["Spacing.Medium"] = new NumericToken { Key = "Spacing.Medium", Value = 16 }
            },
            BorderRadius = new Dictionary<string, NumericToken>
            {
                ["Radius.Medium"] = new NumericToken { Key = "Radius.Medium", Value = 6 }
            },
            BorderWidth = new Dictionary<string, NumericToken>
            {
                ["BorderWidth.Default"] = new NumericToken { Key = "BorderWidth.Default", Value = 1 }
            }
        };

        // Act
        var result = _generator.GenerateTokensXaml(tokens);

        // Assert
        result.ShouldContain("Color Tokens");
        result.ShouldContain("Typography Tokens");
        result.ShouldContain("Spacing Tokens");
        result.ShouldContain("Corner Radius Tokens");
        result.ShouldContain("Border Width Tokens");
    }
}
