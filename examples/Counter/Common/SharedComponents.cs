using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia;
using Avalonia.Markup.Declarative;
using Avalonia.Interactivity;
using static SolidAvalonia.Solid;

namespace Counter.Common;

/// <summary>
/// A library of reusable UI components following functional design patterns.
/// These components can be used in both class-based and functional implementations.
/// </summary>
public static class SharedComponents
{
    #region Text Components

    /// <summary>
    /// Creates a header text component.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="fontSize">Optional font size (default: 20).</param>
    /// <param name="horizontalAlignment">Optional horizontal alignment (default: Center).</param>
    /// <returns>A TextBlock control.</returns>
    public static TextBlock Header(
        string text,
        double fontSize = 20,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
    {
        return new TextBlock()
            .Text(text)
            .FontSize(fontSize)
            .FontWeight(FontWeight.Bold)
            .HorizontalAlignment(horizontalAlignment);
    }

    /// <summary>
    /// Creates a reactive header text component.
    /// </summary>
    /// <param name="getText">Function that returns the text to display.</param>
    /// <param name="fontSize">Optional font size (default: 20).</param>
    /// <param name="horizontalAlignment">Optional horizontal alignment (default: Center).</param>
    /// <returns>A TextBlock control with reactive text.</returns>
    public static Component ReactiveHeader(
        Func<string> getText,
        double fontSize = 20,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
    {
        return Reactive(() => new TextBlock()
            .Text(getText)
            .FontSize(fontSize)
            .FontWeight(FontWeight.Bold)
            .HorizontalAlignment(horizontalAlignment));
    }

    /// <summary>
    /// Creates a label text component.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="fontSize">Optional font size (default: 14).</param>
    /// <param name="textAlignment">Optional text alignment (default: Left).</param>
    /// <returns>A TextBlock control.</returns>
    public static TextBlock Label(
        string text,
        double fontSize = 14,
        TextAlignment textAlignment = TextAlignment.Left)
    {
        return new TextBlock()
            .Text(text)
            .FontSize(fontSize)
            .TextAlignment(textAlignment);
    }

    /// <summary>
    /// Creates a reactive label text component.
    /// </summary>
    /// <param name="getText">Function that returns the text to display.</param>
    /// <param name="fontSize">Optional font size (default: 14).</param>
    /// <param name="textAlignment">Optional text alignment (default: Left).</param>
    /// <returns>A TextBlock control with reactive text.</returns>
    public static Component ReactiveLabel(
        Func<string> getText,
        double fontSize = 14,
        TextAlignment textAlignment = TextAlignment.Left)
    {
        return Reactive(() => new TextBlock()
            .Text(getText)
            .FontSize(fontSize)
            .TextAlignment(textAlignment));
    }

    /// <summary>
    /// Creates a status text component with customizable color based on status.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="isSuccess">Whether the status is successful.</param>
    /// <param name="fontSize">Optional font size (default: 14).</param>
    /// <returns>A TextBlock control with color based on status.</returns>
    public static TextBlock StatusText(
        string text,
        bool isSuccess,
        double fontSize = 14)
    {
        return new TextBlock()
            .Text(text)
            .FontSize(fontSize)
            .Foreground(isSuccess ? Brushes.Green : Brushes.Red)
            .TextAlignment(TextAlignment.Center);
    }

    /// <summary>
    /// Creates a reactive status text component with customizable color based on status.
    /// </summary>
    /// <param name="getText">Function that returns the text to display.</param>
    /// <param name="getIsSuccess">Function that determines if the status is successful.</param>
    /// <param name="fontSize">Optional font size (default: 14).</param>
    /// <returns>A TextBlock control with reactive text and color.</returns>
    public static Component ReactiveStatusText(
        Func<string> getText,
        Func<bool> getIsSuccess,
        double fontSize = 14)
    {
        return Reactive(() => new TextBlock()
            .Text(getText)
            .FontSize(fontSize)
            .Foreground(() => getIsSuccess() ? Brushes.Green : Brushes.Red)
            .TextAlignment(TextAlignment.Center));
    }

    #endregion

    #region Input Components

    /// <summary>
    /// Creates a button component.
    /// </summary>
    /// <param name="content">The content of the button.</param>
    /// <param name="onClick">The action to perform when the button is clicked.</param>
    /// <param name="minWidth">Optional minimum width (default: 80).</param>
    /// <returns>A Button control.</returns>
    public static Button PrimaryButton(
        string content,
        Action<RoutedEventArgs> onClick,
        double minWidth = 80)
    {
        return new Button()
            .Content(content)
            .MinWidth(minWidth)
            .OnClick(onClick);
    }

    /// <summary>
    /// Creates a text input component.
    /// </summary>
    /// <param name="initialText">The initial text value.</param>
    /// <param name="onTextChanged">The action to perform when text changes.</param>
    /// <param name="watermark">Optional watermark text (default: null).</param>
    /// <param name="width">Optional width (default: 200).</param>
    /// <returns>A TextBox control.</returns>
    public static TextBox TextInput(
        string initialText,
        Action<TextChangedEventArgs> onTextChanged,
        string? watermark = null,
        double width = 200)
    {
        var textBox = new TextBox()
            .Text(initialText)
            .Width(width)
            .OnTextChanged(onTextChanged);

        if (watermark != null)
        {
            textBox = textBox.Watermark(watermark);
        }

        return textBox;
    }

    /// <summary>
    /// Creates a numeric input component.
    /// </summary>
    /// <param name="value">The initial value.</param>
    /// <param name="onValueChanged">The action to perform when the value changes.</param>
    /// <param name="minimum">Optional minimum value (default: 0).</param>
    /// <param name="maximum">Optional maximum value (default: 100).</param>
    /// <param name="width">Optional width (default: 100).</param>
    /// <returns>A NumericUpDown control.</returns>
    public static NumericUpDown NumericInput(
        decimal value,
        Action<NumericUpDownValueChangedEventArgs> onValueChanged,
        decimal minimum = 0,
        decimal maximum = 100,
        double width = 100)
    {
        return new NumericUpDown()
            .Value(value)
            .Minimum(minimum)
            .Maximum(maximum)
            .Width(width)
            .OnValueChanged(onValueChanged);
    }

    #endregion

    #region Layout Components

    /// <summary>
    /// Creates a vertical stack panel.
    /// </summary>
    /// <param name="spacing">Optional spacing between items (default: 10).</param>
    /// <param name="horizontalAlignment">Optional horizontal alignment (default: Stretch).</param>
    /// <param name="children">The children controls to add to the stack panel.</param>
    /// <returns>A StackPanel control with vertical orientation.</returns>
    public static StackPanel VStack(
        double spacing = 10,
        params Control[] children)
    {
        return new StackPanel()
            .Orientation(Orientation.Vertical)
            .Spacing(spacing)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Children(children);
    }

    /// <summary>
    /// Creates a horizontal stack panel.
    /// </summary>
    /// <param name="spacing">Optional spacing between items (default: 10).</param>
    /// <param name="verticalAlignment">Optional vertical alignment (default: Center).</param>
    /// <param name="children">The children controls to add to the stack panel.</param>
    /// <returns>A StackPanel control with horizontal orientation.</returns>
    public static StackPanel HStack(
        double spacing = 10,
        params Control[] children)
    {
        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(spacing)
            .VerticalAlignment(VerticalAlignment.Center)
            .Children(children);
    }

    /// <summary>
    /// Creates a card container.
    /// </summary>
    /// <param name="child">The child control to add to the card.</param>
    /// <param name="padding">Optional padding (default: 15).</param>
    /// <param name="cornerRadius">Optional corner radius (default: 8).</param>
    /// <returns>A Border control styled as a card.</returns>
    public static Border Card(
        Control child,
        double padding = 15,
        double cornerRadius = 8)
    {
        return new Border()
            .CornerRadius(cornerRadius)
            .Padding(padding)
            .BorderThickness(1)
            .BorderBrush(new SolidColorBrush(Color.FromRgb(220, 220, 220)))
            .Background(Brushes.White)
            .Child(child);
    }

    #endregion
}