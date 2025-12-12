# Flagstone UI Roadmap (Updated)

This document provides an updated roadmap based on current implementation progress and lessons learned.

## Current Status (November 2025)

**Phase 1 (MVP) Progress**: ~85% Complete

### Recently Completed ✅

- [x] Solution structure and build system
- [x] Design token system (Tokens.xaml)
- [x] Card control implementation
- [x] Basic CI/CD pipeline
- [x] ThemeLoader utility
- [x] Material theme foundation
- [x] Test project structure
- [x] **FsEntry control** with visual state support
- [x] **FsButton control** (basic implementation)
- [x] **Visual State Pattern** - Theme-driven state styling
- [x] **BorderlessEntry handlers** - Platform-specific native styling removal
- [x] **Sample App** - Working demonstration with Controls showcase
- [x] **CSS-aligned Visual States** - Empty Normal state pattern for future conversion tooling

### Current Focus 🎯

#### Alignment with .NET 10 Launch (November 12, 2025)

The project is being positioned as a **preview/experimental release** to coincide with the .NET 10 launch. The focus has shifted from building more controls to demonstrating the **unique value proposition**: CSS/Tailwind-to-XAML theme conversion tooling.

**Strategic Decision**: MCP tooling development is being prioritized because:

1. Control libraries are commodities - theming automation is differentiation
2. .NET 10 launch provides high-visibility opportunity
3. Theme switching demonstration requires conversion tooling anyway
4. MVP control coverage (Entry, Button, Card) is sufficient to prove the concept
5. Tooling validates architecture before expanding control library

### Completed Milestone: Bootstrap Theme Converter ✅

**Status**: COMPLETE (December 12, 2025)

- [x] **Bootstrap → Flagstone Converter Tool**
  - ✅ Parse Bootstrap SCSS variables (`$primary`, `$font-size-base`, etc.)
  - ✅ Parse CSS classes (`.btn-primary`, `.card`) - limited by ExCSS for Bootstrap 5+
  - ✅ Multi-mode analysis: variables (recommended), css, hybrid
  - ✅ Generate `Tokens.xaml` with 20+ mapped tokens (colors, spacing, typography, borders)
  - ✅ Generate `Styles.xaml` with FsButton variants (Default, OutlinedButton, TextButton)
  - ✅ Generate `Theme.xaml` with merged resource dictionaries
  - ✅ Multi-file SCSS parsing with variable resolution
  - ✅ Dark mode color variant generation
  - ✅ Document mapping strategy and supported variables (ADR005)
  - ✅ Validated with Bootswatch themes (Darkly, Flatly)

- [x] **Converter Documentation**
  - ✅ Library README with architecture, API, examples
  - ✅ CLI README with usage, analysis modes, integration guide
  - ✅ ADR005: Analysis modes architecture decision
  - ✅ Known limitations documented (ExCSS CSS custom properties)

- [ ] **Theme Switching Demo** (Next Priority)
  - Toggle between Material/Modern/Bootstrap-converted themes
  - Demonstrate runtime theme switching
  - Lives in ThemePlayground sample app

### Next Milestone: Bootstrap Converter UI App 🚀

**Target**: Q1 2026

**Purpose**: .NET MAUI desktop app for visual Bootstrap theme conversion

- [ ] **Visual Theme Converter**
  - File picker for local SCSS/CSS files
  - URL input for remote themes (Bootswatch, CDN)
  - Analysis mode selection (variables/css/hybrid)
  - Dark mode strategy selector (auto/manual/none)
  - Real-time preview of converted theme
  - Token inspection (colors, typography, spacing, borders)
  - Export to XAML files

- [ ] **Live Preview**
  - Apply converted theme to sample Flagstone UI controls in-app
  - Toggle between original Bootstrap and converted Flagstone theme
  - Side-by-side comparison
  - Control showcase (FsButton, FsEntry, FsCard, etc.)

- [ ] **Bootswatch Integration**
  - Browse Bootswatch theme catalog
  - One-click theme download and conversion
  - Preview before conversion
  - Save favorite themes

**Benefits**:
- ✅ No command-line knowledge required
- ✅ Visual feedback during conversion
- ✅ Real-time theme validation with Flagstone UI controls
- ✅ Demonstrates .NET MAUI desktop capabilities
- ✅ Showcases Flagstone UI's theming system
- ✅ Lower barrier to entry for designers/non-developers

**Post-.NET 10 Priorities**:

1. Control property audit (document gap analysis)
2. Tailwind → XAML MCP tool (more complex, post-validation)
3. Additional controls based on MCP tooling insights

### Current Blockers 🚫

None! All critical MVP blockers resolved.

**Previously Resolved**:

- ✅ **Resource Loading**: Cross-component XAML resource references fully functional
- ✅ **Missing Core Controls**: FsButton and FsEntry complete
- ✅ **Sample App**: Working demonstration application with showcase

