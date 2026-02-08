using CommunityToolkit.Maui.Animations;
using FlagstoneUI.Core.Controls;

namespace FlagstoneUI.Integrations.MCT.Animations;

public class FsEditorBorderAnimation : BaseAnimation<FsEditor>
{
	#region Gradient Property
	public static readonly BindableProperty GradientProperty =
		BindableProperty.Create(
			nameof(Gradient),
			typeof(GradientBrush),
			typeof(FsEditorBorderAnimation),
			new LinearGradientBrush(),
			propertyChanged: OnGradientChanged);

	static void OnGradientChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is FsEditorBorderAnimation animation && newValue is GradientBrush brush)
		{
			animation.SetBrush(brush);
		}
	}

	public GradientBrush Gradient
	{
		get => (GradientBrush)GetValue(GradientProperty);
		set => SetValue(GradientProperty, value);
	}
	#endregion

	/// <summary>
	/// Animates a border gradient by rotating the gradient stops around a fixed diagonal line.
	/// For best results, use 3-5 evenly-spaced gradient stops with the first color repeated at offset 1.0.
	/// </summary>
	public override async Task Animate(FsEditor view, CancellationToken token = new CancellationToken())
	{
		ArgumentNullException.ThrowIfNull(view);
		token.ThrowIfCancellationRequested();

		var originalBrush = view.BorderBrush;

		if (_brush is not LinearGradientBrush linearBrush || linearBrush.GradientStops.Count < 2)
		{
			return;
		}

		const int frameDelayMs = 50; // 20 fps
		var stopShiftPerFrame = 1.0f / ((float)Length / frameDelayMs);
		var startPoint = new Point(0, 0);
		var endPoint = new Point(1, 1);
		var currentShift = 0f;

		while (!token.IsCancellationRequested)
		{
			var rotatedStops = RotateGradientStops(linearBrush.GradientStops, currentShift);
			var animatedBrush = new LinearGradientBrush(rotatedStops, startPoint, endPoint);

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (!token.IsCancellationRequested)
				{
					view.BorderBrush = animatedBrush;
				}
			});

			await Task.Delay(frameDelayMs, token);
			currentShift = (currentShift + stopShiftPerFrame) % 1.0f;
		}

		await MainThread.InvokeOnMainThreadAsync(() =>
		{
			view.BorderBrush = originalBrush;
		});
	}

	private GradientBrush _brush = new LinearGradientBrush();

	public void SetBrush(GradientBrush brush) => _brush = brush;

	private static GradientStopCollection RotateGradientStops(GradientStopCollection stops, float shift)
	{
		var result = new GradientStopCollection();

		// Shift all stops except the last (which is the closing stop at 1.0)
		foreach (var stop in stops.Take(stops.Count - 1))
		{
			var newOffset = (stop.Offset + shift) % 1.0f;
			result.Add(new GradientStop(stop.Color, newOffset));
		}

		// Sort and add closing stop
		var sorted = result.OrderBy(s => s.Offset).ToList();
		result.Clear();
		foreach (var stop in sorted)
		{
			result.Add(stop);
		}
		result.Add(new GradientStop(sorted[0].Color, 1.0f));

		return result;
	}
}
