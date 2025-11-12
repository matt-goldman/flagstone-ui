# Unit Testing Guide for Flagstone UI

This guide explains how to write unit tests for MAUI UI components in the Flagstone UI project.

## Overview

Testing MAUI UI components (which inherit from `BindableObject`) requires special setup because these components need:
- A dispatcher for thread synchronization
- An active MAUI application context
- Proper initialization of the MAUI framework

This project provides infrastructure to handle these requirements automatically.

## Test Infrastructure

Each test project includes two helper classes:

### `MauiTestApplicationBuilder`
Creates a minimal MAUI application for testing purposes with:
- A test application instance
- A synchronous test dispatcher (executes actions immediately on the current thread)
- Minimal UI infrastructure (no actual windows or UI rendering)

### `MauiTestBase`
Abstract base class that:
- Initializes the MAUI application once before any tests run
- Sets the application as current so `BindableObject` components can find the dispatcher
- Implements `IDisposable` for proper cleanup

## Writing Tests

### For UI Components (Controls, Pages, etc.)

Inherit from `MauiTestBase` for any test class that tests UI components:

```csharp
using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class FsCardTests : MauiTestBase
{
    [Fact]
    public void Card_can_be_instantiated()
    {
        var card = new FsCard();
        card.ShouldNotBeNull();
    }

    [Fact]
    public void Card_background_color_property_can_be_set()
    {
        var card = new FsCard();
        card.BackgroundColor = Colors.Blue;
        card.BackgroundColor.ShouldBe(Colors.Blue);
    }
}
```

Key points:
- **No need for `MainThread.BeginInvokeOnMainThread`** - the test dispatcher handles threading automatically
- The MAUI application is initialized once and shared across all tests
- Tests run synchronously on the current thread

### For Non-UI Code

For testing non-UI code (utilities, converters, parsers, etc.), don't inherit from `MauiTestBase`:

```csharp
public class UtilityTests // No base class needed
{
    [Fact]
    public void Utility_method_works()
    {
        // Test non-UI code directly
    }
}
```

## Common Errors and Solutions

### Error: "BindableObject was not instantiated on a thread with a dispatcher"

**Solution:** Make sure your test class inherits from `MauiTestBase`:

```csharp
public class MyTests : MauiTestBase // Add this inheritance
{
    // Your tests...
}
```

### Error: "This functionality is not implemented in the portable version"

This error occurs when using `MainThread.BeginInvokeOnMainThread` in tests.

**Solution:** Remove the `MainThread.BeginInvokeOnMainThread` wrapper and inherit from `MauiTestBase` instead:

```csharp
// ❌ Don't do this:
[Fact]
public void Test_something()
{
    MainThread.BeginInvokeOnMainThread(() =>
    {
        var card = new FsCard();
        // test code...
    });
}

// ✅ Do this:
public class MyTests : MauiTestBase
{
    [Fact]
    public void Test_something()
    {
        var card = new FsCard();
        // test code...
    }
}
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests for specific framework
```bash
dotnet test --framework net10.0
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~ClassName"
```

### Run specific test project
```bash
dotnet test tests/FlagstoneUI.Core.Tests/FlagstoneUI.Core.Tests.csproj
```

## Technical Details

### Test Dispatcher

The `TestDispatcher` is synchronous, meaning:
- `IsDispatchRequired` always returns `false`
- `Dispatch(Action)` executes the action immediately
- No actual thread switching occurs

This makes tests:
- **Faster** - no thread context switches
- **Deterministic** - actions execute in predictable order
- **Debuggable** - stack traces are simpler

### Application Lifecycle

The MAUI application is initialized:
1. Once per test session (singleton pattern)
2. Thread-safely (using lock for initialization)
3. Before any test that inherits from `MauiTestBase` runs

The application remains active for all tests to improve performance.

## Best Practices

1. **Only use `MauiTestBase` when testing UI components** - Non-UI tests don't need the overhead
2. **Don't use `MainThread.BeginInvokeOnMainThread` in tests** - The test infrastructure handles this
3. **Keep tests focused** - Test one behavior per test method
4. **Use descriptive test names** - Clearly state what is being tested
5. **Use Shouldly assertions** - For more readable test failures

## Example Test Class

```csharp
using FlagstoneUI.Core.Controls;
using Shouldly;
using Xunit;

namespace FlagstoneUI.Core.Tests.Controls;

public class FsButtonTests : MauiTestBase
{
    [Fact]
    public void Button_can_be_instantiated()
    {
        var button = new FsButton();
        button.ShouldNotBeNull();
    }

    [Fact]
    public void Button_text_property_can_be_set()
    {
        var button = new FsButton();
        button.Text = "Click Me";
        button.Text.ShouldBe("Click Me");
    }

    [Fact]
    public void Button_command_executes_when_triggered()
    {
        var executed = false;
        var button = new FsButton
        {
            Command = new Command(() => executed = true)
        };

        button.Command.Execute(null);

        executed.ShouldBeTrue();
    }
}
```

## Troubleshooting

### Tests fail with platform-specific errors

Make sure you're running tests with a supported target framework:
```bash
# Use net10.0 for cross-platform tests
dotnet test --framework net10.0

# Or use platform-specific framework if needed
dotnet test --framework net10.0-android  # On Linux/macOS for CI
```

### Tests are slow

Ensure you're inheriting from `MauiTestBase` only when necessary. Non-UI tests should not use the MAUI infrastructure.

### Need to test platform-specific behavior

For platform-specific testing, you may need to use conditional compilation or separate test methods for each platform:

```csharp
[Fact]
public void Platform_specific_behavior()
{
#if ANDROID
    // Android-specific test
#elif IOS
    // iOS-specific test
#elif WINDOWS
    // Windows-specific test
#else
    // Cross-platform test
#endif
}
```

## See Also

- [xUnit Documentation](https://xunit.net/)
- [Shouldly Documentation](https://docs.shouldly.org/)
- [.NET MAUI Unit Testing](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/unit-testing)