## Updated Phase 1: POC → MVP (Priority: Critical)

**Current**: POC at 60% Complete  
**Next**: MVP after POC validation  
**Target**: Preview release aligned with .NET 10 launch

### Completed Items ✅

1. **Fix Resource System** ✅ COMPLETED
   - ✅ Resolved cross-component XAML resource loading
   - ✅ Enabled themes to properly consume core tokens
   - ✅ Removed temporary workarounds
   - ✅ Implemented proper cross-assembly ResourceDictionary pattern

2. **Implement FsButton** ✅ COMPLETED
   - ✅ Created Button wrapper with neutral base
   - ✅ Implemented platform handlers to strip native styling
   - ✅ Added theme styles to Material theme
   - ✅ Created unit tests
   - ✅ Visual state support for Pressed/Normal states

3. **Implement FsEntry** ✅ COMPLETED
   - ✅ Created Entry wrapper with BorderlessEntry base
   - ✅ Implemented platform handlers (Windows, Android, iOS, MacCatalyst)
   - ✅ Added theme styles in Material theme (Filled and Outlined variants)
   - ✅ Visual state support for Focused/Normal states
   - ✅ Platform-specific fixes (Android padding, Windows focus visuals)
   - Note: Per ADR001, validation states are NOT included - consumers attach MCT ValidationBehavior

4. **Create Functional Sample App** ✅ COMPLETED
   - ✅ Controls gallery (Button, Entry, Card)
   - ✅ Showcase page with examples
   - ✅ Theme system validation
   - ✅ ThemePlayground for theme development

5. **Visual State Architecture** ✅ COMPLETED
   - ✅ Theme-driven visual state pattern documented
   - ✅ CSS-aligned empty Normal state pattern
   - ✅ Official MAUI VisualStateManager.CommonStates integration
   - ✅ Brush properties for gradient/pattern support
   - ✅ Documentation for future CSS/Tailwind conversion

### Critical Path Items (Pre-Launch)

1. **Bootstrap Theme Converter MCP** (Estimated: 2-3 days)
   - Parse Bootstrap variables and generate Tokens.xaml
   - Create theme mapping system
   - Implement MCP server interface
   - Generate theme variants from Bootstrap sources

2. **Theme Switching Demo** (Estimated: 1 day)
   - Implement runtime theme switching in ThemePlayground
   - Demonstrate Material → Bootstrap theme conversion
   - Add theme selection UI

3. **Preview Release Documentation** (Estimated: 1 day)
   - Quickstart guide for preview users
   - Bootstrap converter usage instructions
   - Known limitations and roadmap
   - .NET 10 compatibility notes

**MVP Delivery Target**: November 12, 2025 (.NET 10 launch)

## Phase 1.5: MVP Polish (Priority: High)

**Status**: 0% Complete  
**Target**: Post-.NET 10 launch refinement

### Core Items (Deferred from MVP)

- [ ] **FsSwitch**: Enhanced switch with improved theming
- [ ] **Snackbar**: Complete service implementation with overlay
- [ ] **Control Property Audit**: Document gap analysis for all MAUI controls
  - Evaluate which properties should be wrapped
  - Determine wrap vs subclass strategy for each control
  - Create decision matrix and implementation guide
- [ ] **Tailwind → XAML MCP Tool**: More complex CSS conversion
  - Utility class parsing
  - Visual state generation from pseudo-classes
  - Gradient, shadow, and effect mapping
- [ ] **Enhanced Builder API**: Full configuration options
- [ ] **Improved Theme System**: Dark mode variants
- [ ] **Release Pipeline**: Automated versioning and NuGet publishing

### Quality Improvements

- [ ] **Expanded Testing**: Comprehensive unit and integration tests
- [ ] **Accessibility**: Screen reader support and high contrast themes
- [ ] **Performance**: Optimize resource loading and theme switching
- [ ] **Documentation**: Complete API documentation

**Target Timeline**: 2-3 weeks after MVP completion

## Phase 2: Core Control Set (Priority: Medium)

**Target**: Comprehensive control library for common scenarios

### Form Controls

- [ ] **FsCheckBox**: Themed checkbox with indeterminate state
- [ ] **FsRadioButton**: Radio button with group management
- [ ] **FsPicker**: Enhanced picker with token-driven styling
- [ ] **FsSlider**: Customizable slider control
- [ ] **FsRating**: Star rating control
- [ ] **FormField**: Wrapper with label, helper text, validation

### Display Controls

- [ ] **Badge**: Notification and status indicators
- [ ] **Avatar**: User profile image container
- [ ] **ProgressBar**: Progress indication with theming
- [ ] **Divider**: Separator lines with consistent styling
- [ ] **Tooltip**: Contextual help overlay

