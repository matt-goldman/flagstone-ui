using FlagstoneUI.Integrations.MCT.Animations;
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

	readonly FsEditorBorderAnimation _animation = new FsEditorBorderAnimation
	{
		Length = 2000,
		Gradient = App.Current!.Resources["AiGradientBrush"] as LinearGradientBrush,
	};

	private CancellationTokenSource? _cts;
	
	async void AiEditor_OnFocused(object? sender, EventArgs e)
	{
		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		try
		{
			await _animation.Animate(AiEditor, _cts.Token);
		}
		catch (OperationCanceledException)
		{
			// Animation was cancelled, no action needed
		}
	}
	
	async void AiEditor_OnUnfocused(object? sender, EventArgs e)
	{
		await _cts?.CancelAsync()!;
	}
}
