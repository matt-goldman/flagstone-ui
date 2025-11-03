using FlagstoneUI.BootstrapConverter.Models;
using Shouldly;
using Xunit;

namespace FlagstoneUI.BootstrapConverter.Tests;

public class BootstrapMapperTests
{
    private readonly BootstrapMapper _mapper = new();

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapPrimaryColors()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Colors = new Dictionary<string, string>
            {
                ["primary"] = "#0d6efd",
                ["secondary"] = "#6c757d",
                ["success"] = "#198754",
                ["danger"] = "#dc3545"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Colors.ShouldNotBeEmpty();
        result.Colors["Color.Primary"].Value.ShouldBe("#0D6EFD");
        result.Colors["Color.Secondary"].Value.ShouldBe("#6C757D");
        result.Colors["Color.Success"].Value.ShouldBe("#198754");
        result.Colors["Color.Error"].Value.ShouldBe("#DC3545");
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapSemanticColors()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Colors = new Dictionary<string, string>
            {
                ["body-bg"] = "#ffffff",
                ["body-color"] = "#212529",
                ["border-color"] = "#dee2e6"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Colors["Color.Background"].Value.ShouldBe("#FFFFFF");
        result.Colors["Color.OnBackground"].Value.ShouldBe("#212529");
        result.Colors["Color.Outline"].Value.ShouldBe("#DEE2E6");
    }

    [Fact]
    public void MapToFlagstoneTokens_WithAutoStrategy_ShouldGenerateDarkModeColors()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Colors = new Dictionary<string, string>
            {
                ["primary"] = "#0d6efd"
            }
        };
        var options = new ConversionOptions
        {
            DarkModeStrategy = DarkModeStrategy.Auto
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables, options);

        // Assert
        result.Colors["Color.Primary"].DarkValue.ShouldNotBeNull();
        result.Colors["Color.Primary"].DarkValue.ShouldStartWith("#");
    }

    [Fact]
    public void MapToFlagstoneTokens_WithNoneStrategy_ShouldNotGenerateDarkMode()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Colors = new Dictionary<string, string>
            {
                ["primary"] = "#0d6efd"
            }
        };
        var options = new ConversionOptions
        {
            DarkModeStrategy = DarkModeStrategy.None
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables, options);

        // Assert
        result.Colors["Color.Primary"].DarkValue.ShouldBeNull();
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapTypography()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Typography = new Dictionary<string, string>
            {
                ["font-family-base"] = "-apple-system, BlinkMacSystemFont, \"Segoe UI\"",
                ["font-size-base"] = "1rem",
                ["line-height-base"] = "1.5"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Typography.ShouldNotBeEmpty();
        result.Typography["FontFamily.Default"].Value.ShouldBe("System");
        result.Typography["FontSize.Body"].Value.ShouldBe("16"); // 1rem = 16px
        result.Typography["LineHeight.Default"].Value.ShouldBe("1.5");
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapSpacing()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Spacing = new Dictionary<string, string>
            {
                ["spacer"] = "1rem"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Spacing.ShouldNotBeEmpty();
        result.Spacing["Spacing.ExtraSmall"].Value.ShouldBe(4); // 0.25rem = 4px
        result.Spacing["Spacing.Small"].Value.ShouldBe(8); // 0.5rem = 8px
        result.Spacing["Spacing.Medium"].Value.ShouldBe(16); // 1rem = 16px
        result.Spacing["Spacing.Large"].Value.ShouldBe(24); // 1.5rem = 24px
        result.Spacing["Spacing.ExtraLarge"].Value.ShouldBe(48); // 3rem = 48px
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapBorderRadius()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Borders = new Dictionary<string, string>
            {
                ["border-radius"] = "0.375rem",
                ["border-radius-sm"] = "0.25rem",
                ["border-radius-lg"] = "0.5rem"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.BorderRadius.ShouldNotBeEmpty();
        result.BorderRadius["Radius.Small"].Value.ShouldBe(4); // 0.25rem = 4px
        result.BorderRadius["Radius.Medium"].Value.ShouldBe(6); // 0.375rem = 6px
        result.BorderRadius["Radius.Large"].Value.ShouldBe(8); // 0.5rem = 8px
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldMapBorderWidth()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Borders = new Dictionary<string, string>
            {
                ["border-width"] = "1px"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.BorderWidth.ShouldNotBeEmpty();
        result.BorderWidth["BorderWidth.Default"].Value.ShouldBe(1);
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldConvertRemToPixels()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Typography = new Dictionary<string, string>
            {
                ["font-size-base"] = "1.5rem"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Typography["FontSize.Body"].Value.ShouldBe("24"); // 1.5rem = 24px
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldHandleEmUnits()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Borders = new Dictionary<string, string>
            {
                ["border-radius"] = "0.5em"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.BorderRadius["Radius.Medium"].Value.ShouldBe(8); // 0.5em = 8px
    }

    [Fact]
    public void MapToFlagstoneTokens_ShouldIncludePurposeComments()
    {
        // Arrange
        var variables = new BootstrapVariables
        {
            Colors = new Dictionary<string, string>
            {
                ["primary"] = "#0d6efd"
            }
        };

        // Act
        var result = _mapper.MapToFlagstoneTokens(variables);

        // Assert
        result.Colors["Color.Primary"].Purpose.ShouldNotBeNullOrEmpty();
        result.Colors["Color.Primary"].Purpose.ShouldBe("Primary brand color");
    }
}
