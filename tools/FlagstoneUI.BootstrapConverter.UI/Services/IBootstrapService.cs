namespace FlagstoneUI.BootstrapConverter.UI.Services;

public interface IBootstrapService
{
	Task<UIConversionResult> ConvertAsync(ConversionRequest request);
}

internal class BootstrapService : IBootstrapService
{
	private readonly BootstrapConverterService _service = new();
	private readonly XamlThemeGenerator _xamlThemeGenerator = new();

	public async Task<UIConversionResult> ConvertAsync(ConversionRequest request)
	{
		var result = await _service.ConvertAsync(request);

		// Generate both tokens and styles XAML (without merged dictionaries for in-memory loading)
		var tokensXaml = _xamlThemeGenerator.GenerateTokensXaml(result.Tokens, request.Options);
		var stylesXaml = _xamlThemeGenerator.GenerateStylesXaml(result.Tokens, result.ThemeName, result.ComponentStyles, request.Options, includeMergedDictionaries: false);

		// Load into a single resource dictionary
		var dict = new ResourceDictionary();
		dict.LoadFromXaml(tokensXaml);
		dict.LoadFromXaml(stylesXaml);

		return new UIConversionResult(result.ThemeName, dict, result.Fonts);
	}
}
