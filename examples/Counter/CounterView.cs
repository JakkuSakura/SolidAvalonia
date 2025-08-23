using System;
using Avalonia.Media;
using SolidAvalonia;
using SolidAvalonia.Mixins;
using SolidAvalonia.ReactiveSystem;

namespace Counter;

/// <summary>
/// Example counter view demonstrating the use of SolidControl with layout extensions
/// </summary>
public class CounterView : SolidControl
{
    public CounterView() : base()
    {
        InitializeView();
    }
    
    private void InitializeView()
    {
        // Create a reactive signal for the counter value
        var (count, setCount) = CreateSignal(0);
        
        // Create the UI
        Content = this.Centered(
            this.Card(
                this.VStack(spacing: 15, margin: 0,
                    this.StyledText(text: "Counter Example", fontSize: 20, fontWeight: FontWeight.Bold),
                    this.StyledText($"Current count: {count()}"),
                    this.HStack(spacing: 10, margin: 0,
                        this.StyledButton("Decrement", () => setCount(count() - 1)),
                        this.StyledButton("Increment", () => setCount(count() + 1))
                    ),
                    this.StyledButton("Reset", () => setCount(0))
                )
            ),
            maxWidth: 300
        );
        
        // Create an effect that updates the UI when the count changes
        CreateEffect(() => {
            // This will automatically re-run when count() changes
            var currentCount = count();
            Console.WriteLine($"Count changed to: {currentCount}");
        });
    }
}