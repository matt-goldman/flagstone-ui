using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlagstoneUI.SampleApp.ViewModels;

public class FsEditorViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	// Text
	private string _editorText = string.Empty;
	public string EditorText
	{
		get => _editorText;
		set
		{
			if (_editorText == value)
				return;

			_editorText = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CharacterCount));
		}
	}

	public string CharacterCount => $"Characters: {EditorText.Length}";

	// AutoSize
	private string _selectedAutoSize = "Disabled";
	public string SelectedAutoSize
	{
		get => _selectedAutoSize;
		set
		{
			if (_selectedAutoSize == value)
				return;

			_selectedAutoSize = value;
			AutoSize = value == "TextChanges" ? EditorAutoSizeOption.TextChanges : EditorAutoSizeOption.Disabled;
			OnPropertyChanged();
		}
	}

	public List<string> AutoSizeOptions { get; } = ["Disabled", "TextChanges"];

	private EditorAutoSizeOption _autoSize = EditorAutoSizeOption.Disabled;
	public EditorAutoSizeOption AutoSize
	{
		get => _autoSize;
		set
		{
			if (_autoSize == value)
				return;

			_autoSize = value;
			OnPropertyChanged();
		}
	}

	// IsReadOnly
	private bool _isReadOnly;
	public bool IsReadOnly
	{
		get => _isReadOnly;
		set
		{
			if (_isReadOnly == value)
				return;

			_isReadOnly = value;
			OnPropertyChanged();
		}
	}

	// IsSpellCheckEnabled
	private bool _isSpellCheckEnabled = true;
	public bool IsSpellCheckEnabled
	{
		get => _isSpellCheckEnabled;
		set
		{
			if (_isSpellCheckEnabled == value)
				return;

			_isSpellCheckEnabled = value;
			OnPropertyChanged();
		}
	}

	// IsTextPredictionEnabled
	private bool _isTextPredictionEnabled = true;
	public bool IsTextPredictionEnabled
	{
		get => _isTextPredictionEnabled;
		set
		{
			if (_isTextPredictionEnabled == value)
				return;

			_isTextPredictionEnabled = value;
			OnPropertyChanged();
		}
	}

	// FontAttributes
	private string _selectedFontAttributes = "None";
	public string SelectedFontAttributes
	{
		get => _selectedFontAttributes;
		set
		{
			if (_selectedFontAttributes == value)
				return;

			_selectedFontAttributes = value;
			FontAttributes = value switch
			{
				"Bold" => FontAttributes.Bold,
				"Italic" => FontAttributes.Italic,
				"Bold + Italic" => FontAttributes.Bold | FontAttributes.Italic,
				_ => FontAttributes.None
			};
			OnPropertyChanged();
		}
	}

	public List<string> FontAttributesOptions { get; } = ["None", "Bold", "Italic", "Bold + Italic"];

	private FontAttributes _fontAttributes = FontAttributes.None;
	public FontAttributes FontAttributes
	{
		get => _fontAttributes;
		set
		{
			if (_fontAttributes == value)
				return;

			_fontAttributes = value;
			OnPropertyChanged();
		}
	}

	// FontAutoScalingEnabled
	private bool _fontAutoScalingEnabled = true;
	public bool FontAutoScalingEnabled
	{
		get => _fontAutoScalingEnabled;
		set
		{
			if (_fontAutoScalingEnabled == value)
				return;

			_fontAutoScalingEnabled = value;
			OnPropertyChanged();
		}
	}

	// FontSize
	private double _fontSize = 14.0;
	public double FontSize
	{
		get => _fontSize;
		set
		{
			if (_fontSize == value)
				return;

			_fontSize = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(FontSizeLabel));
		}
	}

	public string FontSizeLabel => $"Font Size: {FontSize:F0}";

	// TextTransform
	private string _selectedTextTransform = "Default";
	public string SelectedTextTransform
	{
		get => _selectedTextTransform;
		set
		{
			if (_selectedTextTransform == value)
				return;

			_selectedTextTransform = value;
			TextTransform = value switch
			{
				"None" => TextTransform.None,
				"Lowercase" => TextTransform.Lowercase,
				"Uppercase" => TextTransform.Uppercase,
				_ => TextTransform.Default
			};
			OnPropertyChanged();
		}
	}

	public List<string> TextTransformOptions { get; } = ["Default", "None", "Lowercase", "Uppercase"];

	private TextTransform _textTransform = TextTransform.Default;
	public TextTransform TextTransform
	{
		get => _textTransform;
		set
		{
			if (_textTransform == value)
				return;

			_textTransform = value;
			OnPropertyChanged();
		}
	}

	// MaxLength
	private int _maxLength = int.MaxValue;
	public int MaxLength
	{
		get => _maxLength;
		set
		{
			if (_maxLength == value)
				return;

			_maxLength = value;
			OnPropertyChanged();
		}
	}

	private bool _maxLengthEnabled;
	public bool MaxLengthEnabled
	{
		get => _maxLengthEnabled;
		set
		{
			if (_maxLengthEnabled == value)
				return;

			_maxLengthEnabled = value;
			MaxLength = value ? 100 : int.MaxValue;
			OnPropertyChanged();
		}
	}

	// Keyboard
	private string _selectedKeyboard = "Default";
	public string SelectedKeyboard
	{
		get => _selectedKeyboard;
		set
		{
			if (_selectedKeyboard == value)
				return;

			_selectedKeyboard = value;
			Keyboard = value switch
			{
				"Chat" => Keyboard.Chat,
				"Email" => Keyboard.Email,
				"Numeric" => Keyboard.Numeric,
				"Plain" => Keyboard.Plain,
				"Telephone" => Keyboard.Telephone,
				"Text" => Keyboard.Text,
				"Url" => Keyboard.Url,
				_ => Keyboard.Default
			};
			OnPropertyChanged();
		}
	}

	public List<string> KeyboardOptions { get; } =
	[
		"Default", "Chat", "Email", "Numeric", "Plain", "Telephone", "Text", "Url"
	];

	private Keyboard _keyboard = Keyboard.Default;
	public Keyboard Keyboard
	{
		get => _keyboard;
		set
		{
			if (_keyboard == value)
				return;

			_keyboard = value;
			OnPropertyChanged();
		}
	}
}
