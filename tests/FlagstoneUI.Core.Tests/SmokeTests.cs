using FlagstoneUI.Core.Builders;
using Xunit;
using FluentAssertions;

namespace FlagstoneUI.Core.Tests;

public class SmokeTests
{
    [Fact]
    public void Builder_can_be_instantiated()
    {
        var builder = new FlagstoneUIBuilder();
        builder.Should().NotBeNull();
    }
}
