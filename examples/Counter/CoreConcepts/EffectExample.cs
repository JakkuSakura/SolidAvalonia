using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace Counter.CoreConcepts;

/// <summary>
/// Example demonstrating the usage of effects for side effects.
/// 
/// This example shows:
/// - Creating signals with CreateSignal()
/// - Creating effects with CreateEffect()
/// - How effects automatically run when their dependencies change
/// - Logging state changes and other side effects
/// </summary>
public static class EffectExample
{
    /// <summary>
    /// Creates a component that demonstrates effect usage for side effects.
    /// </summary>
    public static Control SideEffects()
    {
        return Component(() =>
        {
            // 1. Create signals for state
            var (count, setCount) = CreateSignal(0);
            // Using a mutable string to store logs without reactive updates
            var logTextValue = "";
            // Create a signal for the log view
            var (logText, setLogText) = CreateSignal("");
            
            // 2. Helper function to add timestamped log entries
            void AddLog(string message)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var entry = $"[{timestamp}] {message}\n";
                
                // Update the mutable string
                logTextValue = entry + logTextValue;
                
                // Update the signal outside the effect
                // This prevents infinite loop since our modification to 
                // the signal won't trigger the effect we're already in
                setLogText(logTextValue);
                Console.WriteLine(entry);
            }
            
            // 3. Create an effect that logs when count changes
            CreateEffect(() => 
            {
                var currentCount = count(); // Create dependency on count
                AddLog($"Count changed to: {currentCount}");
                
                // The effect will automatically re-run when count changes
            });
            
            // 4. Create an effect that logs different messages based on count value
            CreateEffect(() => 
            {
                var currentCount = count(); // Create dependency on count
                
                if (currentCount == 0)
                {
                    AddLog("Count is zero");
                }
                else if (currentCount > 0)
                {
                    AddLog("Count is positive");
                }
                else
                {
                    AddLog("Count is negative");
                }
                
                // This effect will also re-run when count changes
            });
            
            // Initial log
            AddLog("Component initialized");
            
            // 5. Return the UI component tree
            return new StackPanel()
                .Spacing(15)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Effect Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates how effects run side effects when dependencies change")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Signal display
                    Reactive(() => new TextBlock()
                        .Text(() => $"Count: {count()}")
                        .FontSize(16)
                        .TextAlignment(TextAlignment.Center)
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
                        ),
                    
                    // Divider
                    new Separator()
                        .Height(1)
                        .Margin(new Thickness(0, 10, 0, 10))
                        .Background(new SolidColorBrush(Color.FromRgb(200, 200, 220))),
                    
                    // Log header
                    new TextBlock()
                        .Text("Effect Log")
                        .FontWeight(FontWeight.Bold),
                    
                    // Log display
                    Reactive(() => new TextBox()
                        .Text(logText())
                        .IsReadOnly(true)
                        .AcceptsReturn(true)
                        .FontFamily("Consolas, Menlo, monospace")
                        .FontSize(12)
                        .Height(150)
                        .Width(400)
                    )
                );
        });
    }
}