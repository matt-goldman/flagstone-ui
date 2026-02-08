using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class BorderShorthandTests : MauiTestBase
{
	[Fact]
	public void BorderShorthand_parses_single_value()
	{
		var shorthand = BorderShorthand.Parse("2 Red");

		shorthand.Top.Thickness.ShouldBe(2.0);
		shorthand.Right.Thickness.ShouldBe(2.0);
		shorthand.Bottom.Thickness.ShouldBe(2.0);
		shorthand.Left.Thickness.ShouldBe(2.0);

		shorthand.Top.Color.ShouldBe(Colors.Red);
		shorthand.Right.Color.ShouldBe(Colors.Red);
		shorthand.Bottom.Color.ShouldBe(Colors.Red);
		shorthand.Left.Color.ShouldBe(Colors.Red);
	}

	[Fact]
	public void BorderShorthand_parses_two_values()
	{
		var shorthand = BorderShorthand.Parse("1 Black, 2 Grey");

		shorthand.Top.Thickness.ShouldBe(1.0);
		shorthand.Bottom.Thickness.ShouldBe(1.0);
		shorthand.Left.Thickness.ShouldBe(2.0);
		shorthand.Right.Thickness.ShouldBe(2.0);

		shorthand.Top.Color.ShouldBe(Colors.Black);
		shorthand.Bottom.Color.ShouldBe(Colors.Black);
		shorthand.Left.Color.ShouldBe(Colors.Grey);
		shorthand.Right.Color.ShouldBe(Colors.Grey);
	}

	[Fact]
	public void BorderShorthand_parses_four_values()
	{
		var shorthand = BorderShorthand.Parse("1 White, 2 Black, 3 Blue, 4 Green");

		shorthand.Top.Thickness.ShouldBe(1.0);
		shorthand.Right.Thickness.ShouldBe(2.0);
		shorthand.Bottom.Thickness.ShouldBe(3.0);
		shorthand.Left.Thickness.ShouldBe(4.0);

		shorthand.Top.Color.ShouldBe(Colors.White);
		shorthand.Right.Color.ShouldBe(Colors.Black);
		shorthand.Bottom.Color.ShouldBe(Colors.Blue);
		shorthand.Left.Color.ShouldBe(Colors.Green);
	}

	[Fact]
	public void BorderShorthand_parses_hex_colors()
	{
		var shorthand = BorderShorthand.Parse("1 #FF0000");

		shorthand.Top.Color.Red.ShouldBe(1.0f, 0.01f);
		shorthand.Top.Color.Green.ShouldBe(0.0f, 0.01f);
		shorthand.Top.Color.Blue.ShouldBe(0.0f, 0.01f);
	}

	[Fact]
	public void BorderShorthand_throws_on_three_values()
	{
		Should.Throw<ArgumentException>(() => BorderShorthand.Parse("1 Black, 2 Grey, 3 White"));
	}

	[Fact]
	public void BorderShorthand_throws_on_invalid_thickness()
	{
		Should.Throw<ArgumentException>(() => BorderShorthand.Parse("abc Red"));
	}

	[Fact]
	public void BorderShorthand_throws_on_invalid_color()
	{
		Should.Throw<ArgumentException>(() => BorderShorthand.Parse("1 NotAColor"));
	}

	[Fact]
	public void BorderShorthand_handles_empty_string()
	{
		var shorthand = BorderShorthand.Parse("");

		shorthand.Top.Thickness.ShouldBe(0.0);
		shorthand.Top.Color.ShouldBe(Colors.Transparent);
	}

	// DISABLED: Creates FsBorder which hangs in headless CI (OnSizeAllocated requires layout infrastructure)
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	//[Fact]
	//public void FsBorder_applies_shorthand_single_value()
	//{
	//	var border = new FsBorder { Border = "2 Blue" };
	//	
	//	border.BorderTopThickness.ShouldBe(2.0);
	//	border.BorderRightThickness.ShouldBe(2.0);
	//	border.BorderBottomThickness.ShouldBe(2.0);
	//	border.BorderLeftThickness.ShouldBe(2.0);
	//	
	//	(border.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	//	(border.BorderRightBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	//	(border.BorderBottomBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	//	(border.BorderLeftBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	//}

	// DISABLED: Creates FsBorder which hangs in headless CI (OnSizeAllocated requires layout infrastructure)
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	//[Fact]
	//public void FsBorder_applies_shorthand_two_values()
	//{
	//	var border = new FsBorder { Border = "1 Black, 2 Grey" };
	//	
	//	border.BorderTopThickness.ShouldBe(1.0);
	//	border.BorderBottomThickness.ShouldBe(1.0);
	//	border.BorderLeftThickness.ShouldBe(2.0);
	//	border.BorderRightThickness.ShouldBe(2.0);
	//}

	// DISABLED: Creates FsBorder which hangs in headless CI (OnSizeAllocated requires layout infrastructure)
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	//[Fact]
	//public void FsBorder_applies_shorthand_four_values()
	//{
	//	var border = new FsBorder { Border = "1 White, 2 Black, 3 Black, 4 White" };
	//	
	//	border.BorderTopThickness.ShouldBe(1.0);
	//	border.BorderRightThickness.ShouldBe(2.0);
	//	border.BorderBottomThickness.ShouldBe(3.0);
	//	border.BorderLeftThickness.ShouldBe(4.0);
	//	
	//	(border.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.White);
	//	(border.BorderRightBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Black);
	//	(border.BorderBottomBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Black);
	//	(border.BorderLeftBrush as SolidColorBrush)?.Color.ShouldBe(Colors.White);
	//}

	// DISABLED: Creates FsCard which may hang in headless CI (needs verification if it overrides layout methods)
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	//[Fact]
	//public void FsCard_applies_shorthand()
	//{
	//	var card = new FsCard { Border = "2 Red" };
	//	
	//	card.BorderTopThickness.ShouldBe(2.0);
	//	card.BorderRightThickness.ShouldBe(2.0);
	//	card.BorderBottomThickness.ShouldBe(2.0);
	//	card.BorderLeftThickness.ShouldBe(2.0);
	//	
	//	(card.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Red);
	//}

	// DISABLED: Creates FsEntry which may hang in headless CI (needs verification if it overrides layout methods)
	// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
	//[Fact]
	//public void FsEntry_applies_shorthand()
	//{
	//	var entry = new FsEntry { Border = "2 Blue" };
	//	
	//	entry.BorderTopThickness.ShouldBe(2.0);
	//	entry.BorderRightThickness.ShouldBe(2.0);
	//	entry.BorderBottomThickness.ShouldBe(2.0);
	//	entry.BorderLeftThickness.ShouldBe(2.0);
	//	
	//	(entry.BorderTopBrush as SolidColorBrush)?.Color.ShouldBe(Colors.Blue);
	//}
}
