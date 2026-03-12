using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class FsEditorTests : MauiTestBase
{
	// ── Default value tests ──────────────────────────────────────────────────

	[Fact]
	public void FsEditor_FontAttributes_defaults_to_None()
	{
		var editor = new FsEditor();
		editor.FontAttributes.ShouldBe(FontAttributes.None);
	}

	[Fact]
	public void FsEditor_FontAutoScalingEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.FontAutoScalingEnabled.ShouldBeTrue();
	}

	[Fact]
	public void FsEditor_FontFamily_defaults_to_null()
	{
		var editor = new FsEditor();
		editor.FontFamily.ShouldBeNull();
	}

	[Fact]
	public void FsEditor_IsSpellCheckEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.IsSpellCheckEnabled.ShouldBeTrue();
	}

	[Fact]
	public void FsEditor_IsTextPredictionEnabled_defaults_to_true()
	{
		var editor = new FsEditor();
		editor.IsTextPredictionEnabled.ShouldBeTrue();
	}

	[Fact]
	public void FsEditor_SelectionLength_defaults_to_zero()
	{
		var editor = new FsEditor();
		editor.SelectionLength.ShouldBe(0);
	}

	[Fact]
	public void FsEditor_TextTransform_defaults_to_Default()
	{
		var editor = new FsEditor();
		editor.TextTransform.ShouldBe(TextTransform.Default);
	}

	[Fact]
	public void FsEditor_AutoSize_defaults_to_Disabled()
	{
		var editor = new FsEditor();
		editor.AutoSize.ShouldBe(EditorAutoSizeOption.Disabled);
	}

	[Fact]
	public void FsEditor_IsReadOnly_defaults_to_false()
	{
		var editor = new FsEditor();
		editor.IsReadOnly.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_MaxLength_defaults_to_MaxValue()
	{
		var editor = new FsEditor();
		editor.MaxLength.ShouldBe(int.MaxValue);
	}

	// ── Setter tests ─────────────────────────────────────────────────────────

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
	public void FsEditor_FontAutoScalingEnabled_can_be_disabled()
	{
		var editor = new FsEditor { FontAutoScalingEnabled = false };
		editor.FontAutoScalingEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_FontFamily_can_be_set()
	{
		var editor = new FsEditor { FontFamily = "Arial" };
		editor.FontFamily.ShouldBe("Arial");
	}

	[Fact]
	public void FsEditor_IsSpellCheckEnabled_can_be_disabled()
	{
		var editor = new FsEditor { IsSpellCheckEnabled = false };
		editor.IsSpellCheckEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_IsTextPredictionEnabled_can_be_disabled()
	{
		var editor = new FsEditor { IsTextPredictionEnabled = false };
		editor.IsTextPredictionEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_SelectionLength_can_be_set()
	{
		var editor = new FsEditor { SelectionLength = 5 };
		editor.SelectionLength.ShouldBe(5);
	}

	[Fact]
	public void FsEditor_TextTransform_can_be_set_to_Uppercase()
	{
		var editor = new FsEditor { TextTransform = TextTransform.Uppercase };
		editor.TextTransform.ShouldBe(TextTransform.Uppercase);
	}

	[Fact]
	public void FsEditor_TextTransform_can_be_set_to_Lowercase()
	{
		var editor = new FsEditor { TextTransform = TextTransform.Lowercase };
		editor.TextTransform.ShouldBe(TextTransform.Lowercase);
	}

	// ── Binding propagation tests ─────────────────────────────────────────────
	// These tests verify that FsEditor properties propagate through XAML bindings
	// to the inner BorderlessEditor. They rely on the MAUI binding system being
	// active after InitializeComponent() and BindingContext assignment.

	[Fact]
	public void FsEditor_FontAttributes_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.FontAttributes = FontAttributes.Bold;

		inner.FontAttributes.ShouldBe(FontAttributes.Bold);
	}

	[Fact]
	public void FsEditor_FontAutoScalingEnabled_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.FontAutoScalingEnabled = false;

		inner.FontAutoScalingEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_FontFamily_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.FontFamily = "Arial";

		inner.FontFamily.ShouldBe("Arial");
	}

	[Fact]
	public void FsEditor_IsSpellCheckEnabled_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.IsSpellCheckEnabled = false;

		inner.IsSpellCheckEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_IsTextPredictionEnabled_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.IsTextPredictionEnabled = false;

		inner.IsTextPredictionEnabled.ShouldBeFalse();
	}

	[Fact]
	public void FsEditor_TextTransform_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.TextTransform = TextTransform.Uppercase;

		inner.TextTransform.ShouldBe(TextTransform.Uppercase);
	}

	[Fact]
	public void FsEditor_IsReadOnly_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.IsReadOnly = true;

		inner.IsReadOnly.ShouldBeTrue();
	}

	[Fact]
	public void FsEditor_MaxLength_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.MaxLength = 200;

		inner.MaxLength.ShouldBe(200);
	}

	[Fact]
	public void FsEditor_AutoSize_propagates_to_inner_editor()
	{
		var editor = new FsEditor();
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.AutoSize = EditorAutoSizeOption.TextChanges;

		inner.AutoSize.ShouldBe(EditorAutoSizeOption.TextChanges);
	}

	[Fact]
	public void FsEditor_SelectionLength_propagates_to_inner_editor()
	{
		var editor = new FsEditor { Text = "Hello World" };
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		editor.SelectionLength = 5;

		inner.SelectionLength.ShouldBe(5);
	}

	[Fact]
	public void FsEditor_SelectionLength_TwoWay_updates_FsEditor_when_inner_editor_changes()
	{
		var editor = new FsEditor { Text = "Hello World" };
		var inner = editor.FindByName<BorderlessEditor>("InnerEditor");
		inner.ShouldNotBeNull();

		inner.SelectionLength = 3;

		editor.SelectionLength.ShouldBe(3);
	}
}
