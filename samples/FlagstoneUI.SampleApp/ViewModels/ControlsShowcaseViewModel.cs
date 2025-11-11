using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlagstoneUI.SampleApp.ViewModels;

public class ControlsShowcaseViewModel : INotifyPropertyChanged
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

	public List<string> AvailableThemes { get; } = new()
	{
		"Material",
		"NovaPop",
		"SlatePro",
		"Litera",
		"Minty",
		"Brite"
	};

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
