using FlagstoneUI.SampleApp.ViewModels;

namespace FlagstoneUI.SampleApp.Pages;

public partial class ControlsShowcasePage : ContentPage
{
	public ControlsShowcasePage()
	{
		InitializeComponent();
		BindingContext = new ControlsShowcaseViewModel();
	}

	private async void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			await DisplayAlert("Button Clicked", $"You clicked: {button.Text}", "OK");
		}
	}

	private async void OnCardActionClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			await DisplayAlert("Card Action", $"You clicked: {button.Text}", "OK");
		}
	}

	private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		if (sender is Microsoft.Maui.Controls.Entry entry)
		{
			var message = $"Text changed: '{e.NewTextValue}' (Length: {e.NewTextValue?.Length ?? 0})";

			// Update both feedback labels (only one will be visible at a time)
			if (EntryFeedbackLabel != null)
			{
				EntryFeedbackLabel.Text = message;
			}
			if (EntryFeedbackLabelAlt != null)
			{
				EntryFeedbackLabelAlt.Text = message;
			}
		}
	}
}
