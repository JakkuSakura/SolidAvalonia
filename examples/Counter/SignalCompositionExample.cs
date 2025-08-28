using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using static SolidAvalonia.Solid; // Import Solid functions statically

namespace Counter;

/// <summary>
/// Example of using Component with functions that depend on signals
/// </summary>
public static class SignalCompositionExample
{
    // Simple Text Display component
    public static Control TextDisplay(Func<string> getText, Func<IBrush>? getColor = null)
    {
        return new TextBlock()
            .Text(getText)
            .FontSize(16)
            .Foreground(getColor ?? (() => Brushes.Black))
            .HorizontalAlignment(HorizontalAlignment.Center);
    }
    
    // Status Indicator component
    public static Control StatusIndicator(Func<string> getText, Func<IBrush> getBackground, Func<IBrush>? getForeground = null)
    {
        return new Border()
            .Background(getBackground)
            .CornerRadius(5)
            .Padding(10)
            .Child(new TextBlock()
                .Text(getText)
                .Foreground(getForeground ?? (() => Brushes.Black))
                .HorizontalAlignment(HorizontalAlignment.Center)
            );
    }
    
    // Button Row component
    public static Control ButtonRow(params (string content, Action<RoutedEventArgs> onClick, IBrush? background)[] buttons)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(10)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(buttons.Select(btn => {
                var button = new Button()
                    .Content(btn.content)
                    .MinWidth(80)
                    .OnClick(btn.onClick);
                
                if (btn.background != null)
                {
                    button = button.Background(btn.background);
                }
                
                return (Control)button;
            }).ToArray());
    }
    
    // Signal Function Example
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
                // Header
                new TextBlock()
                    .Text("Signal-based Functions")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Using Component with text display
                Component(() => TextDisplay(getDisplayText, getTextColor)),
                
                // Buttons for count
                ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Reset", _ => setCount(0), null)
                ),
                
                // Color selection buttons
                ButtonRow(
                    ("Red", _ => setColor("Red"), Brushes.LightPink),
                    ("Green", _ => setColor("Green"), Brushes.LightGreen),
                    ("Blue", _ => setColor("Blue"), Brushes.LightBlue)
                )
            );
    }
    
    // Composed Components Example
    public static Control ComposedComponentsExample()
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
                // Header
                new TextBlock()
                    .Text("Shared Signals Example")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Input for name
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Spacing(10)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Your Name:")
                            .VerticalAlignment(VerticalAlignment.Center),
                        
                        new TextBox()
                            .Text(name())
                            .Width(200)
                            .OnTextChanged(e => {
                                var source = e.Source as TextBox;
                                if (source != null) {
                                    setName(source.Text ?? "");
                                }
                            })
                    ),
                
                // Components using shared signals
                Component(() => TextDisplay(getGreeting)),
                Component(() => TextDisplay(
                    getCountDisplay, 
                    () => isEven() ? Brushes.Green : Brushes.Red
                )),
                Component(() => StatusIndicator(
                    () => isEven() ? "Even count" : "Odd count",
                    () => isEven() 
                        ? new SolidColorBrush(Color.FromRgb(220, 255, 220)) 
                        : new SolidColorBrush(Color.FromRgb(255, 220, 220))
                )),
                
                // Control buttons
                ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Reset", _ => setCount(0), null)
                )
            );
    }
    
    // Theme Toggle Example
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
                // Header
                new TextBlock()
                    .Text("Theme Toggle Example")
                    .FontSize(18)
                    .FontWeight(FontWeight.Bold)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                
                // Use components with signal-dependent functions
                Component(() => StatusIndicator(
                    getCountText,
                    () => new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    getCountColor
                )),
                
                Component(() => StatusIndicator(
                    getThemeText,
                    () => new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    getThemeColor
                )),
                
                // Control buttons
                ButtonRow(
                    ("Increment", _ => setCount(count() + 1), null),
                    ("Toggle Theme", _ => setTheme(isDarkTheme() ? "Light" : "Dark"), null)
                ),
                
                // Theme-based background
                Component(() => StatusIndicator(
                    () => $"This background changes with the theme ({theme()})",
                    () => isDarkTheme() 
                        ? new SolidColorBrush(Color.FromRgb(50, 50, 50)) 
                        : new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    () => isDarkTheme() ? Brushes.White : Brushes.Black
                ))
            );
    }
}