using Avalonia.Controls;

namespace SolidAvalonia;

/// <summary>
/// Provides a functional API for reactive programming with SolidAvalonia.
/// Can be used with static imports to provide a more SolidJS-like experience.
/// </summary>
public static class Solid
{
    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// Signal changes trigger updates to reactive UI.
    /// </summary>
    /// <typeparam name="T">The type of the signal value.</typeparam>
    /// <param name="initialValue">The initial value of the signal.</param>
    /// <returns>A tuple containing the getter and setter functions.</returns>
    public static (Func<T>, Action<T>) CreateSignal<T>(T initialValue) =>
        ReactiveSystem.Instance.CreateSignal(initialValue);

    // CreateRef method removed - use CreateSignal instead


    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="computation">The function that computes the value.</param>
    /// <returns>A function that returns the computed value.</returns>
    public static Func<T> CreateMemo<T>(Func<T> computation) =>
        ReactiveSystem.Instance.CreateMemo(computation);


    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    /// <param name="effect">The effect function to run.</param>
    public static void CreateEffect(Action effect) =>
        ReactiveSystem.Instance.CreateEffect(effect);


    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts.
    /// </summary>
    /// <param name="cleanup">The cleanup function to register.</param>
    public static void OnCleanup(Action cleanup) =>
        ReactiveSystem.Instance.OnCleanup(cleanup);

    /// <summary>
    /// Creates a component that automatically updates when dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that creates the control.</param>
    /// <returns>A component that updates when dependencies change.</returns>
    public static Component Component<T>(Func<T> factory) where T : Control =>
        new(factory);

    public static Reactive Reactive<T>(Func<T> factory) where T : Control =>
        new(factory);


    /// <summary>
    /// For compatibility with common SolidJS pattern.
    /// </summary>
    public static class For
    {
        /// <summary>
        /// Creates a collection of components from a collection of data.
        /// </summary>
        /// <typeparam name="TSource">The type of the source data.</typeparam>
        /// <typeparam name="TResult">The type of the result control.</typeparam>
        /// <param name="items">The collection of items to render.</param>
        /// <param name="renderItem">The function that renders each item.</param>
        /// <returns>An array of controls.</returns>
        public static Control[] Each<TSource, TResult>(
            IEnumerable<TSource> items,
            Func<TSource, TResult> renderItem)
            where TResult : Control
        {
            return items.Select(renderItem).Cast<Control>().ToArray();
        }
    }

    /// <summary>
    /// For compatibility with common SolidJS pattern.
    /// </summary>
    public static class Show
    {
        /// <summary>
        /// Conditionally renders a component.
        /// </summary>
        /// <typeparam name="T">The type of control to create.</typeparam>
        /// <param name="condition">The condition to check.</param>
        /// <param name="factory">The function that creates the control if condition is true.</param>
        /// <param name="fallback">Optional function that creates a fallback control if condition is false.</param>
        /// <returns>The created control or null if condition is false and no fallback is provided.</returns>
        public static Control When<T>(
            Func<bool> condition,
            Func<T> factory,
            Func<Control>? fallback = null)
            where T : Control
        {
            return Reactive(() =>
                condition()
                    ? factory()
                    : fallback?.Invoke() ?? new Panel());
        }
    }
}