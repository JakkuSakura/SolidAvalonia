using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Base class for reactive components that can be both inherited from or used functionally.
/// Provides fine-grained reactivity for building reactive UI components.
/// </summary>
public class Component : ViewBase, ISolid, IDisposable
{
    private bool _isInitialized;
    private readonly Func<Component, Control>? _factory;
    private readonly List<Action> _cleanupCallbacks = new();

    #region Constructors

    /// <summary>
    /// Creates a new component that uses the abstract Build method.
    /// Use this constructor when inheriting from Component.
    /// </summary>
    /// <param name="deferredLoading">Whether to defer loading until Initialize is called.</param>
    protected Component(bool deferredLoading = false) : base(deferredLoading)
    {
        _factory = null;

        // Register the effect to handle initialization and updates
        Register();
    }

    /// <summary>
    /// Creates a new component that uses the provided factory function.
    /// Use this constructor for functional components.
    /// </summary>
    /// <param name="factory">The function that creates the control.</param>
    protected Component(Func<Control> factory) : this(_ => factory())
    {
    }

    // TODO: new ctor

    /// <summary>
    /// Creates a new component with a factory function that receives the component instance.
    /// This allows for convenient access to the component for setup and cleanup.
    /// </summary>
    /// <param name="factory">The function that receives the component and creates the control.</param>
    protected Component(Func<Component, Control> factory) : base(true)
    {
        _factory = factory;

        // Register the effect to handle initialization and updates
        Register();
    }

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Builds the component. Override this in derived classes.
    /// This method will not be called if a factory was provided in the constructor.
    /// </summary>
    /// <returns>The built control.</returns>
    protected override object Build()
    {
        return _factory?.Invoke(this) ?? new Panel();
    }

    // Create an effect to rebuild the component when dependencies change
    private void Register()
    {
        Solid.CreateEffect(() =>
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }
            else
            {
                Reload();
            }
        });
    }

    /// <summary>
    /// Runs all registered cleanup callbacks.
    /// </summary>
    private void RunCleanup()
    {
        // Create a copy of the callbacks to avoid issues if callbacks modify the list
        var callbacks = new List<Action>(_cleanupCallbacks);
        _cleanupCallbacks.Clear();

        foreach (var callback in callbacks)
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                // Log the error but continue with other callbacks
                Console.WriteLine($"Error in cleanup callback: {ex}");
            }
        }
    }

    public void Dispose()
    {
        // Run cleanup callbacks when the component is disposed
        RunCleanup();
        // base.Dispose();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a reactive control that wraps a non-reactive control factory.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that creates the control.</param>
    /// <returns>A reactive component that updates when its dependencies change.</returns>
    public Component<T> Reactive<T>(Func<T> factory) where T : Control =>
        new(factory);

    /// <summary>
    /// Creates a reactive control with a factory that receives the component instance.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that receives the component and creates the control.</param>
    /// <returns>A reactive component that updates when its dependencies change.</returns>
    public Component<T> Reactive<T>(Func<Component, T> factory) where T : Control =>
        new(factory);

    #endregion

    #region Fine-grained Reactivity API

    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// </summary>
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue) =>
        IReactiveSystem.Instance.CreateSignal(initialValue);

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change.
    /// </summary>
    public Func<T> CreateMemo<T>(Func<T> computation) =>
        IReactiveSystem.Instance.CreateMemo(computation);

    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    public void CreateEffect(Action effect) =>
        IReactiveSystem.Instance.CreateEffect(effect);

    /// <summary>
    /// Registers a cleanup function to be called when the component is disposed.
    /// </summary>
    /// <param name="cleanup">The cleanup function to register.</param>
    public void OnCleanup(Action cleanup)
    {
        if (cleanup == null)
            throw new ArgumentNullException(nameof(cleanup));

        _cleanupCallbacks.Add(cleanup);
    }

    #endregion
}

/// <summary>
/// A strongly-typed reactive component that wraps a control of type T.
/// </summary>
/// <typeparam name="T">The type of control this component wraps.</typeparam>
public class Component<T> : Component where T : Control
{
    public Component(Func<T> factory): base(factory) {}
    public Component(Func<Component, T> factory) : base(factory) {}
}