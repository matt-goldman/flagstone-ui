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
		=> new FsShellItemRenderer(this);
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

	public FsShellItemRenderer(IShellContext shellContext) : base(shellContext)
	{
		_shellContext = shellContext;
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

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
		ApplyBarInset();
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
			platformBar.TranslatesAutoresizingMaskIntoConstraints = false;
			view.AddSubview(platformBar);

			// Pinning to safeAreaLayoutGuide.bottomAnchor keeps the bar above the home indicator;
			// consumers writing the item template do not deal with insets.
			NSLayoutConstraint.ActivateConstraints(new[]
			{
				platformBar.LeadingAnchor.ConstraintEqualTo(view.LeadingAnchor),
				platformBar.TrailingAnchor.ConstraintEqualTo(view.TrailingAnchor),
				platformBar.BottomAnchor.ConstraintEqualTo(view.SafeAreaLayoutGuide.BottomAnchor),
			});
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
		if (_hostedBar is not { } bar)
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
		var inset = visible ? bar.Bounds.Height : 0;

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
		var translate = bar.Bounds.Height + safeBottom;

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
		var inset = visible ? bar.Bounds.Height : 0;

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
