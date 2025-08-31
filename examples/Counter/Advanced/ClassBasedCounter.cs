using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia;
using Avalonia.Markup.Declarative;
using R3;

namespace Counter.Advanced;

/// <summary>
/// Example of a counter component built using the class-based approach with SolidAvalonia.
/// 
/// This example demonstrates:
/// - Creating reactive components by inheriting from Component class
/// - Using signals, memos, and effects within a class component
/// - Breaking down the UI into reusable sub-components with clean separation
/// - Throttling user interactions using Reactive Extensions
/// </summary>
public class ClassBasedCounter : Component
{
    /// <summary>
    /// Creates a component that displays the current count and its doubled value
    /// with reactive styling based on the count value.
    /// </summary>
    private Component CreateCountDisplay(Func<int> count, Func<int> doubledCount, Func<bool> isPositive)
    {
        return Reactive(() => new TextBlock()
            .Text(() => $"Count: {count()}, Double: {doubledCount()}")
            .FontSize(16)
            .TextAlignment(TextAlignment.Center)
            .Foreground(() => isPositive() ? Brushes.Green :
                count() == 0 ? Brushes.Blue : Brushes.Red
            )
        );
    }

    /// <summary>
    /// Creates a component that displays the last update timestamp.
    /// </summary>
    private Component CreateLastUpdateDisplay(Func<string> lastUpdateTime)
    {
        return Reactive(() => new TextBlock()
            .Text(() => $"Last Updated: {lastUpdateTime()}")
            .FontSize(14)
            .TextAlignment(TextAlignment.Center)
        );
    }

    /// <summary>
    /// Creates a component for selecting the increment/decrement step size.
    /// </summary>
    private StackPanel CreateStepSelector(Func<int> step, Action<int> setStep)
    {
        return new StackPanel()
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
                    .OnValueChanged(e => { setStep((int)e.NewValue!.Value); })
            );
    }

    /// <summary>
    /// Creates a row of buttons for incrementing, decrementing, and resetting the counter.
    /// </summary>
    private StackPanel CreateButtonRow(Func<int> step, Subject<int> clickSubject, Action<int> setCount)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(10)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                new Button()
                    .Content("-")
                    .MinWidth(80)
                    .OnClick(_ => { clickSubject.OnNext(-step()); }),
                new Button()
                    .Content("Reset")
                    .MinWidth(80)
                    .OnClick(_ => setCount(0)),
                new Button()
                    .Content("+")
                    .MinWidth(80)
                    .OnClick(_ => { clickSubject.OnNext(step()); })
            );
    }

    /// <summary>
    /// Creates a component that displays the counter's status (even/odd, positive/negative).
    /// </summary>
    private Component CreateStatusIndicator(Func<bool> isEven, Func<bool> isPositive, Func<int> count)
    {
        return Reactive(() =>
            {
                OnCleanup(() => Console.WriteLine("StatusIndicator unmounted"));
                return new TextBlock()
                    .Text(() =>
                    {
                        var evenText = isEven() ? "Even" : "Odd";
                        var signText = isPositive() ? "Positive" : count() == 0 ? "Zero" : "Negative";
                        return $"{evenText} • {signText} • Throttled to 2 clicks/sec";
                    })
                    .FontSize(14)
                    .TextAlignment(TextAlignment.Center)
                    .Foreground(() => isPositive() ? Brushes.Green :
                        count() == 0 ? Brushes.Blue : Brushes.Red);
            }
        );
    }

    /// <summary>
    /// Creates the header component for the counter.
    /// </summary>
    private TextBlock CreateHeader()
    {
        return new TextBlock()
            .Text("Class-based Counter with Throttling")
            .FontSize(20)
            .FontWeight(FontWeight.Bold)
            .HorizontalAlignment(HorizontalAlignment.Center);
    }

    /// <summary>
    /// Builds the component by creating signals, memos, effects, and composing the UI.
    /// </summary>
    protected override object Build()
    {
        // 1. Create signals for state management
        var (count, setCount) = CreateSignal(0);
        var (step, setStep) = CreateSignal(1);
        var (doubledCount, setDoubledCount) = CreateSignal(0);

        // 2. Create derived values with memos and effects
        CreateEffect(() => { setDoubledCount(count() * 2); });

        var isPositive = CreateMemo(() => count() > 0);
        var isEven = CreateMemo(() => count() % 2 == 0);

        // Log state changes to console for debugging
        CreateEffect(() => { Console.WriteLine($"Count: {count()}, Step: {step()}, Doubled: {doubledCount()}"); });

        // Track last click time for UI display
        var (lastUpdateTime, setLastUpdateTime) = CreateSignal(DateTime.Now.ToString("HH:mm:ss.fff"));
        var incrementCommand = new ReactiveCommand<int>(increment =>
        {
            setCount(count() + increment);
            setLastUpdateTime(DateTime.Now.ToString("HH:mm:ss.fff"));
        });

        // Create a subject for throttling button clicks
        var clickSubject = new Subject<int>();

        // Throttle the clicks to prevent rapid-fire clicking
        clickSubject
            .ThrottleFirst(TimeSpan.FromMilliseconds(500))
            .Subscribe(v => incrementCommand.Execute(v));

        // 3. Build UI by composing components
        return new Border()
            .CornerRadius(10)
            .Padding(25)
            .MaxWidth(400)
            .Child(
                new StackPanel()
                    .Spacing(15)
                    .Children(
                        // Header
                        CreateHeader(),

                        // Main counter display
                        CreateCountDisplay(count, doubledCount, isPositive),

                        // Last update timestamp
                        CreateLastUpdateDisplay(lastUpdateTime),

                        // Step size selector
                        CreateStepSelector(step, setStep),

                        // Control buttons
                        CreateButtonRow(step, clickSubject, setCount),

                        // Status indicator
                        CreateStatusIndicator(isEven, isPositive, count)
                    )
            );
    }
}