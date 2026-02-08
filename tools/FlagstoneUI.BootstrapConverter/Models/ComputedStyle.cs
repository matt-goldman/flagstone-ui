namespace FlagstoneUI.BootstrapConverter.Models;

/// <summary>
/// Represents a computed CSS style with all properties resolved
/// </summary>
public class ComputedStyle
{
	/// <summary>
	/// CSS selector that produced this style
	/// </summary>
	public string Selector { get; set; } = string.Empty;

	/// <summary>
	/// All CSS property declarations (property name → value)
	/// </summary>
	public Dictionary<string, string> Properties { get; set; } = [];

	/// <summary>
	/// Get a property value, or null if not found
	/// </summary>
	public string? GetProperty(string propertyName)
	{
		return Properties.TryGetValue(propertyName, out var value) ? value : null;
	}

	/// <summary>
	/// Check if a property exists
	/// </summary>
	public bool HasProperty(string propertyName)
	{
		return Properties.ContainsKey(propertyName);
	}
}

/// <summary>
/// Container for Bootstrap component styles extracted from CSS
/// </summary>
public class BootstrapComponentStyles
{
	// Button variants
	public ComputedStyle? ButtonBase { get; set; }
	public ComputedStyle? ButtonPrimary { get; set; }
	public ComputedStyle? ButtonSecondary { get; set; }
	public ComputedStyle? ButtonSuccess { get; set; }
	public ComputedStyle? ButtonDanger { get; set; }
	public ComputedStyle? ButtonWarning { get; set; }
	public ComputedStyle? ButtonInfo { get; set; }
	public ComputedStyle? ButtonLight { get; set; }
	public ComputedStyle? ButtonDark { get; set; }

	// Outline button variants
	public ComputedStyle? ButtonOutlinePrimary { get; set; }
	public ComputedStyle? ButtonOutlineSecondary { get; set; }
	public ComputedStyle? ButtonOutlineSuccess { get; set; }
	public ComputedStyle? ButtonOutlineDanger { get; set; }
	public ComputedStyle? ButtonOutlineWarning { get; set; }
	public ComputedStyle? ButtonOutlineInfo { get; set; }
	public ComputedStyle? ButtonOutlineLight { get; set; }
	public ComputedStyle? ButtonOutlineDark { get; set; }

	// Button sizes
	public ComputedStyle? ButtonLarge { get; set; }
	public ComputedStyle? ButtonSmall { get; set; }

	// Form controls
	public ComputedStyle? FormControl { get; set; }
	public ComputedStyle? FormControlFocus { get; set; }
	public ComputedStyle? FormControlPlaceholder { get; set; }

	// Validation states (.is-valid / .is-invalid)
	public ComputedStyle? FormControlValid { get; set; }
	public ComputedStyle? FormControlInvalid { get; set; }
	public ComputedStyle? FormControlValidFocus { get; set; }
	public ComputedStyle? FormControlInvalidFocus { get; set; }

	// Cards
	public ComputedStyle? Card { get; set; }
	public ComputedStyle? CardBody { get; set; }
	public ComputedStyle? CardHeader { get; set; }
	public ComputedStyle? CardFooter { get; set; }

	// Future: Add more component types as needed
	// - Badges
	// - Alerts
	// - Navigation
	// - etc.
}
