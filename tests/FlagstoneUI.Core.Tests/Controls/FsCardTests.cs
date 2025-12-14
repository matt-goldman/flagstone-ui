using FlagstoneUI.Core.Controls;
using Microsoft.Maui.Controls.Shapes;
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
		var card = new FsCard { Elevation = 2 };
		card.Elevation.ShouldBe(2);
	}

	[Fact]
	public void Card_with_zero_elevation_has_no_shadow()
	{
		var card = new FsCard { Elevation = 0 };
		card.Shadow.ShouldBeNull();
	}

	[Fact]
	public void Card_with_positive_elevation_has_shadow()
	{
		var card = new FsCard { Elevation = 2 };
		card.Shadow.ShouldNotBeNull();
	}

	[Fact]
	public void Card_shadow_radius_increases_with_elevation()
	{
		var card = new FsCard { Elevation = 1 };

		var shadow1Radius = card.Shadow?.Radius ?? 0f;

		card.Elevation = 3;
		var shadow3Radius = card.Shadow?.Radius ?? 0f;

		shadow3Radius.ShouldBeGreaterThan(shadow1Radius);
	}

	[Fact]
	public void Card_corner_radius_property_can_be_set()
	{
		var card = new FsCard { CornerRadius = 12.0 };
		card.CornerRadius.ShouldBe(12.0);
	}

	[Fact]
	public void Card_border_color_property_can_be_set()
	{
		var card = new FsCard { BorderColor = Colors.Red };
		card.BorderColor.ShouldBe(Colors.Red);
	}

	[Fact]
	public void Card_border_width_property_can_be_set()
	{
		var card = new FsCard { BorderWidth = 2.0 };
		card.BorderWidth.ShouldBe(2.0);
	}

	[Fact]
	public void Card_background_color_property_can_be_set()
	{
		var card = new FsCard { BackgroundColor = Colors.Blue };
		card.BackgroundColor.ShouldBe(Colors.Blue);
	}

	[Fact]
	public void Card_border_color_sets_all_edge_brushes()
	{
		var card = new FsCard { BorderColor = Colors.Red };
		
		// Border color should set all edge brushes
		var expectedBrush = card.BorderTopBrush as SolidColorBrush;
		expectedBrush.ShouldNotBeNull();
		expectedBrush.Color.ShouldBe(Colors.Red);
		
		(card.BorderRightBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Red);
		(card.BorderBottomBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Red);
		(card.BorderLeftBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Red);
	}

	[Fact]
	public void Card_border_width_sets_all_edge_thicknesses()
	{
		var card = new FsCard { BorderWidth = 3.0 };
		
		card.BorderTopThickness.ShouldBe(3.0);
		card.BorderRightThickness.ShouldBe(3.0);
		card.BorderBottomThickness.ShouldBe(3.0);
		card.BorderLeftThickness.ShouldBe(3.0);
	}

	[Fact]
	public void Card_per_edge_thickness_can_be_set()
	{
		var card = new FsCard
		{
			BorderTopThickness = 1.0,
			BorderRightThickness = 2.0,
			BorderBottomThickness = 3.0,
			BorderLeftThickness = 4.0
		};

		card.BorderTopThickness.ShouldBe(1.0);
		card.BorderRightThickness.ShouldBe(2.0);
		card.BorderBottomThickness.ShouldBe(3.0);
		card.BorderLeftThickness.ShouldBe(4.0);
	}

	[Fact]
	public void Card_per_edge_brush_can_be_set()
	{
		var topBrush = new SolidColorBrush(Colors.Red);
		var rightBrush = new SolidColorBrush(Colors.Blue);
		var bottomBrush = new SolidColorBrush(Colors.Green);
		var leftBrush = new SolidColorBrush(Colors.Yellow);

		var card = new FsCard
		{
			BorderTopBrush = topBrush,
			BorderRightBrush = rightBrush,
			BorderBottomBrush = bottomBrush,
			BorderLeftBrush = leftBrush
		};

		card.BorderTopBrush.ShouldBe(topBrush);
		card.BorderRightBrush.ShouldBe(rightBrush);
		card.BorderBottomBrush.ShouldBe(bottomBrush);
		card.BorderLeftBrush.ShouldBe(leftBrush);
	}

	[Fact]
	public void Card_background_color_sets_background_brush()
	{
		var card = new FsCard { BackgroundColor = Colors.Blue };
		
		var brush = card.BackgroundBrush as SolidColorBrush;
		brush.ShouldNotBeNull();
		brush.Color.ShouldBe(Colors.Blue);
	}

	[Fact]
	public void Card_border_stroke_cap_can_be_set()
	{
		var card = new FsCard { BorderStrokeCap = PenLineCap.Round };
		card.BorderStrokeCap.ShouldBe(PenLineCap.Round);
	}
}
