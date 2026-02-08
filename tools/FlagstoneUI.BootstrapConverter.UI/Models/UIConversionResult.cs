using FlagstoneUI.BootstrapConverter.Models;

namespace FlagstoneUI.BootstrapConverter.UI.Models;

public record UIConversionResult(string ThemeName, ResourceDictionary Style, FontInformation? Fonts = null);
