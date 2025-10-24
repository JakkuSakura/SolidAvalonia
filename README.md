# SolidAvalonia

[![NuGet](https://img.shields.io/nuget/v/SolidAvalonia?logo=nuget)](https://www.nuget.org/packages/SolidAvalonia)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SolidAvalonia)](https://www.nuget.org/packages/SolidAvalonia)

A library for building reactive Avalonia UI applications using the SolidJS programming model. SolidAvalonia brings the elegance and simplicity of SolidJS reactive patterns to Avalonia UI development.

SolidAvalonia is built on top of [Avalonia.Markup.Declarative](https://github.com/AvaloniaUI/Avalonia.Markup.Declarative), which means all existing classes and extension methods from that library are available for use. For reactive components, signals should be accessed inside the `Reactive()` method and using the extension methods provided for markup classes.

## Features

- **Reactive primitives**: Signals, memos, and effects inspired by SolidJS
- **Automatic dependency tracking**: Components automatically update when their dependencies change
- **Simple component model**: Extend `Component` to create reactive components
- **Declarative layout helpers**: Build UIs with expressive extension methods
- **No XAML required**: Pure C# approach to building Avalonia applications
- **R3 integration**: Seamless integration with R3 for advanced reactive patterns and event handling
- **Throttling support**: Built-in throttling for UI events to optimize performance

## Installation

```bash
# Install the base library that SolidAvalonia extends
dotnet add package Avalonia.Markup.Declarative

# Add SolidAvalonia from NuGet when published
```

To use R3 for reactive programming and throttling features:

```bash
dotnet add package R3
```

## Getting Started

1. Create a new Avalonia application
2. Add the SolidAvalonia library
3. Create a new control by extending `Component`

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace MyApp;

public class MyComponent : Component
{
    protected override object Build()
    {
        // Create signals
        var (count, setCount) = CreateSignal(0);
        
        // Create derived state
        var doubled = CreateMemo(() => count() * 2);
        
        // Create UI with extension methods
        return new StackPanel()
            .Spacing(20)
            .Children(
                // Reactive components automatically update when dependencies change
                Reactive(() => new TextBlock()
                    .Text(() => $"Count: {count()}, Doubled: {doubled()}")
                ),
                new Button()
                    .Content("+")
                    .OnClick(_ => setCount(count() + 1))
            );
        
        // Create effects for side effects
        CreateEffect(() => {
            Console.WriteLine($"Count changed: {count()}");
        });
    }
}
```

## Core Concepts

### Signals

Signals are the reactive primitives that store state:

```csharp
var (count, setCount) = CreateSignal(0);
Console.WriteLine(count()); // Get value: 0
setCount(1); // Set value to 1
```

### Memos

Memos create derived state that updates automatically:

```csharp
var doubled = CreateMemo(() => count() * 2);
// doubled() updates automatically when count changes
```

### Effects

Effects run side effects when dependencies change:

```csharp
CreateEffect(() => {
    Console.WriteLine($"Count is now: {count()}");
});
```

### Reactive Components

Create UI components that automatically update when their dependencies change:

```csharp
Reactive(() => new TextBlock()
    .Text(() => $"Count: {count()}")
    .Foreground(() => count() > 0 ? Brushes.Green : Brushes.Red)
);
```

### Reactive Extensions

SolidAvalonia provides extension methods for common reactive UI patterns:

#### Text

The `Text` extension method allows you to reactively bind a function to a TextBlock's text:

```csharp
using Avalonia.Controls;
using SolidAvalonia.Extensions;
using SolidAvalonia.ReactiveSystem;

// ...

new TextBlock()
    .Text(rs, () => $"Count: {count()}")
```

#### ShowWhen

The `ShowWhen` extension method conditionally shows or hides a control based on a reactive expression:

```csharp
using Avalonia.Controls;
using SolidAvalonia.Extensions;
using SolidAvalonia.ReactiveSystem;

// ...

new Button()
    .Content("Reset")
    .ShowWhen(rs, () => count() > 0)
```

### Layout Helpers

SolidAvalonia leverages Avalonia.Markup.Declarative to build UIs with chainable extensions. All extension methods from Avalonia.Markup.Declarative are available:

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;

// ...

Content = new Border()
    .CornerRadius(10)
    .Padding(25)
    .Child(
        new StackPanel()
            .Spacing(10)
            .Children(
                new TextBlock()
                    .Text("Hello World")
                    .FontSize(20),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(5)
                    .Children(
                        new Button().Content("Cancel"),
                        new Button().Content("OK")
                    )
            )
    );
```

When you need to access signals or reactive state within your Avalonia.Markup.Declarative classes, wrap the components in a `Reactive()` call:

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using SolidAvalonia.Extensions;

// ...

Content = new Border()
    .CornerRadius(10)
    .Padding(25)
    .Child(
        new StackPanel()
            .Spacing(10)
            .Children(
                // Static content uses normal Avalonia.Markup.Declarative extensions
                new TextBlock()
                    .Text("Counter App")
                    .FontSize(20),
                
                // Reactive content that depends on signals
                Reactive(() => new TextBlock()
                    .Text(() => $"Count: {count()}")
                    .Foreground(() => count() > 0 ? Brushes.Green : Brushes.Red)
                ),
                
                // Buttons with event handlers
                new Button()
                    .Content("+")
                    .OnClick(_ => setCount(count() + 1))
            )
    );
```

### Throttling Events with R3

Optimize UI performance with event throttling using R3:

```csharp
using System;
using Avalonia.Controls;
using R3;
using SolidAvalonia;

// ...

// Create a subject for button clicks
var clickSubject = new Subject<int>();

// Throttle the clicks using R3's ThrottleFirst operator
clickSubject
    .ThrottleFirst(TimeSpan.FromMilliseconds(500))
    .Subscribe(v => setCount(count() + v));
    
// Use in button click handlers
new Button()
    .Content("+")
    .OnClick(_ => { clickSubject.OnNext(step()); })
```

## Example Applications

See the `/examples` directory for sample applications:

- **Counter**: A comprehensive example collection demonstrating:
  - Class-based components with inheritance
  - Functional components with pure functions
  - Component composition with shared signals
  - Dynamic theming and styling
  - List rendering with For.Each
  - Conditional rendering with Show.When
  - Throttling for performance optimization

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

## License

MIT License

## Publishing

Use the helper script to build, pack, and publish to NuGet:

```bash
# 1) Export your NuGet API key (or pass via --api-key)
export NUGET_API_KEY=your_nuget_org_api_key

# 2) Build + pack + publish (default to nuget.org)
scripts/publish-nuget.sh --version 0.1.0

# Build/pack without publishing
scripts/publish-nuget.sh --version 0.1.0 --no-push

# Dry-run (show commands only)
scripts/publish-nuget.sh --version 0.1.0 --dry-run
```

Options:
- `--version` overrides the package version (or read from `SolidAvalonia.csproj` if present).
- `--api-key` sets the API key (or use `NUGET_API_KEY`).
- `--source` sets the NuGet source (defaults to `https://api.nuget.org/v3/index.json`).
- `--no-push` to only build and pack.
- `--dry-run` to print commands without executing.

Notes:
- The script passes common NuGet metadata via MSBuild properties (license=MIT, readme=README.md). You can define these in `SolidAvalonia.csproj` to override.
- Output packages are written to `artifacts/nuget/`.
