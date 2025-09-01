using System;

namespace SolidAvalonia;

/// <summary>
/// Represents a cleanup action as a reactive node.
/// This allows cleanup actions to be treated as reactive nodes.
/// </summary>
internal class CleanupNode : IReactiveNode
{
    private readonly Action _cleanup;
    private readonly object _syncRoot = new();
    private bool _disposed;
    
    public CleanupNode(Action cleanup)
    {
        _cleanup = cleanup ?? throw new ArgumentNullException(nameof(cleanup));
    }
    
    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_disposed) return;
            _disposed = true;
        }
        
        try
        {
            _cleanup();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in cleanup action: {ex}");
        }
    }
    
    public void AddOwnedNode(IReactiveNode node)
    {
        // Cleanup nodes are leaf nodes and don't own other nodes
        throw new InvalidOperationException("Cleanup nodes cannot own other nodes");
    }
    
    public void RemoveOwnedNode(IReactiveNode node)
    {
        // Cleanup nodes are leaf nodes and don't own other nodes
        throw new InvalidOperationException("Cleanup nodes cannot own other nodes");
    }
}