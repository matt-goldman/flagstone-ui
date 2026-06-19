using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace FlagstoneUI.Core.Controls;

// Windows is the only platform where Shell uses the handler architecture (ShellHandler / ShellView)
// rather than the legacy renderer compatibility layer (ShellRenderer). The ShellItemHandler creates a
// MauiNavigationView with PaneDisplayMode.Top for tabs. This handler suppresses that top nav area and
// hosts the FsTabBar as a bottom overlay on the ShellView's root grid.
internal sealed partial class FsShellRenderer : ShellHandler
{
	private FrameworkElement? _hostedBar;
	private ContentView? _barContentView;
	private FsShell? _shell;
	private bool _barHosted;
	private StackPanel? _topNavArea;
	private NavigationView? _innerNavigationView;
	private long _paneDisplayModeCallbackToken;
	private long _contentChangedCallbackToken;

	protected override void ConnectHandler(ShellView platformView)
	{
		base.ConnectHandler(platformView);
		_shell = VirtualView as FsShell;

		_contentChangedCallbackToken = platformView.RegisterPropertyChangedCallback(
			ContentControl.ContentProperty, OnShellContentChanged);

		platformView.Loaded += OnShellViewLoaded;
	}

	protected override void DisconnectHandler(ShellView platformView)
	{
		platformView.UnregisterPropertyChangedCallback(
			ContentControl.ContentProperty, _contentChangedCallbackToken);

		platformView.Loaded -= OnShellViewLoaded;

		CleanupBar();
		_shell = null;

		base.DisconnectHandler(platformView);
	}

	private void OnShellViewLoaded(object sender, RoutedEventArgs e)
	{
		TryHostBar();
	}

	private void OnShellContentChanged(DependencyObject sender, DependencyProperty dp)
	{
		if (sender is not ShellView sv || sv.Content is not FrameworkElement innerNav) return;

		SuppressNativeTabs(innerNav);
		TryHostBar();
	}

	private void TryHostBar()
	{
		if (_barHosted || _shell?.TabBar is not { } bar) return;

		var mauiContext = MauiContext;
		if (mauiContext is null) return;

		var rootGrid = FindRootGrid(PlatformView);
		if (rootGrid is null) return;

		var platformBar = bar.ToPlatform(mauiContext);

		if (platformBar.Parent is Panel oldParent)
			oldParent.Children.Remove(platformBar);

		platformBar.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Bottom;
		rootGrid.Children.Add(platformBar);

		_hostedBar = platformBar;
		_barContentView = bar;
		_barHosted = true;
	}

	private void SuppressNativeTabs(FrameworkElement innerNav)
	{
		if (_innerNavigationView is not null && ReferenceEquals(_innerNavigationView, innerNav))
			return;

		if (_innerNavigationView is not null)
		{
			_innerNavigationView.UnregisterPropertyChangedCallback(
				NavigationView.PaneDisplayModeProperty, _paneDisplayModeCallbackToken);
		}

		if (innerNav.IsLoaded)
			CollapseTopNav(innerNav);
		else
			innerNav.Loaded += OnInnerNavLoaded;
	}

	private void OnInnerNavLoaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement fe)
		{
			fe.Loaded -= OnInnerNavLoaded;
			CollapseTopNav(fe);
		}
	}

	private void CollapseTopNav(FrameworkElement innerNav)
	{
		_topNavArea = FindByName<StackPanel>(innerNav, "TopNavArea");
		if (_topNavArea is not null)
			_topNavArea.Visibility = WVisibility.Collapsed;

		if (innerNav is NavigationView nv)
		{
			_innerNavigationView = nv;
			_paneDisplayModeCallbackToken = nv.RegisterPropertyChangedCallback(
				NavigationView.PaneDisplayModeProperty, OnPaneDisplayModeChanged);
		}
	}

	private void OnPaneDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
	{
		if (_topNavArea is not null)
			_topNavArea.Visibility = WVisibility.Collapsed;
	}

	private void CleanupBar()
	{
		if (_innerNavigationView is not null)
		{
			_innerNavigationView.UnregisterPropertyChangedCallback(
				NavigationView.PaneDisplayModeProperty, _paneDisplayModeCallbackToken);
			_innerNavigationView = null;
		}

		if (_hostedBar is { Parent: Panel parent })
		{
			parent.Children.Remove(_hostedBar);
		}

		_hostedBar = null;
		_barContentView = null;
		_topNavArea = null;
		_barHosted = false;
	}

	private static WGrid? FindRootGrid(FrameworkElement element)
	{
		var count = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < count; i++)
		{
			if (VisualTreeHelper.GetChild(element, i) is WGrid grid)
				return grid;
		}
		return null;
	}

	private static T? FindByName<T>(DependencyObject root, string name) where T : FrameworkElement
	{
		var count = VisualTreeHelper.GetChildrenCount(root);
		for (int i = 0; i < count; i++)
		{
			var child = VisualTreeHelper.GetChild(root, i);
			if (child is T element && element.Name == name)
				return element;

			var result = FindByName<T>(child, name);
			if (result is not null)
				return result;
		}
		return null;
	}
}
