using FlagstoneUI.BootstrapConverter.Models;
using Shouldly;
using Xunit;

namespace FlagstoneUI.BootstrapConverter.Tests;

public class BootstrapParserTests
{
    private readonly BootstrapParser _parser = new();

    [Fact]
    public void ParseCss_ShouldExtractColorVariables()
    {
        // Arrange
        var css = @"
:root {
  --bs-primary: #0d6efd;
  --bs-secondary: #6c757d;
  --bs-success: #198754;
}";

        // Act
        var result = _parser.ParseCss(css);

        // Assert
        result.Colors.ShouldNotBeEmpty();
        result.Colors["primary"].ShouldBe("#0d6efd");
        result.Colors["secondary"].ShouldBe("#6c757d");
        result.Colors["success"].ShouldBe("#198754");
    }

    [Fact]
    public void ParseCss_ShouldExtractTypographyVariables()
    {
        // Arrange
        var css = @"
:root {
  --bs-font-family-base: -apple-system, BlinkMacSystemFont, ""Segoe UI"";
  --bs-font-size-base: 1rem;
  --bs-line-height-base: 1.5;
}";

        // Act
        var result = _parser.ParseCss(css);

        // Assert
        result.Typography.ShouldNotBeEmpty();
        result.Typography["font-family-base"].ShouldContain("apple-system");
        result.Typography["font-size-base"].ShouldBe("1rem");
        result.Typography["line-height-base"].ShouldBe("1.5");
    }

    [Fact]
    public void ParseCss_ShouldExtractSpacingVariables()
    {
        // Arrange
        var css = @"
:root {
  --bs-spacer: 1rem;
}";

        // Act
        var result = _parser.ParseCss(css);

        // Assert
        result.Spacing.ShouldNotBeEmpty();
        result.Spacing["spacer"].ShouldBe("1rem");
    }

    [Fact]
    public void ParseCss_ShouldExtractBorderVariables()
    {
        // Arrange
        var css = @"
:root {
  --bs-border-radius: 0.375rem;
  --bs-border-radius-sm: 0.25rem;
  --bs-border-radius-lg: 0.5rem;
  --bs-border-width: 1px;
}";

        // Act
        var result = _parser.ParseCss(css);

        // Assert
        result.Borders.ShouldNotBeEmpty();
        result.Borders["border-radius"].ShouldBe("0.375rem");
        result.Borders["border-radius-sm"].ShouldBe("0.25rem");
        result.Borders["border-radius-lg"].ShouldBe("0.5rem");
        result.Borders["border-width"].ShouldBe("1px");
    }

    [Fact]
    public void ParseScss_ShouldExtractVariables()
    {
        // Arrange
        var scss = @"
$primary: #375a7f;
$secondary: #444;
$font-family-base: -apple-system, BlinkMacSystemFont;
$spacer: 1rem;
$border-radius: 0.25rem;
";

        // Act
        var result = _parser.ParseScss(scss);

        // Assert
        result.Colors["primary"].ShouldBe("#375a7f");
        result.Colors["secondary"].ShouldBe("#444");
        result.Typography["font-family-base"].ShouldContain("apple-system");
        result.Spacing["spacer"].ShouldBe("1rem");
        result.Borders["border-radius"].ShouldBe("0.25rem");
    }

    [Fact]
    public async Task ParseFromFileAsync_ShouldLoadCssFile()
    {
        // Arrange
        var filePath = Path.Combine("Fixtures", "bootstrap-default.css");

        // Act
        var result = await _parser.ParseFromFileAsync(filePath);

        // Assert
        result.Colors.ShouldNotBeEmpty();
        result.Colors["primary"].ShouldBe("#0d6efd");
        result.Typography.ShouldNotBeEmpty();
        result.Spacing.ShouldNotBeEmpty();
        result.Borders.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ParseFromFileAsync_ShouldLoadScssFile()
    {
        // Arrange
        var filePath = Path.Combine("Fixtures", "bootswatch-darkly.scss");

        // Act
        var result = await _parser.ParseFromFileAsync(filePath);

        // Assert
        result.Colors.ShouldNotBeEmpty();
        result.Colors["primary"].ShouldBe("#375a7f");
        result.Colors["dark"].ShouldBe("#303030");
    }

    [Fact]
    public async Task ParseFromFileAsync_ShouldAutoDetectFormat()
    {
        // Arrange
        var cssPath = Path.Combine("Fixtures", "bootstrap-default.css");
        var scssPath = Path.Combine("Fixtures", "bootswatch-darkly.scss");

        // Act
        var cssResult = await _parser.ParseFromFileAsync(cssPath, BootstrapFormat.Auto);
        var scssResult = await _parser.ParseFromFileAsync(scssPath, BootstrapFormat.Auto);

        // Assert
        cssResult.Colors.ShouldNotBeEmpty();
        scssResult.Colors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ParseContent_ShouldHandleMinimalScss()
    {
        // Arrange
        var filePath = Path.Combine("Fixtures", "custom-minimal.scss");
        var content = File.ReadAllText(filePath);

        // Act
        var result = _parser.ParseContent(content, BootstrapFormat.Auto, filePath);

        // Assert
        result.Colors.ShouldNotBeEmpty();
        result.Colors["primary"].ShouldBe("#ff6b6b");
        result.Colors["secondary"].ShouldBe("#4ecdc4");
    }

    [Fact]
    public void ParseCss_ShouldIgnoreNonCustomProperties()
    {
        // Arrange
        var css = @"
:root {
  --bs-primary: #0d6efd;
  color: red;
  font-size: 16px;
}";

        // Act
        var result = _parser.ParseCss(css);

        // Assert
        result.Colors["primary"].ShouldBe("#0d6efd");
        result.Colors.ShouldNotContainKey("color");
        result.Typography.ShouldNotContainKey("font-size");
    }
}
