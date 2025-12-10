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
	
	public override async Task Animate(FsEditor view, CancellationToken token = new CancellationToken())
	{
		ArgumentNullException.ThrowIfNull(view);
		token.ThrowIfCancellationRequested();
		
		var originalBrush = view.BorderBrush as GradientBrush;
		GradientBrush brush = _brush;
		
		// Only animate LinearGradientBrush
		if (brush is not LinearGradientBrush linearBrush)
		{
			return;
		}
		
		// Frame rate: 30 fps = ~33ms per frame (smoother, less flickering)
		const int frameDelayMs = 33;
		
		// Calculate how much to progress per frame to complete animation in Length milliseconds
		var totalFrames = Length / frameDelayMs;
		var stepIncrement = 1.0f / totalFrames;
		
		float progress = 0.0f;
		
		while (!token.IsCancellationRequested)
		{
			// Calculate start and end points based on progress around the border
			var (startPoint, endPoint) = CalculateGradientPoints(progress);
			
			// Create new gradient with same stops but updated points
			var animatedBrush = new LinearGradientBrush(
				linearBrush.GradientStops,
				startPoint,
				endPoint);
			
			// Update the border brush on main thread
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (!token.IsCancellationRequested)
				{
					view.BorderBrush = animatedBrush;
				}
			});
			
			// Increment and wrap around
			progress = (progress + stepIncrement) % 1.0f;
			
			await Task.Delay(frameDelayMs, token);
		}
		
		// Restore original brush on main thread
		await MainThread.InvokeOnMainThreadAsync(() =>
		{
			view.BorderBrush = originalBrush ?? new LinearGradientBrush();
		});
	}

	private GradientBrush _brush = new LinearGradientBrush();
	
	public void SetBrush(GradientBrush brush) => _brush = brush;
	
	private (Point startPoint, Point endPoint) CalculateGradientPoints(float progress)
	{
		// Progress goes from 0.0 to 1.0 and represents one full rotation around the border
		// Use a helper method to calculate point position for any progress value
		var startPoint = CalculatePointOnBorder(progress);
		
		// End point is offset by 0.5 (opposite side)
		// Don't use modulo here - let the helper handle wrapping
		var endPoint = CalculatePointOnBorder(progress + 0.5f);
		
		return (startPoint, endPoint);
	}
	
	private Point CalculatePointOnBorder(float progress)
	{
		// Normalize progress to 0-1 range (handle values > 1.0)
		progress = progress % 1.0f;
		
		// Normalize progress to 0-4 range (4 sides)
		var segmentProgress = progress * 4.0f;
		
		// Calculate position based on which segment we're in
		// Segment 0 (0.0-1.0): Top edge, left to right (0,0) -> (1,0)
		// Segment 1 (1.0-2.0): Right edge, top to bottom (1,0) -> (1,1)
		// Segment 2 (2.0-3.0): Bottom edge, right to left (1,1) -> (0,1)
		// Segment 3 (3.0-4.0): Left edge, bottom to top (0,1) -> (0,0)
		
		if (segmentProgress < 1.0f)
		{
			// Top edge: moving right along x-axis
			return new Point(segmentProgress, 0);
		}
		else if (segmentProgress < 2.0f)
		{
			// Right edge: moving down along y-axis
			var t = segmentProgress - 1.0f;
			return new Point(1, t);
		}
		else if (segmentProgress < 3.0f)
		{
			// Bottom edge: moving left along x-axis
			var t = segmentProgress - 2.0f;
			return new Point(1 - t, 1);
		}
		else
		{
			// Left edge: moving up along y-axis
			var t = segmentProgress - 3.0f;
			return new Point(0, 1 - t);
		}
	}
}
