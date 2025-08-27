using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia.Extensions;

/// <summary>
/// Extension methods for reactive controls
/// </summary>
public static class ReactiveExtensions
{
    public static T Text<T>(this T control, IReactiveSystem rs, Func<string> getter)
        where T : TextBlock
    {
        rs.CreateEffect(() => control.Text = getter());
        return control;
    }

    /// <summary>
    /// Creates a control that is conditionally visible based on a condition
    /// </summary>
    public static T ShowWhen<T>(this T control, IReactiveSystem rs, Func<bool> condition) where T : Control
    {
        rs.CreateEffect(() => control.IsVisible = condition());
        return control;
    }
}