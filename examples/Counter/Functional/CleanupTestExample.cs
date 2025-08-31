using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid; // Import Solid functions statically

namespace Counter.Functional;

/// <summary>
/// Examples demonstrating the OnCleanup functionality in different contexts
/// </summary>
public static class CleanupTestExample
{
    /// <summary>
    /// A component that demonstrates cleanup in effects with signal dependencies
    /// </summary>
    public static Component EffectCleanupExample()
    {
        return Component(() =>
        {
            // Create reactive state
            var (count, setCount) = CreateSignal(0);
            var (logText, setLogText) = CreateSignal("");

            // Add log entry with timestamp
            void AddLog(string message)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var entry = $"[{timestamp}] {message}\n";
                setLogText(entry + logText());
                Console.WriteLine(entry);
            }

            // Effect with cleanup that runs on each re-execution
            CreateEffect(() =>
            {
                var currentCount = count(); // Create dependency on count
                var timerId = 1000 + currentCount; // Unique ID for this effect execution

                AddLog($"Effect started with count={currentCount}, timer ID={timerId}");

                // Set up a resource that needs cleanup (simulated timer)
                var timer = new System.Timers.Timer(1000);
                timer.Start();

                // Register cleanup to be called when:
                // 1. Effect re-runs (count changes)
                // 2. Component unmounts
                OnCleanup(() =>
                {
                    timer.Stop();
                    timer.Dispose();
                    AddLog($"Cleaned up timer for count={currentCount}, timer ID={timerId}");
                });
            });

            // Component-level cleanup needs to be registered within a reactive context
            CreateEffect(() =>
            {
                // This effect will run once when the component is created
                OnCleanup(() => { AddLog("Component-level cleanup executed on unmount"); });
            });

            // Return the UI component tree
            return new Border()
                .CornerRadius(10)
                .Padding(20)
                .MaxWidth(500)
                .Background(new SolidColorBrush(Color.FromRgb(240, 240, 250)))
                .Child(
                    new StackPanel()
                        .Spacing(15)
                        .Children(
                            // Header
                            new TextBlock()
                                .Text("Effect Cleanup Example")
                                .FontSize(20)
                                .FontWeight(FontWeight.Bold)
                                .HorizontalAlignment(HorizontalAlignment.Center),

                            // Display the count value
                            Component(() => new TextBlock()
                                .Text(() => $"Count: {count()}")
                                .FontSize(16)
                                .TextAlignment(TextAlignment.Center)
                            ),

                            // Button to increment count and trigger effect re-run
                            new Button()
                                .Content("Increment Count (Triggers Effect Re-run)")
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .OnClick(_ =>
                                {
                                    setCount(count() + 1);
                                    AddLog($"Button clicked, count updated to {count()}");
                                }),

                            // Divider
                            new Separator()
                                .Height(1)
                                .Margin(new Thickness(0, 10, 0, 10))
                                .Background(new SolidColorBrush(Color.FromRgb(200, 200, 220))),

                            // Log header
                            new TextBlock()
                                .Text("Log (newest at top)")
                                .FontWeight(FontWeight.Bold)
                                .Margin(new Thickness(0, 0, 0, 5)),

                            // Log entries
                            Component(() => new TextBox()
                                .Text(logText())
                                .IsReadOnly(true)
                                .AcceptsReturn(true)
                                .FontFamily("Consolas, Menlo, monospace")
                                .FontSize(12)
                                .MaxHeight(200)
                            )
                        )
                );
        });
    }

    /// <summary>
    /// A component that demonstrates nested components with cleanup
    /// </summary>
    public static Component NestedComponentsExample()
    {
        return Component(() =>
        {
            // Create signals for controlling visibility
            var (showChild, setShowChild) = CreateSignal(true);


            // Add log entry with timestamp
            void AddLog(string message)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var entry = $"[{timestamp}] {message}\n";
                Console.WriteLine(entry);
            }

            // Create a stable child component reference using CreateMemo
            // This ensures we don't create a new component on every render
            var childComponent = Component(() =>
                {
                    // Log when component is created
                    AddLog("Child component created");

                    // Register cleanup that runs when unmounted
                    OnCleanup(() => { AddLog("Child component cleanup executed"); });

                    // Create effect inside child component
                    CreateEffect(() =>
                    {
                        AddLog("Child component effect executed");

                        // Register cleanup that runs when effect re-runs or component unmounts
                        OnCleanup(() => { AddLog("Child component effect cleanup executed"); });
                    });

                    return new Border()
                        .Padding(15)
                        .CornerRadius(5)
                        .Background(new SolidColorBrush(Color.FromRgb(220, 240, 220)))
                        .Child(
                            new StackPanel()
                                .Spacing(10)
                                .Children(
                                    new TextBlock()
                                        .Text("Child Component")
                                        .FontWeight(FontWeight.Bold)
                                        .HorizontalAlignment(HorizontalAlignment.Center),
                                    new TextBlock()
                                        .Text("This component has cleanup handlers")
                                        .HorizontalAlignment(HorizontalAlignment.Center)
                                )
                        );
                }
            );

            // Component-level cleanup needs to be registered within a reactive context
            CreateEffect(() =>
            {
                // This effect will run once when the component is created
                OnCleanup(() => { AddLog("Parent component-level cleanup executed on unmount"); });
            });

            // Return the parent component
            return new StackPanel()
                .Spacing(15)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Nested Components Cleanup Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Toggle button
                    new Button()
                        .Content(() => showChild() ? "Unmount Child Component" : "Mount Child Component")
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .OnClick(_ =>
                        {
                            setShowChild(!showChild());
                            AddLog($"Child component visibility toggled to {showChild()}");
                        }),

                    // Conditional rendering of child component
                    // Use the memoized component to prevent recreation on each render
                    Show.When(
                        showChild,
                        () => childComponent
                    ),

                    // Divider
                    new Separator()
                        .Height(1)
                        .Margin(new Thickness(0, 10, 0, 10))
                        .Background(new SolidColorBrush(Color.FromRgb(200, 200, 220))),

                    // Log header
                    new TextBlock()
                        .Text("Log (newest at top)")
                        .FontWeight(FontWeight.Bold)
                        .Margin(new Thickness(0, 0, 0, 5))
                );
        });
    }
}