using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using Counter.Common;
using static SolidAvalonia.Solid;

namespace Counter.Composition;

/// <summary>
/// Examples of using Component with functions that depend on signals and composition patterns.
/// 
/// This class demonstrates:
/// - Creating reusable UI components that work with signals
/// - Composing components together with shared state
/// - Creating and using signal-dependent functions
/// - Theme switching and dynamic styling
/// </summary>
public static class SignalCompositionExamples
{
    /// <summary>
    /// Example demonstrating how to create and use functions that depend on signals.
    /// 
    /// Features:
    /// - Signal-dependent text formatting
    /// - Signal-dependent color computation
    /// - Reactive components using custom functions
    /// </summary>
    public static Control SignalFunctionExample()
    {
        // Create reactive state
        var (count, setCount) = CreateSignal(0);
        var (color, setColor) = CreateSignal("Blue");
        
        // Create a function that depends on signals
        Func<string> getDisplayText = () => $"Count: {count()} with color {color()}";
        
        // Create a function that computes a color from signals
        Func<IBrush> getTextColor = () => color() switch
        {
            "Red" => Brushes.Red,
            "Green" => Brushes.Green,
            "Blue" => Brushes.Blue,
            _ => Brushes.Black
        };
        
        return new StackPanel()
            .Spacing(20)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                // Header with explanation
                new StackPanel()
                    .Spacing(5)
                    .Children(
                        SignalComponents.Header("Signal-based Functions"),
                        new TextBlock()
                            .Text("This example shows how functions can depend on signals and update automatically")
                            .FontSize(14)
                            .TextWrapping(TextWrapping.Wrap)
                            .MaxWidth(400)
                            .TextAlignment(TextAlignment.Center)
                    ),
                
                // Using Component with text display
                Component(() => SignalComponents.TextDisplay(getDisplayText, getTextColor)),
                
                // Buttons for count
                SignalComponents.ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Reset", _ => setCount(0), null)
                ),
                
                // Color selection buttons
                SignalComponents.ButtonRow(
                    ("Red", _ => setColor("Red"), Brushes.LightPink),
                    ("Green", _ => setColor("Green"), Brushes.LightGreen),
                    ("Blue", _ => setColor("Blue"), Brushes.LightBlue)
                )
            );
    }
    
    /// <summary>
    /// Example demonstrating how multiple components can share signals for coordinated updates.
    /// 
    /// Features:
    /// - Multiple components sharing the same signal
    /// - Input field connected to signals
    /// - Derived values with memos
    /// </summary>
    public static Control SharedSignalsExample()
    {
        // Create shared signals that will be used across components
        var (count, setCount) = CreateSignal(0);
        var (name, setName) = CreateSignal("User");
        
        // Create component-specific functions
        Func<string> getGreeting = () => $"Hello, {name()}!";
        Func<string> getCountDisplay = () => $"You clicked {count()} times";
        
        // Create a computed value from signals
        var isEven = CreateMemo(() => count() % 2 == 0);
        
        return new StackPanel()
            .Spacing(20)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                // Header with explanation
                new StackPanel()
                    .Spacing(5)
                    .Children(
                        SignalComponents.Header("Shared Signals Example"),
                        new TextBlock()
                            .Text("Multiple components can share signals to coordinate their state")
                            .FontSize(14)
                            .TextWrapping(TextWrapping.Wrap)
                            .MaxWidth(400)
                            .TextAlignment(TextAlignment.Center)
                    ),
                
                // Input for name
                SignalComponents.LabeledTextInput(
                    "Your Name:", 
                    name(),
                    e => {
                        var source = e.Source as TextBox;
                        if (source != null) {
                            setName(source.Text ?? "");
                        }
                    }
                ),
                
                // Components using shared signals
                Component(() => SignalComponents.TextDisplay(getGreeting)),
                Component(() => SignalComponents.TextDisplay(
                    getCountDisplay, 
                    () => isEven() ? Brushes.Green : Brushes.Red
                )),
                Component(() => SignalComponents.StatusIndicator(
                    () => isEven() ? "Even count" : "Odd count",
                    () => isEven() 
                        ? new SolidColorBrush(Color.FromRgb(220, 255, 220)) 
                        : new SolidColorBrush(Color.FromRgb(255, 220, 220))
                )),
                
                // Control buttons
                SignalComponents.ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Reset", _ => setCount(0), null)
                )
            );
    }
    
    /// <summary>
    /// Example demonstrating theme toggling and dynamic styling with signals.
    /// 
    /// Features:
    /// - Theme state management
    /// - Dynamic styling based on theme
    /// - Component composition with theme awareness
    /// </summary>
    public static Control ThemeToggleExample()
    {
        // Create shared signals
        var (count, setCount) = CreateSignal(0);
        var (theme, setTheme) = CreateSignal("Light");
        
        // Create signal-dependent functions
        Func<string> getCountText = () => $"Current count: {count()}";
        Func<string> getThemeText = () => $"Current theme: {theme()}";
        
        // Create computed values
        Func<bool> isDarkTheme = () => theme() == "Dark";
        
        // Create color functions based on the theme
        Func<IBrush> getCountColor = () => isDarkTheme() ? Brushes.LightGreen : Brushes.DarkGreen;
        Func<IBrush> getThemeColor = () => isDarkTheme() ? Brushes.LightBlue : Brushes.DarkBlue;
        
        return new StackPanel()
            .Spacing(20)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                // Header with explanation
                new StackPanel()
                    .Spacing(5)
                    .Children(
                        SignalComponents.Header("Theme Toggle Example"),
                        new TextBlock()
                            .Text("This example shows how signals can control themes and styling")
                            .FontSize(14)
                            .TextWrapping(TextWrapping.Wrap)
                            .MaxWidth(400)
                            .TextAlignment(TextAlignment.Center)
                    ),
                
                // Use components with signal-dependent functions
                Component(() => SignalComponents.StatusIndicator(
                    getCountText,
                    () => new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    getCountColor
                )),
                
                Component(() => SignalComponents.StatusIndicator(
                    getThemeText,
                    () => new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    getThemeColor
                )),
                
                // Control buttons
                SignalComponents.ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Toggle Theme", _ => setTheme(isDarkTheme() ? "Light" : "Dark"), null)
                ),
                
                // Theme-based background
                Component(() => SignalComponents.StatusIndicator(
                    () => $"This background changes with the theme ({theme()})",
                    () => isDarkTheme() 
                        ? new SolidColorBrush(Color.FromRgb(50, 50, 50)) 
                        : new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    () => isDarkTheme() ? Brushes.White : Brushes.Black
                ))
            );
    }
}