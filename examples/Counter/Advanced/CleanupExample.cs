using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;

namespace Counter.Advanced;

/// <summary>
/// Example demonstrating cleanup and lifecycle management.
/// 
/// This example shows:
/// - Using OnCleanup to register cleanup functions
/// - How cleanup works with components and effects
/// - Managing resources properly in reactive contexts
/// </summary>
public static class CleanupExample
{
    /// <summary>
    /// Creates a component that demonstrates cleanup in different contexts.
    /// </summary>
    public static Control LifecycleManagement()
    {
        return Component(() =>
        {
            // 1. Create signals for state
            var (showChild, setShowChild) = CreateSignal(true);
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

            // 3. Create a child component with cleanup
            var childComponent = Component(() =>
            {
                // Log creation
                AddLog("Child component created");

                // Register component cleanup
                OnCleanup(() => { AddLog("Child component cleanup executed"); });

                // Create an effect with its own cleanup
                CreateEffect(() =>
                {
                    // This effect depends on count
                    var currentCount = count();
                    AddLog($"Child effect executed with count={currentCount}");

                    // Simulate resource allocation (e.g., timer, subscription)
                    var timerId = new Random().Next(1000, 9999);
                    AddLog($"Child effect allocated resource with ID={timerId}");

                    // Register cleanup for the effect
                    OnCleanup(() => { AddLog($"Child effect cleanup: releasing resource ID={timerId}"); });
                });

                return new Border()
                    .Padding(15)
                    .CornerRadius(5)
                    .Background(new SolidColorBrush(Color.FromRgb(230, 245, 230)))
                    .Child(new StackPanel()
                        .Spacing(10)
                        .Children(
                            new TextBlock()
                                .Text("Child Component")
                                .FontWeight(FontWeight.Bold)
                                .HorizontalAlignment(HorizontalAlignment.Center),
                            new TextBlock()
                                .Text("This component has cleanup handlers")
                                .HorizontalAlignment(HorizontalAlignment.Center),
                            Reactive(() => new TextBlock()
                                .Text(() => $"Current count: {count()}")
                                .HorizontalAlignment(HorizontalAlignment.Center)
                            ),
                            new Button()
                                .Content("Increment from Child")
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .OnClick(_ => setCount(count() + 1))
                        )
                    );
            });

            // 4. Create a parent component effect with cleanup
            CreateEffect(() =>
            {
                AddLog("Parent effect executed");

                // Register cleanup for parent effect
                OnCleanup(() => { AddLog("Parent effect cleanup executed"); });
            });

            // 5. Return the UI component tree
            return new StackPanel()
                .Spacing(15)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Cleanup and Lifecycle Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates cleanup handling in components and effects")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(400)
                        .TextAlignment(TextAlignment.Center),

                    // Controls
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(10)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            new Button()
                                .Content(() => showChild() ? "Unmount Child" : "Mount Child")
                                .OnClick(_ =>
                                {
                                    setShowChild(!showChild());
                                    AddLog($"Child visibility toggled to {showChild()}");
                                }),
                            new Button()
                                .Content("Increment Count")
                                .OnClick(_ =>
                                {
                                    setCount(count() + 1);
                                    AddLog($"Count incremented to {count()}");
                                })
                        ),

                    // Child component (conditional)
                    Show.When(showChild, () => childComponent),

                    // Divider
                    new Separator()
                        .Height(1)
                        .Margin(new Thickness(0, 10, 0, 10))
                        .Background(new SolidColorBrush(Color.FromRgb(200, 200, 220))),

                    // Log header
                    new TextBlock()
                        .Text("Lifecycle Log (newest at top)")
                        .FontWeight(FontWeight.Bold)
                        .Margin(new Thickness(0, 0, 0, 5)),
                        
                    // Log display
                    Reactive(() => new TextBox()
                        .Text(logText())
                        .IsReadOnly(true)
                        .AcceptsReturn(true)
                        .FontFamily("Consolas, Menlo, monospace")
                        .FontSize(12)
                        .Height(200)
                        .Width(450)
                    )
                );
        });
    }
}