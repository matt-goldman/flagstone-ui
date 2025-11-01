namespace FlagstoneUI.SampleApp.Pages;

public partial class ControlsShowcasePage : ContentPage
{
	public ControlsShowcasePage()
	{
		InitializeComponent();
	}

	private async void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			await DisplayAlertAsync("Button Clicked", $"You clicked: {button.Text}", "OK");
		}
	}

	private async void OnCardActionClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			await DisplayAlertAsync("Card Action", $"You clicked: {button.Text}", "OK");
		}
	}

	private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is Microsoft.Maui.Controls.Entry entry)
		{
			EntryFeedbackLabel.Text = $"Text changed: '{e.NewTextValue}' (Length: {e.NewTextValue?.Length ?? 0})";
		}
	}
}
