namespace SolidAvalonia;

/// <summary>
/// Unified interface for objects that can own reactive nodes and manage cleanup actions
/// </summary>
public interface IReactiveOwner : IDisposable
{
    /// <summary>
    /// Registers a cleanup action to be run when the owner is disposed
    /// </summary>
    void AddCleanup(Action cleanup);
    
    /// <summary>
    /// Adds a reactive node to this owner
    /// </summary>
    void AddOwnedNode(ReactiveNode node);
    
    /// <summary>
    /// Removes a reactive node from this owner
    /// </summary>
    void RemoveOwnedNode(ReactiveNode node);
}