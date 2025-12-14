namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IBootstrapService
{
	Task<UIConversionResult> ConvertAsync(ConversionRequest request);
}

internal class BootstrapService : IBootstrapService
{
	private readonly BootstrapConverterService _service = new ();
	private readonly XamlThemeGenerator _xamlThemeGenerator = new();

	public async Task<UIConversionResult> ConvertAsync(ConversionRequest request)
	{
		var result = await _service.ConvertAsync(request);
		var tokens = _xamlThemeGenerator.GenerateTokensXaml(result.Tokens);

		var dict = new ResourceDictionary();
		dict.LoadFromXaml(tokens);

		return new UIConversionResult(result.ThemeName, dict, result.Fonts);
	}
}
