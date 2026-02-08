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
			var message = $"Text changed: '{e.NewTextValue}' (Length: {e.NewTextValue?.Length ?? 0})";

			// Update both feedback labels (only one will be visible at a time)
			EntryFeedbackLabel?.Text = message;
			EntryFeedbackLabelAlt?.Text = message;
		}
	}

	readonly FsEditorBorderAnimation _animation = new()
	{
		Length = 2000,
#pragma warning disable CS8601 // Possible null reference assignment. - Resource is expected to exist.
		Gradient = Application.Current!.Resources["AiGradientBrush"] as LinearGradientBrush,
#pragma warning restore CS8601 // Possible null reference assignment.
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
