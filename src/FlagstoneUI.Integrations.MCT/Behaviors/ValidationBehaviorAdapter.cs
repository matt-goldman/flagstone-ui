using CommunityToolkit.Maui.Behaviors;
using FlagstoneUI.Core.Controls;

namespace FlagstoneUI.Integrations.MCT.Behaviors;

public static class ValidationBehaviorAdapter
{
    public static BindableProperty ValidationBehaviorProperty = BindableProperty.CreateAttached(
        "ValidationBehavior",
        typeof(Behavior),
        typeof(ValidationBehaviorAdapter),
        null,
        propertyChanged: OnValidationBehaviorChanged);

    public static void SetValidationBehavior(BindableObject view, Behavior behavior)
    {
        view.SetValue(ValidationBehaviorProperty, behavior);
    }

    public static Behavior GetValidationBehavior(BindableObject view)
    {
        return (Behavior)view.GetValue(ValidationBehaviorProperty);
    }

    private static void OnValidationBehaviorChanged(BindableObject view, object oldValue, object newValue)
    {
        if (view is FsEntry fsEntry)
        {
            if (newValue is ValidationBehavior newBehavior)
            {
                var innerEntry = fsEntry.FindByName<Entry>("InnerEntry");

                if (innerEntry is not null)
                {
                    innerEntry.Behaviors.Add(newBehavior);
                    newBehavior.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ValidationBehavior.IsValid))
                        {
                            // perform style switch
                            VisualStateManager.GoToState(fsEntry, newBehavior.IsValid ? "Valid" : "Invalid");
                            fsEntry.Style = newBehavior.IsValid ? ValidStyle : InvalidStyle;
                        }
                    };
                }
            }
        }
    }

    /// <summary>
	/// Backing BindableProperty for the <see cref="ValidStyle"/> property.
	/// </summary>
	public static readonly BindableProperty ValidStyleProperty =
        BindableProperty.Create(nameof(ValidStyle), typeof(Style), typeof(ValidationBehavior), propertyChanged: OnValidationPropertyChanged);

    /// <summary>
    /// Backing BindableProperty for the <see cref="InvalidStyle"/> property.
    /// </summary>
    public static readonly BindableProperty InvalidStyleProperty =
        BindableProperty.Create(nameof(InvalidStyle), typeof(Style), typeof(ValidationBehavior), propertyChanged: OnValidationPropertyChanged);

    /// <summary>
	/// The <see cref="Style"/> to apply to the element when validation is successful. This is a bindable property.
	/// </summary>
	public static Style? ValidStyle
    {
        get => (Style?)GetValue(ValidStyleProperty);
        set => SetValue(ValidStyleProperty, value);
    }

    /// <summary>
    /// The <see cref="Style"/> to apply to the element when validation fails. This is a bindable property.
    /// </summary>
    public static Style? InvalidStyle
    {
        get => (Style?)GetValue(InvalidStyleProperty);
        set => SetValue(InvalidStyleProperty, value);
    }
}
