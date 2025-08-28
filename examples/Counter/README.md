# SolidAvalonia Examples

This project contains a collection of examples demonstrating various approaches to building reactive UI components using SolidAvalonia. SolidAvalonia combines the declarative UI building of Avalonia.Markup.Declarative with fine-grained reactivity inspired by SolidJS.

## Getting Started

The examples are organized into a structured catalog to help you explore different approaches to reactive UI development:

### Class-Based Approach

Examples of traditional class-based components that inherit from `Component`:

- **ClassBasedCounter** - A complete counter example using class inheritance with reactive state

### Functional Approach

Examples of functional components using pure functions:

- **Advanced Counter** - A full-featured counter with throttling, reactive styling, and derived state
- **Simple Counter** - A minimal counter example with basic increment/decrement functionality
- **Conditional Rendering** - Example of toggling UI elements using `Show.When`
- **Dynamic List** - Example of dynamic list rendering with `For.Each`

### Composition Approach

Examples of composing reactive components together:

- **Signal Functions** - Using functions that depend on signals for reactive values
- **Shared Signals** - Multiple components sharing the same reactive state
- **Theme Toggle** - Dynamic styling based on reactive theme state

## Code Organization

- **Common/** - Shared components used across examples
  - `SharedComponents.cs` - General-purpose UI components
  - `SignalComponents.cs` - Components specifically designed for signal composition

- **ClassBased/** - Examples using class inheritance
  - `ClassBasedCounter.cs` - Counter component implemented as a class

- **Functional/** - Examples using functional components
  - `CounterExamples.cs` - Collection of functional component examples

- **Composition/** - Examples using component composition
  - `SignalCompositionExamples.cs` - Examples of advanced component composition with signals

## Key Concepts

### Signals

Signals are the core reactive primitive in SolidAvalonia. They are created using `CreateSignal<T>()` and provide a getter and setter:

```csharp
var (count, setCount) = CreateSignal(0);
// Use count() to get the value
// Use setCount(newValue) to set the value
```

### Memos

Memos create derived values that automatically update when their dependencies change:

```csharp
var doubled = CreateMemo(() => count() * 2);
// doubled() will always be count() * 2
```

### Effects

Effects run side-effects when their dependencies change:

```csharp
CreateEffect(() => {
    Console.WriteLine($"Count changed to {count()}");
});
```

### Components

Components wrap controls to make them reactive:

```csharp
Component(() => new TextBlock()
    .Text(() => $"Count: {count()}")
)
```

## Learning Path

1. Start with the **Simple Counter** example to understand basic reactivity
2. Explore **Class-Based Counter** to see how to structure larger components
3. Check out **Signal Functions** and **Shared Signals** to learn about composition
4. Try **Conditional Rendering** and **Dynamic List** for UI patterns
5. See **Theme Toggle** for advanced usage with dynamic styling

Each example includes detailed comments explaining the concepts and techniques being demonstrated.