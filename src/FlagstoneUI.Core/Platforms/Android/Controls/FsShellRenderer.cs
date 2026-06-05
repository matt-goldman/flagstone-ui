using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using LP = Android.Widget.LinearLayout.LayoutParams;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Android-specific Shell renderer for <see cref="FsShell"/>. Suppresses the native
/// <c>BottomNavigationView</c> and hosts an <see cref="FsTabBar"/> (or the consumer-supplied bar)
/// in its place.
/// </summary>
/// <remarks>
/// Stock Shell builds each <c>ShellItem</c> as a vertical <c>LinearLayout</c> containing the page
/// content area (weight 1) above a <c>BottomNavigationView</c>. This renderer overrides
/// <see cref="ShellRenderer.CreateShellItemRenderer"/> to return an <see cref="FsShellItemRenderer"/>
/// that hides the native bar and appends the FlagstoneUI bar's platform view as the bottom sibling,
/// so the consumer's declared bar template participates in normal MAUI layout.
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	public FsShellRenderer(Context context) : base(context)
	{
	}

	protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
		=> new FsShellItemRenderer(this);
}

/// <summary>
/// Replaces the native <c>BottomNavigationView</c> chrome for a single <c>ShellItem</c> with the
/// FlagstoneUI bar hosted from <see cref="FsShell.TabBar"/>.
/// </summary>
internal sealed class FsShellItemRenderer : ShellItemRenderer
{
	private AView? _hostedBar;

	public FsShellItemRenderer(IShellContext shellContext) : base(shellContext)
	{
	}

	public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
	{
		var root = base.OnCreateView(inflater, container, savedInstanceState);

		// Stock Shell returns the outer vertical LinearLayout. Suppress the native bar and host the
		// FlagstoneUI bar in its place. If the shape is ever something other than a LinearLayout we
		// fall through and leave stock chrome untouched rather than risk a misplaced bar.
		if (root is LinearLayout outerLayout)
		{
			HostFlagstoneBar(outerLayout);
			SuppressNativeBar(outerLayout);
		}

		return root;
	}

	/// <summary>
	/// Hides the native <c>BottomNavigationView</c>: every direct child of the outer layout that is
	/// neither the page-content navigation area nor the FlagstoneUI bar we host. Comparing against
	/// <see cref="ShellItemRenderer.GetNavigationTarget"/> avoids a hard dependency on the Material
	/// bottom-nav type and is resilient to it being recreated.
	/// </summary>
	private void SuppressNativeBar(LinearLayout outerLayout)
	{
		var navigationArea = GetNavigationTarget();
		for (var i = 0; i < outerLayout.ChildCount; i++)
		{
			var child = outerLayout.GetChildAt(i);
			if (child is null || ReferenceEquals(child, navigationArea) || ReferenceEquals(child, _hostedBar))
			{
				continue;
			}

			child.Visibility = ViewStates.Gone;
		}
	}

	private void HostFlagstoneBar(LinearLayout outerLayout)
	{
		if (ShellContext.Shell is not FsShell shell || shell.TabBar is not { } bar)
		{
			return;
		}

		var mauiContext = shell.Handler?.MauiContext;
		if (mauiContext is null)
		{
			return;
		}

		var platformBar = bar.ToPlatform(mauiContext);

		// The same bar instance is shared across ShellItem switches; detach it from any previous
		// host before re-parenting. A platform view may only belong to one parent.
		(platformBar.Parent as ViewGroup)?.RemoveView(platformBar);

		platformBar.LayoutParameters = new LP(LP.MatchParent, LP.WrapContent);
		outerLayout.AddView(platformBar);
		_hostedBar = platformBar;

		ApplyBottomInsets(platformBar);
	}

	/// <summary>
	/// Pads the bar's bottom edge by the system gesture/navigation inset so its content clears the
	/// Android navigation bar. Consumers writing the item template do not deal with insets; the bar
	/// handles it at the container level.
	/// </summary>
	private static void ApplyBottomInsets(AView platformBar)
	{
		ViewCompat.SetOnApplyWindowInsetsListener(platformBar, new BottomInsetListener());
		if (platformBar.IsAttachedToWindow)
		{
			ViewCompat.RequestApplyInsets(platformBar);
		}
	}

	/// <summary>
	/// Keeps the native bar suppressed even after stock Shell re-runs its own visibility logic
	/// (e.g. on tab/section changes). Per-page show/hide of the FlagstoneUI bar is driven from the
	/// cross-platform layer via <c>Shell.SetTabBarIsVisible</c>, which toggles the bar's IsVisible.
	/// </summary>
	protected override void UpdateTabBarVisibility()
	{
		base.UpdateTabBarVisibility();

		if (_hostedBar?.Parent is LinearLayout outerLayout)
		{
			SuppressNativeBar(outerLayout);
		}
	}

	private sealed class BottomInsetListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
		{
			if (v is null || insets is null)
			{
				return insets;
			}

			var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.Ime());
			v.SetPadding(v.PaddingLeft, v.PaddingTop, v.PaddingRight, systemBars?.Bottom ?? 0);
			return insets;
		}
	}
}
