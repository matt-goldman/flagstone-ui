# ADR007: CI UI Test Strategy and Test Filtering

## Status
Accepted

## Context

During CI implementation, we discovered that certain .NET MAUI UI component tests hang indefinitely in GitHub Actions' headless Windows environment.

### Root Cause Analysis

**Initial Finding**: Tests creating `SolidColorBrush` instances hung in CI
- Test: `Border_bottom_brush_can_be_set` hung for 4+ minutes until CI timeout (5 minute limit)
- Hypothesis: Brush creation requires UI thread/dispatcher initialization

**Deeper Investigation**: The problem is architectural, not brush-specific
- **Actual Root Cause**: FsBorder control overrides `OnSizeAllocated()` method
- Creating **any** FsBorder instance (even without brushes) triggers size allocation
- Headless CI environment lacks layout/rendering infrastructure to complete layout passes
- Control initialization waits indefinitely for size allocation that never completes
- `MauiTestBase` with `TestDispatcher` is insufficient for controls with layout overrides

**Impact**:
- All FsBorderTests (17 tests) disabled - ANY FsBorder instantiation hangs
- Affects controls that override: `OnSizeAllocated`, `Measure`, `Arrange`, or similar layout methods
- Local Behavior: All tests pass successfully in ~1-2 seconds with full UI environment
- CI Behavior: Indefinite hang on first `new FsBorder()` call

**Why Compiler Directives Won't Help**:
- Layout infrastructure is fundamental to control behavior
- Cannot conditionally disable layout without breaking the control
- Problem is environmental (headless), not code logic

### Investigation Summary

1. **Initial Hypothesis**: Debug vs Release configuration difference
   - **Result**: Both configurations work locally, issue is CI-specific
   
2. **Platform Analysis**: 
   - Tests use `MauiTestBase` which initializes a minimal MAUI application context
   - Provides `TestDispatcher` for synchronous execution
   - Works in local development but hangs in GitHub Actions runners
   
3. **Test Breakdown** (as of 2025-12-15):
   - Total: ~53 tests across all projects
   - FlagstoneUI.Core.Tests: 47 tests (includes UI component tests)
   - FlagstoneUI.BootstrapConverter.Tests: 40 tests (no UI dependencies)
   - FlagstoneUI.Blocks.Tests: 1 test
   - FlagstoneUI.Themes.Material.Tests: 1 test

### Problem Scope

The issue affects tests that:
- Instantiate controls with layout method overrides (`OnSizeAllocated`, `Measure`, `Arrange`)
- Create MAUI visual components (`SolidColorBrush`, `Border`, `Line` elements, etc.)
- Rely on platform-specific UI rendering infrastructure
- Need actual UI thread context beyond what `TestDispatcher` provides

**Controls Known to Hang in Headless CI**:
- `FsBorder` - Overrides `OnSizeAllocated()` - 22 tests disabled across multiple test classes:
  - FsBorderTests.cs: 17 tests (entire class commented out)
  - BorderShorthandTests.cs: 3 FsBorder application tests, 1 FsCard test, 1 FsEntry test
- `FsCard` - Property setter tests PASS (does not override layout methods)
- `FsEntry` - Disabled preemptively (not verified if hangs)

**Additional Issues Found**:
- BorderShorthand color parsing failed for named colors ("Red", "Blue", etc.)
- Fixed by using reflection on `Colors` class to support both hex and named colors
- This was a separate code bug unrelated to CI environment

**Key Learning**: ANY test that instantiates FsBorder hangs, regardless of which test file it's in

## Decision

### Short-term: Comment Out Problematic Test Classes

Entire test classes for controls with layout dependencies are commented out with multi-line comments:

```csharp
// DISABLED: FsBorder instantiation hangs in headless CI environment
// FsBorder.OnSizeAllocated() requires layout/rendering infrastructure
// See ADR007 (docs/Decisions/adr007-ci-ui-test-strategy.md)
// TODO: Re-enable when proper UI testing infrastructure is in place
public class FsBorderTests : MauiTestBase
{
    /* ... all tests commented out ... */
}
```

**Rationale**:
- Unblocks CI pipeline immediately
- Maintains test coverage for non-UI tests (majority of test suite)
- Tests remain in codebase for local development/debugging
- Clear documentation of why tests are disabled
- TODO markers for future re-enablement

**Limitations**:
- Reduces CI test coverage for UI controls
- Manual audit needed for each new control
- Doesn't solve underlying problem
- May hide regressions in UI component layout behavior

### Long-term: Proper UI Testing Infrastructure

Implement visual/UI testing using appropriate tools for .NET MAUI applications:

#### Option 1: Appium + .NET MAUI (Recommended)
- **Tool**: Appium with WinAppDriver for Windows
- **Scope**: End-to-end UI testing of actual app screens
- **Coverage**: User interactions, visual regression, cross-platform behavior
- **Timeline**: Post-MVP, as part of comprehensive testing strategy

