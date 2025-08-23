using System;
using Avalonia.Media;
using SolidAvalonia;
using SolidAvalonia.Mixins;

namespace Counter;

/// <summary>
/// Example counter view demonstrating the use of SolidControl with layout extensions
/// and showing how one signal can set another signal
/// </summary>
public class CounterView : SolidControl
{
    public CounterView() : base()
    {
        InitializeView();
    }
    
    private void InitializeView()
    {
        // Create two signals - a primary count and a doubled count
        var (count, setCount) = CreateSignal(0);
        var (doubledCount, setDoubledCount) = CreateSignal(0);
        
        // Create an effect to update doubledCount whenever count changes
        CreateEffect(() => {
            // When count changes, set doubledCount to twice the value of count
            setDoubledCount(count() * 2);
        });
        
        // Create the UI
        Content = this.Centered(
            this.Card(
                this.VStack(spacing: 15, margin: 0,
                    this.StyledText(text: "Counter Example", fontSize: 20, fontWeight: FontWeight.Bold),
                    this.ReactiveText(() => $"Count: {count()}"),
                    this.ReactiveText(() => $"Doubled Count: {doubledCount()}"),
                    this.HStack(spacing: 10, margin: 0,
                        this.StyledButton("Decrement", () => setCount(count() - 1)),
                        this.StyledButton("Increment", () => setCount(count() + 1))
                    ),
                    this.ReactiveButton(() => count() == 0 ? "Reset (Disabled)" : "Reset", 
                        () => setCount(0))
                        .ShowWhen(this, () => count() != 0)
                )
            ),
            maxWidth: 300
        );
        
        // Create an effect that logs when either count changes
        CreateEffect(() => {
            Console.WriteLine($"Count: {count()}, Doubled Count: {doubledCount()}");
        });
    }
}