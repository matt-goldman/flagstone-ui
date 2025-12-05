# 0001-create-mct-integration-project

Create a new class library to host all integration features between Flagstone UI and the MAUI Community Toolkit (MCT).

## Description
This assembly will host:
- Validation adapter(s)
- Token-aware animations
- Future integrations (Popup, Snackbar, etc.)

Project should not depend on themes or samples.

### Requirements
- New project under `src/FlagstoneUI.Integrations.MCT/`
- Reference `CommunityToolkit.Maui`
- Reference `FlagstoneUI.Core`
- Add folder structure:
  - Behaviors/
  - Animations/
  - Extensions/
  - Docs/

## Acceptance Criteria
- Project builds successfully
- No circular dependencies
- No theme references

---

# 0002-validation-delegation-behavior

Implement a generic adapter that allows any MCT ValidationBehavior to work with FsEntry.

## Description
The adapter should:
- Accept any ValidationBehavior
- Attach it to the wrapped Entry inside FsEntry
- Forward validation results (IsValid) to FsEntry via VisualStateManager
- Automatically apply ValidateOnValueChanged flag
- Support theme-level styling for valid/invalid states

## Acceptance Criteria
- MCT validators function correctly with FsEntry
- No validator-specific wrappers required
- Styles target FsEntry, not Entry

## Sample:

```csharp
validator.PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(ValidationBehavior.IsValid))
    {
        ApplyValidationState(validator.IsValid);
    }
};


protected override void OnAttachedTo(FsEntry bindable)
{
    base.OnAttachedTo(bindable);

    _entry = bindable.FindByName<Entry>("WrappedEntry");

    if (_entry == null)
        return;

    if (Behavior is ValidationBehavior validator)
    {
        _validator = validator;

        // Attach validator to the real Entry
        _entry.Behaviors.Add(validator);

        // Ensure correct triggering
        validator.Flags |= ValidationFlags.ValidateOnValueChanged;

        // Listen for validation result changes
        validator.PropertyChanged += OnValidatorPropertyChanged;
    }
}

private void OnValidatorPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(ValidationBehavior.IsValid))
    {
        bool isValid = _validator.IsValid;

        // Apply visual state to FsEntry
        VisualStateManager.GoToState(_fsEntry, isValid ? "Valid" : "Invalid");
    }
}

public class ValidationDelegationBehavior : Behavior<FsEntry>
{
    public Behavior<FsEntry>? Behavior { get; set; }

    private ValidationBehavior? _underlyingValidator;
    private Entry? _innerEntry;
    private FsEntry? _owner;

    protected override void OnAttachedTo(FsEntry bindable)
    {
        _owner = bindable;
        _innerEntry = bindable.FindByName<Entry>("WrappedEntry");

        if (Behavior is ValidationBehavior validator)
        {
            _underlyingValidator = validator;
            _innerEntry.Behaviors.Add(validator);

            HookValidationResult(validator);
        }
    }

    private void HookValidationResult(ValidationBehavior validator)
    {
        // They all raise PropertyChanged for “IsValid”
        validator.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "IsValid")
            {
                bool valid = validator.IsValid;

                VisualStateManager.GoToState(_owner, valid ? "Valid" : "Invalid");
            }
        };
    }

    protected override void OnDetachingFrom(FsEntry bindable)
    {
        if (_innerEntry != null && _underlyingValidator != null)
        {
            _innerEntry.Behaviors.Remove(_underlyingValidator);
        }
    }
}
```

```xml
<fs:FsEntry>
    <fs:ValidatorAdapter Behavior="{x:Reference myEmailValidator}" 
                         InvalidStyle="..."/>
</fs:FsEntry>

<fs:FsEntry>
    <fs:ValidatorAdapter>
        <toolkit:EmailValidationBehavior …/>
    </fs:ValidatorAdapter>
</fs:FsEntry>

```
---

# 0003-fsentry-valid-invalid-vsm-states

Add VSM states to FsEntry so the validation adapter can apply UI changes.

## Description
Add VisualStates:
- Valid
- Invalid

These must be tokenised and overridable by themes.

## Acceptance Criteria
- States included in FsEntry control template
- Default visuals defined
- Themes can override the state styles

---

# 0004-docs-using-mct-validators-with-fsentry

Add developer documentation showing how to apply MCT validators to FsEntry via the adapter.

## Description
Must include:
- Explanation of the adapter
- ValuePropertyName usage
- Examples of valid/invalid styling
- Troubleshooting guide

