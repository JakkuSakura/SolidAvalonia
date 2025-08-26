namespace SolidAvalonia.ReactiveSystem;

/// <summary>
/// Interface for a reactive system providing signal, memo, and effect functionality
/// </summary>
public interface IReactiveSystem : IDisposable
{
    /// <summary>
    /// Creates a reactive signal with getter and setter
    /// </summary>
    (Func<T>, Action<T>) CreateSignal<T>(T initialValue);
    
    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    Func<T> CreateMemo<T>(Func<T> computation);
    
    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    void CreateEffect(Action effect);
}