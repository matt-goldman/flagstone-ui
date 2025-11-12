<div align="center">
  <img src="assets/logov1.svg" alt="Flagstone UI Logo" width="400" height="400">
  <h1>Flagstone UI</h1>
  <p><strong>A customisable UI framework for .NET MAUI</strong></p>
  <h2>⚠️ WARNING! Experimental ⚠️</h2>
</div>

A cross-platform, open-source, community-driven, customisable UI kit and framework for .NET MAUI.

<video src="assets/FlagstoneUI-poc-demo.m4v" style="max-width: 400px" controls autoplay></video>

## What is it?

Flagstone UI is a UI framework for .NET MAUI that provides a common set of styling properties and building blocks (tokens) to help developers create custom themes and styles for their applications. It is designed to be flexible and extensible, allowing developers to create their own custom controls and components that fit their design system and branding.

Flagstone UI also aims to be a fully AI-friendly framework, with plans to integrate AI-powered design tools and features in the future. Much of the AI tooling infrastructure has already been built, with more to come.

It's 2025 (barely at this stage), and many developers are embracing AI tools in their design and development workflows. Flagstone UI has both designer and developer experience in mind, with in-progress and planned features to support both, and integrate with popular AI tools across both design and development.

### What is it NOT?

Flagstone UI is NOT a control or component library. Instead of shipping pre-built controls, Flagstone UI provides a consistent set of stylable properties that can be applied to the built-in .NET MAUI controls. In much the same way that Bootstrap does not provide custom HTML elements, but instead styles the built-in HTML elements, Flagstone UI aims to do the same for .NET MAUI.

### What problems does it solve?

.NET MAUI is a powerful framework for building modern, secure, performant cross-platform mobile and desktop applications. It has some powerful styling capabilities, and gives full access to the native platform APIs, allowing essentially unlimited flexibility when building applications.

However, to fully exploit the capabilities of .NET MAUI, developers often need to reach down into platform APIs via the handler architecture. This is by no means difficult, but for .NET developers without deep platform expertise, it can be a steep learning curve to understand how to achieve the desired look and feel on each platform.

> **Example:** A developer wants to create a text input control with rounded corners, a specific border colour, and a custom background colour. On iOS, this might involve customising the native `UITextField` control via a handler, while on Android, it might involve creating a custom drawable for the `EditText` control. Each platform has its own way of achieving this, and the developer needs to understand the specifics of each platform to implement the desired styling.
>
> Note that this is a very common scenario, and one that every developer faces eventually when building cross-platform applications with .NET MAUI.

Flagstone UI aims to solve this problem by surfacing a common set of properties across all the build-in controls, allowing developers to easily style and customise the appearance of their applications without needing to understand the intricacies of each platform.

Developers used to the web often feel restricted by the limited styling capabilities of .NET MAUI, and Flagstone UI aims to bridge that gap by providing a familiar CSS-like styling experience, but in an idiomatic .NET MAUI way.

Using a common set of building blocks (referred to as "tokens"), developers can create "themes", specific definitions of how controls should appear in their application.

The simplest way to think of it is that Flagstone UI aims to do for .NET MAUI what Bootstrap does for web development - providing a common framework and set of conventions for building beautiful, customisable user interfaces quickly and easily.

### What are the trade-offs?

Because the tokens and properties are explicitly defined in Flagstone UI, by adopting these controls you lose the full flexibility that the native platform controls provide. However, for many applications, the benefits of a consistent and customisable design system outweigh the need for full platform flexibility.

For developers coming from the broader .NET ecosystem, this allows them to achieve a more flexible and bespoke UI, and focus on learning the .NET MAUI layer API, rather than contending with learning the intricacies of each platform at the same time.

For developers who _do_ need to reach down into the platform APIs, the built-in controls are still available, and as you become more familiar with both, you can start to apply some of these customisations to the Flagstone UI controls via handlers as needed, as well as the native controls where necessary.

## What are the features?

- A common set of styling properties that can be applied to built-in .NET MAUI controls.
- A set of "tokens" that define common building blocks for styling controls.
- Support for creating custom themes and styles.
- An extensible architecture that allows developers to create their own custom controls and components.
- A sample application that demonstrates how to use Flagstone UI in a .NET MAUI application
- Comprehensive documentation and examples to help developers get started quickly.

### Completed Work

The foundational token system is in place, as well as three initial controls:

- **`FsButton`** - a stylable button control
- **`FsEntry`** - a stylable text entry control
- **`FsCard`** - a stylable card container control

A sample application is included that demonstrates how to use these controls and apply themes. Several sample themes have been included to showcase the flexibility of the framework.

### Planned Work

- Additional controls, including labels, lists, navigation components, and more
- Theme generator application (web and native) to help developers create custom themes visually
- Improved documentation and examples
- Potential theme sharing gallery
- AI-friendly tools for converting design files (Figma, Adobe XD, etc) into Flagstone UI themes

### In Progress Work

- Additional sample themes
- AI-friendly tooling for generating themes, styles, and full applications
- AI-friendly tools for converting popular web design systems (e.g., Tailwind CSS, Bootstrap) into Flagstone UI themes

## Getting Started

At this stage, Flagstone UI is experimental, and in the proof-of-concept phase. However, you can get started by cloning the repository and exploring the sample application included.

## Getting Involved

**Yes, please!**

I would appreciate any help anyone is willing to provide, but even if you can't contribute code at the moment, the most important thing you could do is tell me whether you think this is a good idea or not, and why. Feedback is crucial at this stage to ensure that the project is heading in the right direction.

If this is something you would use, I would love to hear about it. Equally, if you would _not_ use this, I would also love to hear why not. At the end of the day, if nobody wants this, I'm not going to burn time building something nobody wants!

### Ways to Contribute

- **Feedback**: Share your thoughts on the project direction and design
- **Issues**: Report bugs or suggest new features
- **Code**: Submit pull requests for bug fixes or new features
- **Documentation**: Help improve or expand the documentation
- **Testing**: Try it out and share your experience
