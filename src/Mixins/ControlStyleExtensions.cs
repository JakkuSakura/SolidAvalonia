using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for styled control creation
/// </summary>
public static class ControlStyleExtensions
{
    /// <summary>
    /// Creates a button with consistent styling
    /// </summary>
    public static Button StyledButton(this SolidControl _, string content, Action? onClick = null, double width = double.NaN, double height = 35,
        IBrush? background = null, IBrush? foreground = null)
    {
        var button = new Button
        {
            Content = content,
            Height = height,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 8),
            CornerRadius = new CornerRadius(6)
        };

        if (!double.IsNaN(width))
            button.Width = width;

        if (background != null)
            button.Background = background;

        if (foreground != null)
            button.Foreground = foreground;

        if (onClick != null)
            button.Click += (_, _) => onClick();

        return button;
    }

    /// <summary>
    /// Creates a text input with consistent styling
    /// </summary>
    public static TextBox StyledTextBox(this SolidControl _, string watermark = "", double width = 200, double height = 35)
    {
        return new TextBox
        {
            Watermark = watermark,
            Width = width,
            Height = height,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(206, 212, 218))
        };
    }

    /// <summary>
    /// Creates a styled text block
    /// </summary>
    public static TextBlock StyledText(this SolidControl _, string text = "", double fontSize = 14, FontWeight fontWeight = FontWeight.Normal,
        IBrush? foreground = null, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = fontWeight,
            HorizontalAlignment = alignment
        };

        if (foreground != null)
            textBlock.Foreground = foreground;

        return textBlock;
    }
}