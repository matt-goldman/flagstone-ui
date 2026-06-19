using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRowDefinition = Microsoft.UI.Xaml.Controls.RowDefinition;
using WGridLength = Microsoft.UI.Xaml.GridLength;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace FlagstoneUI.Core.Controls;

// Windows is the only platform where Shell uses the handler architecture (ShellHandler / ShellView)
// rather than the legacy renderer compatibility layer (ShellRenderer). The ShellItemHandler creates a
// MauiNavigationView with PaneDisplayMode.Top for tabs. This handler suppresses that top nav area and
// hosts the FsTabBar at the bottom of the ShellItemHandler's ContentGrid.
internal sealed partial class FsShellRenderer : ShellHandler
{
	private FrameworkElement? _hostedBar;
	private ContentView? _barContentView;
	private FsShell? _shell;
	private bool _barSetupComplete;
	private StackPanel? _topNavArea;
	private NavigationView? _innerNavigationView;
	private FrameworkElement? _pendingInnerNav;
	private long _paneDisplayModeCallbackToken;
	private long _contentChangedCallbackToken;

	protected override void ConnectHandler(ShellView platformView)
	{
		base.ConnectHandler(platformView);
		_shell = VirtualView as FsShell;

		_contentChangedCallbackToken = platformView.RegisterPropertyChangedCallback(
			ContentControl.ContentProperty, OnShellContentChanged);

		if (platformView.Content is FrameworkElement existingContent)
			SubscribeInnerNav(existingContent);
	}

	protected override void DisconnectHandler(ShellView platformView)
	{
		platformView.UnregisterPropertyChangedCallback(
			ContentControl.ContentProperty, _contentChangedCallbackToken);

		CleanupBar();
		_shell = null;

		base.DisconnectHandler(platformView);
	}

	private void OnShellContentChanged(DependencyObject sender, DependencyProperty dp)
	{
		if (_barSetupComplete) return;
		if (sender is not ShellView sv || sv.Content is not FrameworkElement innerNav) return;

		SubscribeInnerNav(innerNav);
	}

	private void SubscribeInnerNav(FrameworkElement innerNav)
	{
		if (_barSetupComplete) return;

		if (innerNav.IsLoaded)
		{
			SetupBar(innerNav);
		}
		else
		{
			_pendingInnerNav = innerNav;
			innerNav.Loaded += OnInnerNavLoaded;
		}
	}

	private void OnInnerNavLoaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement innerNav)
		{
			innerNav.Loaded -= OnInnerNavLoaded;
			_pendingInnerNav = null;
			SetupBar(innerNav);
		}
	}

	private void SetupBar(FrameworkElement innerNav)
	{
		if (_barSetupComplete || _shell?.TabBar is not { } bar) return;

		var mauiContext = MauiContext;
		if (mauiContext is null) return;

		SuppressNativeTabs(innerNav);
		HostBar(innerNav, bar, mauiContext);
	}

	private void SuppressNativeTabs(FrameworkElement innerNav)
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

	private void HostBar(FrameworkElement innerNav, ContentView bar, IMauiContext mauiContext)
	{
		var contentGrid = FindByName<WGrid>(innerNav, "ContentGrid");
		if (contentGrid is null) return;

		var platformBar = bar.ToPlatform(mauiContext);

		if (platformBar.Parent is Panel oldParent)
			oldParent.Children.Remove(platformBar);

		contentGrid.RowDefinitions.Add(new WRowDefinition { Height = WGridLength.Auto });
		var newRow = contentGrid.RowDefinitions.Count - 1;
		WGrid.SetRow(platformBar, newRow);

		if (contentGrid.ColumnDefinitions.Count > 1)
			WGrid.SetColumnSpan(platformBar, contentGrid.ColumnDefinitions.Count);

		contentGrid.Children.Add(platformBar);

		_hostedBar = platformBar;
		_barContentView = bar;
		_barSetupComplete = true;
	}

	private void CleanupBar()
	{
		if (_pendingInnerNav is not null)
		{
			_pendingInnerNav.Loaded -= OnInnerNavLoaded;
			_pendingInnerNav = null;
		}

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
		_barSetupComplete = false;
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
