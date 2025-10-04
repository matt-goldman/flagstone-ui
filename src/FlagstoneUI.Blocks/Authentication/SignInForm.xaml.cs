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
    public static BindableProperty UsernamePlaceholderProperty = BindableProperty.Create(
		nameof(UsernamePlaceholder), typeof(string), typeof(SigninForm), "Username", propertyChanged: UsernamePlaceholderPropertyChanged);
	/// <summary>
	/// Gets or sets the placeholder text displayed in the username input field.
	/// </summary>
	public string UsernamePlaceholder
    {
		get => (string)GetValue(UsernamePlaceholderProperty);
		set => SetValue(UsernamePlaceholderProperty, value);
    }
	private static void UsernamePlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is SigninForm signinForm && newValue is string newPlaceholder)
		{
			signinForm.UsernameEntry.Placeholder = newPlaceholder;
		}
	}
    #endregion

    public void OnSigninClicked(object sender, EventArgs e)
	{
		
    }
}