#### Option 2: .NET MAUI DeviceTests Framework
- **Tool**: Microsoft's `Microsoft.Maui.TestUtils.DeviceTests` 
- **Scope**: Component-level UI tests on actual devices/emulators
- **Coverage**: Control rendering, layout, platform-specific behavior
- **Timeline**: Investigate during MVP phase

#### Option 3: Headless Testing Infrastructure
- **Tool**: Custom headless rendering mock/stub for CI
- **Scope**: Stub out platform-specific UI rendering for unit tests
- **Coverage**: Component API contracts without actual rendering
- **Timeline**: As needed if test coverage gaps identified

## Consequences

### Positive
- ✅ CI pipeline unblocked and functional
- ✅ Non-UI tests (majority) run successfully in CI
- ✅ All tests still run and pass locally
- ✅ Clear path forward for comprehensive UI testing
- ✅ Maintains fast feedback loop for code changes

### Negative
- ❌ Reduced CI test coverage (22 tests disabled: 17 FsBorderTests + 5 application tests)
- ❌ UI component layout behavior not validated in CI
- ❌ May hide regressions in FsBorder, potentially FsEntry if they override layout methods
- ❌ Additional burden on developers to verify UI tests locally before merging

### Neutral
- ℹ️ Deferred comprehensive UI testing to later phase
- ℹ️ Current approach scales: any control with layout overrides gets commented out with ADR reference
- ℹ️ Local development workflow unchanged (tests run fine locally)
- ℹ️ FsCard verified safe (property tests pass, no layout overrides)

## Implementation Plan

### Phase 1: Immediate (Completed)
- [x] Commented out FsBorderTests.cs (17 tests)
- [x] Commented out BorderShorthand application tests (5 tests)
- [x] Fixed color parsing in BorderShorthand for named colors
- [x] Documented decision in ADR007
- [x] Verified FsCard safe (tests pass)
- [ ] Add local-only test category/trait for UI tests
- [ ] Update testing documentation

### Phase 2: Short-term (Next Sprint)
- [ ] Audit all `FlagstoneUI.Core.Tests` for UI dependencies
- [ ] Add `[Trait("Category", "UI")]` to affected tests
- [ ] Update CI filter to exclude entire UI test category
- [ ] Create baseline of what UI tests exist and what they validate

### Phase 3: Medium-term (Post-POC, Pre-MVP)
- [ ] Research MAUI DeviceTests framework
- [ ] Spike: Set up basic Appium test with sample MAUI app
- [ ] Evaluate cost/benefit of each UI testing approach
- [ ] Create UI testing strategy document

### Phase 4: Long-term (Post-MVP)
- [ ] Implement chosen UI testing infrastructure
- [ ] Migrate filtered tests to proper UI test framework
- [ ] Add visual regression testing
- [ ] Document UI testing patterns and best practices

## Alternatives Considered

### Alternative 1: Fix Headless Environment
**Approach**: Configure GitHub Actions runner with UI infrastructure
- ❌ Complex setup (requires Windows UI session, display drivers)
- ❌ Slower CI execution (full UI stack initialization)
- ❌ Unreliable (Windows UI in CI is notoriously flaky)
- ❌ High maintenance overhead

### Alternative 2: Skip All Core.Tests in CI
**Approach**: Only run BootstrapConverter tests in CI
- ❌ Loses too much coverage (47 tests)
- ❌ Non-UI component tests wouldn't run in CI
- ❌ Overly broad solution

### Alternative 3: Mock/Stub Brush Creation
**Approach**: Replace `SolidColorBrush` with test doubles
- ❌ Doesn't test actual MAUI behavior
- ❌ Requires significant test refactoring
- ❌ May miss platform-specific bugs
- ✅ Could be useful for pure unit tests in future

## References

- [GitHub Actions: Windows Runners](https://docs.github.com/en/actions/using-github-hosted-runners/about-github-hosted-runners#supported-runners-and-hardware-resources)
- [xUnit Test Filtering](https://xunit.net/docs/running-tests-in-vs#filtering)
- [Appium for Windows](http://appium.io/docs/en/drivers/windows/)
- [MAUI DeviceTests (Microsoft.Maui repo)](https://github.com/dotnet/maui/tree/main/src/TestUtils/src/DeviceTests)
- CI Logs: `gh_test_log_archive/build/6_Test.txt` (test hanging after 4+ minutes)
- Related: `docs/test-timeout-configuration.md`

## Notes

- Current filter is intentionally narrow (single test) to minimize coverage loss
- Will expand to category-based filtering as more UI tests identified
- Local development unaffected - all tests run and pass
- This ADR may be superseded when comprehensive UI testing strategy is implemented

## Review History

- 2025-12-15: Initial decision - filter single problematic test
- Future: Review after UI testing infrastructure is implemented
