using Avalonia;
using Avalonia.Threading;
using static SolidAvalonia.Solid;

namespace SolidAvalonia;

/// <summary>
/// Extension methods for binding Solid signals to Avalonia properties.
/// </summary>
public static class BindingExtensions
{
    /// <summary>
    /// Binds a signal value to an Avalonia property.
    /// </summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="control">The control to bind to.</param>
    /// <param name="accessor">The signal accessor function.</param>
    /// <param name="property">The Avalonia property to bind to.</param>
    /// <returns>The control instance for method chaining.</returns>
    public static T BindSignal<T, TValue>(
        this T control,
        Func<TValue> accessor,
        AvaloniaProperty<TValue> property)
        where T : AvaloniaObject
    {
        CreateEffect(() =>
        {
            var value = accessor(); // Auto-tracks dependency
                
            if (Dispatcher.UIThread.CheckAccess())
                control.SetValue(property, value);
            else
                Dispatcher.UIThread.Post(() => control.SetValue(property, value));
        });
            
        return control;
    }
        
    /// <summary>
    /// Binds a signal value to an Avalonia property with conversion.
    /// </summary>
    /// <typeparam name="T">The type of the control.</typeparam>
    /// <typeparam name="TSource">The source type of the signal value.</typeparam>
    /// <typeparam name="TTarget">The target type of the property value.</typeparam>
    /// <param name="control">The control to bind to.</param>
    /// <param name="accessor">The signal accessor function.</param>
    /// <param name="property">The Avalonia property to bind to.</param>
    /// <param name="converter">A function to convert from the source type to the target type.</param>
    /// <returns>The control instance for method chaining.</returns>
    public static T BindSignal<T, TSource, TTarget>(
        this T control,
        Func<TSource> accessor,
        AvaloniaProperty<TTarget> property,
        Func<TSource, TTarget> converter)
        where T : AvaloniaObject
    {
        CreateEffect(() =>
        {
            var sourceValue = accessor(); // Auto-tracks dependency
            var targetValue = converter(sourceValue);
                
            if (Dispatcher.UIThread.CheckAccess())
                control.SetValue(property, targetValue);
            else
                Dispatcher.UIThread.Post(() => control.SetValue(property, targetValue));
        });
            
        return control;
    }
}