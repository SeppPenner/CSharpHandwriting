// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkConnection.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network connection class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkConnections;

/// <inheritdoc cref="IArchiveSerialization"/>
/// <summary>
/// The neuronal network connection class.
/// </summary>
/// <seealso cref="IArchiveSerialization"/>
public class NeuronalNetworkConnection : IArchiveSerialization
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkConnection"/> class.
    /// </summary>
    public NeuronalNetworkConnection()
    {
        this.NeuronIndex = 0xffffffff;
        this.WeightIndex = 0xffffffff;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkConnection"/> class.
    /// </summary>
    /// <param name="neuronIndex">The neuron index.</param>
    /// <param name="weightIndex">The weight index.</param>
    public NeuronalNetworkConnection(uint neuronIndex, uint weightIndex)
    {
        this.NeuronIndex = neuronIndex;
        this.WeightIndex = weightIndex;
    }

    /// <summary>
    /// Gets or sets the neuron index.
    /// </summary>
    public uint NeuronIndex { get; set; }

    /// <summary>
    /// Gets or sets the weight index.
    /// </summary>
    public uint WeightIndex { get; set; }

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
