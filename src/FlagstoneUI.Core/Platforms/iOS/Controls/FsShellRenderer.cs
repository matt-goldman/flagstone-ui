using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace FlagstoneUI.Core.Controls;

/// <summary>
/// iOS-specific Shell renderer for <see cref="FsShell"/>. Suppresses the native
/// <c>UITabBar</c> and hosts <see cref="FsShell.TabBar"/> as a subview of the tab controller's
/// root view.
/// </summary>
/// <remarks>
/// <para>
/// The renderer's responsibilities are intentionally narrow: hide the stock <c>UITabBar</c>, add
/// the hosted bar to the view, keep it pinned to the bottom edge, and slide it out of the way
/// when the keyboard appears (when <see cref="FsShell.HideTabBarOnKeyboard"/> is set).
/// </para>
/// <para>
/// Reserving room for the bar inside page content is deliberately <em>not</em> handled here —
/// the cross-platform layer publishes the bar's measured height under
/// <see cref="FsShell.BottomChromeHeightResourceKey"/> and consumers opt in via
/// <see cref="FsLayout.BottomChromePaddingProperty"/> (or by reading the resource directly).
/// That keeps this file small and avoids fighting MAUI's iOS page layout for safe-area control.
/// </para>
/// </remarks>
internal sealed partial class FsShellRenderer : ShellRenderer
{
	protected override IShellItemRenderer CreateShellItemRenderer(ShellItem item)
		=> new FsShellItemRenderer(this) { ShellItem = item };
}

