// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkWeightList.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network weight list class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkWeights
{
    using System.Collections.Generic;

    using NeuronalNetworkLibrary.ArchiveSerialization;

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <inheritdoc cref="List{T}"/>
    /// <summary>
    /// The neuronal network weight list class.
    /// </summary>
    /// <seealso cref="List{T}"/>
    /// <seealso cref="IArchiveSerialization"/>
    public class NeuronalNetworkWeightList : List<NeuronalNetworkWeight>, IArchiveSerialization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkWeightList"/> class.
        /// </summary>
        public NeuronalNetworkWeightList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkWeightList"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public NeuronalNetworkWeightList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkWeightList"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkWeightList(IEnumerable<NeuronalNetworkWeight> collection) : base(collection)
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
}