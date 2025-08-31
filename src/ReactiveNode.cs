using System;

namespace SolidAvalonia;

/// <summary>
/// Base class for all reactive nodes in the dependency graph
/// </summary>
public abstract class ReactiveNode : IDisposable
{
    protected readonly object SyncRoot = new();
    public bool Disposed;
    public long Version { get; protected set; }
    internal IReactiveOwner? Owner { get; private set; }
        
    /// <summary>
    /// Sets the owner of this reactive node
    /// </summary>
    internal void SetOwner(IReactiveOwner? owner)
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
                
            // Remove from old owner if exists
            Owner?.RemoveOwnedNode(this);
                
            // Set new owner
            Owner = owner;
                
            // Add to new owner if not null
            owner?.AddOwnedNode(this);
        }
    }
        
    public abstract void Dispose();
}