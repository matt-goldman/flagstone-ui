namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Context passed to an <see cref="ITabTransitionAnimator"/> when <see cref="FsShell"/>
/// changes its selected tab.
/// </summary>
public sealed class FsTabTransitionContext
{
	/// <summary>Initializes a new <see cref="FsTabTransitionContext"/>.</summary>
	public FsTabTransitionContext(
		FsShell shell,
		View? outgoingView,
		View? incomingView,
		int previousIndex,
		int newIndex)
	{
		Shell = shell ?? throw new ArgumentNullException(nameof(shell));
		OutgoingView = outgoingView;
		IncomingView = incomingView;
		PreviousIndex = previousIndex;
		NewIndex = newIndex;
	}

	/// <summary>The shell driving the transition.</summary>
	public FsShell Shell { get; }

	/// <summary>The view of the tab being deselected. May be null on the first selection.</summary>
	public View? OutgoingView { get; }

	/// <summary>The view of the tab being selected.</summary>
	public View? IncomingView { get; }

	/// <summary>The index of the previously selected tab. Negative on the first selection.</summary>
	public int PreviousIndex { get; }

	/// <summary>The index of the newly selected tab.</summary>
	public int NewIndex { get; }
}

/// <summary>
/// Pluggable animator invoked by <see cref="FsShell"/> on tab selection changes. Implementations
/// drive the transition between the outgoing and incoming tab content.
/// </summary>
public interface ITabTransitionAnimator
{
	/// <summary>
	/// Animates the transition described by <paramref name="context"/>.
	/// </summary>
	/// <param name="context">The transition context.</param>
	/// <param name="cancellationToken">Cancellation token signalled if the transition is superseded.</param>
	/// <returns>A task that completes when the transition is finished.</returns>
	Task AnimateAsync(FsTabTransitionContext context, CancellationToken cancellationToken);
}
