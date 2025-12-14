using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FlagstoneUI.BootstrapConverter.Models;
using FlagstoneUI.BootstrapConverter.UI.Services;
using static FlagstoneUI.BootstrapConverter.BootstrapConverterService;

namespace FlagstoneUI.BootstrapConverter.UI.ViewModels;

public partial class MainViewModel(IFileService file, IBootstrapService bootstrapService) : ObservableObject
{
	public ObservableCollection<ResourceDictionaryFormat> ResourceDictionaryFormats { get; set; } = 
	[
		ResourceDictionaryFormat.CSharp,
		ResourceDictionaryFormat.Xaml,
	];

	[ObservableProperty]
	public partial ResourceDictionaryFormat OutputType { get; set; }


	public ObservableCollection<DarkModeStrategy> DarkModeStrategies { get; set; } =
	[
			DarkModeStrategy.Auto,
			DarkModeStrategy.Manual,
		DarkModeStrategy.None

	];

	[ObservableProperty]
	public partial DarkModeStrategy DarkModeStrategy { get; set; }

	
	public ObservableCollection<AnalysisStrategy> AnalysisStrategies { get; set; } =
	[
		AnalysisStrategy.CssOnly,
		AnalysisStrategy.Hybrid,
		AnalysisStrategy.VariablesOnly
	];

	[ObservableProperty]
	public partial AnalysisStrategy AnalysisStrategy { get; set; }


	public ObservableCollection<BootstrapFormat> BootstrapFormats { get; set; } =
	[
		BootstrapFormat.Auto,
		BootstrapFormat.Css,
		BootstrapFormat.Scss
	];

	[ObservableProperty]
	public partial BootstrapFormat BootstrapFormat { get; set; }



	[ObservableProperty]
	public partial string ConversionResults { get; set; }

	[ObservableProperty]
	public partial string ThemeName { get; set; }

	[ObservableProperty]
	public partial bool IncludeComments { get; set; }

	[ObservableProperty]
	public partial bool IncludeFonts { get; set; }

	public ObservableCollection<string> SelectedFiles { get; set; } = [];

	public ObservableCollection<string> FsButtonStyleNames { get; set; } = [];

}
