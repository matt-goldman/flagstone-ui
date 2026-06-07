using System.ComponentModel;
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
/// <c>UITabBar</c> and hosts an <see cref="FsTabBar"/> (or the consumer-supplied bar)
/// in its place.
/// </summary>
/// <remarks>
/// Stock Shell builds each <c>ShellItem</c> as a <c>UITabBarController</c> with a native
/// <c>UITabBar</c> mounted at the bottom. This renderer overrides
/// <see cref="ShellRenderer.CreateShellItemRenderer"/> to return an <see cref="FsShellItemRenderer"/>
/// that hides the native bar and adds the FlagstoneUI bar's platform view as a subview of the
/// tab controller's root view, pinned to the bottom safe-area, with the child view controllers'
/// content shrunk by the bar's height via <c>AdditionalSafeAreaInsets</c>.
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
internal sealed class FsShellItemRenderer : ShellItemRenderer
{
	private readonly IShellContext _shellContext;
	private UIView? _hostedBar;
	private ContentView? _barContentView;
	private FsShell? _shell;
	private NSObject? _kbShowToken;
	private NSObject? _kbHideToken;
	private nfloat _appliedInset;
	private bool _keyboardActive;
	private bool _viewLoaded;
	private nfloat _cachedBarHeight;
	private nfloat _cachedForWidth;
	private bool _inLayout;

	public FsShellItemRenderer(IShellContext shellContext) : base(shellContext)
	{
		_shellContext = shellContext;
	}

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

		// Only intervene when this item actually has bottom tabs to replace — i.e. more than one
		// section, exactly the condition under which stock Shell shows a UITabBar. Items with a
		// single section (e.g. a plain flyout page) are left entirely on stock rendering, so the
		// FlagstoneUI bar never sits in a controller that has no bottom bar to begin with.
		if ((ShellItem?.Items?.Count ?? 0) <= 1)
		{
			return;
		}

		SuppressNativeBar();
		HostFlagstoneBar();
		HookKeyboard();
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
		ApplyBarInset();
	}

	/// <summary>
	/// Measures the bar against the available width and positions it along the bottom safe area
	/// edge. Driven imperatively rather than via auto-layout because the platform ContentView
	/// reports a zero intrinsic size until .NET MAUI has measured its children, which would otherwise
	/// collapse the bar to zero height. The measured height is cached per width so re-layouts
	/// triggered by AdditionalSafeAreaInsets changes don't re-enter Measure with the bar's just-
	/// arranged height as a hint, which would feed back as the new "desired" size on each pass.
	/// </summary>
	private void LayoutBar()
	{
		if (_hostedBar is not { } bar || _barContentView is not { } cv || View is not { } view)
		{
			return;
		}

		if (_keyboardActive || _inLayout)
		{
			return;
		}

		var width = view.Bounds.Width;
		if (width <= 0)
		{
			return;
		}

		// Cap measure height so a Fill-defaulting ContentView doesn't return the entire available
		// space when given an unbounded constraint. 240pt is comfortably above any realistic tab-bar
		// content height (stock iOS tab bar is ~83pt with home indicator).
		const double MaxBarHeight = 240;

		if (_cachedBarHeight <= 0 || Math.Abs(_cachedForWidth - width) > 0.5)
		{
			_inLayout = true;
			try
			{
				var measured = ((IView)cv).Measure(width, MaxBarHeight);
				var h = measured.Height;
				if (h <= 0 || double.IsInfinity(h) || double.IsNaN(h))
				{
					return;
				}
				_cachedBarHeight = (nfloat)Math.Min(h, MaxBarHeight);
				_cachedForWidth = (nfloat)width;
			}
			finally
			{
				_inLayout = false;
			}
		}

		var height = _cachedBarHeight;
		var safeBottom = view.SafeAreaInsets.Bottom - _appliedInset;
		if (safeBottom < 0)
		{
			safeBottom = 0;
		}

		var y = view.Bounds.Height - safeBottom - height;
		_inLayout = true;
		try
		{
			// IView.Arrange routes through the handler which sets the platform frame in one shot, so
			// drive position and size together rather than setting Frame and then having Arrange reset
			// the origin back to (0, 0).
			((IView)cv).Arrange(new Rect(0, y, width, height));
		}
		finally
		{
			_inLayout = false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			UnhookKeyboard();

			if (_barContentView is { } cv)
			{
				cv.PropertyChanged -= OnBarPropertyChanged;
			}

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
			// Manage the frame imperatively in ViewDidLayoutSubviews rather than relying on auto-
			// layout + intrinsic content size. The platform ContentView's intrinsic size reports
			// zero until .NET MAUI has measured its children, so an auto-layout-only setup collapses to
			// zero height. Driving the frame from a .NET MAUI Measure call mirrors the Android renderer's
			// approach (LinearLayout with WRAP_CONTENT) and stays predictable.
			platformBar.TranslatesAutoresizingMaskIntoConstraints = true;
			view.AddSubview(platformBar);
		}

		_hostedBar = platformBar;
		_barContentView = bar;
		bar.PropertyChanged += OnBarPropertyChanged;
	}

	private void OnBarPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		// IsVisible flips when FsShell.BridgeTabBarVisibility responds to Shell.SetTabBarIsVisible
		// or to the tab count dropping to ≤1. When hidden we drop the safe-area inset so child
		// content fills the screen with no dead gap at the bottom.
		if (e.PropertyName == nameof(VisualElement.IsVisible))
		{
			ApplyBarInset();
		}
	}

	private void ApplyBarInset()
	{
		if (_hostedBar is null)
		{
			return;
		}

		// While the keyboard is up we deliberately want zero inset so the page can expand into the
		// space the bar vacated; the keyboard handlers own the inset until they restore it.
		if (_keyboardActive)
		{
			return;
		}

		var visible = _barContentView?.IsVisible ?? false;
		// Drive inset from the cached measured height rather than bar.Bounds.Height, because the
		// bar's bounds reflect the last Arrange call — using them creates a feedback loop where each
		// inset change triggers a re-layout that re-arranges the bar with the inset-affected height.
		var inset = visible ? _cachedBarHeight : 0;

		if (inset == _appliedInset)
		{
			return;
		}

		_appliedInset = inset;
		AdditionalSafeAreaInsets = new UIEdgeInsets(0, 0, inset, 0);
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
		_appliedInset = 0;

		UIView.AnimateNotify(duration, 0, curve, () =>
		{
			bar.Transform = CGAffineTransform.MakeTranslation(0, translate);
			AdditionalSafeAreaInsets = new UIEdgeInsets(0, 0, 0, 0);
		}, null);
	}

	private void OnKeyboardWillHide(NSNotification notification)
	{
		if (_shell is not { HideTabBarOnKeyboard: true } || _hostedBar is not { } bar)
		{
			return;
		}

		var (duration, curve) = ReadAnimationInfo(notification);
		var visible = _barContentView?.IsVisible ?? false;
		var inset = visible ? _cachedBarHeight : 0;

		_keyboardActive = false;
		_appliedInset = inset;

		UIView.AnimateNotify(duration, 0, curve, () =>
		{
			bar.Transform = CGAffineTransform.MakeIdentity();
			AdditionalSafeAreaInsets = new UIEdgeInsets(0, 0, inset, 0);
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
