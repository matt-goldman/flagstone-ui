# Follow-Up Issues from CI Testing Work

## Issue 1: FsEntry borders prevent user interaction

### Description
When FsEntry has border properties set (either individual edge properties or via Border shorthand), the control becomes disabled/non-interactive - users cannot tap into the field or enter text.

### Steps to Reproduce
1. Run the sample app (FlagstoneUI.SampleApp)
2. Navigate to a page with FsEntry controls that have borders
3. Attempt to tap/click on the entry field
4. Observe that the control does not respond to input

### Expected Behavior
FsEntry should remain fully interactive when borders are applied, allowing users to focus the field and enter text.

### Actual Behavior
FsEntry appears disabled when borders are rendered. The control does not respond to tap/click events.

### Root Cause Hypothesis
Likely causes:
- Border rendering creates an overlay that intercepts hit testing
- Z-order issue where border layers block the input surface
- Event handling being consumed by border decoration elements

### Impact
- **Severity**: High - This breaks core functionality of text input controls
- **Affected Component**: FsEntry
- **Workaround**: Don't use borders on FsEntry (not acceptable for production)

### Related Files
- `src/FlagstoneUI.Core/Controls/FsEntry.xaml.cs`
- `src/FlagstoneUI.Core/Controls/FsEntry.xaml`
- Sample app demonstrates the issue

### Acceptance Criteria
- [ ] FsEntry with borders responds to tap/click events
- [ ] Text input functions normally with all border configurations
- [ ] Border rendering does not interfere with hit testing
- [ ] Add test case to verify interactive behavior with borders (when UI testing infrastructure available)

---

## Issue 2: Theme applies both unified Border and individual edge properties

### Description
The theme/style system is setting both the `Border` shorthand property AND individual border edge properties (BorderTopThickness, BorderTopBrush, etc.) simultaneously. This creates redundant styling and may cause unexpected visual results.

### Steps to Reproduce
1. Run the sample app
2. Inspect FsBorder, FsCard, or FsEntry controls in the visual tree
3. Observe that both `Border` property and individual `BorderTop/Right/Bottom/Left` properties have values

### Expected Behavior
The theme should use EITHER:
- The `Border` shorthand property for uniform borders, OR
- Individual edge properties for asymmetric borders

But not both simultaneously.

### Actual Behavior
Both mechanisms are active, which means:
- Property changes may have unexpected precedence
- Unclear which value "wins" if they conflict
- Unnecessary property churn during initialization

### Root Cause Hypothesis
Likely in base styles:
- `src/FlagstoneUI.Core/Styles/Tokens.xaml` - May define default border values
- `src/FlagstoneUI.Themes.Material/Theme.xaml` - Styles may redundantly set both properties

### Impact
- **Severity**: Medium - Doesn't break functionality but creates technical debt
- **Affected Components**: FsBorder, FsCard, FsEntry (all controls with Border support)
- **Workaround**: None needed - visual output appears correct despite redundancy

### Investigation Tasks
- [ ] Audit `Tokens.xaml` for default border property definitions
- [ ] Review `Theme.xaml` control styles for border property setters
- [ ] Determine intended pattern: shorthand vs. individual properties
- [ ] Document recommended approach in theming guide

### Acceptance Criteria
- [ ] Theme uses consistent border property pattern (shorthand OR individual, not both)
- [ ] Documentation clarifies when to use each approach
- [ ] No redundant property assignments in default styles
- [ ] Visual output remains identical after cleanup

---

## Additional Context

Both issues discovered during:
- PR: [CI test fixes and UI test strategy]
- Related ADR: `docs/Decisions/adr007-ci-ui-test-strategy.md`
- Work session: 2025-12-15

### Testing Notes
- Issue #1 requires manual testing in sample app (UI interaction cannot be automated in current headless CI)
- Issue #2 can be verified via visual tree inspection and style auditing
- Both issues should be addressed before MVP milestone
