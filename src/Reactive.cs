using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// A reactive control that automatically updates when its dependencies change
/// </summary>
/// <typeparam name="T">The type of control to render</typeparam>
public class Reactive<T> : ViewBase
    where T : Control
{
    private bool _isInitialized;
    private readonly Func<T> _getter;

    /// <summary>
    /// Creates a new reactive control
    /// </summary>
    /// <param name="getter">Function that returns the control to render</param>
    public Reactive(Func<T> getter) : base(true)
    {
        _getter = getter;
        Register();
    }

    // Render the control
    protected override object Build() => _getter();

    // Create an effect to rebuild the control when dependencies change
    private void Register()
    {
        IReactiveSystem.Instance.CreateEffect(() =>
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }
            else
                Reload();
        });
    }
}