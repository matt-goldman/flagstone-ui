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
	public partial string ConversionResults { get; set; } = "Conversion log will appear here...";

	[ObservableProperty]
	public partial string ThemeName { get; set; }

	[ObservableProperty]
	public partial bool IncludeComments { get; set; }

	[ObservableProperty]
	public partial bool IncludeFonts { get; set; }

	private bool _downloadFonts;
	public bool DownloadFonts
	{
		get => _downloadFonts;
		set => SetProperty(ref _downloadFonts, value);
	}

	[ObservableProperty]
	public partial string TargetNamespace { get; set; } = "FlagstoneUI.Themes";

	public ObservableCollection<SourceFile> SelectedFiles { get; set; } = [];

	private bool _isFromUrl;
	public bool IsFromUrl
	{
		get => _isFromUrl;
		set
		{
			if (SetProperty(ref _isFromUrl, value))
			{
				OnPropertyChanged(nameof(IsFromFileComputed));
				OnPropertyChanged(nameof(IsFromFile));
			}
		}
	}

	public bool IsFromFileComputed => !IsFromUrl;

	public bool IsFromFile
	{
		get => !IsFromUrl;
		set => IsFromUrl = !value;
	}

	private string _newUrlInput = string.Empty;
	public string NewUrlInput
	{
		get => _newUrlInput;
		set => SetProperty(ref _newUrlInput, value);
	}

	private UIConversionResult? _lastConversionResult;
	public bool HasConversionResult => _lastConversionResult != null;

	[RelayCommand]
	public void SelectedFilesChanged()
	{
		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}

	[RelayCommand]
	public async Task AddFiles()
	{
		var files = await file.GetFilePaths();

		// Filter files that are not already in SelectedFiles
		var newFiles = files.Where(f => !SelectedFiles.Any(existing => existing.Path == f.Path));

		foreach (var f in newFiles)
		{
			SelectedFiles.Add(f);
		}

		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}

	[RelayCommand]
	public void AddUrl()
	{
		if (!string.IsNullOrWhiteSpace(NewUrlInput))
		{
			if (!SelectedFiles.Any(existing => existing.Path == NewUrlInput))
			{
				SelectedFiles.Add(new SourceFile(NewUrlInput));
			}
			NewUrlInput = string.Empty;
		}

		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}

	[RelayCommand]
	public void RemoveFile(SourceFile fileToRemove)
	{
		SelectedFiles.Remove(fileToRemove);
		IsConvertButtonEnabled = SelectedFiles.Count > 0;
	}

	[RelayCommand]
	public void ClearFiles()
	{
		SelectedFiles.Clear();
		IsConvertButtonEnabled = false;
	}


	public ObservableCollection<string> FsButtonStyleNames { get; set; } = [];

	public ObservableCollection<string> FsEntryStyleNames { get; set; } = [];

	public ObservableCollection<string> FsEditorStyleNames { get; set; } = [];

	public ObservableCollection<string> FsCardStyleNames { get; set; } = [];

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
			Inputs = [.. SelectedFiles.Select(static f => f.Path)],
			Format = BootstrapFormat,
			EnableDebugLogging = false,
			Strategy = AnalysisStrategy,
			Options = new ConversionOptions
			{
				DarkModeStrategy = DarkModeStrategy,
				IncludeComments = IncludeComments,
				IncludeFonts = IncludeFonts,
				Namespace = TargetNamespace,
				OutputFormat = OutputType
			}
		};

		var result = await bootstrapService.ConvertAsync(request);
		_lastConversionResult = result;
		ThemeName = result.ThemeName;

		// Update conversion results text
		ConversionResults = $"✅ Conversion successful!\n" +
			$"Theme: {result.ThemeName}\n\n";

		// Add font information if available
		if (result.Fonts != null && result.Fonts.HasFonts)
		{
			ConversionResults += "⚠️ Font Setup Required:\n\n";
			foreach (var family in result.Fonts.Families)
			{
				ConversionResults += $"Font: {family.Name}\n";
				ConversionResults += $"  Source: {family.Source}\n";
				if (family.Weights.Count > 0)
				{
					ConversionResults += $"  Weights: {string.Join(", ", family.Weights.OrderBy(w => w))}\n";
				}
				if (family.HasItalic)
				{
					ConversionResults += $"  Italic: Yes\n";
				}
				ConversionResults += $"  Suggested Alias: \"{family.SuggestedAlias}\"\n\n";
			}

			if (result.Fonts.DownloadUrls.Count > 0)
			{
				ConversionResults += "Download URLs:\n";
				foreach (var url in result.Fonts.DownloadUrls)
				{
					ConversionResults += $"  {url}\n";
				}
			}
		}

		OnPropertyChanged(nameof(HasConversionResult));

		// clear collections before clearing the current theme and adding a new one
		FsButtonStyleNames.Clear();
		FsEntryStyleNames.Clear();
		FsEditorStyleNames.Clear();
		FsCardStyleNames.Clear();

		themeService.ReloadThemes(result.Style);

		// get style names from the result resource dictionary
		foreach (var resource in result.Style)
		{
			if (resource.Value is Style style)
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
					if (style.TargetType == typeof(FsButton))
					{
						FsButtonStyleNames.Add(key);
					}
					else if (style.TargetType == typeof(FsEntry))
					{
						FsEntryStyleNames.Add(key);
					}
					else if (style.TargetType == typeof(FsEditor))
					{
						FsEditorStyleNames.Add(key);
					}
					else if (style.TargetType == typeof(FsCard))
					{
						FsCardStyleNames.Add(key);
					}
				}
			}
		}
	}

	[RelayCommand]
	public async Task SaveOutputAsync()
	{
		if (_lastConversionResult == null)
			return;

		try
		{
			ConversionResults += "\n\n💾 Preparing files for save...\n";

			// Generate XAML files
			var generator = new XamlThemeGenerator();
			var options = new ConversionOptions
			{
				DarkModeStrategy = DarkModeStrategy,
				IncludeComments = IncludeComments,
				IncludeFonts = IncludeFonts,
				Namespace = TargetNamespace,
				OutputFormat = OutputType
			};

			// Extract tokens from the resource dictionary
			var tokens = ExtractTokensFromResourceDictionary(_lastConversionResult.Style);

			// Generate XAML content
			var tokensXaml = generator.GenerateTokensXaml(tokens, options);
			var themeXaml = generator.GenerateThemeXaml(tokens, _lastConversionResult.ThemeName, options);
			var stylesXaml = generator.GenerateStylesXaml(tokens, _lastConversionResult.ThemeName, componentStyles: null, options);

			// Generate code-behind files
			var sanitizedThemeName = SanitizeThemeName(_lastConversionResult.ThemeName);
			var themeCodeBehind = generator.GenerateCodeBehind($"{TargetNamespace}.{sanitizedThemeName}", _lastConversionResult.ThemeName);
			var stylesCodeBehind = generator.GenerateCodeBehind($"{TargetNamespace}.{sanitizedThemeName}Styles", $"{_lastConversionResult.ThemeName} Styles");

			// Prepare files dictionary
			var files = new Dictionary<string, string>
			{
				["Tokens.xaml"] = tokensXaml,
				["Theme.xaml"] = themeXaml,
				["Theme.xaml.cs"] = themeCodeBehind,
				["Styles.xaml"] = stylesXaml,
				["Styles.xaml.cs"] = stylesCodeBehind
			};

			// Handle font downloads if requested
			if (DownloadFonts && _lastConversionResult.Fonts != null && _lastConversionResult.Fonts.HasFonts)
			{
				ConversionResults += "⬇️ Downloading fonts...\n";

				var fontFiles = await DownloadFontsAsync(_lastConversionResult.Fonts);
				foreach (var (fontFileName, fontData) in fontFiles)
				{
					// Convert byte array to base64 string for now (we'll save it as binary in the actual file)
					// For the dictionary, we'll just track that we have it
					ConversionResults += $"  ✓ Downloaded: {fontFileName}\n";
				}
			}

			// Save files
			ConversionResults += "💾 Saving files...\n";
			var saveResult = await file.SaveFilesAsync($"{sanitizedThemeName}_Theme", files);

			if (saveResult.IsSuccessful)
			{
				ConversionResults += $"\n✅ {saveResult.Message}\n\n";
				ConversionResults += "Saved files:\n";
				foreach (var savedFile in saveResult.SavedFiles ?? [])
				{
					ConversionResults += $"  • {savedFile}\n";
				}

				// Add font setup instructions if fonts are required
				if (_lastConversionResult.Fonts != null && _lastConversionResult.Fonts.HasFonts)
				{
					ConversionResults += "\n📝 Next Steps - Font Registration:\n\n";
					ConversionResults += "Add the following to your MauiProgram.cs:\n\n";
					ConversionResults += "builder.ConfigureFonts(fonts =>\n{\n";

					foreach (var family in _lastConversionResult.Fonts.Families.Where(f => f.Source != BootstrapConverter.Models.FontSource.System))
					{
						var fileName = $"{family.SuggestedAlias}-Regular.ttf";
						ConversionResults += $"    fonts.AddFont(\"{fileName}\", \"{family.SuggestedAlias}\");\n";
					}

					ConversionResults += "});\n";
				}
			}
			else
			{
				ConversionResults += $"\n❌ {saveResult.Message}\n";
			}
		}
		catch (Exception ex)
		{
			ConversionResults += $"\n\n❌ Error saving: {ex.Message}\n";
		}
	}

	private async Task<Dictionary<string, byte[]>> DownloadFontsAsync(FontInformation fontInfo)
	{
		var fontFiles = new Dictionary<string, byte[]>();

		foreach (var url in fontInfo.DownloadUrls)
		{
			try
			{
				var fontData = await file.DownloadFileAsync(url);
				if (fontData != null)
				{
					// Extract filename from URL
					var uri = new Uri(url);
					var fileName = Path.GetFileName(uri.LocalPath);
					if (string.IsNullOrWhiteSpace(fileName))
					{
						fileName = $"font_{fontFiles.Count}.ttf";
					}

					fontFiles[fileName] = fontData;
				}
			}
			catch
			{
				// Continue with other fonts if one fails
			}
		}

		return fontFiles;
	}

	private FlagstoneTokens ExtractTokensFromResourceDictionary(ResourceDictionary resourceDict)
	{
		var tokens = new FlagstoneTokens();

		foreach (var key in resourceDict.Keys)
		{
			var keyStr = key?.ToString() ?? "";
			var value = resourceDict[key];

			if (keyStr.StartsWith("Color.", StringComparison.Ordinal) && value is Color color)
			{
				tokens.Colors[keyStr] = new ColorToken
				{
					Key = keyStr,
					Value = color.ToHex(),
					Purpose = $"Extracted from theme"
				};
			}
			else if (keyStr.StartsWith("FontSize.", StringComparison.Ordinal) && value is double fontSize)
			{
				tokens.Typography[keyStr] = new TypographyToken
				{
					Key = keyStr,
					Value = fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
					Unit = "px",
					Purpose = $"Extracted from theme"
				};
			}
			else if (keyStr.StartsWith("Spacing.", StringComparison.Ordinal) && value is double spacing)
			{
				tokens.Spacing[keyStr] = new NumericToken
				{
					Key = keyStr,
					Value = spacing,
					Unit = "px",
					Purpose = $"Extracted from theme"
				};
			}
			else if (keyStr.StartsWith("Radius.", StringComparison.Ordinal) && value is double radius)
			{
				tokens.BorderRadius[keyStr] = new NumericToken
				{
					Key = keyStr,
					Value = radius,
					Unit = "px",
					Purpose = $"Extracted from theme"
				};
			}
			else if (keyStr.StartsWith("BorderWidth.", StringComparison.Ordinal) && value is double borderWidth)
			{
				tokens.BorderWidth[keyStr] = new NumericToken
				{
					Key = keyStr,
					Value = borderWidth,
					Unit = "px",
					Purpose = $"Extracted from theme"
				};
			}
		}

		return tokens;
	}

	private static string SanitizeThemeName(string themeName)
	{
		if (string.IsNullOrWhiteSpace(themeName))
			return "Theme";

		var sanitized = new System.Text.StringBuilder();
		var needsCapital = true;

		foreach (var ch in themeName)
		{
			if (char.IsLetterOrDigit(ch))
			{
				sanitized.Append(needsCapital ? char.ToUpper(ch) : ch);
				needsCapital = false;
			}
			else if (ch == '_')
			{
				sanitized.Append('_');
				needsCapital = false;
			}
			else
			{
				needsCapital = true;
			}
		}

		var result = sanitized.ToString();

		if (result.Length > 0 && char.IsDigit(result[0]))
			result = "_" + result;

		return string.IsNullOrEmpty(result) ? "Theme" : result;
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