### Navigation Controls

- [ ] **AppBar**: Top navigation bar with actions
- [ ] **TabBar**: Bottom navigation tabs
- [ ] **Drawer**: Side navigation panel

### Feedback Controls

- [ ] **Dialog**: Modal dialog service
- [ ] **Toast**: Lightweight notification system

### Additional Features

- [ ] **Modern Theme**: Tailwind-inspired theme alternative
- [ ] **Theme Density**: Compact, default, spacious variants
- [ ] **Motion System**: Animation and transition guidelines

**Target Timeline**: 3-4 months after Phase 1.5

## Phase 3: Blocks and Templates (Priority: Low)

**Target**: Reusable page and section templates

### Authentication Blocks

- [-] **Sign In Form**: WIP: Login with validation
- [ ] **Sign Up Form**: Registration with terms
- [ ] **Forgot Password**: Password reset flow
- [ ] **Onboarding Carousel**: Welcome screens

### CRUD Blocks

- [ ] **List View**: Data listing with search and filters
- [ ] **Detail View**: Item display with actions
- [ ] **Create/Edit Form**: Data entry with validation
- [ ] **Master-Detail**: Combined list and detail layout

### App Chrome Blocks

- [ ] **Settings Page**: Application preferences
- [ ] **About Page**: App information and credits
- [ ] **Profile Page**: User account management

### Content Blocks

- [ ] **Product Grid**: E-commerce product display
- [ ] **Pricing Table**: Feature comparison
- [ ] **Testimonials**: Customer feedback display
- [ ] **FAQ Section**: Expandable question list

### Utility Blocks

- [ ] **Skeleton Loader**: Loading state placeholder
- [ ] **Error Page**: Error handling with retry
- [ ] **Empty State**: No content placeholder

**Target Timeline**: 4-6 months after Phase 2

## Phase 4: Advanced Features (Priority: Future)

**Target**: Advanced theming and community features

### Advanced Theming

- [ ] **Theme Editor**: Visual theme creation tool
- [ ] **Dynamic Theming**: Runtime theme switching
- [ ] **Color Generation**: Automatic palette creation
- [ ] **Custom Font Support**: Typography system expansion

### Community Features

- [ ] **Theme Gallery**: Community theme sharing
- [ ] **Theme Templates**: Starter templates for common brands
- [ ] **Contribution Guidelines**: Community development docs
- [ ] **Plugin System**: Extensibility for third-party controls

### Developer Experience

- [ ] **Visual Studio Templates**: Project templates
- [ ] **Live Preview**: Hot reload for theme development
- [ ] **Documentation Site**: Comprehensive docs with examples
- [ ] **Design System**: Figma/Sketch design resources

## Risk Assessment and Mitigation

### Technical Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Resource loading continues to fail | High | Medium | Investigate MAUI resource patterns, consider alternative approaches |
| Platform handler complexity | Medium | Medium | Start simple, expand gradually, leverage community knowledge |
| Performance with many controls | Medium | Low | Profile early, optimize resource loading, consider virtualization |

### Project Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Scope creep beyond MVP | High | Medium | Strict MVP definition, defer non-critical features |
| Community adoption challenges | Medium | Medium | Focus on documentation and examples |
| Maintenance burden | Medium | Low | Modular design, good test coverage |

## Success Metrics

### Phase 1 (MVP)

- [ ] All core controls functional with theming
- [ ] Sample app demonstrates capabilities
- [ ] Basic documentation available
- [ ] CI/CD pipeline building and testing

### Phase 2

- [ ] 15+ controls available
- [ ] 2+ complete themes (Material, Modern)
- [ ] Community feedback and adoption
- [ ] Performance benchmarks met

### Phase 3

- [ ] 10+ reusable blocks available
- [ ] Sample apps using blocks
- [ ] Community contributions
- [ ] Documentation site launched

## Resource Requirements

### Current Team

- Primary developer: Available for implementation
- Documentation: Need technical writing support
- Design: Consider design system consultation

### Community

- Early adopters for feedback
- Contributors for theme development
- Maintainers for long-term sustainability

## Decision Points

### Short Term (Next 2 weeks)

1. **Resource Loading**: Decide on approach for cross-component resources
2. **Handler Strategy**: Confirm platform handler implementation approach
3. **Testing Strategy**: Define testing standards and tools

### Medium Term (Next 2 months)

1. **Modern Theme**: Scope and design decisions
2. **Block Architecture**: Define block patterns and standards
3. **Community Strategy**: Contribution guidelines and governance

### Long Term (6+ months)

1. **Advanced Features**: Prioritize based on community feedback
2. **Maintenance Model**: Sustainable development approach
3. **Commercial Considerations**: Support and enterprise features

*This roadmap is living document and will be updated based on progress and community feedback.*

**Next Review**: After MVP completion
