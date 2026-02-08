using FlagstoneUI.Core.Controls;
using Microsoft.Maui.Controls.Shapes;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

// DISABLED: FsBorder instantiation hangs in headless CI environment
// FsBorder.OnSizeAllocated() requires layout/rendering infrastructure that doesn't exist in headless tests
// Creating any FsBorder instance triggers size allocation which waits indefinitely for layout pass
// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
public class FsBorderTests : MauiTestBase
{
	/*
	[Fact]
	public void Border_can_be_instantiated()
	{
		var border = new FsBorder();
		border.ShouldNotBeNull();
	}

	[Fact]
	public void Border_has_default_zero_thickness()
	{
		var border = new FsBorder();
		border.BorderTopThickness.ShouldBe(0d);
		border.BorderRightThickness.ShouldBe(0d);
		border.BorderBottomThickness.ShouldBe(0d);
		border.BorderLeftThickness.ShouldBe(0d);
	}

	[Fact]
	public void Border_top_thickness_can_be_set()
	{
		var border = new FsBorder { BorderTopThickness = 2.0 };
		border.BorderTopThickness.ShouldBe(2.0);
	}

	[Fact]
	public void Border_right_thickness_can_be_set()
	{
		var border = new FsBorder { BorderRightThickness = 3.0 };
		border.BorderRightThickness.ShouldBe(3.0);
	}

	[Fact]
	public void Border_bottom_thickness_can_be_set()
	{
		var border = new FsBorder { BorderBottomThickness = 4.0 };
		border.BorderBottomThickness.ShouldBe(4.0);
	}

	[Fact]
	public void Border_left_thickness_can_be_set()
	{
		var border = new FsBorder { BorderLeftThickness = 5.0 };
		border.BorderLeftThickness.ShouldBe(5.0);
	}

	// DISABLED: Creates SolidColorBrush which hangs in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_top_brush_can_be_set()
	//{
	//	var brush = new SolidColorBrush(Colors.Red);
	//	var border = new FsBorder { BorderTopBrush = brush };
	//	border.BorderTopBrush.ShouldBe(brush);
	//}

	// DISABLED: Creates SolidColorBrush which hangs in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_right_brush_can_be_set()
	//{
	//	var brush = new SolidColorBrush(Colors.Blue);
	//	var border = new FsBorder { BorderRightBrush = brush };
	//	border.BorderRightBrush.ShouldBe(brush);
	//}

	// DISABLED: Creates SolidColorBrush which hangs in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_bottom_brush_can_be_set()
	//{
	//	var brush = new SolidColorBrush(Colors.Green);
	//	var border = new FsBorder { BorderBottomBrush = brush };
	//	border.BorderBottomBrush.ShouldBe(brush);
	//}

	// DISABLED: Creates SolidColorBrush which hangs in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_left_brush_can_be_set()
	//{
	//	var brush = new SolidColorBrush(Colors.Yellow);
	//	var border = new FsBorder { BorderLeftBrush = brush };
	//	border.BorderLeftBrush.ShouldBe(brush);
	//}

	[Fact]
	public void Border_stroke_cap_can_be_set()
	{
		var border = new FsBorder { BorderStrokeCap = PenLineCap.Round };
		border.BorderStrokeCap.ShouldBe(PenLineCap.Round);
	}

	[Fact]
	public void Border_stroke_cap_defaults_to_flat()
	{
		var border = new FsBorder();
		border.BorderStrokeCap.ShouldBe(PenLineCap.Flat);
	}

	// DISABLED: Creates SolidColorBrush which hangs in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_background_can_be_set()
	//{
	//	var brush = new SolidColorBrush(Colors.White);
	//	var border = new FsBorder { Background = brush };
	//	border.Background.ShouldBe(brush);
	//}

	[Fact]
	public void Border_padding_can_be_set()
	{
		var padding = new Thickness(10);
		var border = new FsBorder { Padding = padding };
		border.Padding.ShouldBe(padding);
	}

	[Fact]
	public void Border_content_can_be_set()
	{
		var label = new Label { Text = "Test" };
		var border = new FsBorder { BorderContent = label };
		border.BorderContent.ShouldBe(label);
	}

	[Fact]
	public void Border_can_set_asymmetric_thickness()
	{
		var border = new FsBorder
		{
			BorderTopThickness = 1.0,
			BorderRightThickness = 2.0,
			BorderBottomThickness = 3.0,
			BorderLeftThickness = 4.0
		};

		border.BorderTopThickness.ShouldBe(1.0);
		border.BorderRightThickness.ShouldBe(2.0);
		border.BorderBottomThickness.ShouldBe(3.0);
		border.BorderLeftThickness.ShouldBe(4.0);
	}

	// DISABLED: Creates multiple SolidColorBrush instances which hang in headless CI environment
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	// TODO: Re-enable when proper UI testing infrastructure is in place (DeviceTests/Appium)
	//[Fact]
	//public void Border_can_set_different_brushes_per_edge()
	//{
	//	var topBrush = new SolidColorBrush(Colors.Red);
	//	var rightBrush = new SolidColorBrush(Colors.Blue);
	//	var bottomBrush = new SolidColorBrush(Colors.Green);
	//	var leftBrush = new SolidColorBrush(Colors.Yellow);
	//
	//	var border = new FsBorder
	//	{
	//		BorderTopBrush = topBrush,
	//		BorderRightBrush = rightBrush,
	//		BorderBottomBrush = bottomBrush,
	//		BorderLeftBrush = leftBrush
	//	};
	//
	//	border.BorderTopBrush.ShouldBe(topBrush);
	//	border.BorderRightBrush.ShouldBe(rightBrush);
	//	border.BorderBottomBrush.ShouldBe(bottomBrush);
	//	border.BorderLeftBrush.ShouldBe(leftBrush);
	//}
	*/
}
