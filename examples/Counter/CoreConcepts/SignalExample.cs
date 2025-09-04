using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace Counter.CoreConcepts;

/// <summary>
/// Example demonstrating the basic usage of signals for reactive state.
/// 
/// This example shows:
/// - Creating signals with CreateSignal()
/// - Reading signal values with signal()
/// - Setting signal values with setSignal()
/// - Using signals in reactive UI with Reactive(() => ...)
/// - Binding signals directly to Avalonia properties with BindSignal
/// </summary>
public static class SignalExample
{
    /// <summary>
    /// Creates a simple counter component using signals for state management.
    /// </summary>
    public static Control SimpleCounter()
    {
        // Use Component to provide context for all signals
        return Component(() =>
        {
            // 1. Create a signal for the counter state
            var (count, setCount) = CreateSignal(0);

            // 2. Return a UI component tree
            return new StackPanel()
                .Spacing(15)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Signal Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates basic signal usage for state management")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Signal display - wrap in Reactive to make it update when signals change
                    Reactive(() => new TextBlock()
                        .Text(() => $"Count value: {count()}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center)
                    ),
                    
                    // Buttons to modify the signal
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(10)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            new Button()
                                .Content("Decrement")
                                .OnClick(_ => setCount(count() - 1)),
                            
                            new Button()
                                .Content("Reset")
                                .OnClick(_ => setCount(0)),
                            
                            new Button()
                                .Content("Increment")
                                .OnClick(_ => setCount(count() + 1))
                        )
                );
        });
    }
    
    /// <summary>
    /// Creates a counter component using signals with direct binding.
    /// Shows how to use BindSignal extension methods for cleaner code.
    /// </summary>
    public static Control SignalBindingExample()
    {
        return Component(() =>
        {
            // 1. Create signals for state
            var (count, setCount) = CreateSignal(0);
            var doubled = () => count() * 2;
            
            // 2. Return a UI component tree with direct signal binding
            return new StackPanel()
                .Spacing(15)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Signal Binding Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),
                        
                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates binding signals directly to Avalonia properties")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Direct signal binding with conversion
                    new TextBlock()
                        .BindSignal(count, TextBlock.TextProperty, c => $"Count: {c}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center),
                        
                    // Direct signal binding with computed value
                    new TextBlock()
                        .BindSignal(doubled, TextBlock.TextProperty, d => $"Doubled: {d}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Buttons to modify the signal
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(10)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            new Button()
                                .Content("Decrement")
                                .OnClick(_ => setCount(count() - 1)),
                            
                            new Button()
                                .Content("Reset")
                                .OnClick(_ => setCount(0)),
                            
                            new Button()
                                .Content("Increment")
                                .OnClick(_ => setCount(count() + 1))
                        )
                );
        });
    }
}