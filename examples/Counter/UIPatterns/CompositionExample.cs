using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace Counter.UIPatterns;

/// <summary>
/// Example demonstrating component composition techniques.
/// 
/// This example shows:
/// - Creating reusable components
/// - Composing components together
/// - Passing signals between components
/// - Creating component functions
/// </summary>
public static class CompositionExample
{
    /// <summary>
    /// Creates a component that demonstrates component composition techniques.
    /// </summary>
    public static Control ComponentComposition()
    {
        return Component(() =>
        {
            // 1. Create shared signals for the example
            var (count, setCount) = CreateSignal(0);
            var (theme, setTheme) = CreateSignal("Light");
            
            // 2. Derived values
            var isDarkTheme = CreateMemo(() => theme() == "Dark");
            var backgroundColor = CreateMemo(() => isDarkTheme() 
                ? new SolidColorBrush(Color.FromRgb(50, 50, 50)) 
                : new SolidColorBrush(Color.FromRgb(240, 240, 240)));
            var textColor = CreateMemo(() => isDarkTheme() 
                ? Brushes.White 
                : Brushes.Black);
            
            // 3. Create component functions
            
            // Display component for count
            Func<Control> createCounterDisplay = () => Reactive(() => new Border()
                .Padding(15)
                .CornerRadius(5)
                .Background(() => isDarkTheme() 
                    ? new SolidColorBrush(Color.FromRgb(70, 70, 70)) 
                    : new SolidColorBrush(Color.FromRgb(230, 230, 230)))
                .Child(new TextBlock()
                    .Text(() => $"Current count: {count()}")
                    .FontSize(16)
                    .TextAlignment(TextAlignment.Center)
                    .Foreground(textColor)
                )
            );
            
            // Theme toggle component
            Func<Control> createThemeToggle = () => new Button()
                .Content(() => $"Switch to {(isDarkTheme() ? "Light" : "Dark")} Theme")
                .HorizontalAlignment(HorizontalAlignment.Center)
                .OnClick(_ => setTheme(isDarkTheme() ? "Light" : "Dark"));
            
            // Counter buttons component
            Func<Control> createCounterButtons = () => new StackPanel()
                .Orientation(Orientation.Horizontal)
                .Spacing(10)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    new Button()
                        .Content("-")
                        .MinWidth(80)
                        .OnClick(_ => setCount(count() - 1)),
                    
                    new Button()
                        .Content("Reset")
                        .MinWidth(80)
                        .OnClick(_ => setCount(0)),
                    
                    new Button()
                        .Content("+")
                        .MinWidth(80)
                        .OnClick(_ => setCount(count() + 1))
                );
            
            // Info component
            Func<string, Control> createInfoBlock = (text) => Reactive(() => new Border()
                .Padding(10)
                .Margin(new Thickness(0, 5, 0, 5))
                .CornerRadius(4)
                .BorderThickness(1)
                .BorderBrush(() => isDarkTheme() 
                    ? new SolidColorBrush(Color.FromRgb(100, 100, 100)) 
                    : new SolidColorBrush(Color.FromRgb(200, 200, 200)))
                .Background(() => isDarkTheme() 
                    ? new SolidColorBrush(Color.FromRgb(60, 60, 60)) 
                    : new SolidColorBrush(Color.FromRgb(250, 250, 250)))
                .Child(new TextBlock()
                    .Text(text)
                    .TextWrapping(TextWrapping.Wrap)
                    .Foreground(textColor)
                )
            );
            
            // 4. Return the composed UI
            return Reactive(() => new Border()
                .Padding(20)
                .Background(backgroundColor)
                .Child(new StackPanel()
                    .Spacing(15)
                    .Children(
                        // Header
                        new TextBlock()
                            .Text("Component Composition")
                            .FontSize(20)
                            .FontWeight(FontWeight.Bold)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Foreground(textColor),

                        // Explanation
                        new TextBlock()
                            .Text("This example demonstrates how to compose reusable components")
                            .FontSize(14)
                            .TextWrapping(TextWrapping.Wrap)
                            .MaxWidth(400)
                            .TextAlignment(TextAlignment.Center)
                            .Foreground(textColor),
                        
                        // Use our component functions
                        createCounterDisplay(),
                        createCounterButtons(),
                        createThemeToggle(),
                        
                        // Info blocks
                        createInfoBlock("Components share the same signals for state, allowing them to stay in sync."),
                        createInfoBlock("The theme affects all components because they all reference the same theme signal.")
                    )
                )
            );
        });
    }
}