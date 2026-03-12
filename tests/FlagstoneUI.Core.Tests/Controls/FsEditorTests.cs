using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class FsEditorTests : MauiTestBase
{
	[Fact]
	public void FsEditor_FontAttributes_defaults_to_None()
	{
		var editor = new FsEditor();
		editor.FontAttributes.ShouldBe(FontAttributes.None);
	}

	[Fact]
	public void FsEditor_FontAttributes_can_be_set_to_Bold()
	{
		var editor = new FsEditor { FontAttributes = FontAttributes.Bold };
		editor.FontAttributes.ShouldBe(FontAttributes.Bold);
	}

	[Fact]
	public void FsEditor_FontAttributes_can_be_set_to_Italic()
	{
		var editor = new FsEditor { FontAttributes = FontAttributes.Italic };
		editor.FontAttributes.ShouldBe(FontAttributes.Italic);
	}

	[Fact]
	public void FsEditor_FontAutoScalingEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.FontAutoScalingEnabled.ShouldBeTrue();
	}

	[Fact]
	public void FsEditor_FontAutoScalingEnabled_can_be_disabled()
	{
		var editor = new FsEditor { FontAutoScalingEnabled = false };
		editor.FontAutoScalingEnabled.ShouldBeFalse();
	}

	[Fact]
	public void Editor_FontFamily_defaults_to_null()
	{
		var editor = new FsEditor();
		editor.FontFamily.ShouldBeNull();
	}

	[Fact]
	public void Editor_FontFamily_can_be_set()
	{
		var editor = new FsEditor { FontFamily = "Arial" };
		editor.FontFamily.ShouldBe("Arial");
	}

	[Fact]
	public void Editor_IsSpellCheckEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.IsSpellCheckEnabled.ShouldBeTrue();
	}

	[Fact]
	public void Editor_IsSpellCheckEnabled_can_be_disabled()
	{
		var editor = new FsEditor { IsSpellCheckEnabled = false };
		editor.IsSpellCheckEnabled.ShouldBeFalse();
	}

	[Fact]
	public void Editor_IsTextPredictionEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.IsTextPredictionEnabled.ShouldBeTrue();
	}

	[Fact]
	public void Editor_IsTextPredictionEnabled_can_be_disabled()
	{
		var editor = new FsEditor { IsTextPredictionEnabled = false };
		editor.IsTextPredictionEnabled.ShouldBeFalse();
	}

	[Fact]
	public void Editor_SelectionLength_defaults_to_zero()
	{
		var editor = new FsEditor();
		editor.SelectionLength.ShouldBe(0);
	}

	[Fact]
	public void Editor_SelectionLength_can_be_set()
	{
		var editor = new FsEditor { SelectionLength = 5 };
		editor.SelectionLength.ShouldBe(5);
	}

	[Fact]
	public void Editor_TextTransform_defaults_to_Default()
	{
		var editor = new FsEditor();
		editor.TextTransform.ShouldBe(TextTransform.Default);
	}

	[Fact]
	public void Editor_TextTransform_can_be_set_to_Uppercase()
	{
		var editor = new FsEditor { TextTransform = TextTransform.Uppercase };
		editor.TextTransform.ShouldBe(TextTransform.Uppercase);
	}

	[Fact]
	public void Editor_TextTransform_can_be_set_to_Lowercase()
	{
		var editor = new FsEditor { TextTransform = TextTransform.Lowercase };
		editor.TextTransform.ShouldBe(TextTransform.Lowercase);
	}

	[Fact]
	public void Editor_existing_AutoSize_defaults_to_Disabled()
	{
		var editor = new FsEditor();
		editor.AutoSize.ShouldBe(EditorAutoSizeOption.Disabled);
	}

	[Fact]
	public void Editor_existing_IsReadOnly_defaults_to_false()
	{
		var editor = new FsEditor();
		editor.IsReadOnly.ShouldBeFalse();
	}

	[Fact]
	public void Editor_existing_MaxLength_defaults_to_MaxValue()
	{
		var editor = new FsEditor();
		editor.MaxLength.ShouldBe(int.MaxValue);
	}
}
