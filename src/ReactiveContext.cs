namespace SolidAvalonia;

/// <summary>
/// Unified context for tracking reactive state during execution.
/// Replaces separate ComputationContext and OwnerContext with a single tracking mechanism.
/// </summary>
internal class ReactiveContext : IDisposable
{
    private readonly Stack<IReactiveNode> _stack = new();
    
    /// <summary>
    /// Gets the current computation (if any)
    /// </summary>
    public Computation? CurrentComputation => 
        _stack.FirstOrDefault(x => x is Computation) as Computation;
    
    /// <summary>
    /// Gets the current reactive owner (if any)
    /// </summary>
    public IReactiveNode? CurrentOwner =>  _stack.Count > 0 ?
        _stack.Peek() : null;
    
    /// <summary>
    /// Flag indicating whether batching is in progress
    /// </summary>
    public bool IsBatching { get; set; }
    
    /// <summary>
    /// Pushes a context object onto the stack
    /// </summary>
    public void Push(IReactiveNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _stack.Push(node);
    }
    
    /// <summary>
    /// Pops the most recently pushed context object
    /// </summary>
    public object? Pop()
    {
        return _stack.Count > 0 ? _stack.Pop() : null;
    }
    
    
    /// <summary>
    /// Clears all context
    /// </summary>
    public void Clear() => _stack.Clear();
    
    /// <summary>
    /// Disposes of the context
    /// </summary>
    public void Dispose() => Clear();
}