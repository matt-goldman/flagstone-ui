# Test Timeout Configuration

This document describes the timeout configurations implemented to prevent CI test hangs.

## Problem Statement

The CI workflow was experiencing indefinite timeouts during the test step, requiring manual cancellation after extended periods (90+ minutes). This issue was caused by:

1. No timeout limits on test execution
2. Multi-target framework test projects
3. Potential MCP server interaction during test discovery

## Solution

A **three-layer timeout protection** system has been implemented:

### Layer 1: xUnit Test-Level Timeout (10 seconds)

Individual test methods are limited to 10 seconds execution time via `xunit.runner.json`:

```json
{
  "test": {
    "timeout": 10000
  },
  "longRunningTestSeconds": 5
}
```

- **Purpose**: Prevents individual test methods from hanging indefinitely
- **Behavior**: xUnit will abort any test that exceeds 10 seconds
- **Warning Threshold**: Tests taking longer than 5 seconds are flagged as "long-running"

### Layer 2: xUnit Assembly-Level Timeout (2 minutes)

Test assembly execution is limited to 2 minutes via `xunit.runner.json`:

```json
{
  "assembly": {
    "timeout": 120000
  }
}
```

- **Purpose**: Prevents test discovery and assembly execution from hanging
- **Behavior**: xUnit will abort the entire test assembly if it exceeds 2 minutes
- **Rationale**: Current test suites complete in under 1 second; 2 minutes provides ample buffer

### Layer 3: GitHub Actions Step Timeout (5 minutes)

The GitHub Actions test step has a hard timeout via `timeout-minutes`:

```yaml
- name: Test
  timeout-minutes: 5
  run: dotnet test --no-build --verbosity normal --framework net10.0-windows10.0.19041.0
```

- **Purpose**: Final safety net that forcefully terminates hung processes
- **Behavior**: GitHub Actions will kill the entire step after 5 minutes
- **Rationale**: With 2-minute assembly timeouts across multiple test projects, 5 minutes is sufficient

## Configuration Files

### xunit.runner.json

Located in each test project directory:
- `tests/FlagstoneUI.Core.Tests/xunit.runner.json`
- `tests/FlagstoneUI.Blocks.Tests/xunit.runner.json`
- `tests/FlagstoneUI.Themes.Material.Tests/xunit.runner.json`
- `tests/FlagstoneUI.BootstrapConverter.Tests/xunit.runner.json`

Configuration:
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "methodDisplay": "method",
  "diagnosticMessages": true,
  "internalDiagnosticMessages": false,
  "methodDisplayOptions": "all",
  "maxParallelThreads": 1,
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "preEnumerateTheories": true,
  "shadowCopy": false,
  "stopOnFail": false,
  "longRunningTestSeconds": 5,
  "assembly": {
    "timeout": 120000
  },
  "test": {
    "timeout": 10000
  }
}
```

**Important**: These files must be copied to the output directory. Each test project's `.csproj` includes:

```xml
<ItemGroup>
  <None Update="xunit.runner.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Directory.Build.props

Global MSBuild properties for test projects:

```xml
<!-- Test project configuration for timeouts -->
<PropertyGroup Condition="'$(IsTestProject)' == 'true'">
  <!-- VSTest timeout settings: 2 minutes per test assembly -->
  <VSTestTimeout>120000</VSTestTimeout>
  <!-- Disable parallel test execution to prevent resource contention -->
  <VSTestParallel>false</VSTestParallel>
</PropertyGroup>
```

### CI Workflow (.github/workflows/ci.yml)

GitHub Actions configuration:

```yaml
- name: Test
  timeout-minutes: 5
  run: dotnet test --no-build --verbosity normal --framework net10.0-windows10.0.19041.0 --logger "console;verbosity=detailed"
```

## Test Execution Behavior

### Normal Execution
- Tests complete in under 1 second (typical for current test suite)
- No timeout warnings or errors
- Exit code: 0 (success)

### Long-Running Test Warning
- Tests taking 5-10 seconds trigger a warning
- Test still completes normally
- Review test for optimization opportunities

### Test Timeout (10+ seconds)
- xUnit aborts the test
- Test marked as failed
- Assembly continues with remaining tests

### Assembly Timeout (2+ minutes)
- xUnit aborts the entire assembly
- Remaining tests are not executed
- VSTest reports assembly failure

### GitHub Actions Timeout (5+ minutes)
- GitHub Actions forcefully terminates the step
- All processes are killed
- Workflow step marked as failed
- **This is the ultimate safety net to prevent indefinite hangs**

## Platform-Specific Considerations

### Windows (windows-latest)
- Tests target: `net10.0-windows10.0.19041.0`
- All MAUI-based test projects are compatible
- Expected test execution time: < 5 seconds total

### Linux (ubuntu-latest, Copilot)
- Tests target: `net10.0-android`
- Requires Android workload installation
- Android runtime framework may not be available for actual test execution
- Use `FlagstoneUI.BootstrapConverter.Tests` as validation (targets `net10.0`)

## Troubleshooting

### Tests Timing Out

If tests are legitimately timing out:

1. **Check test logic**: Are tests performing expensive operations?
2. **Review MAUI initialization**: Is MauiTestBase initialization hanging?
3. **Examine external dependencies**: Are tests waiting on external resources?
4. **Increase timeouts**: If necessary, increase the values in `xunit.runner.json`

### Timeout Settings Not Taking Effect

1. **Verify file copy**: Ensure `xunit.runner.json` is copied to output directory
2. **Check .csproj**: Verify `<None Update="xunit.runner.json">` entry exists
3. **Clean build**: Run `dotnet clean` followed by `dotnet build`
4. **Review logs**: Check for xUnit timeout configuration messages

### CI Still Hanging

If the CI workflow still hangs despite timeout configurations:

1. **Check GitHub Actions logs**: Look for timeout enforcement messages
2. **Review test discovery**: Check if tests are being discovered correctly
3. **Verify framework selection**: Ensure `--framework` flag specifies correct TFM
4. **Reduce GitHub Actions timeout**: Lower `timeout-minutes` to fail faster

## Best Practices

1. **Keep tests fast**: Individual tests should complete in milliseconds, not seconds
2. **Avoid I/O in tests**: Use in-memory operations where possible
3. **Mock external dependencies**: Don't rely on network, file system, or databases
4. **Review long-running tests**: Investigate any test flagged as long-running (5+ seconds)
5. **Test timeout locally**: Verify timeout configuration works before pushing to CI

## References

- [xUnit Configuration Files](https://xunit.net/docs/configuration-files)
- [GitHub Actions Timeout](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepstimeout-minutes)
- [VSTest Configuration](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
