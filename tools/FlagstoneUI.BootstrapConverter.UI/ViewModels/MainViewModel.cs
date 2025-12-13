using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FlagstoneUI.BootstrapConverter.Models;
using FlagstoneUI.BootstrapConverter.UI.Services;
using static FlagstoneUI.BootstrapConverter.BootstrapConverterService;

namespace FlagstoneUI.BootstrapConverter.UI.ViewModels;

public partial class MainViewModel(IFileService file, IBootstrapService bootstrapService) : ObservableObject
{
	public ObservableCollection<string> SelectedFiles { get; set; } = [];

	[ObservableProperty]
	public partial string ThemeName { get; set; }

	[ObservableProperty]
	public partial bool IncludeComments { get; set; }

	[ObservableProperty]
	public partial bool IncludeFonts { get; set; }

	[ObservableProperty]
	public partial ResourceDictionaryFormat OutputType { get; set; }

	[ObservableProperty]
	public partial DarkModeStrategy DarkModeStrategy { get; set; }

	[ObservableProperty]
	public partial AnalysisStrategy AnalyisStrategy { get; set; }

	[ObservableProperty]
	public partial BootstrapFormat BootstrapFormat { get; set; }

	[ObservableProperty]
	public partial string ConversionResults { get; set; }



}
