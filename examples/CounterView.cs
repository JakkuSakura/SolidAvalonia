using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SolidAvalonia;

public class CounterView : SolidControl
{
    public CounterView()
    {
        // 1. Create signals
        var (count, setCount) = CreateSignal(0);
        var (step, setStep) = CreateSignal(1);

        // 2. Create derived signals
        var doubleCount = CreateMemo(() => count() * 2);
        var isPositive = CreateMemo(() => count() > 0);
        var isEven = CreateMemo(() => count() % 2 == 0);
        var displayText = CreateMemo(() => $"Count: {count()}, Double: {doubleCount()}");

        // 3. Build UI using layout helpers
        var header = StyledText("Solid-Style Counter", fontSize: 20, fontWeight: FontWeight.Bold, 
                               alignment: Avalonia.Layout.HorizontalAlignment.Center);

        var display = StyledText(fontSize: 16, alignment: Avalonia.Layout.HorizontalAlignment.Center);

        var stepInput = new NumericUpDown 
        { 
            Value = 1, 
            Minimum = 1, 
            Maximum = 10,
            CornerRadius = new Avalonia.CornerRadius(6)
        };

        var decrementButton = StyledButton("-");
        var resetButton = StyledButton("Reset");
        var incrementButton = StyledButton("+");

        var statusIndicator = StyledText(alignment: Avalonia.Layout.HorizontalAlignment.Center);

        // Layout using helpers
        var stepSection = Section("Step Size:", stepInput, spacing: 5);
        var buttonRow = HStack(10, 0, decrementButton, resetButton, incrementButton);
        var counterCard = Card(
            VStack(15, 20, 
                header,
                display,
                stepSection,
                buttonRow,
                statusIndicator
            ),
            background: new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            cornerRadius: 12,
            padding: 25
        );

        Content = Centered(counterCard, maxWidth: 400);

        // 4. Set up reactive effects
        CreateEffect(() => display.Text = displayText());

        CreateEffect(() =>
        {
            display.Foreground = isPositive() ? Brushes.Green : 
                               count() == 0 ? Brushes.Blue : Brushes.Red;
        });

        CreateEffect(() =>
        {
            var evenText = isEven() ? "Even" : "Odd";
            var signText = isPositive() ? "Positive" : count() == 0 ? "Zero" : "Negative";
            statusIndicator.Text = $"{evenText} • {signText}";
            
            statusIndicator.Foreground = isPositive() ? Brushes.DarkGreen : 
                                       count() == 0 ? Brushes.DarkBlue : Brushes.DarkRed;
        });

        // Event handlers
        CreateEventEffect(() =>
        {
            Subscribe(stepInput.GetObservable(NumericUpDown.ValueProperty), 
                     value => setStep(value.HasValue ? (int)value.Value : 1));

            incrementButton.Click += (_, _) => setCount(count() + step());
            decrementButton.Click += (_, _) => setCount(count() - step());
            resetButton.Click += (_, _) => setCount(0);
        });
    }
}
