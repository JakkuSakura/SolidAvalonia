using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.Markup.Declarative;
using SolidAvalonia;
using static SolidAvalonia.Solid;
using R3;
using System;

namespace Counter.Advanced;

/// <summary>
/// Example demonstrating throttling and debouncing techniques.
/// 
/// This example shows:
/// - How to throttle user interactions with Reactive Extensions
/// - Using ThrottleFirst for rate limiting
/// - Using Debounce for stabilization
/// - Comparing different throttling strategies
/// </summary>
public static class ThrottlingExample
{
    /// <summary>
    /// Creates a component that demonstrates throttling techniques.
    /// </summary>
    public static Control PerformanceOptimizations()
    {
        return Component(() =>
        {
            // 1. Create signals for each counter
            var (regularCount, setRegularCount) = CreateSignal(0);
            var (throttledCount, setThrottledCount) = CreateSignal(0);
            var (debouncedCount, setDebouncedCount) = CreateSignal(0);
            // Using a mutable string to store logs without reactive updates
            var logTextValue = "";
            // Create a signal for the log view
            var (logText, setLogText) = CreateSignal("");
            
            // 2. Create signals for click timestamps
            var (regularTimestamp, setRegularTimestamp) = CreateSignal("");
            var (throttledTimestamp, setThrottledTimestamp) = CreateSignal("");
            var (debouncedTimestamp, setDebouncedTimestamp) = CreateSignal("");
            
            // Helper function to add timestamped log entries
            void AddLog(string message)
            {
                // Update the mutable string
                logTextValue = message + "\n" + logTextValue;
                
                // Update the signal outside the effect
                // This prevents infinite loop since our modification to 
                // the signal won't trigger the effect we're already in
                setLogText(logTextValue);
                Console.WriteLine(message);
            }
            
            // 3. Create subjects for throttling
            var throttleSubject = new Subject<int>();
            var debounceSubject = new Subject<int>();
            
            // 4. Configure throttling behaviors
            
            // ThrottleFirst: Only lets the first event through in a time window
            throttleSubject
                .ThrottleFirst(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => 
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    setThrottledCount(throttledCount() + 1);
                    setThrottledTimestamp(timestamp);
                    AddLog($"[{timestamp}] Throttled event processed");
                });
            
            // Debounce: Only lets the last event through after a period of inactivity
            debounceSubject
                .Debounce(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => 
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    setDebouncedCount(debouncedCount() + 1);
                    setDebouncedTimestamp(timestamp);
                    AddLog($"[{timestamp}] Debounced event processed");
                });
            
            // 5. Helper function for creating counter displays
            Func<string, Func<int>, Func<string>, Control> createCounterDisplay = (title, getCount, getTimestamp) =>
            {
                return new Border()
                    .Padding(15)
                    .CornerRadius(5)
                    .BorderThickness(1)
                    .BorderBrush(new SolidColorBrush(Color.FromRgb(200, 200, 200)))
                    .Child(new StackPanel()
                        .Spacing(8)
                        .Children(
                            new TextBlock()
                                .Text(title)
                                .FontWeight(FontWeight.Bold)
                                .HorizontalAlignment(HorizontalAlignment.Center),
                            
                            Reactive(() => new TextBlock()
                                .Text(() => $"Count: {getCount()}")
                                .FontSize(16)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                            ),
                            
                            Reactive(() => new TextBlock()
                                .Text(() => getTimestamp().Length > 0 
                                    ? $"Last update: {getTimestamp()}" 
                                    : "No updates yet")
                                .FontSize(12)
                                .Foreground(Brushes.Gray)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                            )
                        )
                    );
            };
            
            // 6. Return the UI component tree
            return new StackPanel()
                .Spacing(20)
                .HorizontalAlignment(HorizontalAlignment.Center)
                .Children(
                    // Header
                    new TextBlock()
                        .Text("Throttling & Debouncing Example")
                        .FontSize(20)
                        .FontWeight(FontWeight.Bold)
                        .HorizontalAlignment(HorizontalAlignment.Center),

                    // Explanation
                    new TextBlock()
                        .Text("This example demonstrates how to throttle and debounce user interactions for better performance. Click the buttons rapidly to see the differences.")
                        .FontSize(14)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxWidth(450)
                        .TextAlignment(TextAlignment.Center),
                    
                    // Button row
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(15)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            new Button()
                                .Content("Regular")
                                .Width(120)
                                .OnClick(_ => 
                                {
                                    setRegularCount(regularCount() + 1);
                                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                                    setRegularTimestamp(timestamp);
                                    AddLog($"[{timestamp}] Regular button clicked");
                                }),
                            
                            new Button()
                                .Content("Throttled")
                                .Width(120)
                                .OnClick(_ => 
                                {
                                    throttleSubject.OnNext(1);
                                    AddLog($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] Throttled button clicked");
                                }),
                            
                            new Button()
                                .Content("Debounced")
                                .Width(120)
                                .OnClick(_ => 
                                {
                                    debounceSubject.OnNext(1);
                                    AddLog($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] Debounced button clicked");
                                })
                        ),
                    
                    // Counter displays
                    new StackPanel()
                        .Orientation(Orientation.Horizontal)
                        .Spacing(15)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Children(
                            createCounterDisplay("Regular (No Limiting)", regularCount, regularTimestamp),
                            createCounterDisplay("Throttled (500ms)", throttledCount, throttledTimestamp),
                            createCounterDisplay("Debounced (500ms)", debouncedCount, debouncedTimestamp)
                        ),
                    
                    // Explanation of differences
                    new Border()
                        .Padding(15)
                        .CornerRadius(5)
                        .Background(new SolidColorBrush(Color.FromRgb(245, 245, 255)))
                        .Child(new StackPanel()
                            .Spacing(8)
                            .Children(
                                new TextBlock()
                                    .Text("What's the difference?")
                                    .FontWeight(FontWeight.Bold),
                                
                                new TextBlock()
                                    .Text("• Regular: Processes every click immediately")
                                    .FontSize(14),
                                
                                new TextBlock()
                                    .Text("• Throttled: Limits to one click per 500ms window (first one wins)")
                                    .FontSize(14),
                                
                                new TextBlock()
                                    .Text("• Debounced: Waits until clicks stop for 500ms (last one wins)")
                                    .FontSize(14)
                            )
                        ),
                    
                    // Divider
                    new Separator()
                        .Height(1)
                        .Margin(new Thickness(0, 10, 0, 10))
                        .Background(new SolidColorBrush(Color.FromRgb(200, 200, 220))),
                    
                    // Log header
                    new TextBlock()
                        .Text("Event Log (newest at top)")
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