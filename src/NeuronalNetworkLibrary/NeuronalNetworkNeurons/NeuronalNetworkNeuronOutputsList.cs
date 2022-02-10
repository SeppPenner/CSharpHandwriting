// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkNeuronOutputsList.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network neuron outputs list class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkNeurons;

/// <inheritdoc cref="List{T}"/>
/// <summary>
/// The neuronal network neuron outputs list class.
/// </summary>
/// <seealso cref="List{T}"/>
public class NeuronalNetworkNeuronOutputsList : List<NeuronalNetworkNeuronOutputs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputsList"/> class.
    /// </summary>
    public NeuronalNetworkNeuronOutputsList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputsList"/> class.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public NeuronalNetworkNeuronOutputsList(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputsList"/> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    public NeuronalNetworkNeuronOutputsList(IEnumerable<NeuronalNetworkNeuronOutputs> collection) : base(collection)
    {
    }
}
