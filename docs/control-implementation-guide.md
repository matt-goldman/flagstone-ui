# Control Implementation Guide

This guide provides standards and patterns for implementing Flagstone UI controls.

## Control Implementation Philosophy

Flagstone UI controls follow these principles:

1. **Neutral by Default**: Strip platform-specific styling
2. **Token-Driven**: Use design tokens for all visual properties
3. **Themeable**: Support theme switching through resource dictionaries
4. **Accessible**: Implement proper semantics and screen reader support
5. **Consistent**: Follow established patterns across all controls

## Implementation Pattern

### 1. Control Class Structure

```csharp
namespace FlagstoneUI.Core.Controls;

/// <summary>
/// Brief description of the control's purpose
/// </summary>
public partial class ControlName : BaseControlType
{
    // Bindable Properties
    public static readonly BindableProperty PropertyNameProperty = 
        BindableProperty.Create(nameof(PropertyName), typeof(PropertyType), typeof(ControlName), defaultValue);
    
    // Public Properties
    public PropertyType PropertyName
    {
        get => (PropertyType)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }
    
    // Constructor (if needed)
    public ControlName()
    {
        // Initialization code
    }
}
```

### 2. Platform Handlers (when needed)

For controls that need to strip platform styling:

```csharp
namespace FlagstoneUI.Core.Handlers;

public class FsButtonHandler : ButtonHandler
{
    protected override void ConnectHandler(Button platformView)
    {
        base.ConnectHandler(platformView);
        
#if ANDROID
        // Remove Android-specific styling
        platformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS
        // Remove iOS-specific styling
        platformView.BackgroundColor = UIKit.UIColor.Clear;
#elif WINDOWS
        // Remove Windows-specific styling
        platformView.Background = null;
#endif
    }
}
```

### 3. Theme Styles

Each control should have corresponding theme styles:

```xml
<!-- In Theme.xaml files -->
<Style TargetType="fs:ControlName">
    <Setter Property="BackgroundColor" Value="{DynamicResource Color.Surface}" />
    <Setter Property="TextColor" Value="{DynamicResource Color.OnSurface}" />
    <Setter Property="Padding" Value="{DynamicResource Space.16}" />
    <Setter Property="CornerRadius" Value="{DynamicResource Radius.Medium}" />
</Style>
```

## Specific Control Guidelines

### Custom Controls (e.g., Card)

**When to use**: For completely new UI patterns not provided by MAUI

**Base class**: Usually `ContentView` or `TemplatedView`

**Example**: Current Card implementation
- Extends ContentView
- Adds elevation, corner radius, border color properties
- Styled through theme XAML

### Enhanced Standard Controls (e.g., FsButton)

**When to use**: To provide neutral, themeable versions of existing MAUI controls

**Base class**: The standard MAUI control (Button, Entry, etc.)

**Pattern**:
1. Subclass the standard control with "Fs" prefix
2. Create platform handlers to strip native styling
3. Register handlers in builder
4. Apply theme styles

**Example - FsButton** (not yet implemented):
```csharp
public class FsButton : Button
{
    // Additional properties for theming
    public static readonly BindableProperty ElevationProperty = ...;
    
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }
}
```

### Service-Based Controls (e.g., Snackbar)

**When to use**: For overlay or application-level UI components

**Pattern**:
1. Create control class for UI representation
2. Create service interface for programmatic usage
3. Register service in DI container
4. Provide extension methods for easy usage

**Example - Snackbar** (not yet implemented):
```csharp
public interface ISnackbarService
{
    Task ShowAsync(string message, string? actionText = null);
}

public static class ServiceExtensions
{
    public static Task ShowSnackbarAsync(this Page page, string message)
        => page.Handler.GetRequiredService<ISnackbarService>().ShowAsync(message);
}
```

## Token Usage Guidelines

### Color Tokens
- Use semantic color names: `Color.Primary`, `Color.Surface`, `Color.Error`
- Always pair with contrast colors: `Color.OnPrimary`, `Color.OnSurface`
- Support both light and dark themes

### Spacing Tokens
- Use consistent spacing scale: `Space.4`, `Space.8`, `Space.16`, etc.
- Apply to padding, margins, and gaps
- Consider touch target sizes (minimum 44px/44dp)

### Typography Tokens
- Define semantic text styles: `FontSize.Body`, `FontSize.Title`
- Include font family references when custom fonts are added
- Consider accessibility requirements (minimum 14pt)

### Border and Elevation
- Use consistent corner radius values: `Radius.Small`, `Radius.Medium`
- Define elevation levels for depth hierarchy
- Consider platform differences in shadow rendering

## Accessibility Requirements

### Semantic Properties
```csharp
// Set appropriate automation properties
AutomationProperties.SetIsInAccessibleTree(control, true);
AutomationProperties.SetName(control, "Button description");
AutomationProperties.SetRole(control, "Button");
```

### Focus Management
- Ensure controls are focusable with keyboard/screen reader
- Implement proper focus visual states
- Support high contrast themes

### Content Guidelines
- Provide meaningful descriptions for complex controls
- Support dynamic text sizing
- Ensure sufficient color contrast ratios

## Testing Standards

### Unit Tests
```csharp
[Test]
public void Control_Property_Sets_Correctly()
{
    var control = new ControlName();
    control.PropertyName = expectedValue;
    
    Assert.That(control.PropertyName, Is.EqualTo(expectedValue));
}
```

### Theme Tests
```csharp
[Test]
public void Control_Applies_Theme_Correctly()
{
    // Load theme
    var resources = new ResourceDictionary();
    ThemeLoader.Register(resources);
    
    // Create control and verify themed properties
    var control = new ControlName();
    // Verify theme application
}
```

## Registration and Integration

### Handler Registration
```csharp
// In FlagstoneUIBuilder or extension methods
public static class HandlerRegistration
{
    public static void RegisterHandlers(MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<FsButton, FsButtonHandler>();
            handlers.AddHandler<FsEntry, FsEntryHandler>();
        });
    }
}
```

### Service Registration
```csharp
// In builder pattern
public static class ServiceRegistration
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddTransient<IThemeService, ThemeService>();
    }
}
```

## Common Patterns and Utilities

### Visual State Manager
```xml
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup Name="CommonStates">
        <VisualState Name="Normal" />
        <VisualState Name="Disabled">
            <VisualState.Setters>
                <Setter Property="Opacity" Value="0.6" />
            </VisualState.Setters>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

### Property Change Handling
```csharp
protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
{
    base.OnPropertyChanged(propertyName);
    
    if (propertyName == nameof(SomeProperty))
    {
        UpdateVisualState();
    }
}
```

## Implementation Checklist

For each new control:

- [ ] Control class with appropriate base type
- [ ] Bindable properties for themeable aspects
- [ ] Platform handlers (if needed) to strip native styling
- [ ] Theme styles in Material theme
- [ ] Theme styles in Modern theme (when available)
- [ ] Unit tests for property setting and behavior
- [ ] Integration tests for theme application
- [ ] Accessibility properties and testing
- [ ] Documentation and examples
- [ ] Handler registration in builder
- [ ] Sample usage in sample app

*This guide should be updated as new patterns emerge and best practices are established.*