using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.UI.ViewModels;

public partial class MainViewModel(
	IFileService file,
	IBootstrapService bootstrapService,
	IThemeService themeService) : ObservableObject
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

	[ObservableProperty]
	public partial string TargetNamespace { get; set; } = "FlagstoneUI.Themes";

	public ObservableCollection<SourceFile> SelectedFiles { get; set; } = [];
	[RelayCommand]
	public void SelectedFilesChanged()
	{
		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}


	[ObservableProperty]
	public partial bool IsFromUrl { get; set; }

	[ObservableProperty]
	public partial string IsFromFile { get; set; }

	public ObservableCollection<string> FsButtonStyleNames { get; set; } = [];

	public bool IsConvertButtonEnabled
	{
		get => field = SelectedFiles.Count > 0;
		set
		{
			SetProperty(ref field, value);
		}
	}

	[RelayCommand]
	public async Task ConvertThemeAsync()
	{
		var request = new ConversionRequest
		{
			Inputs				= [ ..SelectedFiles.Select(static f => f.Path)],
			Format				= BootstrapFormat,
			EnableDebugLogging	= false,
			Strategy			= AnalysisStrategy,
			Options				= new ConversionOptions
			{
				DarkModeStrategy	= DarkModeStrategy,
				IncludeComments		= IncludeComments,
				IncludeFonts		= IncludeFonts,
				Namespace			= TargetNamespace,
				OutputFormat		= OutputType
			}
		};

		var result = await bootstrapService.ConvertAsync(request);
		ThemeName = result.ThemeName;

		// clear collection before clearing the current theme and adding a new one
		FsButtonStyleNames.Clear();

		themeService.ReloadThemes(result.Style);

		// get button style names from the result resource dictionary
		foreach (var resource in result.Style)
		{
			if (resource.Value is Style style)
			{
				if (style.TargetType == typeof(FsButton))
				{
					string key = "default";
					
					try
					{
						key = resource.Key.ToString();
					}
					catch (Exception)
					{
						// no-op
					}

					if (key != null)
					{
						FsButtonStyleNames.Add(key);
					}
				}
			}
		}
	}

	[RelayCommand]
	public async Task SelectInputFiles()
	{
		var files = await file.GetFilePaths();
		
		SelectedFiles.Clear();
		
		foreach (var f in files)
		{
			SelectedFiles.Add(f);
		}

		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}

}
