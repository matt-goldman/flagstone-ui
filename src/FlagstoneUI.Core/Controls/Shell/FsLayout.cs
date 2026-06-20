namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Attached properties that let pages cooperate with chrome hosted by <see cref="FsShell"/>
/// (or any other host that publishes to the same resource keys) without writing any
/// platform-specific code.
/// </summary>
/// <remarks>
/// Bind to the <c>{DynamicResource FsBottomChromeHeight}</c> key published by
/// <see cref="FsShell"/> when <see cref="FsShell.TabBarIsDocked"/> is <see langword="true"/>.
/// When the bar is undocked, the resource drops to 0 and the consumer manages layout directly.
/// <code>
/// &lt;ContentPage xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
///              fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}" /&gt;
/// </code>
/// </remarks>
public static class FsLayout
{
	/// <summary>
	/// Mirrors its value into <see cref="Page.Padding"/>.Bottom. Bind to
	/// <c>{DynamicResource FsBottomChromeHeight}</c>.
	/// </summary>
	public static readonly BindableProperty BottomChromePaddingProperty =
		BindableProperty.CreateAttached(
			"BottomChromePadding",
			typeof(double),
			typeof(FsLayout),
			defaultValue: 0.0,
			propertyChanged: OnBottomChromePaddingChanged);

	public static double GetBottomChromePadding(BindableObject bindable) =>
		(double)bindable.GetValue(BottomChromePaddingProperty);

	public static void SetBottomChromePadding(BindableObject bindable, double value) =>
		bindable.SetValue(BottomChromePaddingProperty, value);

	private static void OnBottomChromePaddingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Page page || newValue is not double val) return;
		if (double.IsNaN(val) || double.IsInfinity(val) || val < 0) val = 0;
		var p = page.Padding;
		page.Padding = new Thickness(p.Left, p.Top, p.Right, val);
	}
}
