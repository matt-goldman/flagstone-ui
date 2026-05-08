using FlagstoneUI.Core.Controls;
using Microsoft.Maui.Hosting;

namespace FlagstoneUI.Core.Builders;

/// <summary>
/// <see cref="MauiAppBuilder"/> extensions that register FlagstoneUI's platform renderers.
/// </summary>
/// <remarks>
/// <para>
/// Most FlagstoneUI controls (<see cref="FsButton"/>, <see cref="FsEntry"/>, etc.) self-register
/// from their constructors via <c>HandlerName.Mapper.AppendToMapping</c>, because they reuse
/// MAUI's standard handlers. <see cref="FsShell"/> is the exception: Shell still uses MAUI's
/// legacy compat renderer model, which requires renderer registration with the handler
/// collection at app build time. That registration lives here.
/// </para>
/// <para>
/// Consumers call <see cref="UseFlagstoneUI"/> once from <c>MauiProgram.cs</c>; no other
/// platform-specific code is required.
/// </para>
/// </remarks>
public static class FlagstoneUIBuilderExtensions
{
	/// <summary>
	/// Registers FlagstoneUI's platform renderers with the host builder.
	/// </summary>
	/// <param name="builder">The MAUI app builder.</param>
	/// <param name="configure">Optional configuration callback.</param>
	/// <returns>The same <paramref name="builder"/> for chaining.</returns>
	public static MauiAppBuilder UseFlagstoneUI(
		this MauiAppBuilder builder,
		Action<FlagstoneUIBuilder>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureMauiHandlers(handlers =>
		{
			// FsShellRenderer is a partial type: an empty stub on the shared net10.0 TFM and a
			// real ShellRenderer subclass on every platform TFM (Tizen is out of scope).
			handlers.AddHandler(typeof(FsShell), typeof(FsShellRenderer));
		});

		var fsBuilder = new FlagstoneUIBuilder();
		configure?.Invoke(fsBuilder);
		return builder;
	}
}
