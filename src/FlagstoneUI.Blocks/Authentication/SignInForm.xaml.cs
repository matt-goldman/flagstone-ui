using System.Windows.Input;

namespace FlagstoneUI.Blocks.Authentication;

public partial class SigninForm : ContentView
{
	public SigninForm()
	{
		InitializeComponent();
	}

	#region UsernamePlaceholder Property
	/// <summary>
	/// Identifies the bindable property for the username placeholder text.
	/// </summary>
	/// <remarks>This property defines the placeholder text displayed in the username input field of the sign-in
	/// form. The default value is "Username".</remarks>
	public static readonly BindableProperty UsernamePlaceholderProperty = BindableProperty.Create(
		nameof(UsernamePlaceholder), typeof(string), typeof(SigninForm), "Username");
	/// <summary>
	/// Gets or sets the placeholder text displayed in the username input field.
	/// </summary>
	public string UsernamePlaceholder
	{
		get => (string)GetValue(UsernamePlaceholderProperty);
		set => SetValue(UsernamePlaceholderProperty, value);
	}
	#endregion

	#region PasswordPlaceholder Property
	/// <summary>
	/// Identifies the bindable property for the password placeholder text.
	/// </summary>
	/// <remarks>This property defines the placeholder text displayed in the password input field of the sign-in
	/// form. The default value is "Password".</remarks>
	public static readonly BindableProperty PasswordPlaceholderProperty = BindableProperty.Create(
		nameof(PasswordPlaceholder), typeof(string), typeof(SigninForm), "Password");
	/// <summary>
	/// Gets or sets the placeholder text displayed in the password input field.
	/// </summary>
	public string PasswordPlaceholder
	{
		get => (string)GetValue(PasswordPlaceholderProperty);
		set => SetValue(PasswordPlaceholderProperty, value);
	}
	#endregion

	#region Username Property
	/// <summary>
	/// Identifies the bindable property for the username text.
	/// </summary>
	/// <remarks>This property defines the text displayed in the username input field of the sign-in
	/// form. The default value is an empty string.</remarks>
	public static readonly BindableProperty UsernameProperty = BindableProperty.Create(
		nameof(Username), typeof(string), typeof(SigninForm), string.Empty, BindingMode.TwoWay);
	/// <summary>
	/// Gets or sets the text displayed in the username input field.
	/// </summary>
	public string Username
	{
		get => (string)GetValue(UsernameProperty);
		set => SetValue(UsernameProperty, value);
	}
	#endregion

	#region Password Property
	/// <summary>
	/// Identifies the bindable property for the password text.
	/// </summary>
	/// <remarks>This property defines the text displayed in the password input field of the sign-in
	/// form. The default value is an empty string.</remarks>
	public static readonly BindableProperty PasswordProperty = BindableProperty.Create(
		nameof(Password), typeof(string), typeof(SigninForm), string.Empty, BindingMode.TwoWay);
	/// <summary>
	/// Gets or sets the text displayed in the password input field.
	/// </summary>
	public string Password
	{
		get => (string)GetValue(PasswordProperty);
		set => SetValue(PasswordProperty, value);
	}
	#endregion

	#region SigninCommand Property
	/// <summary>
	/// Identifies the bindable property for the sign-in command.
	/// </summary>
	/// <remarks>This property defines the command executed when the sign-in button is clicked.</remarks>
	public static readonly BindableProperty SigninCommandProperty = BindableProperty.Create(
		nameof(SigninCommand), typeof(ICommand), typeof(SigninForm), null);
	/// <summary>
	/// Gets or sets the command executed when the sign-in button is clicked.
	/// </summary>
	public ICommand? SigninCommand
	{
		get => (ICommand?)GetValue(SigninCommandProperty);
		set => SetValue(SigninCommandProperty, value);
	}
	#endregion

	#region SignInClicked Event
	/// <summary>
	/// Occurs when the sign-in button is clicked.
	/// </summary>
	public event EventHandler? SignInClicked;
	#endregion

	#region SignInButtonText Property
	/// <summary>
	/// Identifies the bindable property for the sign-in button text.
	/// </summary>
	/// <remarks>This property defines the text displayed on the sign-in button. The default value is "Sign In".</remarks>
	public static readonly BindableProperty SignInButtonTextProperty = BindableProperty.Create(
		nameof(SignInButtonText), typeof(string), typeof(SigninForm), "Sign In");
	/// <summary>
	/// Gets or sets the text displayed on the sign-in button.
	/// </summary>
	public string SignInButtonText
	{
		get => (string)GetValue(SignInButtonTextProperty);
		set => SetValue(SignInButtonTextProperty, value);
	}
	#endregion

	public void OnSigninClicked(object sender, EventArgs e)
	{
		SignInClicked?.Invoke(this, e);
		SigninCommand?.Execute(null);
	}
}
