using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using static SolidAvalonia.Solid;

namespace Counter.UIPatterns;

/// <summary>
/// Example demonstrating conditional rendering techniques.
/// 
/// This example shows:
/// - Using Show.When for conditional content
/// - Using Show.Map for content variation
/// - Inline conditional rendering with ternary operators
/// </summary>
public static class ConditionalRenderingExample
{
    /// <summary>
    /// Creates a component that demonstrates different conditional rendering techniques.
    /// </summary>
    public static Control ConditionalRendering()
    {
        return Component(() =>
        {
            // 1. Create signals for conditional states
            var (isVisible, setIsVisible) = CreateSignal(true);
            var (selectedTab, setSelectedTab) = CreateSignal("A");
            var (count, setCount) = CreateSignal(0);

            // 2. Create derived values for conditional logic
            var isEven = CreateMemo(() => count() % 2 == 0);
            var isPositive = CreateMemo(() => count() > 0);

            // 3. Return the UI component tree
            return new StackPanel()
                .Spacing(20)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Conditional Rendering")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates different techniques for conditional rendering")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),

                    // Section 1: Show.When - toggle visibility
                    new Border()
                        .Padding(15)
                        .CornerRadius(5)
                        .BorderBrush(new SolidColorBrush(Color.FromRgb(200, 200, 220)))
                        .BorderThickness(1)
                        .Child(new StackPanel()
                            .Spacing(10)
                            .Children(
                                new TextBlock()
                                    .Text("1. Show.When - Toggle Visibility")
                                    .FontWeight(FontWeight.Bold)
                                    .HorizontalAlignment(HorizontalAlignment.Center),
                                new Button()
                                    .Content(() => isVisible() ? "Hide Content" : "Show Content")
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .OnClick(_ => setIsVisible(!isVisible())),

                                // Show.When with fallback content
                                Show.When(
                                    isVisible,
                                    () => new TextBlock()
                                        .Text("This content is visible! 👁️")
                                        .HorizontalAlignment(HorizontalAlignment.Center),
                                    () => new TextBlock()
                                        .Text("Content is hidden. Click button to show.")
                                        .HorizontalAlignment(HorizontalAlignment.Center)
                                        .Foreground(Brushes.Gray)
                                )
                            )
                        ),

                    // Section 2: Show.Map - switch between content
                    new Border()
                        .Padding(15)
                        .CornerRadius(5)
                        .BorderBrush(new SolidColorBrush(Color.FromRgb(200, 200, 220)))
                        .BorderThickness(1)
                        .Child(new StackPanel()
                            .Spacing(10)
                            .Children(
                                new TextBlock()
                                    .Text("2. Show.Map - Switch Between Content")
                                    .FontWeight(FontWeight.Bold)
                                    .HorizontalAlignment(HorizontalAlignment.Center),
                                Reactive(() => new StackPanel()
                                    .Orientation(Orientation.Horizontal)
                                    .Spacing(5)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .Children(
                                        new Button()
                                            .Content("Tab A")
                                            .OnClick(_ => setSelectedTab("A"))
                                            .Background(() => (selectedTab() == "A"
                                                ? new SolidColorBrush(Color.FromRgb(180, 180, 220))
                                                : null)!),
                                        new Button()
                                            .Content("Tab B")
                                            .OnClick(_ => setSelectedTab("B"))
                                            .Background(() => (selectedTab() == "B"
                                                ? new SolidColorBrush(Color.FromRgb(180, 180, 220))
                                                : null)!),
                                        new Button()
                                            .Content("Tab C")
                                            .OnClick(_ => setSelectedTab("C"))
                                            .Background(() => (selectedTab() == "C"
                                                ? new SolidColorBrush(Color.FromRgb(180, 180, 220))
                                                : null)!)
                                    )
                                ),

                                // Tab content using conditional rendering
                                Reactive(() =>
                                {
                                    var tab = selectedTab();
                                    switch (tab)
                                    {
                                        case "A":
                                            return new TextBlock()
                                                .Text("This is Tab A content")
                                                .HorizontalAlignment(HorizontalAlignment.Center)
                                                .Foreground(Brushes.DarkBlue);
                                        case "B":
                                            return new TextBlock()
                                                .Text("This is Tab B content")
                                                .HorizontalAlignment(HorizontalAlignment.Center)
                                                .Foreground(Brushes.DarkGreen);
                                        case "C":
                                            return new TextBlock()
                                                .Text("This is Tab C content")
                                                .HorizontalAlignment(HorizontalAlignment.Center)
                                                .Foreground(Brushes.DarkRed);
                                    }

                                    return new TextBlock()
                                        .Text("Select a tab to see content");
                                })
                            )
                        ),

                    // Section 3: Inline conditional rendering
                    new Border()
                        .Padding(15)
                        .CornerRadius(5)
                        .BorderBrush(new SolidColorBrush(Color.FromRgb(200, 200, 220)))
                        .BorderThickness(1)
                        .Child(new StackPanel()
                            .Spacing(10)
                            .Children(
                                new TextBlock()
                                    .Text("3. Inline Conditionals")
                                    .FontWeight(FontWeight.Bold)
                                    .HorizontalAlignment(HorizontalAlignment.Center),

                                // Count controls
                                new StackPanel()
                                    .Orientation(Orientation.Horizontal)
                                    .Spacing(10)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .Children(
                                        new Button()
                                            .Content("-")
                                            .MinWidth(40)
                                            .OnClick(_ => setCount(count() - 1)),
                                        Reactive(() => new TextBlock()
                                            .Text(() => count().ToString())
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Width(40)
                                            .TextAlignment(TextAlignment.Center)),
                                        new Button()
                                            .Content("+")
                                            .MinWidth(40)
                                            .OnClick(_ => setCount(count() + 1))
                                    ),

                                // Text with conditional styling
                                Reactive(() => new TextBlock()
                                    .Text(() =>
                                        $"Count is {(isEven() ? "even" : "odd")} and {(isPositive() ? "positive" : count() == 0 ? "zero" : "negative")}")
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .Foreground(() =>
                                        isEven()
                                            ? (isPositive() ? Brushes.DarkGreen : Brushes.DarkRed)
                                            : (isPositive() ? Brushes.Green : Brushes.Red))
                                )
                            )
                        )
                );
        });
    }
}