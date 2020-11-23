// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImagePattern.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The image pattern class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.DataFiles
{
    /// <summary>
    ///     The image pattern class.
    /// </summary>
    public class ImagePattern
    {
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public byte Label { get; set; }

        /// <summary>
        /// Gets or sets the pattern.
        /// </summary>
        public byte[] Pattern { get; set; } = new byte[SystemGlobals.ImageSize * SystemGlobals.ImageSize];
    }
}