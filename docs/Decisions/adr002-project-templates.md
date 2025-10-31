# ADR: Project Templates and Recommended Stack (MVVM Toolkit, MAUI Community Toolkit, Smart Nav)

## Context

FlagstoneUI aims to provide a complete, opinionated foundation for building modern .NET MAUI applications. While `FlagstoneUI.Core` remains deliberately minimal and dependency-free, project templates can deliver a pre-configured developer experience that aligns with FlagstoneUI's philosophy of clean architecture, modern patterns, and rapid productivity.

These templates serve as an integration layer that demonstrates how to combine FlagstoneUI with complementary community frameworks such as the **MAUI Community Toolkit**, **MVVM Toolkit**, and **Smart Navigation** plugin. They also provide examples of implementing validation, navigation, and binding patterns consistent with best practices.

---

## Decision

### 1. Template Purpose

* **Decision:** Flagstone project templates will provide a **ready-to-run MAUI app scaffold** that includes:

  * FlagstoneUI (core + theme)
  * MVVM Toolkit for state management
  * MAUI Community Toolkit for common behaviors, converters, and validation
  * Smart Nav plugin for streamlined navigation
* **Rationale:** Developers benefit from a curated setup demonstrating how to use Flagstone as the foundation of a production-ready MAUI app, without adding these dependencies to `FlagstoneUI.Core` itself.

### 2. Scope and Structure

* **Templates Provided:**

  * `flagstoneui-app` (basic single-project setup)
  * `flagstoneui-app-shell` (uses Shell navigation)
  * `flagstoneui-app-smartnav` (uses Smart Nav plugin)
* **Included Packages:**

  ```xml
  <PackageReference Include="CommunityToolkit.Mvvm" Version="*" />
  <PackageReference Include="CommunityToolkit.Maui" Version="*" />
  <PackageReference Include="FlagstoneUI" Version="*" />
  <PackageReference Include="SmartNav.Plugin" Version="*" />
  ```

### 3. Validation Strategy in Templates

* **Decision:** Templates will demonstrate validation using the **MAUI Community Toolkit’s `ValidationBehavior`**.
* **Rationale:** Provides a working example of recommended validation patterns without embedding validation logic in FlagstoneUI.Core.
* **Implementation:** Example viewmodels and pages will showcase MCT validation integrated with `FsEntry` controls.

### 4. Extensibility

* **Decision:** The templates will include clearly marked sections and comments guiding developers on how to:

  * Replace or extend Smart Nav with MAUI Shell
  * Swap validation logic (e.g., FluentValidation or custom behaviors)
  * Adjust MVVM architecture layers as needed
* **Rationale:** Encourages customization while maintaining a consistent baseline structure.

### 5. Distribution and Tooling

* **Decision:** Publish templates as NuGet packages installable via the .NET CLI:

  ```bash
  dotnet new install Flagstone.Templates
  ```

  Then create new projects with:

  ```bash
  dotnet new flagstoneui-app -n MyApp
  ```
* **Rationale:** Streamlines adoption and ensures template updates can ship independently of core library releases.

---

## Consequences

| Aspect                   | Impact                                                                        |
| ------------------------ | ----------------------------------------------------------------------------- |
| **Developer experience** | Provides out-of-the-box Flagstone-based app scaffolds with modern patterns.   |
| **Core library purity**  | `FlagstoneUI.Core` remains free of external dependencies.                     |
| **Onboarding**           | New users can start quickly with opinionated defaults.                        |
| **Maintenance**          | Templates must be updated as MAUI, MCT, and MVVM Toolkit evolve.              |
| **Extensibility**        | Developers can modify or create custom templates for different architectures. |

---

## Outcome

The Flagstone templates will form part of the official onboarding experience, offering opinionated but flexible starting points. This ensures that developers adopting FlagstoneUI can experience its intended developer flow — theming, navigation, and validation — without coupling these frameworks into the FlagstoneUI.Core library.

> **Goal:** Provide a modern, extensible, community-aligned application scaffold for .NET MAUI that reflects FlagstoneUI’s design philosophy.
