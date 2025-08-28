using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using R3;
using static SolidAvalonia.Solid; // Import Solid functions statically

namespace Counter;

/// <summary>
/// Example of a functional approach to creating Avalonia components using SolidAvalonia
/// </summary>
public static class FunctionalCounterExample
{
    // Functional component that creates a counter display
    public static Control CreateCounterDisplay(int initialCount = 0, int initialStep = 1)
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
        
        // Create a command for handling increments
        var incrementCommand = new ReactiveCommand<int>(increment => {
            setCount(count() + increment);
            setLastUpdateTime(DateTime.Now.ToString("HH:mm:ss.fff"));
        });
        
        // Create a subject for throttling button clicks
        var clickSubject = new Subject<int>();
        
        // Throttle clicks
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
                            .Text("Functional Counter with Throttling")
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
    
    // Example of a simple functional component with props
    public static Control CreateCounter(int initialValue = 0)
    {
        var (count, setCount) = CreateSignal(initialValue);
        
        return new StackPanel()
            .Spacing(10)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                Component(() => new TextBlock()
                    .Text(() => $"Count: {count()}")
                    .FontSize(16)
                    .TextAlignment(TextAlignment.Center)
                ),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(10)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Children(
                        new Button()
                            .Content("-")
                            .OnClick(_ => setCount(count() - 1)),
                        new Button()
                            .Content("+")
                            .OnClick(_ => setCount(count() + 1))
                    )
            );
    }
    
    // Example of conditional rendering with Show.When
    public static Control CreateShowExample()
    {
        var (isVisible, setIsVisible) = CreateSignal(true);
        
        return new StackPanel()
            .Spacing(15)
            .Children(
                new Button()
                    .Content("Toggle Visibility")
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .OnClick(_ => setIsVisible(!isVisible())),
                
                Show.When(
                    isVisible,
                    () => new TextBlock()
                        .Text("This text is visible!")
                        .HorizontalAlignment(HorizontalAlignment.Center),
                    () => new TextBlock()
                        .Text("This is the fallback text.")
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Foreground(Brushes.Gray)
                )
            );
    }
    
    // Example of list rendering with For.Each
    public static Control CreateListExample()
    {
        var (items, setItems) = CreateSignal(new[] { "Item 1", "Item 2", "Item 3" });
        var (newItem, setNewItem) = CreateSignal("New Item");
        
        return new StackPanel()
            .Spacing(15)
            .Children(
                new TextBlock()
                    .Text("List Example")
                    .FontSize(16)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
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
                                setItems(items().Append(newItem()).ToArray());
                                setNewItem("");
                            })
                    ),
                
                // List rendered with For.Each
                Component(() => new StackPanel()
                    .Spacing(5)
                    .Children(
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
                                        .Content("X")
                                        .OnClick(_ => setItems(items().Where(i => i != item).ToArray()))
                                )
                        )
                    )
                )
            );
    }
}