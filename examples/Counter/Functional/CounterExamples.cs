using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using R3;
using static SolidAvalonia.Solid; // Import Solid functions statically

namespace Counter.Functional;

/// <summary>
/// Examples of the functional approach to creating Avalonia components using SolidAvalonia.
/// 
/// This class demonstrates:
/// - Creating reactive components using pure functions
/// - Declarative UI construction with signals and memos
/// - Using Component() for reactive wrappers
/// - Creating components with controlled state
/// </summary>
public static class CounterExamples
{
    /// <summary>
    /// A full-featured counter component with throttling, created using the functional approach.
    /// 
    /// Features:
    /// - Reactive state with signals
    /// - Derived state with memos
    /// - Throttled user interactions
    /// - Dynamic styling based on state
    /// </summary>
    public static Control AdvancedCounter(int initialCount = 0, int initialStep = 1)
    {
        // Create reactive state
        var (count, setCount) = CreateSignal(initialCount);
        var (step, setStep) = CreateSignal(initialStep);
        
        // Create derived state with memos
        var doubledCount = CreateMemo(() => count() * 2);
        var isPositive = CreateMemo(() => count() > 0);
        var isEven = CreateMemo(() => count() % 2 == 0);
        var (lastUpdateTime, setLastUpdateTime) = CreateSignal(DateTime.Now.ToString("HH:mm:ss.fff"));
        
        // Log state changes with an effect
        CreateEffect(() => {
            Console.WriteLine($"Count: {count()}, Step: {step()}, Doubled: {doubledCount()}");
        });
        
        // Create a command for handling increments with rate limiting
        var incrementCommand = new ReactiveCommand<int>(increment => {
            setCount(count() + increment);
            setLastUpdateTime(DateTime.Now.ToString("HH:mm:ss.fff"));
        });
        
        // Create a subject for throttling button clicks
        var clickSubject = new Subject<int>();
        
        // Throttle clicks to prevent rapid-fire clicking
        clickSubject
            .ThrottleFirst(TimeSpan.FromMilliseconds(500))
            .Subscribe(v => incrementCommand.Execute(v));
        
        // Return the UI component tree
        return new Border()
            .CornerRadius(10)
            .Padding(25)
            .MaxWidth(400)
            .Child(
                new StackPanel()
                    .Spacing(15)
                    .Children(
                        // Header
                        new TextBlock()
                            .Text("Advanced Functional Counter")
                            .FontSize(20)
                            .FontWeight(FontWeight.Bold)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        
                        // Display the count value reactively
                        Component(() => new TextBlock()
                            .Text(() => $"Count: {count()}, Double: {doubledCount()}")
                            .FontSize(16)
                            .TextAlignment(TextAlignment.Center)
                            .Foreground(() => isPositive() ? Brushes.Green :
                                count() == 0 ? Brushes.Blue : Brushes.Red)
                        ),
                        
                        // Show last update time
                        Component(() => new TextBlock()
                            .Text(() => $"Last Updated: {lastUpdateTime()}")
                            .FontSize(14)
                            .TextAlignment(TextAlignment.Center)
                        ),
                        
                        // Step selector
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .Spacing(5)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Children(
                                new TextBlock()
                                    .Text("Step Size:")
                                    .VerticalAlignment(VerticalAlignment.Center),
                                new NumericUpDown()
                                    .Value(step())
                                    .Minimum(1)
                                    .Maximum(10)
                                    .CornerRadius(6)
                                    .Width(100)
                                    .OnValueChanged(e => {
                                        var source = e.Source as NumericUpDown;
                                        if (source != null && e.NewValue.HasValue) {
                                            setStep((int)e.NewValue.Value);
                                        }
                                    })
                            ),
                        
                        // Button row
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .Spacing(10)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Children(
                                new Button()
                                    .Content("-")
                                    .MinWidth(80)
                                    .OnClick(_ => clickSubject.OnNext(-step())),
                                new Button()
                                    .Content("Reset")
                                    .MinWidth(80)
                                    .OnClick(_ => setCount(0)),
                                new Button()
                                    .Content("+")
                                    .MinWidth(80)
                                    .OnClick(_ => clickSubject.OnNext(step()))
                            ),
                        
                        // Status indicator
                        Component(() => new TextBlock()
                            .Text(() => {
                                var evenText = isEven() ? "Even" : "Odd";
                                var signText = isPositive() ? "Positive" : count() == 0 ? "Zero" : "Negative";
                                return $"{evenText} • {signText} • Throttled to 2 clicks/sec";
                            })
                            .FontSize(14)
                            .TextAlignment(TextAlignment.Center)
                            .Foreground(() => isPositive() ? Brushes.Green :
                                count() == 0 ? Brushes.Blue : Brushes.Red)
                        )
                    )
            );
    }
    
