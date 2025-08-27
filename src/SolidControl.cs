using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.Extensions;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Base class for creating reactive Avalonia controls with SolidJS-like API
/// </summary>
public abstract class SolidControl : ViewBase, IReactiveSystem
{
    protected readonly IReactiveSystem rs = new SolidReactiveSystem();
    public SolidControl(bool deferredLoading = false) : base(deferredLoading)
    {
    }

    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        return rs.CreateSignal(initialValue);
    }

    public Func<T> CreateMemo<T>(Func<T> computation)
    {
        return rs.CreateMemo(computation);
    }

    public void CreateEffect(Action effect)
    {
        rs.CreateEffect(effect);
    }

    public ReactiveControl<T> Reactive<T>(Func<T> func)
        where T : Control
    {
        return new ReactiveControl<T>(rs, func);
    }

    public void Dispose()
    {
        rs.Dispose();
    }
}