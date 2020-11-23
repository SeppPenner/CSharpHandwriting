// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkLayerList.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network layer list class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkLayers
{
    using System.Collections.Generic;

    using NeuronalNetworkLibrary.ArchiveSerialization;

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <inheritdoc cref="List{T}"/>
    /// <summary>
    /// The neuronal network layer list class.
    /// </summary>
    /// <seealso cref="List{T}"/>
    /// <seealso cref="IArchiveSerialization"/>
    public class NeuronalNetworkLayerList : List<NeuronalNetworkLayer>, IArchiveSerialization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkLayerList"/> class.
        /// </summary>
        public NeuronalNetworkLayerList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkLayerList"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public NeuronalNetworkLayerList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkLayerList"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkLayerList(IEnumerable<NeuronalNetworkLayer> collection) : base(collection)
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