    /// <summary>
    /// A simple counter component with minimal features, focused on clarity.
    /// 
    /// Features:
    /// - Basic increment/decrement functionality
    /// - Simple reactive display
    /// - Minimal UI for educational purposes
    /// </summary>
    public static Control SimpleCounter(int initialValue = 0)
    {
        // Create a signal for the count state
        var (count, setCount) = CreateSignal(initialValue);
        
        // Return a simple counter UI
        return new StackPanel()
            .Spacing(10)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                // Header
                new TextBlock()
                    .Text("Simple Counter")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Display the count value reactively
                Component(() => new TextBlock()
                    .Text(() => $"Count: {count()}")
                    .FontSize(16)
                    .TextAlignment(TextAlignment.Center)
                ),
                
                // Control buttons
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(10)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Children(
                        new Button()
                            .Content("-")
                            .MinWidth(80)
                            .OnClick(_ => setCount(count() - 1)),
                        new Button()
                            .Content("+")
                            .MinWidth(80)
                            .OnClick(_ => setCount(count() + 1))
                    )
            );
    }
    
    /// <summary>
    /// An example of conditional rendering using Show.When.
    /// 
    /// Features:
    /// - Toggle visibility of content
    /// - Alternate fallback content
    /// - Reactive UI updates
    /// </summary>
    public static Control ConditionalRenderingExample()
    {
        // Create a signal for visibility state
        var (isVisible, setIsVisible) = CreateSignal(true);
        
        return new StackPanel()
            .Spacing(15)
            .Children(
                // Header
                new TextBlock()
                    .Text("Conditional Rendering")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Toggle button
                new Button()
                    .Content("Toggle Visibility")
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .OnClick(_ => setIsVisible(!isVisible())),
                
                // Conditional content using Show.When
                Show.When(
                    isVisible,
                    () => new TextBlock()
                        .Text("This content is visible! 👁️")
                        .HorizontalAlignment(HorizontalAlignment.Center),
                    () => new TextBlock()
                        .Text("Content is hidden. Click toggle to show.")
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Foreground(Brushes.Gray)
                )
            );
    }
    
    /// <summary>
    /// An example of dynamic list rendering with For.Each.
    /// 
    /// Features:
    /// - Add/remove items from a list
    /// - Dynamic rendering of list items
    /// - Input handling for new items
    /// </summary>
    public static Control DynamicListExample()
    {
        // Create signals for the item list and new item input
        var (items, setItems) = CreateSignal(new[] { "Item 1", "Item 2", "Item 3" });
        var (newItem, setNewItem) = CreateSignal("New Item");
        
        return new StackPanel()
            .Spacing(15)
            .Children(
                // Header
                new TextBlock()
                    .Text("Dynamic List Example")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Description
                new TextBlock()
                    .Text("Add, remove, and manage items in a reactive list")
                    .FontSize(14)
                    .TextAlignment(TextAlignment.Center)
                    .Margin(new Thickness(0, 0, 0, 10)),
                
                // Input for new item
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(10)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Children(
                        new TextBox()
                            .Text(newItem())
                            .Width(200)
                            .OnTextChanged(e => {
                                var source = e.Source as TextBox;
                                if (source != null) {
                                    setNewItem(source.Text ?? "");
                                }
                            }),
                        new Button()
                            .Content("Add")
                            .OnClick(_ => {
                                if (!string.IsNullOrWhiteSpace(newItem())) {
                                    setItems(items().Append(newItem()).ToArray());
                                    setNewItem("");
                                }
                            })
                    ),
                
                // List rendered with For.Each
                Component(() => new StackPanel()
                    .Spacing(5)
                    .Children(
                        // Use For.Each to dynamically render the list items
                        For.Each(
                            items(), 
                            item => new StackPanel()
                                .Orientation(Orientation.Horizontal)
                                .Spacing(10)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .Children(
                                    new TextBlock()
                                        .Text(item)
                                        .VerticalAlignment(VerticalAlignment.Center),
                                    new Button()
                                        .Content("✕")
                                        .Width(30)
                                        .Height(30)
                                        .OnClick(_ => setItems(items().Where(i => i != item).ToArray()))
                                )
                        )
                    )
                ),
                
                // Counter display
                Component(() => new TextBlock()
                    .Text(() => $"Total Items: {items().Length}")
                    .FontSize(14)
                    .TextAlignment(TextAlignment.Center)
                    .Margin(new Thickness(0, 10, 0, 0))
                )
            );
    }
}