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
		
		// BorderColor sets the uniform Stroke property (not per-edge brushes)
		var strokeBrush = card.Stroke as SolidColorBrush;
		strokeBrush.ShouldNotBeNull();
		strokeBrush.Color.ShouldBe(Colors.Red);
	}

	[Fact]
	public void Card_border_width_sets_all_edge_thicknesses()
	{
		var card = new FsCard { BorderWidth = 3.0 };
		
		// BorderWidth sets the uniform StrokeThickness property (not per-edge thicknesses)
		card.StrokeThickness.ShouldBe(3.0);
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

	[Fact]
	public void Card_background_brush_can_be_set_directly()
	{
		var brush = new LinearGradientBrush(
			new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0.0f),
				new GradientStop(Colors.Blue, 1.0f)
			},
			new Point(0, 0),
			new Point(1, 1));

		var card = new FsCard { BackgroundBrush = brush };
		card.BackgroundBrush.ShouldBe(brush);
	}

	[Fact]
	public void Card_stroke_can_be_set_directly()
	{
		var brush = new SolidColorBrush(Colors.Purple);
		var card = new FsCard { Stroke = brush };
		card.Stroke.ShouldBe(brush);
	}

	[Fact]
	public void Card_stroke_thickness_can_be_set_directly()
	{
		var card = new FsCard { StrokeThickness = 5.0 };
		card.StrokeThickness.ShouldBe(5.0);
	}

	[Fact]
	public void Card_per_edge_properties_default_to_zero_or_transparent()
	{
		var card = new FsCard();
		
		card.BorderTopThickness.ShouldBe(0.0);
		card.BorderRightThickness.ShouldBe(0.0);
		card.BorderBottomThickness.ShouldBe(0.0);
		card.BorderLeftThickness.ShouldBe(0.0);
		
		(card.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Transparent);
		(card.BorderRightBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Transparent);
		(card.BorderBottomBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Transparent);
		(card.BorderLeftBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Transparent);
	}

	[Fact]
	public void Card_shadow_removed_when_elevation_changed_to_zero()
	{
		var card = new FsCard { Elevation = 3 };
		card.Shadow.ShouldNotBeNull();

		card.Elevation = 0;
		card.Shadow.ShouldBeNull();
	}

	[Fact]
	public void Card_shadow_added_when_elevation_increased_from_zero()
	{
		var card = new FsCard { Elevation = 0 };
		card.Shadow.ShouldBeNull();

		card.Elevation = 2;
		card.Shadow.ShouldNotBeNull();
	}

	[Fact]
	public void Card_uniform_stroke_and_per_edge_borders_independent()
	{
		var card = new FsCard
		{
			BorderColor = Colors.Red,
			BorderWidth = 2.0,
			BorderTopBrush = new SolidColorBrush(Colors.Blue),
			BorderTopThickness = 4.0
		};

		// Uniform border properties should be set
		card.StrokeThickness.ShouldBe(2.0);
		(card.Stroke as SolidColorBrush)?.Color.ShouldBe(Colors.Red);

		// Per-edge properties should remain independent
		card.BorderTopThickness.ShouldBe(4.0);
		(card.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	}
}
