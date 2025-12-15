# ADR007: CI UI Test Strategy and Test Filtering

## Status
Accepted

## Context

During CI implementation, we discovered that certain MAUI UI component tests hang indefinitely in GitHub Actions' headless Windows environment. Specifically:

- Test: `Border_bottom_brush_can_be_set` (and potentially other tests creating visual components)
- Symptom: Test hangs for 4+ minutes until CI timeout (5 minute limit)
- Root Cause: Creating `SolidColorBrush` instances in headless CI environment without proper MAUI UI thread/dispatcher initialization
- Local Behavior: All tests pass successfully in ~1-2 seconds with full UI environment

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
- Create MAUI visual components (`SolidColorBrush`, potentially `Border`, `Line` elements)
- Rely on platform-specific UI rendering infrastructure
- Need actual UI thread context beyond what `TestDispatcher` provides

## Decision

### Short-term: Test Filtering in CI

Filter out known problematic tests using xUnit's test filtering:

```yaml
dotnet test --filter "FullyQualifiedName!~Border_bottom_brush_can_be_set"
```

**Rationale**:
- Unblocks CI pipeline immediately
- Maintains test coverage for non-UI tests (majority of test suite)
- Tests still run locally where they work
- Minimal configuration change

**Limitations**:
- Reduces CI test coverage
- Manual maintenance of filter list
- Doesn't solve underlying problem
- May hide regressions in UI component behavior

### Long-term: Proper UI Testing Infrastructure

Implement visual/UI testing using appropriate tools for MAUI applications:

#### Option 1: Appium + MAUI (Recommended)
- **Tool**: Appium with WinAppDriver for Windows
- **Scope**: End-to-end UI testing of actual app screens
- **Coverage**: User interactions, visual regression, cross-platform behavior
- **Timeline**: Post-MVP, as part of comprehensive testing strategy

#### Option 2: MAUI DeviceTests Framework
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
- ❌ Reduced CI test coverage (1 test filtered currently, potentially more)
- ❌ UI component behavior not validated in CI
- ❌ Requires manual maintenance of filter list
- ❌ Potential for regressions in UI components to slip through
- ❌ Additional complexity in test infrastructure

### Neutral
- ℹ️ Deferred comprehensive UI testing to later phase
- ℹ️ Current filter is minimal (1 test), may expand as more UI components added
- ℹ️ Local development workflow unchanged

## Implementation Plan

### Phase 1: Immediate (Current)
- [x] Filter `Border_bottom_brush_can_be_set` in CI workflow
- [ ] Document filtered tests and reasoning
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
