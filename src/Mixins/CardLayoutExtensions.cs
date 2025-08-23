using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Declarative;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for card and section layout functionality
/// </summary>
public static class CardLayoutExtensions
{
    /// <summary>
    /// Creates a card-like container with border, background, and padding
    /// </summary>
    public static Border Card(this SolidControl _, Control content, IBrush? background = null, double cornerRadius = 8, double padding = 15,
        double margin = 5)
    {
        return new Border
        {
            Background = background ?? new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(cornerRadius),
            Padding = new Thickness(padding),
            Margin = new Thickness(margin),
            BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
            BorderThickness = new Thickness(1),
            Child = content
        };
    }

    /// <summary>
    /// Creates a section with a header and content
    /// </summary>
    public static Panel Section(this SolidControl _, string title, Control content, double fontSize = 16, double spacing = 10)
    {
        var header = new TextBlock
        {
            Text = title,
            FontSize = fontSize,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = spacing
        };
        
        stackPanel.Children.Add(header);
        stackPanel.Children.Add(content);

        return stackPanel;
    }
    
    /// <summary>
    /// Creates a separator line
    /// </summary>
    public static Separator CreateSeparator(this SolidControl _, double margin = 10)
    {
        return new Separator
        {
            Margin = new Thickness(0, margin)
        };
    }
}