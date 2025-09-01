using System;

namespace SolidAvalonia;

/// <summary>
/// Interface for all reactive nodes in the dependency graph
/// Combined functionality of reactive nodes and reactive owners
/// </summary>
public interface IReactiveNode : IDisposable
{
    /// <summary>
    /// Adds a reactive node to this owner
    /// </summary>
    void AddOwnedNode(IReactiveNode node);
    
    /// <summary>
    /// Removes a reactive node from this owner
    /// </summary>
    void RemoveOwnedNode(IReactiveNode node);
}