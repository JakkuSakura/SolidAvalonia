using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Base class for creating reactive Avalonia controls with SolidJS-like API
/// </summary>
public abstract class SolidControl : ViewBase, IReactiveSystem
{
    public SolidControl(bool deferredLoading = false) : base(deferredLoading)
    {
        // Let derived classes override base(true) and call Initialize()
    }

    // Implement IReactiveSystem interface methods by delegating to the global system
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue) => IReactiveSystem.Instance.CreateSignal(initialValue);
    public Func<T> CreateMemo<T>(Func<T> computation) => IReactiveSystem.Instance.CreateMemo(computation);
    public void CreateEffect(Action effect) => IReactiveSystem.Instance.CreateEffect(effect);

    // Create a reactive control with the current reactive system
    public Reactive<T> Reactive<T>(Func<T> func) where T : Control => new(func);
}