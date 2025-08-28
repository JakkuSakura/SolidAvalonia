using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;
using static SolidAvalonia.Solid;

namespace Counter.Common;

/// <summary>
/// A collection of reusable components specifically designed for working with signals.
/// These components focus on reactive UI elements that can be composed together.
/// </summary>
public static class SignalComponents
{
    /// <summary>
    /// Creates a text display component that reacts to signal changes.
    /// </summary>
    /// <param name="getText">Function that returns the text to display.</param>
    /// <param name="getColor">Optional function that returns the text color.</param>
    /// <returns>A Control with reactive text.</returns>
    public static Control TextDisplay(Func<string> getText, Func<IBrush>? getColor = null)
    {
        return new TextBlock()
            .Text(getText)
            .FontSize(16)
            .Foreground(getColor ?? (() => Brushes.Black))
            .HorizontalAlignment(HorizontalAlignment.Center);
    }
    
    /// <summary>
    /// Creates a status indicator component that reacts to signal changes.
    /// </summary>
    /// <param name="getText">Function that returns the text to display.</param>
    /// <param name="getBackground">Function that returns the background color.</param>
    /// <param name="getForeground">Optional function that returns the text color.</param>
    /// <returns>A Control with reactive background and text.</returns>
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
    
    /// <summary>
    /// Creates a row of buttons that can work with signals.
    /// </summary>
    /// <param name="buttons">Collection of button configurations (content, onClick handler, and optional background).</param>
    /// <returns>A Control containing the buttons in a horizontal layout.</returns>
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
    
    /// <summary>
    /// Creates a header text component.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>A TextBlock styled as a header.</returns>
    public static TextBlock Header(string text)
    {
        return new TextBlock()
            .Text(text)
            .FontSize(18)
            .FontWeight(FontWeight.Bold)
            .HorizontalAlignment(HorizontalAlignment.Center);
    }
    
    /// <summary>
    /// Creates an input field with label for text input that works with signals.
    /// </summary>
    /// <param name="labelText">The label text.</param>
    /// <param name="initialValue">The initial text value.</param>
    /// <param name="onTextChanged">Handler for text changed events.</param>
    /// <param name="width">Width of the text input field.</param>
    /// <returns>A Control containing the label and input field.</returns>
    public static Control LabeledTextInput(string labelText, string initialValue, Action<TextChangedEventArgs> onTextChanged, double width = 200)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(10)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                new TextBlock()
                    .Text(labelText)
                    .VerticalAlignment(VerticalAlignment.Center),
                
                new TextBox()
                    .Text(initialValue)
                    .Width(width)
                    .OnTextChanged(onTextChanged)
            );
    }
}