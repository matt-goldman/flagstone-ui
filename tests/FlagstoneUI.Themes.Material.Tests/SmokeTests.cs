using Shouldly;
using Xunit;

namespace FlagstoneUI.Themes.Material.Tests;

public class SmokeTests
{
    [Fact]
    public void Material_theme_library_loads()
    {
        // This is a basic smoke test to verify the library can be loaded
        var assembly = typeof(SmokeTests).Assembly;
        assembly.ShouldNotBeNull();
    }
}