using FlagstoneUI.Core.Builders;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests;

public class SmokeTests
{
	[Fact]
	public void Builder_can_be_instantiated()
	{
		var builder = new FlagstoneUIBuilder();
		builder.ShouldNotBeNull();
	}
}
