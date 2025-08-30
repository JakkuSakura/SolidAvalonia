namespace SolidAvalonia;

/// <summary>
/// Interface for components that provide reactive functionality.
/// Allows consistent access to reactive primitives like signals, memos, and effects.
/// </summary>
public interface ISolid
{
    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// </summary>
    /// <typeparam name="T">The type of the signal value.</typeparam>
    /// <param name="initialValue">The initial value of the signal.</param>
    /// <returns>A tuple containing the getter and setter functions.</returns>
    (Func<T>, Action<T>) CreateSignal<T>(T initialValue);

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="computation">The function that computes the value.</param>
    /// <returns>A function that returns the computed value.</returns>
    Func<T> CreateMemo<T>(Func<T> computation);

    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    /// <param name="effect">The effect function to run.</param>
    void CreateEffect(Action effect);
    
    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts.
    /// </summary>
    /// <param name="cleanup">The cleanup function to register.</param>
    void OnCleanup(Action cleanup);
}