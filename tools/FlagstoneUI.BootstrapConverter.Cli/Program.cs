using System.CommandLine;
using FlagstoneUI.BootstrapConverter.Cli.Commands;

namespace FlagstoneUI.BootstrapConverter.Cli;

/// <summary>
/// Bootstrap to FlagstoneUI converter CLI
/// </summary>
internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		var rootCommand = new RootCommand("Convert Bootstrap themes to FlagstoneUI XAML resources")
		{
			ConvertCommand.Create(),
			InfoCommand.Create()
		};

		return await rootCommand.InvokeAsync(args);
	}
}
