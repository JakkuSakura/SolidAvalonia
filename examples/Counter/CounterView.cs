using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia;
using Avalonia.Markup.Declarative;
using R3;

namespace Counter;

/// <summary>
/// Example counter view demonstrating the use of SolidControl with reactive signals, memos, effects, and throttling
/// </summary>
public class CounterView : SolidControl
{
    private string value;
    // If you want to set initial state before Build is called, call base(true) and initialize in the constructor
    public CounterView() : base(true)
    {
        value = "Initialized";
        Initialize();
    }
    protected override object Build()
    {
        // 1. Create signals
        var (count, setCount) = CreateSignal(0);
        var (step, setStep) = CreateSignal(1);
        var (doubledCount, setDoubledCount) = CreateSignal(0);

        // ReactiveCommand for increment/decrement operations

        // 2. Create derived signals and memos
        CreateEffect(() => { setDoubledCount(count() * 2); });

        var isPositive = CreateMemo(() => count() > 0);
        var isEven = CreateMemo(() => count() % 2 == 0);

        rs.CreateEffect(() => { Console.WriteLine($"Count: {count()}, Step: {step()}, Doubled: {doubledCount()}"); });

        // Track last click time for UI display
        var (lastUpdateTime, setLastUpdateTime) = CreateSignal(DateTime.Now.ToString("HH:mm:ss.fff"));
        var incrementCommand = new ReactiveCommand<int>(increment =>
        {
            setCount(count() + increment);
            setLastUpdateTime(DateTime.Now.ToString("HH:mm:ss.fff"));
        });
        // Create a subject for button clicks
        var clickSubject = new Subject<int>();

        // Throttle the clicks and execute command
        clickSubject
            .ThrottleFirst(TimeSpan.FromMilliseconds(500))
            .Subscribe(v => incrementCommand.Execute(v));
        // 3. Build UI
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
                            .Text("Solid-Style Counter with Throttling")
                            .FontSize(20)
                            .FontWeight(FontWeight.Bold)
                            .HorizontalAlignment(HorizontalAlignment.Center),

                        // Display
                        Reactive(() => new TextBlock()
                            .Text(() => $"Count: {count()}, Double: {doubledCount()}")
                            .FontSize(16)
                            .TextAlignment(TextAlignment.Center)
                            .Foreground(() => isPositive() ? Brushes.Green :
                                count() == 0 ? Brushes.Blue : Brushes.Red
                            )
                        ),

                        // Last click time display
                        Reactive(() => new TextBlock()
                            .Text(() => $"Last Updated: {lastUpdateTime()}")
                            .FontSize(14)
                            .TextAlignment(TextAlignment.Center)
                        ),

                        // Step section
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
                                    .OnValueChanged(e => { setStep((int)e.NewValue!.Value); })
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
                                    .OnClick(_ => { clickSubject.OnNext(-step()); }),
                                new Button()
                                    .Content("Reset")
                                    .MinWidth(80)
                                    .OnClick(_ => setCount(0)),
                                new Button()
                                    .Content("+")
                                    .MinWidth(80)
                                    .OnClick(_ => { clickSubject.OnNext(step()); })
                            ),

                        // Status indicator
                        Reactive(() =>
                            new TextBlock()
                                .Text(() =>
                                {
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
}