using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace SolidAvalonia;

/// <summary>
/// Base class for reactive components that can be both inherited from or used functionally.
/// Provides fine-grained reactivity for building reactive UI components.
/// </summary>
public class Component : ViewBase, ISolid, IDisposable, ReactiveSystem.ICleanupOwner
{
    private bool _isInitialized;
    private readonly Func<Control>? _factory;
    private readonly List<Action> _cleanupCallbacks = new();
    private bool _isOwnerActive = false;

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
    protected Component(Func<Control> factory) : base(true)
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
        return _factory?.Invoke() ?? new Panel();
    }

    // Create an effect to rebuild the component when dependencies change
    private void Register()
    {
        return;
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
    
    protected override void OnInitialized()
    {
        // Push this component as the current owner before initialization
        ReactiveSystem.Instance.PushOwner(this);
        _isOwnerActive = true;
        base.OnInitialized();
    }
    
    protected override void OnAfterInitialized()
    {
        base.OnAfterInitialized();
        
        // Pop this component as the current owner after initialization
        if (_isOwnerActive)
        {
            ReactiveSystem.Instance.PopOwner();
            _isOwnerActive = false;
        }
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RunCleanup();
        base.OnDetachedFromVisualTree(e);
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


    #endregion

    #region Fine-grained Reactivity API

    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// </summary>
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue) =>
        ReactiveSystem.Instance.CreateSignal(initialValue);

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change.
    /// </summary>
    public Func<T> CreateMemo<T>(Func<T> computation) =>
        ReactiveSystem.Instance.CreateMemo(computation);

    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    public void CreateEffect(Action effect) =>
        ReactiveSystem.Instance.CreateEffect(effect);

    /// <summary>
    /// Registers a cleanup function to be called when the component is disposed.
    /// </summary>
    /// <param name="cleanup">The cleanup function to register.</param>
    /// <summary>
    /// Implements ICleanupOwner.AddCleanup to add cleanup actions
    /// </summary>
    void ReactiveSystem.ICleanupOwner.AddCleanup(Action cleanup)
    {
        if (cleanup == null)
            throw new ArgumentNullException(nameof(cleanup));

        _cleanupCallbacks.Add(cleanup);
    }
    
    /// <summary>
    /// Registers a cleanup function to be called when the component is disposed.
    /// </summary>
    public void OnCleanup(Action cleanup)
    {
        if (cleanup == null)
            throw new ArgumentNullException(nameof(cleanup));

        // Add to the appropriate owner (this or current effect)
        ReactiveSystem.Instance.OnCleanup(cleanup);
    }

    #endregion
}

/// <summary>
/// A strongly-typed reactive component that wraps a control of type T.
/// </summary>
/// <typeparam name="T">The type of control this component wraps.</typeparam>
public class Component<T> : Component where T : Control
{
    public Component(Func<T> factory) : base(factory)
    {
    }
}