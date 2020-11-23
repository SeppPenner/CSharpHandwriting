// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkWeight.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network weight class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetworkWeights
{
    using NeuronalNetworkLibrary.ArchiveSerialization;

    /// <inheritdoc cref="IArchiveSerialization"/>
    /// <summary>
    /// The neuronal network weight class.
    /// </summary>
    /// <seealso cref="IArchiveSerialization"/>
    public sealed class NeuronalNetworkWeight : IArchiveSerialization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkWeight"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkWeight()
        {
            this.Label = string.Empty;
            this.Value = 0.0;
            this.DiagonalHessian = 0.0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkWeight"/> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        public NeuronalNetworkWeight(string label, double value = 0.0)
        {
            this.Label = label;
            this.Value = value;
            this.DiagonalHessian = 0.0;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets or sets the diagonal Hessian.
        /// </summary>
        public double DiagonalHessian { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

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