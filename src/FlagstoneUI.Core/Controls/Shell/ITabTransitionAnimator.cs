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
		Page? outgoingPage,
		Page? incomingPage,
		int previousIndex,
		int newIndex)
	{
		Shell = shell ?? throw new ArgumentNullException(nameof(shell));
		OutgoingPage = outgoingPage;
		IncomingPage = incomingPage;
		PreviousIndex = previousIndex;
		NewIndex = newIndex;
	}

	/// <summary>The shell driving the transition.</summary>
	public FsShell Shell { get; }

	/// <summary>
	/// The page of the tab being deselected. <see langword="null"/> on the first selection (before
	/// any tab had been entered) or when the outgoing tab's content has not yet been materialised.
	/// </summary>
	public Page? OutgoingPage { get; }

	/// <summary>
	/// The page of the tab being selected. <see langword="null"/> only if the incoming tab's
	/// content has not yet been materialised by the time the transition runs — uncommon, since
	/// Shell materialises the content as part of selecting it.
	/// </summary>
	public Page? IncomingPage { get; }

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