/// <summary>
/// Replaces the native <c>UITabBar</c> chrome for a single <c>ShellItem</c> with the FlagstoneUI
/// bar hosted from <see cref="FsShell.TabBar"/>.
/// </summary>
internal sealed class FsShellItemRenderer(IShellContext shellContext) : ShellItemRenderer(shellContext)
{
	private readonly IShellContext _shellContext = shellContext;
	private UIView? _hostedBar;
	private ContentView? _barContentView;
	private FsShell? _shell;
	private NSObject? _kbShowToken;
	private NSObject? _kbHideToken;
	private bool _keyboardActive;
	private bool _viewLoaded;
	private nfloat _cachedBarHeight;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		_viewLoaded = true;
		TrySetupBar();
	}

	protected override void OnShellItemSet(ShellItem shellItem)
	{
		base.OnShellItemSet(shellItem);

		// If ViewDidLoad already fired but ShellItem hadn't been assigned yet (UIKit can load the
		// view between renderer construction and the ShellItem assignment), complete setup now.
		if (_viewLoaded)
		{
			TrySetupBar();
		}
	}

	private void TrySetupBar()
	{
		if (_hostedBar is not null)
		{
			return;
		}

		if ((ShellItem?.Items?.Count ?? 0) <= 1)
		{
			return;
		}

		SuppressNativeBar();
		HostFlagstoneBar();

		if (_shell?.TabBarIsDocked ?? true)
		{
			HookKeyboard();
		}
	}

	public override void ViewWillLayoutSubviews()
	{
		base.ViewWillLayoutSubviews();

		// Stock Shell re-runs its own visibility logic on tab/section changes; keep the native
		// bar suppressed across those re-layouts, analogous to Android's UpdateTabBarVisibility
		// override.
		if (_hostedBar is not null)
		{
			TabBar.Hidden = true;
		}
	}

	public override void ViewDidLayoutSubviews()
	{
		base.ViewDidLayoutSubviews();
		LayoutBar();

		// UIKit inserts child view-controller views (the tab content) as subviews after we
		// host our bar in ViewDidLoad, which would leave the bar behind page content. Re-raise
		// to the front on every layout pass; cheap when already topmost.
		if (_hostedBar is { } bar && bar.Superview is { } sup)
		{
			sup.BringSubviewToFront(bar);
		}
	}

	/// <summary>
	/// Measures the bar's content, expands the arranged height to also cover the device's bottom
	/// safe-area inset, and pins the resulting frame to the bottom of the view. Driven imperatively
	/// rather than via auto-layout because the platform ContentView reports a zero intrinsic size
	/// until .NET MAUI has measured its children, which would otherwise collapse the bar to zero
	/// height. Including the safe-area inset in the arranged height means MAUI sees the bar as the
	/// full chrome height pages need to reserve, so the cross-platform
	/// <see cref="FsShell.BottomChromeHeightResourceKey"/> publication matches the bar's actual
	/// visual footprint (including the home-indicator area on devices that have one).
	/// </summary>
	private void LayoutBar()
	{
		if (_hostedBar is not { } bar || _barContentView is not { } cv || View is not { } view)
		{
			return;
		}

		var width = view.Bounds.Width;
		if (width <= 0)
		{
			return;
		}

		if (_shell is { TabBarIsDocked: false })
		{
			var fullFrame = view.Bounds;
			if (bar.Frame != fullFrame)
			{
				bar.Frame = fullFrame;
			}
			return;
		}

		if (_keyboardActive)
		{
			return;
		}

		const double MaxBarHeight = 240;

		if (_cachedBarHeight <= 0)
		{
			var measured = ((IView)cv).Measure(width, MaxBarHeight);
			var contentHeight = measured.Height;
			if (contentHeight <= 0 || double.IsInfinity(contentHeight) || double.IsNaN(contentHeight))
			{
				return;
			}
			contentHeight = Math.Min(contentHeight, MaxBarHeight);

			var safeBottom = view.SafeAreaInsets.Bottom;
			_cachedBarHeight = (nfloat)(contentHeight + safeBottom);
			((IView)cv).Arrange(new Rect(0, 0, width, _cachedBarHeight));
		}

		var height = _cachedBarHeight;
		var y = view.Bounds.Height - height;
		var newFrame = new CGRect(0, y, width, height);
		if (bar.Frame != newFrame)
		{
			bar.Frame = newFrame;
		}

		if (_shell?.TabBarIsDocked is null or true && Application.Current is { } app)
		{
			app.Resources[FsShell.BottomChromeHeightResourceKey] = (double)height;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			UnhookKeyboard();

			// The bar is a single shared instance hosted into the active item's view controller.
			// Release it before this controller is torn down — unless a newer renderer has already
			// re-parented it — so it survives to be re-hosted by the next item.
			if (_hostedBar is { } bar && ReferenceEquals(bar.Superview, View))
			{
				bar.RemoveFromSuperview();
			}

			_hostedBar = null;
			_barContentView = null;
			_shell = null;
		}

		base.Dispose(disposing);
	}

	private void SuppressNativeBar()
	{
		TabBar.Hidden = true;
	}

	private void HostFlagstoneBar()
	{
		_shell = _shellContext.Shell as FsShell;
		if (_shell?.TabBar is not { } bar)
		{
			return;
		}

		var mauiContext = _shell.Handler?.MauiContext;
		if (mauiContext is null)
		{
			return;
		}

		var platformBar = bar.ToPlatform(mauiContext);

		// The same bar instance is shared across ShellItem switches; detach it from any previous
		// host before re-parenting. A UIView may only belong to one superview.
		var view = View;
		if (view is null)
		{
			return;
		}

		if (platformBar.Superview is { } prev && !ReferenceEquals(prev, view))
		{
			platformBar.RemoveFromSuperview();
		}

		if (platformBar.Superview is null)
		{
			// The frame is set imperatively in LayoutBar; using autoresizing-mask coordinates
			// keeps the bar in lockstep with our explicit frame writes without auto-layout fighting
			// for control.
			platformBar.TranslatesAutoresizingMaskIntoConstraints = true;
			view.AddSubview(platformBar);
		}

		_hostedBar = platformBar;
		_barContentView = bar;
	}

	private void HookKeyboard()
	{
		_kbShowToken = NSNotificationCenter.DefaultCenter.AddObserver(
			UIKeyboard.WillShowNotification, OnKeyboardWillShow);
		_kbHideToken = NSNotificationCenter.DefaultCenter.AddObserver(
			UIKeyboard.WillHideNotification, OnKeyboardWillHide);
	}

	private void UnhookKeyboard()
	{
		if (_kbShowToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(_kbShowToken);
			_kbShowToken.Dispose();
			_kbShowToken = null;
		}

		if (_kbHideToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(_kbHideToken);
			_kbHideToken.Dispose();
			_kbHideToken = null;
		}
	}

	private void OnKeyboardWillShow(NSNotification notification)
	{
		if (_shell is not { HideTabBarOnKeyboard: true } || _hostedBar is not { } bar)
		{
			return;
		}

		var (duration, curve) = ReadAnimationInfo(notification);
		var safeBottom = View?.SafeAreaInsets.Bottom ?? 0;
		var translate = _cachedBarHeight + safeBottom;

		_keyboardActive = true;

		UIView.AnimateNotify(duration, 0, curve, () =>
		{
			bar.Transform = CGAffineTransform.MakeTranslation(0, translate);
		}, null);
	}

	private void OnKeyboardWillHide(NSNotification notification)
	{
		if (_shell is not { HideTabBarOnKeyboard: true } || _hostedBar is not { } bar)
		{
			return;
		}

		var (duration, curve) = ReadAnimationInfo(notification);
		_keyboardActive = false;

		UIView.AnimateNotify(duration, 0, curve, () =>
		{
			bar.Transform = CGAffineTransform.MakeIdentity();
		}, null);
	}

	private static (double Duration, UIViewAnimationOptions Curve) ReadAnimationInfo(NSNotification notification)
	{
		var duration = 0.25;
		var curve = UIViewAnimationOptions.CurveEaseInOut;

		var info = notification.UserInfo;
		if (info is null)
		{
			return (duration, curve);
		}

		if (info[UIKeyboard.AnimationDurationUserInfoKey] is NSNumber d)
		{
			duration = d.DoubleValue;
		}

		// UIView.AnimationCurve raw values (0–3) shift to the high 16 bits of UIViewAnimationOptions;
		// UIKit may also send a private curve value (e.g. 7) that the same shift converts cleanly.
		if (info[UIKeyboard.AnimationCurveUserInfoKey] is NSNumber c)
		{
			curve = (UIViewAnimationOptions)(c.UInt32Value << 16);
		}

		return (duration, curve);
	}
}
