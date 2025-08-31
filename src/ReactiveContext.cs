namespace SolidAvalonia;

/// <summary>
/// Unified context for tracking reactive state during execution.
/// Replaces separate ComputationContext and OwnerContext with a single tracking mechanism.
/// </summary>
internal class ReactiveContext : IDisposable
{
    private readonly Stack<object> _contextStack = new();
    
    /// <summary>
    /// Gets the current computation (if any)
    /// </summary>
    public Computation? CurrentComputation => 
        _contextStack.FirstOrDefault(x => x is Computation) as Computation;
    
    /// <summary>
    /// Gets the current reactive owner (if any)
    /// </summary>
    public IReactiveOwner? CurrentOwner => 
        _contextStack.FirstOrDefault(x => x is IReactiveOwner) as IReactiveOwner;
    
    /// <summary>
    /// Flag indicating whether batching is in progress
    /// </summary>
    public bool IsBatching { get; set; }
    
    /// <summary>
    /// Pushes a context object onto the stack
    /// </summary>
    public void Push(object context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _contextStack.Push(context);
    }
    
    /// <summary>
    /// Pops the most recently pushed context object
    /// </summary>
    public object? Pop()
    {
        return _contextStack.Count > 0 ? _contextStack.Pop() : null;
    }
    
    
    /// <summary>
    /// Clears all context
    /// </summary>
    public void Clear() => _contextStack.Clear();
    
    /// <summary>
    /// Disposes of the context
    /// </summary>
    public void Dispose() => Clear();
}