using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FlagstoneUI.SampleApp.ViewModels;

public class AppShellViewModel : INotifyPropertyChanged
{
    private string _selectedTheme = "Material";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                OnPropertyChanged();
                App.SwitchTheme(value);
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
