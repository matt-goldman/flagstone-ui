using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlagstoneUI.SampleApp.ViewModels;

public partial class ControlsShowcaseViewModel : INotifyPropertyChanged
{
	private string _selectedTheme = "Material";

	public event PropertyChangedEventHandler? PropertyChanged;

	public string SelectedTheme
	{
		get => _selectedTheme;
		set
		{
			if (_selectedTheme == value)
			{
				return;
			}

			_selectedTheme = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsMaterialTheme));
			OnPropertyChanged(nameof(IsNotMaterialTheme));
			App.SwitchTheme(value);
		}
	}

	public bool IsMaterialTheme => SelectedTheme == "Material";
	public bool IsNotMaterialTheme => !IsMaterialTheme;

	public List<string> AvailableThemes { get; } =
	[
		"Material",
		"NovaPop",
		"SlatePro",
		"Litera",
		"Minty",
		"Brite"
	];

	private string _entryText = string.Empty;
	public string EntryText
	{
		get => _entryText;
		set
		{
			if (_entryText == value)
			{
				return;
			}
			_entryText = value;
			OnPropertyChanged();
		}
	}

	private string _editorText = string.Empty;
	public string EditorText
	{
		get => _editorText;
		set
		{
			if (_editorText == value)
			{
				return;
			}
			_editorText = value;
			OnPropertyChanged();
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