## Acceptance Criteria
- New documentation page created
- Sample app code matches documentation

---

# 0005-border-gradient-rotation-animation

Create a Flagstone animation (MCT-based) that rotates gradient stops to produce animated borders.

## Description
Animation must work with:
- BorderBrush
- Stroke
- Any gradient brush

Compatible with MCT AnimationBehavior.

## Acceptance Criteria
- Smooth looping gradient rotation
- Verified to work with FsButton, FsEntry, FsPanel

---

# 0006-shimmer-border-animation

Create a shimmer animation that moves a highlight along the border.

## Description
Animation should simulate:
- A light band travelling the edge
- Token-consistent glow

Works with MCT AnimationBehavior.

## Acceptance Criteria
- Shimmer effect visible on supported controls
- Theme-agnostic

---

# 0007-token-gradient-shift-animation

Add animations that transition between tokenised gradients or colors.

## Description
Animations should:
- Accept token keys (e.g., Color.Primary)
- Animate between token values
- Be theme-usable without code

## Acceptance Criteria
- Animations function with AnimationBehavior
- Themes can declare animated variants

---

# 0008-docs-mct-animations

Document how to apply animations to controls using Flagstone animation primitives + MCT behavior.

## Description
Include:
- Gradient rotation examples
- Shimmer animation examples
- Theme-level animated variants (e.g., AI Button)

## Acceptance Criteria
- Clear, step-by-step documentation
- Includes XAML examples

---

# 0009-theme-ai-button-style

Add a theme-defined visual variant for an AI Button using sparkles and animated gradient borders.

## Description
The variant should:
- Target FsButton
- Include sparkles in text or icon
- Apply gradient border brush
- Attach animation behavior
- Use tokens for colors, radius, elevation

## Acceptance Criteria
- Style added as `FsButton.AI`
- Animation included via MCT behavior

---

# 0010-sample-app-ai-button-and-validation

Demonstrate MCT integration features inside the sample app.

## Description
Add:
- AI button with animated border
- FsEntry with validation adapter
- Companion page demonstrating styling + animation

## Acceptance Criteria
- Sample app builds and runs
- Features match documentation

---

# 0011-popup-snackbar-adapters (optional)

Provide helpers to enforce Flagstone tokens on MCT Popup and Snackbar.

## Description
Adapters should:
- Apply tokenised surfaces
- Apply corner radius
- Apply elevation/shadow tokens

## Acceptance Criteria
- Developers can show Popup/Snackbar with Flagstone styling automatically

---

# 0012-token-animation-converters (optional)

Add converters for token-based animated properties.

## Acceptance Criteria
- Converters implemented for color, gradient, opacity, etc.
- Used by token animations

---

# 0013-adr-mct-integration-architecture

Document the architectural reasoning behind the MCT integration assembly.

## Description
Should cover:
- Why integration lives outside Core
- Why ValidationDelegationBehavior exists
- Why animations belong in integrations
- Why themes express component variants (AI Button)

## Acceptance Criteria
- ADR added under /docs/adr/



✅ 1. Create Integration Project: FlagstoneUI.Integrations.MCT

Title: Create new project: FlagstoneUI.Integrations.MCT

Labels: enhancement, infrastructure, area:integrations

Description:
Create a new class library project to host all integration features between Flagstone UI and the MAUI Community Toolkit (MCT).
This assembly should be separate from Core and Themes, and contain all logic that depends on MCT.

Requirements:

New project in src/FlagstoneUI.Integrations.MCT/

Reference CommunityToolkit.Maui

Reference FlagstoneUI.Core

Add folder structure:

Behaviors/

Animations/

Extensions/

Docs/

Add DI extension if needed later (UseFlagstoneMct)

Acceptance Criteria:

Build succeeds

Project compiles with MCT installed

No circular dependencies

Project does not depend on any theme assemblies

🟦 2. Implement ValidationDelegationBehavior (Generic MCT Validator Adapter)

Title: Implement generic ValidationDelegationBehavior for FsEntry

Labels: feature, validation, area:integrations

Description:
Implement a behavior that allows any ValidationBehavior (Email, Regex, Compare, Length, etc.) to be used with FsEntry.
The adapter must attach the validator to the wrapped Entry, monitor its IsValid state, and apply Valid/Invalid states to FsEntry.

Key Requirements:

Accept any MCT ValidationBehavior instance via property or content

Automatically attach validator to the inner Entry

