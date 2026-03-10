# MAUI UI Testing Implementation Plan (Archived)

> **⚠️ ARCHIVED DOCUMENT**
> This document outlines a plan for implementing comprehensive UI testing for FlagstoneUI. The testing strategy decision was captured in [ADR007: CI/UI Test Strategy](../decisions/adr007-ci-ui-test-strategy.md). UI device tests and Appium integration are deferred to post-MVP.
>
> This document is retained as a reference for when UI testing is implemented.

## Current State

### What Works
- ✅ Unit tests for non-UI logic (Bootstrap converter, token mapping, etc.)
- ✅ Component API tests (property getters/setters that don't trigger rendering)
- ✅ All tests pass locally with full UI environment

### What's Missing
- ❌ UI component visual rendering tests in CI
- ❌ Layout and sizing validation
- ❌ Platform-specific behavior verification
- ❌ Visual regression testing
- ❌ Interaction testing (tap, swipe, etc.)

### Tests Currently Filtered from CI
1. `Border_bottom_brush_can_be_set` - Creates `SolidColorBrush`, hangs in headless CI
2. Potentially other FsBorder tests creating visual elements
3. Future tests for FsButton, FsCard, FsEntry, FsEditor as they add visual validation

## Goals

### Primary Goals
1. **Enable UI component testing in CI** without hanging
2. **Validate visual rendering** of controls across platforms
3. **Catch regressions** in control appearance and behavior
4. **Test platform-specific behavior** (iOS, Android, Windows, macOS)

### Secondary Goals
1. Enable visual regression testing (screenshot comparison)
2. Test accessibility features
3. Performance/rendering benchmarks
4. Integration testing with actual MAUI applications

## Proposed Approaches

### Approach 1: MAUI DeviceTests (Recommended for Phase 1)

**Description**: Use Microsoft's testing framework from the MAUI repository

**Technology Stack**:
- `Microsoft.Maui.TestUtils.DeviceTests` package
- xUnit test runner
- Runs on actual devices/emulators

**Pros**:
- ✅ Official Microsoft solution
- ✅ Integrated with MAUI development workflow
- ✅ Tests on real platforms
- ✅ Can run in CI with emulators
- ✅ Existing patterns from MAUI repo

**Cons**:
- ❌ Requires emulator/device setup in CI
- ❌ Slower test execution
- ❌ May require Android/iOS CI runners (additional cost)
- ❌ Windows-specific tests still require Windows runner

**Implementation Effort**: Medium
**Timeline**: 2-3 sprints

### Approach 2: Appium + Sample App (Recommended for Phase 2)

**Description**: E2E testing using Appium with a sample MAUI application

**Technology Stack**:
- Appium
- WinAppDriver (Windows)
- Appium UIAutomator2 (Android)
- Appium XCUITest (iOS)
- Sample app with control showcase

**Pros**:
- ✅ Cross-platform testing
- ✅ Real user interaction simulation
- ✅ Visual regression testing possible
- ✅ Can test actual app integration
- ✅ Industry standard tool

**Cons**:
- ❌ Complex setup and maintenance
- ❌ Slower than unit tests
- ❌ Requires dedicated test app
- ❌ Brittle selectors/flaky tests

**Implementation Effort**: High
**Timeline**: 4-6 sprints (including infrastructure setup)

### Approach 3: Hybrid - Device Tests + Appium

**Description**: Use DeviceTests for component validation, Appium for E2E

**Split Strategy**:
- **DeviceTests**: Individual control rendering, layout, styling
- **Appium**: Full screen flows, user interactions, real-world scenarios

**Pros**:
- ✅ Best of both worlds
- ✅ Comprehensive coverage
- ✅ Fast component tests + thorough E2E tests

**Cons**:
- ❌ Highest implementation cost
- ❌ Two testing infrastructures to maintain
- ❌ Most complex CI pipeline

**Implementation Effort**: Very High
**Timeline**: 6-8 sprints

### Approach 4: Headless Testing with Mocks (Not Recommended)

**Description**: Mock visual components for headless testing

**Pros**:
- ✅ Fast execution
- ✅ Works in current CI environment

**Cons**:
- ❌ Doesn't test actual rendering
- ❌ Can't catch platform-specific bugs
- ❌ False confidence - tests pass but UI might be broken
- ❌ Significant refactoring required

**Implementation Effort**: Medium
**Timeline**: N/A - Not pursuing

## Recommended Strategy

### Phase 1: MAUI DeviceTests Setup (Post-POC, Pre-MVP)

**Scope**: Basic infrastructure and proof-of-concept

**Deliverables**:
1. DeviceTests project structure
2. Android emulator CI pipeline
3. Basic rendering tests for FsButton and FsCard
4. Documentation and patterns

**Success Criteria**:
- Tests run successfully on Android emulator in CI
- At least 2 controls have visual rendering tests
- Clear patterns established for future tests

**Timeline**: 2-3 weeks

**Effort Breakdown**:
- Research and spike: 3 days
- CI infrastructure setup: 4 days
- Test project setup: 2 days
- Initial tests: 3 days
- Documentation: 2 days

### Phase 2: Expand Coverage (During MVP)

**Scope**: Add tests for all core controls

**Deliverables**:
1. Visual tests for all FlagstoneUI.Core controls
2. Windows runner with DeviceTests
3. Layout and sizing validation
4. Theme switching tests

**Success Criteria**:
- All core controls have rendering tests
- Tests run on both Android and Windows in CI
- No flaky tests (<1% failure rate)

**Timeline**: 4-5 weeks

### Phase 3: Appium Integration (Post-MVP)

**Scope**: E2E testing with sample applications

**Deliverables**:
1. Appium test infrastructure
2. Sample app with control showcase
3. E2E test suite covering common scenarios
4. Visual regression baseline

**Success Criteria**:
- E2E tests run on at least 2 platforms
- Visual regression tests catch UI changes
- CI integration complete

**Timeline**: 6-8 weeks

## Implementation Details

### DeviceTests Setup (Phase 1)

#### 1. Project Structure

```
tests/
  FlagstoneUI.Core.DeviceTests/
    FlagstoneUI.Core.DeviceTests.csproj
    Controls/
      FsButtonRenderingTests.cs
      FsCardRenderingTests.cs
      FsBorderRenderingTests.cs
    Helpers/
      ScreenshotHelper.cs
      RenderingAssertions.cs
    MauiProgram.cs
    TestApplication.cs
```

#### 2. CI Configuration

**GitHub Actions Workflow** (`device-tests.yml`):

```yaml
name: Device Tests

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  android-tests:
    runs-on: macos-latest # Has Android emulator support
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Setup Android SDK
        uses: android-actions/setup-android@v2
      
      - name: Create Android Emulator
        run: |
          echo "y" | $ANDROID_HOME/tools/bin/sdkmanager "system-images;android-33;google_apis;x86_64"
          $ANDROID_HOME/tools/bin/avdmanager create avd -n test -k "system-images;android-33;google_apis;x86_64" --force
      
      - name: Start Emulator
        run: |
          $ANDROID_HOME/emulator/emulator -avd test -no-window -no-audio -no-boot-anim &
          adb wait-for-device shell 'while [[ -z $(getprop sys.boot_completed) ]]; do sleep 1; done;'
      
      - name: Run Device Tests
        run: dotnet test tests/FlagstoneUI.Core.DeviceTests/ --configuration Release
```

#### 3. Sample Test

```csharp
using Microsoft.Maui.TestUtils.DeviceTests;
using Xunit;

namespace FlagstoneUI.Core.DeviceTests.Controls;

public class FsButtonRenderingTests : DeviceTestsBase
{
    [Fact]
    public async Task Button_renders_with_correct_background_color()
    {
        var button = new FsButton
        {
            Text = "Test",
            BackgroundColor = Colors.Blue
        };

        await RunOnUIThreadAsync(() =>
        {
            var handler = CreateHandler<ButtonHandler>(button);
            var platformView = handler.PlatformView;
            
            Assert.NotNull(platformView);
            // Platform-specific assertions
        });
    }

    [Fact]
    public async Task Button_applies_theme_correctly()
    {
        // Test with Material theme
        var button = new FsButton { Text = "Test" };
        
        await RunOnUIThreadAsync(() =>
        {
            // Apply theme
            // Validate styling
        });
    }
}
```

### Appium Setup (Phase 3)

#### Project Structure

```
tests/
  FlagstoneUI.Appium.Tests/
    FlagstoneUI.Appium.Tests.csproj
    Screens/
      ControlShowcaseScreen.cs
    Tests/
      ButtonInteractionTests.cs
      CardLayoutTests.cs
    Helpers/
      AppiumDriver.cs
      ScreenshotComparer.cs
    app.config.json
```

#### Sample Test

```csharp
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace FlagstoneUI.Appium.Tests;

public class ButtonInteractionTests : IClassFixture<AppiumFixture>
{
    private readonly WindowsDriver<WindowsElement> _driver;

    public ButtonInteractionTests(AppiumFixture fixture)
    {
        _driver = fixture.Driver;
    }

    [Fact]
    public void Button_click_triggers_command()
    {
        var button = _driver.FindElementByAccessibilityId("TestButton");
        button.Click();
        
        var result = _driver.FindElementByAccessibilityId("ClickResult");
        Assert.Equal("Clicked", result.Text);
    }

    [Fact]
    public void Button_disabled_state_prevents_interaction()
    {
        var button = _driver.FindElementByAccessibilityId("DisabledButton");
        Assert.False(button.Enabled);
    }
}
```

## Cost Analysis

### DeviceTests (Phase 1)
- **Development**: 80-120 hours
- **CI Infrastructure**: GitHub Actions minutes (macOS runner for Android: ~$0.08/min)
- **Estimated Monthly CI Cost**: $50-100 (assuming 100 test runs/month @ 5 min each)

### Appium (Phase 3)
- **Development**: 160-240 hours
- **Infrastructure**: Same as DeviceTests + Windows runner for Windows tests
- **Estimated Monthly CI Cost**: $100-200

### Total First Year
- **Development**: 240-360 hours ($24k-$36k if outsourced @$100/hr)
- **CI Costs**: $1,200-$2,400/year
- **Maintenance**: 20-30 hours/month

## Success Metrics

1. **Coverage**: >80% of UI controls have visual rendering tests
2. **CI Reliability**: <5% flaky test rate
3. **Speed**: DeviceTests complete in <10 minutes
4. **Regression Detection**: Catch visual bugs before production
5. **Developer Experience**: Clear patterns, easy to add new tests

## Risks and Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Emulator flakiness in CI | High | Medium | Retry logic, stable emulator images, pre-warmed emulators |
| High CI costs | Medium | Low | Optimize test parallelization, reduce test frequency for dev branches |
| Tests become maintenance burden | High | Medium | Clear patterns, good documentation, refactor brittle tests |
| Platform-specific failures hard to debug | Medium | High | Screenshot capture, detailed logging, local reproduction guides |
| DeviceTests framework changes | Low | Low | Pin versions, monitor MAUI repo for changes |

## Next Steps

1. **Immediate** (This Sprint):
   - [x] Document current state (this document)
   - [ ] Review and approve approach with team
   - [ ] Create epic and stories for Phase 1

2. **Next Sprint**:
   - [ ] Spike: Set up basic DeviceTests project
   - [ ] Spike: Run simple test on Android emulator locally
   - [ ] Document findings and update plan if needed

3. **Following Sprint**:
   - [ ] Begin Phase 1 implementation
   - [ ] Set up CI infrastructure
   - [ ] Create first rendering tests

## References

- [MAUI DeviceTests Framework](https://github.com/dotnet/maui/tree/main/src/TestUtils/src/DeviceTests)
- [Appium Documentation](http://appium.io/docs/en/about-appium/intro/)
- [WinAppDriver](https://github.com/microsoft/WinAppDriver)
- [GitHub Actions: Running Tests with Emulators](https://github.com/ReactiveCircus/android-emulator-runner)
- Related ADR: `docs/Decisions/adr007-ci-ui-test-strategy.md`
