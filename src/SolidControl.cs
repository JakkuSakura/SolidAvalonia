using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Base class for creating reactive Avalonia controls with SolidJS-like API
/// </summary>
public abstract class SolidControl : ViewBase
{
    protected readonly IReactiveSystem rs = new SolidReactiveSystem();
}