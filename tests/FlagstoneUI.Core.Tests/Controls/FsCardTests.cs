using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class FsCardTests : MauiTestBase
{
	[Fact]
	public void Card_can_be_instantiated()
	{
		var card = new FsCard();
		card.ShouldNotBeNull();
	}

	[Fact]
	public void Card_has_default_elevation_zero()
	{
		var card = new FsCard();
		card.Elevation.ShouldBe(0);
	}

	[Fact]
	public void Card_elevation_property_can_be_set()
	{
		var card = new FsCard();
		card.Elevation = 2;
		card.Elevation.ShouldBe(2);
	}

	[Fact]
	public void Card_with_zero_elevation_has_no_shadow()
	{
		var card = new FsCard();
		card.Elevation = 0;
		card.Shadow.ShouldBeNull();
	}

	[Fact]
	public void Card_with_positive_elevation_has_shadow()
	{
		var card = new FsCard();
		card.Elevation = 2;
		card.Shadow.ShouldNotBeNull();
	}

	[Fact]
	public void Card_shadow_radius_increases_with_elevation()
	{
		var card = new FsCard();

		card.Elevation = 1;
		var shadow1Radius = card.Shadow?.Radius ?? 0f;

		card.Elevation = 3;
		var shadow3Radius = card.Shadow?.Radius ?? 0f;

		shadow3Radius.ShouldBeGreaterThan(shadow1Radius);
	}

	[Fact]
	public void Card_corner_radius_property_can_be_set()
	{
		var card = new FsCard();
		card.CornerRadius = 12.0;
		card.CornerRadius.ShouldBe(12.0);
	}

	[Fact]
	public void Card_border_color_property_can_be_set()
	{
		var card = new FsCard();
		card.BorderColor = Colors.Red;
		card.BorderColor.ShouldBe(Colors.Red);
	}

	[Fact]
	public void Card_border_width_property_can_be_set()
	{
		var card = new FsCard();
		card.BorderWidth = 2.0;
		card.BorderWidth.ShouldBe(2.0);
	}

	[Fact]
	public void Card_background_color_property_can_be_set()
	{
		var card = new FsCard();
		card.BackgroundColor = Colors.Blue;
		card.BackgroundColor.ShouldBe(Colors.Blue);
	}
}
