# SolidAvalonia

A library for building reactive Avalonia UI applications using the SolidJS programming model. SolidAvalonia brings the elegance and simplicity of SolidJS reactive patterns to Avalonia UI development.

## Features

- **Reactive primitives**: Signals, memos, and effects inspired by SolidJS
- **Automatic dependency tracking**: Components automatically update when their dependencies change
- **Simple component model**: Extend `SolidControl` to create reactive components
- **Declarative layout helpers**: Build UIs with expressive extension methods
- **No XAML required**: Pure C# approach to building Avalonia applications

## Installation

```bash
dotnet add package Avalonia.Markup.Declarative
# Add SolidAvalonia from NuGet when published
```

## Getting Started

1. Create a new Avalonia application
2. Add the SolidAvalonia library
3. Create a new control by extending `SolidControl`

```csharp
public class MyComponent : SolidControl
{
    public MyComponent()
    {
        // Create signals
        var (count, setCount) = CreateSignal(0);
        
        // Create derived state
        var doubled = CreateMemo(() => count() * 2);
        
        // Create UI with extension methods
        Content = this.Card(
            this.VStack(spacing: 20, margin: 0,
                this.ReactiveText(() => $"Count: {count()}, Doubled: {doubled()}"),
                this.StyledButton("+", () => setCount(count() + 1))
            )
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

### Layout Helpers

Build UIs with chainable extensions:

```csharp
Content = this.Card(
    this.VStack(spacing: 10, margin: 0,
        this.StyledText("Hello World", fontSize: 20),
        this.HStack(spacing: 5, margin: 0,
            this.StyledButton("Cancel"),
            this.StyledButton("OK")
        )
    )
);
```

## Example Applications

See the `/examples` directory for sample applications:

- **Counter**: Simple counter demonstrating signals and effects

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

## License

MIT License