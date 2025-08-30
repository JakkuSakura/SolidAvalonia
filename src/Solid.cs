using Avalonia.Controls;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Provides a functional API for reactive programming with SolidAvalonia.
/// Can be used with static imports to provide a more SolidJS-like experience.
/// </summary>
public static class Solid
{
    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// </summary>
    /// <typeparam name="T">The type of the signal value.</typeparam>
    /// <param name="initialValue">The initial value of the signal.</param>
    /// <returns>A tuple containing the getter and setter functions.</returns>
    public static (Func<T>, Action<T>) CreateSignal<T>(T initialValue) => 
        IReactiveSystem.Instance.CreateSignal(initialValue);

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="computation">The function that computes the value.</param>
    /// <returns>A function that returns the computed value.</returns>
    public static Func<T> CreateMemo<T>(Func<T> computation) => 
        IReactiveSystem.Instance.CreateMemo(computation);

    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    /// <param name="effect">The effect function to run.</param>
    public static void CreateEffect(Action effect) => 
        IReactiveSystem.Instance.CreateEffect(effect);
        
    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts.
    /// </summary>
    /// <param name="cleanup">The cleanup function to register.</param>
    public static void OnCleanup(Action cleanup) =>
        IReactiveSystem.Instance.OnCleanup(cleanup);

    /// <summary>
    /// Creates a component that automatically updates when dependencies change.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that creates the control.</param>
    /// <returns>A component that updates when dependencies change.</returns>
    public static Component<T> Component<T>(Func<T> factory) where T : Control => 
        new(factory);
        
    /// <summary>
    /// Creates a component with a factory function that receives the component instance.
    /// This allows for convenient access to the component for setup and cleanup.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that receives the component and creates the control.</param>
    /// <returns>A component that updates when dependencies change.</returns>
    public static Component<T> Component<T>(Func<Component, T> factory) where T : Control => 
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
            return new Component<Control>(() => 
                condition() 
                    ? factory()
                    : fallback?.Invoke() ?? new Panel());
        }
    }
    
    /// <summary>
    /// Shorthand for creating a signal.
    /// </summary>
    public static class Signal
    {
        /// <summary>
        /// Creates a reactive signal with getter and setter.
        /// </summary>
        /// <typeparam name="T">The type of the signal value.</typeparam>
        /// <param name="initialValue">The initial value of the signal.</param>
        /// <returns>A tuple containing the getter and setter functions.</returns>
        public static (Func<T>, Action<T>) Create<T>(T initialValue) => 
            CreateSignal(initialValue);
    }
    
    /// <summary>
    /// Shorthand for creating a memo.
    /// </summary>
    public static class Memo
    {
        /// <summary>
        /// Creates a computed value that automatically updates when dependencies change.
        /// </summary>
        /// <typeparam name="T">The type of the computed value.</typeparam>
        /// <param name="computation">The function that computes the value.</param>
        /// <returns>A function that returns the computed value.</returns>
        public static Func<T> Create<T>(Func<T> computation) => 
            CreateMemo(computation);
    }
    
    /// <summary>
    /// Shorthand for creating an effect.
    /// </summary>
    public static class Effect
    {
        /// <summary>
        /// Creates an effect that runs when dependencies change.
        /// </summary>
        /// <param name="effect">The effect function to run.</param>
        public static void Create(Action effect) => 
            CreateEffect(effect);
            
        /// <summary>
        /// Registers a cleanup function to be called before the current effect re-runs
        /// or when the component unmounts.
        /// </summary>
        /// <param name="cleanup">The cleanup function to register.</param>
        public static void OnCleanup(Action cleanup) =>
            Solid.OnCleanup(cleanup);
    }
}