using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia.Mixins;
using SolidAvalonia;
using Avalonia.Markup.Declarative;

namespace Counter;

/// <summary>
/// Example counter view demonstrating the use of SolidControl with reactive signals, memos, and effects
/// </summary>
public class CounterView : SolidControl
{

    protected override object Build()
    {
        // 1. Create signals
        var (count, setCount) = rs.CreateSignal(0);
        var (step, setStep) = rs.CreateSignal(1);
        var (doubledCount, setDoubledCount) = rs.CreateSignal(0);

        // 2. Create derived signals and memos
        rs.CreateEffect(() => { setDoubledCount(count() * 2); });

        var isPositive = rs.CreateMemo(() => count() > 0);
        var isEven = rs.CreateMemo(() => count() % 2 == 0);

        // 3. Create reactive text blocks
        var displayText = new TextBlock().Text(rs, () => $"Count: {count()}, Double: {doubledCount()}");
        var statusText = new TextBlock().Text(rs, () =>
        {
            var evenText = isEven() ? "Even" : "Odd";
            var signText = isPositive() ? "Positive" : count() == 0 ? "Zero" : "Negative";
            return $"{evenText} • {signText}";
        });

        // 4. Set up reactive effects for colors
        rs.CreateEffect(() =>
        {
            displayText.Foreground = isPositive() ? Brushes.Green :
                count() == 0 ? Brushes.Blue : Brushes.Red;
        });

        rs.CreateEffect(() =>
        {
            statusText.Foreground = isPositive() ? Brushes.DarkGreen :
                count() == 0 ? Brushes.DarkBlue : Brushes.DarkRed;
        });

        // 5. Log state changes
        rs.CreateEffect(() => { Console.WriteLine($"Count: {count()}, Step: {step()}, Doubled: {doubledCount()}"); });

        // 6. Build UI
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
                            .Text("Solid-Style Counter")
                            .FontSize(20)
                            .FontWeight(FontWeight.Bold)
                            .HorizontalAlignment(HorizontalAlignment.Center),

                        // Display
                        displayText
                            .FontSize(16)
                            .TextAlignment(TextAlignment.Center),

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
                                    .OnValueChanged(e =>
                                    {
                                        if (e.NewValue.HasValue)
                                            setStep((int)e.NewValue.Value);
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
                                    .OnClick(_ => setCount(count() - step())),
                                new Button()
                                    .Content("Reset")
                                    .MinWidth(80)
                                    .OnClick(_ => setCount(0)),
                                new Button()
                                    .Content("+")
                                    .MinWidth(80)
                                    .OnClick(_ => setCount(count() + step()))
                            ),

                        // Status indicator
                        statusText
                            .FontSize(14)
                            .TextAlignment(TextAlignment.Center)
                    )
            );
    }
}