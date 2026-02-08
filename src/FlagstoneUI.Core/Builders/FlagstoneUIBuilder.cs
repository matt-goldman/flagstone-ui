namespace FlagstoneUI.Core.Builders;

/// <summary>
/// Provides functionality to build and configure a Flagstone UI with customizable options.
/// </summary>
/// <remarks>This class supports a fluent API for configuring the UI. Call methods in a chain to apply multiple
/// configurations.</remarks>
public class FlagstoneUIBuilder
{
    /// <summary>
    /// Configures the builder to use the default theme for the UI.
    /// </summary>
    /// <returns>The current <see cref="FlagstoneUIBuilder"/> instance, allowing for method chaining.</returns>
    public FlagstoneUIBuilder UseDefaultTheme() => this;
}
