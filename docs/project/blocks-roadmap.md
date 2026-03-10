# FlagstoneUI Blocks Roadmap

FlagstoneUI.Blocks is a separate package that provides pre-built, composable UI components built on top of FlagstoneUI controls. The goal is to make common mobile app patterns trivially easy to drop into any .NET MAUI app, fully styled and themeable through the standard FlagstoneUI theming system.

This document organises blocks by priority based on real-world mobile app patterns and what developers most commonly need to build.

> **Note:** Blocks drive the controls roadmap too — if a block requires a control that doesn't yet exist, that becomes a signal to prioritise that control. However, FlagstoneUI is fundamentally a theming and styling library; blocks exist to make the library immediately useful, not to become a controls library.

---

## Phase 1: Do Now

These blocks deliver the most immediate value. They cover the scenarios almost every mobile app needs and represent patterns that are universally in demand.

### Authentication

| Block | Description | Status |
|---|---|---|
| **Sign In** | Email/username + password fields, sign-in button, link to sign up | 🔧 Needs polish |
| **Sign Up / Register** | Name, email, password, confirm password fields, register button | 📋 Planned |
| **Forgot Password** | Email field with submit button and instructional copy | 📋 Planned |

The existing `SignInForm` block is a starting point but needs polish — proper spacing, responsive layout, configurable labels, and visual state support (loading, error).

### AI Chat

| Block | Description | Status |
|---|---|---|
| **AI Chat UI** | Full chat interface: user and assistant message bubbles in a `CollectionView`, `FsEditor` input with a send button (configurable icon, defaults to an upward arrow). Supports streamed responses via `IsTyping` indicator. | 📋 Planned |

This is one of the highest-value blocks to deliver given how common AI-powered features are in modern apps. Key design requirements:
- User message bubbles (right-aligned, primary colour)
- Assistant message bubbles (left-aligned, surface/secondary colour)
- `FsEditor` input at the bottom with a send button
- Send button uses a configurable icon (default: upward arrow ↑)
- `IsTyping` indicator for streamed/in-progress responses
- Empty state when no messages yet

> **MCT integration demo opportunity:** The `FsEditorBorderAnimation` from `FlagstoneUI.Integrations.MCT` pairs naturally with this block — an animated gradient border on the input editor while a response is being generated makes a great demo and a compelling showcase of the MCT integration.

### Profile

| Block | Description | Status |
|---|---|---|
| **View Profile** | Avatar, display name, key info fields, edit button | 📋 Planned |
| **Edit Profile** | Editable form for name, email, bio, and avatar change | 📋 Planned |

### Settings

| Block | Description | Status |
|---|---|---|
| **Settings Screen** | Grouped settings rows with toggles, navigation arrows, and section headers | 📋 Planned |

A grouped settings screen is one of the most frequently replicated patterns in mobile apps. Covers toggles (e.g. notifications, dark mode), navigation items (e.g. "Change Password"), and info rows (e.g. "App Version").

### Feedback & States

| Block | Description | Status |
|---|---|---|
| **Empty State** | Illustration placeholder, heading, subheading, and optional call-to-action button | 📋 Planned |
| **Error / Retry** | Error message with retry button; covers network errors and failed loads | 📋 Planned |

---

## Phase 2: Soon

These blocks are high-value additions that cover a wide range of app types. They build on Phase 1 and deliver the next tier of commonly-needed UI patterns.

### Onboarding

| Block | Description |
|---|---|
| **Onboarding Carousel** | Swipeable pages with illustration, heading, subheading, and progress dots. Final page has a "Get Started" CTA. |

Nearly every app with a first-run experience needs this. Pairs well with the Authentication blocks.

### CRUD Patterns

| Block | Description |
|---|---|
| **List Screen** | `CollectionView`-based list with configurable item template, search bar, and pull-to-refresh |
| **Detail Screen** | Read-only detail view with action button(s) in a scrollable layout |
| **Create / Edit Form** | Generic form layout with labelled fields, validation support, and save/cancel actions |

These three blocks together cover the full CRUD lifecycle and are the backbone of most data-driven apps.

### Notifications

| Block | Description |
|---|---|
| **Notification List** | List of notification items with icon, title, body, and timestamp; supports read/unread states |

A notification centre is a near-universal mobile pattern.

### Search

| Block | Description |
|---|---|
| **Search with Results** | Search bar with live-updating results list; supports empty state and loading state |

---

## Phase 3: Nice to Have

These blocks cover useful but less universally-required patterns. They extend FlagstoneUI's usefulness into more specialised app scenarios. Some may require additional controls not yet in FlagstoneUI.Core.

