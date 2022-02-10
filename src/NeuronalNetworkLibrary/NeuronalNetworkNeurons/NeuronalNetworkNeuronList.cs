// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkNeuron.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network neuron list class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkNeurons;

/// <inheritdoc cref="IArchiveSerialization"/>
/// <summary>
/// The neuronal network neuron list class.
/// </summary>
/// <seealso cref="IArchiveSerialization"/>
public class NeuronalNetworkNeuronList : List<NeuronalNetworkNeuron>, IArchiveSerialization
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronList"/> class.
    /// </summary>
    public NeuronalNetworkNeuronList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronList"/> class.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public NeuronalNetworkNeuronList(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronList"/> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    public NeuronalNetworkNeuronList(IEnumerable<NeuronalNetworkNeuron> collection) : base(collection)
    {
    }

    /// <summary>
    /// Serializes the archive.
    /// </summary>
    /// <param name="archive">The archive.</param>
    public void Serialize(Archive archive)
    {
    }
}
