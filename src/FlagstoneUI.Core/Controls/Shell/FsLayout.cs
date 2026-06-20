namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Attached properties that let pages cooperate with chrome hosted by <see cref="FsShell"/>
/// (or any other host that publishes to the same resource keys) without writing any
/// platform-specific code.
/// </summary>
/// <remarks>
/// Each property owns one edge of <see cref="Page.Padding"/> and leaves the other three
/// untouched, so multiple properties can coexist on the same page. Bind each to the
/// corresponding <c>{DynamicResource}</c> key published by <see cref="FsShell"/>:
/// <code>
/// &lt;ContentPage xmlns:fs="clr-namespace:FlagstoneUI.Core.Controls;assembly=FlagstoneUI.Core"
///              fs:FsLayout.BottomChromePadding="{DynamicResource FsBottomChromeHeight}"
///              fs:FsLayout.TopChromePadding="{DynamicResource FsTopChromeHeight}" /&gt;
/// </code>
/// </remarks>
public static class FsLayout
{
	#region BottomChromePadding

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
		val = Sanitize(val);
		var p = page.Padding;
		page.Padding = new Thickness(p.Left, p.Top, p.Right, val);
	}

	#endregion

	#region TopChromePadding

	/// <summary>
	/// Mirrors its value into <see cref="Page.Padding"/>.Top. Bind to
	/// <c>{DynamicResource FsTopChromeHeight}</c>.
	/// </summary>
	public static readonly BindableProperty TopChromePaddingProperty =
		BindableProperty.CreateAttached(
			"TopChromePadding",
			typeof(double),
			typeof(FsLayout),
			defaultValue: 0.0,
			propertyChanged: OnTopChromePaddingChanged);

	public static double GetTopChromePadding(BindableObject bindable) =>
		(double)bindable.GetValue(TopChromePaddingProperty);

	public static void SetTopChromePadding(BindableObject bindable, double value) =>
		bindable.SetValue(TopChromePaddingProperty, value);

	private static void OnTopChromePaddingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Page page || newValue is not double val) return;
		val = Sanitize(val);
		var p = page.Padding;
		page.Padding = new Thickness(p.Left, val, p.Right, p.Bottom);
	}

	#endregion

	#region LeftChromePadding

	/// <summary>
	/// Mirrors its value into <see cref="Page.Padding"/>.Left. Bind to
	/// <c>{DynamicResource FsLeftChromeWidth}</c>.
	/// </summary>
	public static readonly BindableProperty LeftChromePaddingProperty =
		BindableProperty.CreateAttached(
			"LeftChromePadding",
			typeof(double),
			typeof(FsLayout),
			defaultValue: 0.0,
			propertyChanged: OnLeftChromePaddingChanged);

	public static double GetLeftChromePadding(BindableObject bindable) =>
		(double)bindable.GetValue(LeftChromePaddingProperty);

	public static void SetLeftChromePadding(BindableObject bindable, double value) =>
		bindable.SetValue(LeftChromePaddingProperty, value);

	private static void OnLeftChromePaddingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Page page || newValue is not double val) return;
		val = Sanitize(val);
		var p = page.Padding;
		page.Padding = new Thickness(val, p.Top, p.Right, p.Bottom);
	}

	#endregion

	#region RightChromePadding

	/// <summary>
	/// Mirrors its value into <see cref="Page.Padding"/>.Right. Bind to
	/// <c>{DynamicResource FsRightChromeWidth}</c>.
	/// </summary>
	public static readonly BindableProperty RightChromePaddingProperty =
		BindableProperty.CreateAttached(
			"RightChromePadding",
			typeof(double),
			typeof(FsLayout),
			defaultValue: 0.0,
			propertyChanged: OnRightChromePaddingChanged);

	public static double GetRightChromePadding(BindableObject bindable) =>
		(double)bindable.GetValue(RightChromePaddingProperty);

	public static void SetRightChromePadding(BindableObject bindable, double value) =>
		bindable.SetValue(RightChromePaddingProperty, value);

	private static void OnRightChromePaddingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not Page page || newValue is not double val) return;
		val = Sanitize(val);
		var p = page.Padding;
		page.Padding = new Thickness(p.Left, p.Top, val, p.Bottom);
	}

	#endregion

	private static double Sanitize(double value) =>
		double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? 0 : value;
}