### Content & Media

| Block | Description | Notes |
|---|---|---|
| **Activity Feed / Timeline** | Chronological list with avatar, action text, timestamp, and optional image | Useful for social or productivity apps |
| **Comments / Reviews List** | User comment items with avatar, rating (optional), and body text | Common in e-commerce and content apps |
| **Image Grid / Gallery** | Uniform grid of images with optional caption overlay | Requires platform image handling |

### Commerce

| Block | Description | Notes |
|---|---|---|
| **Product Card** | Image, title, price, and add-to-cart button in a card layout | Core of any e-commerce experience |
| **Product List** | Grid or list of product cards with filtering/sorting | Extends Product Card |
| **Checkout Summary** | Order line items, totals, and confirm action | Terminal step in a purchase flow |

### Utility

| Block | Description | Notes |
|---|---|---|
| **Confirmation Dialog** | Modal/overlay with title, message, confirm, and cancel buttons | Universal utility; MCT's `Popup` control makes this a strong MCT integration candidate |
| **Rating / Review Form** | Star rating selector with optional text field | Common in consumer apps |
| **Dashboard / Stats Cards** | Summary metric cards in a grid, suitable for analytics or dashboards | Requires some data-binding convention |

> **MCT integration opportunity:** The Confirmation Dialog block is a natural fit for MCT's `Popup` control. Rather than building a modal overlay from scratch, an `FlagstoneUI.Integrations.MCT` integration block could wrap `Popup` with FlagstoneUI-themed content and bindable properties (title, message, confirm/cancel commands).

---

## Aspirational

These are things that would genuinely deliver value in line with FlagstoneUI's principles — composable, themeable, cross-platform UI — but are not realistic candidates for inclusion given what this library actually is.

> **MCT Integration note:** Several items in this section could be addressed through the MAUI Community Toolkit rather than being treated as permanently out of scope. Integration/interop with MCT is one of FlagstoneUI's original stated goals — we don't need to reinvent the wheel. Items marked **MCT candidate** could be delivered as an explicit integration block (similar to `FlagstoneUI.Integrations.MCT`) or as a sample/demo showing how to combine MCT's platform-capable controls with FlagstoneUI's theming surface. The Rich Text Editor is the exception: there is no MCT equivalent, but it could be a valuable sample demonstrating how to integrate a third-party rich text component with FlagstoneUI styling.

| Idea | Why It Would Be Valuable | Why It's Aspirational | MCT Path |
|---|---|---|---|
| **Rich Text Editor Block** | In-app content editing is a common requirement for productivity apps | Requires a rich text control that doesn't exist in .NET MAUI; would need to be built from scratch or wrapped from native, well outside the scope of a styling library | No MCT equivalent — consider as a **sample/demo** showing third-party control integration |
| **Video Player Block** | Media playback is a common pattern in content and social apps | Requires deep native integration; no cross-platform abstraction that fits the FlagstoneUI model | **MCT candidate** — MCT's `MediaElement` provides cross-platform video/audio playback; a themed wrapper block is feasible |
| **Camera / Photo Capture Block** | Profile photos, receipts, document scans — all common mobile use cases | Platform-specific APIs with significant complexity; more in the domain of a platform toolkit than a UI styling library | **MCT candidate** — MCT's `CameraView` provides cross-platform camera access; a styled capture UI is feasible |
| **Maps Block** | Location-based features are common, and a styled map container would be useful | Maps require platform-specific map SDKs (Google, Apple, Bing); no portable styling surface exists | Unlikely MCT path — map rendering is owned by the SDK, not a UI layer |
| **Biometric Auth Block** | Biometric prompt UI is increasingly expected | Platform-specific flow; the UI is largely owned by the OS/device | Unlikely MCT path — the OS controls the biometric prompt UI |

---

## Guidance for Block Authors

When implementing blocks:

1. **Blocks are self-contained** — each block should work independently with minimal setup
2. **Respect the theme** — all visual properties should use `DynamicResource` tokens so blocks automatically adopt the active theme
3. **Expose configurable properties** — labels, commands, and key visual properties should be bindable
4. **Provide defaults** — every block should look good out of the box with no configuration beyond adding it to a page
5. **Handle states** — implement loading, empty, and error states where applicable
6. **Use FlagstoneUI controls** — blocks are built exclusively on FsButton, FsCard, FsEntry, FsEditor, and other FlagstoneUI controls; do not use standard MAUI controls directly

---

*Last Updated: March 2026*
