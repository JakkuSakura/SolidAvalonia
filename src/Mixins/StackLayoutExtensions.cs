using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for stack layout functionality
/// </summary>
public static class StackLayoutExtensions
{
    /// <summary>
    /// Creates a vertical layout container with consistent spacing and padding
    /// </summary>
    public static Panel VStack(this SolidControl control, double spacing = 10, double margin = 20, params Control[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = spacing,
            Margin = new Thickness(margin)
        };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>
    /// Creates a horizontal layout container with consistent spacing and padding
    /// </summary>
    public static Panel HStack(this SolidControl control, double spacing = 10, double margin = 0, params Control[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = spacing,
            Margin = new Thickness(margin)
        };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }
    
    /// <summary>
    /// Creates a centered container
    /// </summary>
    public static Panel Centered(this SolidControl control, Control content, double maxWidth = double.NaN)
    {
        var container = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (!double.IsNaN(maxWidth))
            container.MaxWidth = maxWidth;

        container.Children.Add(content);
        return container;
    }
}