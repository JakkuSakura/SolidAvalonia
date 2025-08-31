using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace Counter.CoreConcepts;

/// <summary>
/// Example demonstrating the usage of memos for derived state.
/// 
/// This example shows:
/// - Creating signals with CreateSignal()
/// - Creating derived values with CreateMemo()
/// - How memos automatically update when their dependencies change
/// - Using memos in reactive UI components
/// </summary>
public static class MemoExample
{
    /// <summary>
    /// Creates a component that demonstrates memo usage for derived values.
    /// </summary>
    public static Control DerivedValues()
    {
        return Component(() =>
        {
            // 1. Create signals for base state
            var (count, setCount) = CreateSignal(0);
            var (multiplier, setMultiplier) = CreateSignal(2);
            
            // 2. Create derived values with memos
            var squared = CreateMemo(() => count() * count());
            var multiplied = CreateMemo(() => count() * multiplier());
            var isEven = CreateMemo(() => count() % 2 == 0);
            
            // 3. Log derived value calculations for demonstration
            CreateEffect(() => 
            {
                Console.WriteLine($"Count: {count()}, Squared: {squared()}, " +
                                  $"Multiplied by {multiplier()}: {multiplied()}, " +
                                  $"Is Even: {isEven()}");
            });
            
            // 4. Return the UI component tree
            return new StackPanel()
                .Spacing(15)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Memo Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates how memos create derived values that automatically update")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Signal and memo displays
                    Reactive(() => new TextBlock()
                        .Text(() => $"Count: {count()}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center)
                    ),
                    
                    Reactive(() => new TextBlock()
                        .Text(() => $"Count² = {squared()}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center)
                        .Foreground(Brushes.DarkBlue)
                    ),
                    
                    // Multiplier selector
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(10)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            new TextBlock()
                                .Text("Multiplier:")
                                .VerticalAlignment(VerticalAlignment.Center),
                            
                            new NumericUpDown()
                                .Value(multiplier())
                                .Minimum(1)
                                .Maximum(10)
                                .Width(80)
                                .OnValueChanged(e => setMultiplier((int)e.NewValue!.Value))
                        ),
                    
                    Reactive(() => new TextBlock()
                        .Text(() => $"Count × {multiplier()} = {multiplied()}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center)
                        .Foreground(Brushes.DarkGreen)
                    ),
                    
                    // Even/Odd indicator with reactive styling
                    Reactive(() => new Border()
                        .Padding(8)
                        .CornerRadius(4)
                        .Background(() => isEven() 
                            ? new SolidColorBrush(Color.FromRgb(230, 250, 230)) 
                            : new SolidColorBrush(Color.FromRgb(250, 230, 230)))
                        .Child(new TextBlock()
                            .Text(() => isEven() ? "Count is Even" : "Count is Odd")
                            .Foreground(() => isEven() ? Brushes.DarkGreen : Brushes.DarkRed)
                        )
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
}