using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia;
using SolidAvalonia.Mixins;

namespace Counter;

/// <summary>
/// Example counter view demonstrating the use of SolidControl with reactive signals, memos, and effects
/// </summary>
public class CounterView : SolidControl
{
    public CounterView() : base()
    {
        InitializeView();
    }
    
    private void InitializeView()
    {
        // 1. Create signals
        var (count, setCount) = CreateSignal(0);
        var (step, setStep) = CreateSignal(1);
        var (doubledCount, setDoubledCount) = CreateSignal(0);

        // 2. Create derived signals and memos
        CreateEffect(() => {
            // When count changes, set doubledCount to twice the value of count
            setDoubledCount(count() * 2);
        });
        
        var isPositive = CreateMemo(() => count() > 0);
        var isEven = CreateMemo(() => count() % 2 == 0);
        var displayText = CreateMemo(() => $"Count: {count()}, Double: {doubledCount()}");

        // 3. Build UI using layout helpers
        var header = this.StyledText(
            text: "Solid-Style Counter", 
            fontSize: 20, 
            fontWeight: FontWeight.Bold,
            alignment: HorizontalAlignment.Center
        );

        var display = this.ReactiveText(
            () => displayText(),
            fontSize: 16,
            textAlignment: TextAlignment.Center
        );

        var stepInput = new NumericUpDown
        {
            Value = 1,
            Minimum = 1,
            Maximum = 10,
            CornerRadius = new CornerRadius(6)
        };

        var decrementButton = this.StyledButton("-", () => setCount(count() - step()));
        var resetButton = this.ReactiveButton(
            () => "Reset",
            () => setCount(0)
        );
        var incrementButton = this.StyledButton("+", () => setCount(count() + step()));

        var statusIndicator = this.ReactiveText(
            () => {
                var evenText = isEven() ? "Even" : "Odd";
                var signText = isPositive() ? "Positive" : count() == 0 ? "Zero" : "Negative";
                return $"{evenText} • {signText}";
            },
            fontSize: 14,
            textAlignment: TextAlignment.Center
        );

        // // Create layout using helpers
        var stepSection = this.Section("Step Size:", stepInput, spacing: 5);
        var buttonRow = this.HStack(spacing: 10, margin: 0, decrementButton, resetButton, incrementButton);
        
        var counterCard = this.Card(
            this.VStack(spacing: 15, margin: 20,
                header,
                display,
                stepSection,
                buttonRow,
                statusIndicator
            ),
            padding: 25
        );

        Content = this.Centered(counterCard, maxWidth: 400);

        // 4. Set up reactive effects
        CreateEffect(() => {
            display.Foreground = isPositive() ? Brushes.Green :
                count() == 0 ? Brushes.Blue : Brushes.Red;
        });

        CreateEffect(() => {
            statusIndicator.Foreground = isPositive() ? Brushes.DarkGreen :
                count() == 0 ? Brushes.DarkBlue : Brushes.DarkRed;
        });

        // Event handler for step input
        stepInput.ValueChanged += (_, e) => {
            if (e.NewValue.HasValue)
            {
                setStep((int)e.NewValue.Value);
            }
        };
        
        // Log state changes
        CreateEffect(() => {
            Console.WriteLine($"Count: {count()}, Step: {step()}, Doubled: {doubledCount()}");
        });
    }
}

// Helper extension method
public static class ControlExtensions
{
    public static T Apply<T>(this T control, Action<T> action) where T : Control
    {
        action(control);
        return control;
    }
}