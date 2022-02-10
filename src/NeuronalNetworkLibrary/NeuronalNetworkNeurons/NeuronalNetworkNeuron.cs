// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkNeuron.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network neuron class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkNeurons;

/// <inheritdoc cref="IArchiveSerialization"/>
/// <summary>
/// The neuronal network neuron class.
/// </summary>
/// <seealso cref="IArchiveSerialization"/>
public sealed class NeuronalNetworkNeuron : IArchiveSerialization
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuron"/> class.
    /// </summary>
    public NeuronalNetworkNeuron()
    {
        this.Label = string.Empty;
        this.Output = 0.0;
        this.Connections = new NeuronalNetworkConnectionList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuron"/> class.
    /// </summary>
    /// <param name="label">The label.</param>
    public NeuronalNetworkNeuron(string label)
    {
        this.Label = label;
        this.Output = 0.0;
        this.Connections = new NeuronalNetworkConnectionList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkNeuron"/> class.
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="count">The count.</param>
    public NeuronalNetworkNeuron(string label, int count)
    {
        this.Label = label;
        this.Output = 0.0;
        this.Connections = new NeuronalNetworkConnectionList(count);
    }

    /// <summary>
    /// Gets the connections.
    /// </summary>
    public NeuronalNetworkConnectionList Connections { get; }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the output.
    /// </summary>
    public double Output { get; set; }

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <summary>
    /// Serializes the archive.
    /// </summary>
    /// <param name="archive">The archive.</param>
    /// <seealso cref="IArchiveSerialization"/>
    public void Serialize(Archive archive)
    {
    }

    /// <summary>
    /// Adds a new connection.
    /// </summary>
    /// <param name="neuronIndex">The neuron index.</param>
    /// <param name="weightIndex">The weight index.</param>
    public void AddConnection(uint neuronIndex, uint weightIndex)
    {
        var conn = new NeuronalNetworkConnection(neuronIndex, weightIndex);
        this.Connections.Add(conn);
    }

    /// <summary>
    /// Adds a new connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public void AddConnection(NeuronalNetworkConnection connection)
    {
        this.Connections.Add(connection);
    }
}