Enforce Flags |= ValidateOnValueChanged

Listen for PropertyChanged("IsValid")

Apply "Valid" / "Invalid" visual states to FsEntry

Support optional ValidStyle / InvalidStyle for FsEntry

Clean removal on detach

Acceptance Criteria:

MCT validators work with FsEntry in sample

No validators required to be wrapped individually

Styles target FsEntry, not Entry

🟦 3. Add Valid / Invalid VisualStates to FsEntry

Title: Add Valid/Invalid VSM states to FsEntry template

Labels: ui, feature, validation, area:core

Description:
Add two visual states to FsEntry:

Valid

Invalid

These states should modify default tokenised styling (border color, thickness, glow, etc.), and be theme-overridable.

Acceptance Criteria:

Visual states exist in control template

Default state changes are visible (e.g., border color change)

Themes can override styling

ValidationDelegationBehavior triggers these states

🟦 4. Documentation: Using MCT validators with FsEntry

Title: Add documentation: Using MAUI Community Toolkit validators with FsEntry

Labels: docs, validation, area:docs

Description:
Create documentation explaining how to:

Add the integration package

Apply MCT validators using ValidationDelegationBehavior

Style Valid vs Invalid states

Override ValuePropertyName when needed

Troubleshoot common validation issues

Acceptance Criteria:

Page added under docs/integrations/mct-validation.md

Includes sample code and screenshots

Sample app updated to match docs

🟦 5. Create BorderGradientRotationAnimation

Title: Implement BorderGradientRotationAnimation (MCT-based)

Labels: feature, animation, area:integrations

Description:
Add a Flagstone animation (built on MCT BaseAnimation) that rotates a gradient along a border or stroke.
Must work with:

BorderBrush

Stroke

Any gradient brush supported by MAUI

Acceptance Criteria:

Animation rotates gradient stops

Works with FsButton, FsPanel, FsEntry

AnimationBehavior can reference it from XAML

Smooth looping supported

🟦 6. Create ShimmerBorderAnimation

Title: Implement ShimmerBorderAnimation

Labels: feature, animation, area:integrations

Description:
Add a shimmer effect animation for borders using MCT BaseAnimation.
Should simulate a moving highlight or glow band around the edge.

Acceptance Criteria:

Shimmer animation visible on any Flagstone control

Is theme-agnostic

Works with AnimationBehavior repeat mode

🟦 7. Add TokenGradientShiftAnimation and token-based animations

Title: Implement TokenGradientShiftAnimation and token-based animations

Labels: animation, feature, tokens, area:integrations

Description:
Implement a set of animations that transition between tokenised gradients or colors.
These allow themes to define animated surface effects without custom code.

Acceptance Criteria:

Animations accept token keys (Color.Primary, Color.Secondary, etc.)

Can animate between tokens

Works with AnimationBehavior in themes

🟦 8. Documentation: Flagstone Animations via MCT

Title: Document how to use Flagstone animations via MCT AnimationBehavior

Labels: docs, animation, area:docs

Description:
Document how to apply animations to controls via MCT’s AnimationBehavior, including gradient rotation, shimmer, glow, and token-based transitions.

Acceptance Criteria:

Document added under docs/integrations/mct-animations.md

Shows XAML examples

Shows theme-level variant creation (animated AI button)

🟦 9. Add theme-level “AI Button” variant

Title: Add AI Button style to theme (e.g., SlatePro)

Labels: feature, theme, design-system

Description:
Add a predefined theme variant for an “AI Button” that uses:

sparkles emoji or icon

gradient border

animated border rotation or shimmer

appropriate padding, corner radius, elevation

Acceptance Criteria:

Style added as x:Key="FsButton.AI"

Works by attaching MCT AnimationBehavior

Demonstrates modern AI UX conventions

🟦 10. Update sample app to showcase AI Button + validation

Title: Add AI Button and validation examples to sample app

Labels: sample, ui, integration

Description:
Update the sample app to demonstrate:

FsEntry with validation via ValidationDelegationBehavior

FsButton with AI variant styling and animated border

MCT AnimationBehavior in action

Acceptance Criteria:

Sample page shows AI button with rotating gradient

FsEntry shows working validation

Code matches documentation

🟦 Optional Future Issues
11. Popup/Snackbar Adapters

Title: Add Flagstone styling adapters for MCT Popup and Snackbar

12. Token Converters

Title: Add converters for animated token-based color/gradient properties

13. ADR

Title: Write ADR for MCT integration layer architecture