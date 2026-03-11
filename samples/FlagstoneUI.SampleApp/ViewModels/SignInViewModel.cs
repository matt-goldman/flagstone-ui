using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace FlagstoneUI.SampleApp.ViewModels;

public partial class SignInViewModel : INotifyPropertyChanged
{
	private string _email = string.Empty;
	public string Email
	{
		get => _email;
		set
		{
			if (_email != value)
			{
				_email = value;
				OnPropertyChanged();
			}
		}
	}

	private string _password = string.Empty;
	public string Password
	{
		get => _password;
		set
		{
			if (_password != value)
			{
				_password = value;
				OnPropertyChanged();
			}
		}
	}

	public ICommand SignInCommand => new Command(async () => await SignIn());

	public async Task SignIn()
	{
		// Implement sign-in logic here
		await App.Current.Windows[0].Page.DisplayAlertAsync("Sign In", $"Email: {Email}\nPassword: {Password}", "OK");
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
