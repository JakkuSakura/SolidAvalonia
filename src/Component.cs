using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Declarative;

namespace SolidAvalonia;

/// <summary>
/// Base class for reactive components that can be both inherited from or used functionally.
/// Component is a ReactiveNode that provides fine-grained reactivity for building UI components.
/// Components can own other reactive nodes and manage their lifecycle.
/// </summary>
public class Component : ViewBase, ISolid, IReactiveNode
{
    internal Func<Control>? _factory;
    private readonly List<IReactiveNode> _ownedNodes = new();
    protected readonly object SyncRoot = new();

    public bool Disposed { get; private set; }


    #region Constructors

    /// <summary>
    /// Creates a new component that uses the abstract Build method.
    /// Use this constructor when inheriting from Component.
    /// </summary>
    protected Component(bool deferredLoading = false) : base(deferredLoading)
    {
        _factory = null;
        if (deferredLoading) return;

        OnCreatedCore();
        Initialize();
    }

    /// <summary>
    /// Creates a new component that uses the provided factory function.
    /// Use this constructor for functional components.
    /// </summary>
    /// <param name="factory">The function that creates the control.</param>
    public Component(Func<Control> factory) : base(true)
    {
        _factory = factory;
        OnCreatedCore();
        Initialize();
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


    protected override void OnCreated()
    {
        // Push this component as the current owner before initialization
        ReactiveSystem.Instance.PushOwner(this);
    }

    protected override void OnAfterInitialized()
    {
        base.OnAfterInitialized();

        // Pop this component as the current owner after initialization
        var owner = ReactiveSystem.Instance.PopOwner();
        if (!ReferenceEquals(owner, this))
        {
            throw new InvalidOperationException("Reactive owner stack corrupted.");
        }
    }

    /// <summary>
    /// Runs all registered cleanup callbacks.
    /// </summary>
    private void RunCleanup()
    {
        List<IReactiveNode> ownedNodes;

        lock (SyncRoot)
        {
            ownedNodes = new List<IReactiveNode>(_ownedNodes);
            _ownedNodes.Clear();
        }


        // Dispose all owned nodes
        foreach (var node in ownedNodes)
        {
            try
            {
                node.Dispose();
            }
            catch (Exception ex)
            {
                // Log the error but continue with other nodes
                Console.WriteLine($"Error disposing reactive node: {ex}");
            }
        }
    }


    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
    }


    public void Dispose()
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            Disposed = true;
        }

        RunCleanup();
    }


    public void AddOwnedNode(IReactiveNode node)
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            _ownedNodes.Add(node);
        }
    }

    public void RemoveOwnedNode(IReactiveNode node)
    {
        lock (SyncRoot)
        {
            _ownedNodes.Remove(node);
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a reactive control that wraps a non-reactive control factory.
    /// </summary>
    /// <typeparam name="T">The type of control to create.</typeparam>
    /// <param name="factory">The function that creates the control.</param>
    /// <returns>A reactive component that updates when its dependencies change.</returns>
    public Reactive Reactive<T>(Func<T> factory) where T : Control =>
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
    public void OnCleanup(Action cleanup)
    {
        if (cleanup == null)
            throw new ArgumentNullException(nameof(cleanup));

        // Add to the appropriate owner (this or current effect)
        Solid.OnCleanup(cleanup);
    }

    #endregion
}