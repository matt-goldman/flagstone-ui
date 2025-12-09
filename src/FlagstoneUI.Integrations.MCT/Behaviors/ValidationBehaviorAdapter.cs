using CommunityToolkit.Maui.Behaviors;
using FlagstoneUI.Core.Controls;

namespace FlagstoneUI.Integrations.MCT.Behaviors;

public class ValidationBehaviorAdapter : Behavior<FsEntry>
{
    private Entry? _innerEntry;
    private ValidationBehavior? _currentBehavior;
    private FsEntry? _fsEntry;

    /// <summary>
    /// Backing BindableProperty for the <see cref="Behavior"/> property.
    /// </summary>
    public static readonly BindableProperty BehaviorProperty =
        BindableProperty.Create(
            nameof(Behavior),
            typeof(ValidationBehavior),
            typeof(ValidationBehaviorAdapter),
            null,
            propertyChanged: OnBehaviorChanged);

    /// <summary>
    /// Backing BindableProperty for the <see cref="ValidStyle"/> property.
    /// </summary>
    public static readonly BindableProperty ValidStyleProperty =
        BindableProperty.Create(
            nameof(ValidStyle),
            typeof(Style),
            typeof(ValidationBehaviorAdapter));

    /// <summary>
    /// Backing BindableProperty for the <see cref="InvalidStyle"/> property.
    /// </summary>
    public static readonly BindableProperty InvalidStyleProperty =
        BindableProperty.Create(
            nameof(InvalidStyle),
            typeof(Style),
            typeof(ValidationBehaviorAdapter));

    /// <summary>
    /// The <see cref="ValidationBehavior"/> to adapt from the MAUI Community Toolkit.
    /// </summary>
    public ValidationBehavior? Behavior
    {
        get => (ValidationBehavior?)GetValue(BehaviorProperty);
        set => SetValue(BehaviorProperty, value);
    }

    /// <summary>
    /// The <see cref="Style"/> to apply to the FsEntry when validation is successful.
    /// </summary>
    public Style? ValidStyle
    {
        get => (Style?)GetValue(ValidStyleProperty);
        set => SetValue(ValidStyleProperty, value);
    }

    /// <summary>
    /// The <see cref="Style"/> to apply to the FsEntry when validation fails.
    /// </summary>
    public Style? InvalidStyle
    {
        get => (Style?)GetValue(InvalidStyleProperty);
        set => SetValue(InvalidStyleProperty, value);
    }

    protected override void OnAttachedTo(FsEntry bindable)
    {
        base.OnAttachedTo(bindable);

        _fsEntry = bindable;
        _innerEntry = bindable.FindByName<Entry>("InnerEntry");

        if (_innerEntry != null && Behavior != null)
        {
            AttachBehavior(bindable);
        }
    }

    protected override void OnDetachingFrom(FsEntry bindable)
    {
        if (_innerEntry != null && _currentBehavior != null)
        {
            DetachBehavior(bindable);
        }

        _innerEntry = null;
        _fsEntry = null;
        base.OnDetachingFrom(bindable);
    }

    private static void OnBehaviorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationBehaviorAdapter adapter && adapter._fsEntry != null)
        {
            if (oldValue is ValidationBehavior)
            {
                adapter.DetachBehavior(adapter._fsEntry);
            }

            if (newValue is ValidationBehavior)
            {
                adapter.AttachBehavior(adapter._fsEntry);
            }
        }
    }

    private void AttachBehavior(FsEntry fsEntry)
    {
        if (_innerEntry == null || Behavior == null)
		{
			return;
		}

		_currentBehavior = Behavior;
		_currentBehavior.Flags = ValidationFlags.ValidateOnValueChanged; // TODO: make configurable
		_innerEntry.Behaviors.Add(_currentBehavior);

        _currentBehavior.PropertyChanged += OnValidationBehaviorPropertyChanged;

        // Set initial state
        UpdateValidationState(fsEntry, _currentBehavior.IsValid);
    }

    private void DetachBehavior(FsEntry fsEntry)
    {
        if (_innerEntry == null || _currentBehavior == null)
		{
			return;
		}

		_currentBehavior.PropertyChanged -= OnValidationBehaviorPropertyChanged;
        _innerEntry.Behaviors.Remove(_currentBehavior);
        _currentBehavior = null;
    }

    private void OnValidationBehaviorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ValidationBehavior.IsValid) && 
            sender is ValidationBehavior validationBehavior &&
            _fsEntry != null)
        {
            UpdateValidationState(_fsEntry, validationBehavior.IsValid);
        }
    }

    private void UpdateValidationState(FsEntry fsEntry, bool isValid)
    {
        VisualStateManager.GoToState(fsEntry, isValid ? "Valid" : "Invalid");

        if (isValid && ValidStyle != null)
        {
            fsEntry.Style = ValidStyle;
        }
        else if (!isValid && InvalidStyle != null)
        {
            fsEntry.Style = InvalidStyle;
        }
    }
}
