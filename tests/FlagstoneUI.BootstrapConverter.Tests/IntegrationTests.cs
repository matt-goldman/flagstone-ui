using FlagstoneUI.BootstrapConverter.Models;
using Shouldly;
using Xunit;

namespace FlagstoneUI.BootstrapConverter.Tests;

/// <summary>
/// Integration tests for the full conversion pipeline
/// </summary>
public class IntegrationTests
{
    private readonly BootstrapParser _parser = new();
    private readonly BootstrapMapper _mapper = new();
    private readonly XamlThemeGenerator _generator = new();

    [Fact]
    public async Task FullPipeline_CssToXaml_ShouldWork()
    {
        // Arrange
        var cssPath = Path.Combine("Fixtures", "bootstrap-default.css");

        // Act - Full pipeline
        var variables = await _parser.ParseFromFileAsync(cssPath);
        var tokens = _mapper.MapToFlagstoneTokens(variables);
        var xaml = _generator.GenerateTokensXaml(tokens);

        // Assert
        xaml.ShouldNotBeNull();
        xaml.ShouldContain("Color.Primary");
        xaml.ShouldContain("#0D6EFD");
        xaml.ShouldContain("Spacing.Medium");
        xaml.ShouldContain("Radius.Medium");
    }

    [Fact]
    public async Task FullPipeline_ScssToXaml_ShouldWork()
    {
        // Arrange
        var scssPath = Path.Combine("Fixtures", "bootswatch-darkly.scss");

        // Act - Full pipeline
        var variables = await _parser.ParseFromFileAsync(scssPath);
        var tokens = _mapper.MapToFlagstoneTokens(variables);
        var xaml = _generator.GenerateTokensXaml(tokens);

        // Assert
        xaml.ShouldNotBeNull();
        xaml.ShouldContain("Color.Primary");
        xaml.ShouldContain("#375A7F"); // Darkly primary color
    }

    [Fact]
    public async Task FullPipeline_WithDarkMode_ShouldGenerateCompleteTheme()
    {
        // Arrange
        var cssPath = Path.Combine("Fixtures", "bootstrap-default.css");
        var options = new ConversionOptions
        {
            DarkModeStrategy = DarkModeStrategy.Auto,
            IncludeComments = true
        };

        // Act
        var variables = await _parser.ParseFromFileAsync(cssPath);
        var tokens = _mapper.MapToFlagstoneTokens(variables, options);
        var xaml = _generator.GenerateTokensXaml(tokens, options);

        // Assert
        xaml.ShouldContain("Dark mode:");
        xaml.ShouldContain("Primary brand color");
    }

    [Fact]
    public async Task FullPipeline_MinimalTheme_ShouldWork()
    {
        // Arrange
        var scssPath = Path.Combine("Fixtures", "custom-minimal.scss");

        // Act
        var variables = await _parser.ParseFromFileAsync(scssPath);
        var tokens = _mapper.MapToFlagstoneTokens(variables);
        var xaml = _generator.GenerateTokensXaml(tokens);

        // Assert
        xaml.ShouldContain("Color.Primary");
        xaml.ShouldContain("#FF6B6B");
        xaml.ShouldContain("Color.Secondary");
        xaml.ShouldContain("#4ECDC4");
    }

    [Fact]
    public async Task FullPipeline_GenerateFiles_ShouldCreateCompleteTheme()
    {
        // Arrange
        var cssPath = Path.Combine("Fixtures", "bootstrap-default.css");
        var outputDir = Path.Combine(Path.GetTempPath(), "flagstone-integration-" + Guid.NewGuid());

        try
        {
            // Act - Full pipeline with file generation
            var variables = await _parser.ParseFromFileAsync(cssPath);
            var tokens = _mapper.MapToFlagstoneTokens(variables);
            await _generator.GenerateFilesAsync(tokens, "Bootstrap Default", outputDir);

            // Assert - Both files should exist and be valid
            var tokensPath = Path.Combine(outputDir, "Tokens.xaml");
            var themePath = Path.Combine(outputDir, "Theme.xaml");

            File.Exists(tokensPath).ShouldBeTrue();
            File.Exists(themePath).ShouldBeTrue();

            var tokensContent = await File.ReadAllTextAsync(tokensPath);
            tokensContent.ShouldContain("Color.Primary");
            tokensContent.ShouldContain("Spacing.Medium");

            var themeContent = await File.ReadAllTextAsync(themePath);
            themeContent.ShouldContain("Bootstrap Default");
            themeContent.ShouldContain("Tokens.xaml");
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
    public async Task FullPipeline_AllBootstrapFeatures_ShouldBePreserved()
    {
        // Arrange
        var cssPath = Path.Combine("Fixtures", "bootstrap-default.css");

        // Act
        var variables = await _parser.ParseFromFileAsync(cssPath);
        var tokens = _mapper.MapToFlagstoneTokens(variables);

        // Assert - All categories should have tokens
        tokens.Colors.ShouldNotBeEmpty("Colors should be mapped");
        tokens.Typography.ShouldNotBeEmpty("Typography should be mapped");
        tokens.Spacing.ShouldNotBeEmpty("Spacing should be mapped");
        tokens.BorderRadius.ShouldNotBeEmpty("Border radius should be mapped");
        tokens.BorderWidth.ShouldNotBeEmpty("Border width should be mapped");

        // Verify specific mappings
        tokens.Colors.ShouldContainKey("Color.Primary");
        tokens.Colors.ShouldContainKey("Color.Background");
        tokens.Typography.ShouldContainKey("FontSize.Body");
        tokens.Spacing.ShouldContainKey("Spacing.Medium");
        tokens.BorderRadius.ShouldContainKey("Radius.Medium");
    }
}
