namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Attached properties that let pages cooperate with chrome hosted by <see cref="FsShell"/>
/// (or any other host that publishes to the same resource keys) without writing any
/// platform-specific code.
/// </summary>
/// <remarks>
/// The intended consumption pattern is to bind the attached property to the well-known
/// resource keys via <c>{DynamicResource}</c>. For example, a page that wants to leave room
/// for the bottom bar hosted by <see cref="FsShell"/>:
/// <code>
/// &lt;ContentPage xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
///              fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}" /&gt;
/// </code>
/// FsShell writes the current bar height into <see cref="Application.Resources"/> under
/// <see cref="FsShell.BottomChromeHeightResourceKey"/>; the attached property reflects that
/// value into <see cref="Page.Padding"/>.Bottom so page content stays above the bar.
/// </remarks>
public static class FsLayout
{
	/// <summary>
	/// Attached <see cref="double"/> property that mirrors its value into the target
	/// <see cref="Page.Padding"/>.Bottom. Designed to be bound to
	/// <c>{DynamicResource FsBottomChromeHeight}</c> on pages that should reserve room for
	/// the bottom bar <see cref="FsShell"/> hosts.
	/// </summary>
	public static readonly BindableProperty BottomChromePaddingProperty =
		BindableProperty.CreateAttached(
			"BottomChromePadding",
			typeof(double),
			typeof(FsLayout),
			defaultValue: 0.0,
			propertyChanged: OnBottomChromePaddingChanged);

	/// <summary>Gets the current <see cref="BottomChromePaddingProperty"/> value.</summary>
	public static double GetBottomChromePadding(BindableObject bindable) =>
		(double)bindable.GetValue(BottomChromePaddingProperty);

	/// <summary>Sets the <see cref="BottomChromePaddingProperty"/> value.</summary>
	public static void SetBottomChromePadding(BindableObject bindable, double value) =>
		bindable.SetValue(BottomChromePaddingProperty, value);

	private static void OnBottomChromePaddingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		// The attached property owns the page's Padding.Bottom while it is set; left/top/right
		// are left untouched so callers can still control the other edges declaratively. Pages
		// that need different behaviour should not opt in and instead read the resource by hand.
		if (bindable is not Page page || newValue is not double height)
		{
			return;
		}

		if (double.IsNaN(height) || double.IsInfinity(height) || height < 0)
		{
			height = 0;
		}

		var padding = page.Padding;
		page.Padding = new Thickness(padding.Left, padding.Top, padding.Right, height);
	}
}
