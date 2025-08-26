using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for reactive controls
/// </summary>
public static class ReactiveExtensions
{
    public static T Text<T>(this T control, IReactiveSystem system, Func<string> getter) 
        where T : TextBlock
    {
        system.CreateEffect(() => control.Text = getter());
        return control;
    }

    /// <summary>
    /// Creates a control that is conditionally visible based on a condition
    /// </summary>
    public static T ShowWhen<T>(this T control, IReactiveSystem system, Func<bool> condition) where T : Control
    {
        system.CreateEffect(() => control.IsVisible = condition());
        return control;
    }
}