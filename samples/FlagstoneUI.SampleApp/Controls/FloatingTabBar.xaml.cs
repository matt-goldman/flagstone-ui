using CommunityToolkit.Maui.Core;
using FlagstoneUI.Core.Controls;

namespace FlagstoneUI.SampleApp.Controls;

public partial class FloatingTabBar : FsTabBarBase
{
	public FloatingTabBar()
	{
		InitializeComponent();
		InitializeTabContainer();
	}

	protected override Layout TabContainer => TabBar;

	void Expander_OnExpandedChanged(object? sender, ExpandedChangedEventArgs e)
	{
		if (e.IsExpanded)
		{
			_ = SparkleLabel.RotateToAsync(180);
		}
		else
		{
			_ = SparkleLabel.RotateToAsync(0);
		}
	}
}

