// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkConnectionList.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network connection list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkConnections;

/// <inheritdoc cref="IArchiveSerialization"/>
/// <inheritdoc cref="List{T}"/>
/// <summary>
/// The neuronal network connection list.
/// </summary>
/// <seealso cref="List{T}"/>
/// <seealso cref="IArchiveSerialization"/>
public class NeuronalNetworkConnectionList : List<NeuronalNetworkConnection>, IArchiveSerialization
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkConnectionList"/> class.
    /// </summary>
    public NeuronalNetworkConnectionList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkConnectionList"/> class.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public NeuronalNetworkConnectionList(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkConnectionList"/> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    public NeuronalNetworkConnectionList(IEnumerable<NeuronalNetworkConnection> collection) : base(collection)
    {
    }

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <summary>
    /// Serializes the archive.
    /// </summary>
    /// <param name="archive">The archive.</param>
    /// <seealso cref="IArchiveSerialization"/>
    public void Serialize(Archive archive)
    {
    }
}
