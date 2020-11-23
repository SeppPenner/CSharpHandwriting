﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkNeuronOutputs.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network neuron outputs class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkNeurons
{
    using System.Collections.Generic;

    /// <inheritdoc cref="List{T}"/>
    /// <summary>
    /// The neuronal network neuron outputs class.
    /// </summary>
    /// <seealso cref="List{T}"/>
    public class NeuronalNetworkNeuronOutputs : List<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputs"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkNeuronOutputs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputs"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public NeuronalNetworkNeuronOutputs(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkNeuronOutputs"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkNeuronOutputs(IEnumerable<double> collection) : base(collection)
        {
        }
    }
}