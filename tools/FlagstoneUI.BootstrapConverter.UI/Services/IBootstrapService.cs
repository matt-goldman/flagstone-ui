using FlagstoneUI.BootstrapConverter.Models;
using FlagstoneUI.BootstrapConverter;

namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IBootstrapService
{
	Task<(string comments, ResourceDictionary dictionary)> ConvertAsync(ConversionRequest request);
}

internal class BootstrapService : IBootstrapService
{
	private BootstrapConverterService service = new ();

	public async Task<(string comments, ResourceDictionary dictionary)> ConvertAsync(ConversionRequest request)
	{
		var result = await service.ConvertAsync(request);


	}
}
