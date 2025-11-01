# Flagstone UI Roadmap (Updated)

This document provides an updated roadmap based on current implementation progress and lessons learned.

## Current Status (September 2025)

**Phase 1 (MVP) Progress**: ~40% Complete

### Recently Completed ✅

- [x] Solution structure and build system
- [x] Design token system (Tokens.xaml)
- [x] Card control implementation
- [x] Basic CI/CD pipeline
- [x] ThemeLoader utility
- [x] Material theme foundation
- [x] Test project structure

### Current Blockers 🚫

1. **Missing Core Controls**: FsButton needed for MVP (FsEntry ✅ complete)
2. **Sample App**: No working demonstration application

**Previously Resolved**:

- ✅ **Resource Loading**: Cross-component XAML resource references now fully functional

## Updated Phase 1: MVP (Priority: Critical)

**Target**: Functional UI kit with essential controls and theme system

### Critical Path Items

1. **Fix Resource System** ✅ COMPLETED (Estimated: 1-2 days)
   - ✅ Resolved cross-component XAML resource loading
   - ✅ Enabled themes to properly consume core tokens
   - ✅ Removed temporary workarounds
   - ✅ Implemented proper cross-assembly ResourceDictionary pattern

**Note:** May not be necessary. As per [the docs](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/resource-dictionaries?view=net-maui-10.0&viewFallbackFrom=net-maui-8.0#merge-resource-dictionaries-from-other-assemblies):

> When merged ResourceDictionary resources share identical x:Key attribute values, .NET MAUI uses the following resource precedence:
>
> 1. The resources local to the resource dictionary.
> 3. The resources contained in the resource dictionaries that were merged via the MergedDictionaries collection, in the > reverse order they are listed in the MergedDictionaries property.

2. **Implement FsButton** (Estimated: 2-3 days)
   - Create Button subclass with neutral styling
   - Implement platform handlers to strip native styling
   - Add theme styles to Material theme
   - Create unit tests

3. **Implement FsEntry** ✅ COMPLETE
   - [x] Create Entry subclass
   - [x] Implement platform handlers to strip native styling
   - [x] Add theme styles in Material theme
   - Note: Per ADR001, validation states are NOT included - consumers attach MCT ValidationBehavior

4. **Create Functional Sample App** (Estimated: 1-2 days)
   - Basic controls gallery showing FsButton, FsEntry, Card
   - Theme switching demonstration
   - Validation of token system

5. **Quickstart Documentation** (Estimated: 1 day)
   - Getting started guide
   - Basic theming instructions
   - Sample code examples

### Optional MVP Items

- **FsSwitch**: Enhanced switch control (can defer to Phase 1.5)
- **Snackbar**: Service-based overlay messaging (can defer to Phase 1.5)
- **Release Pipeline**: Automated NuGet publishing (can defer)

**MVP Delivery Target**: 2-3 weeks from current state

## Phase 1.5: MVP Polish (Priority: High)

**Target**: Complete MVP feature set and polish

### Core Items

- [ ] **FsSwitch**: Enhanced switch with improved theming
- [ ] **Snackbar**: Complete service implementation with overlay
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
