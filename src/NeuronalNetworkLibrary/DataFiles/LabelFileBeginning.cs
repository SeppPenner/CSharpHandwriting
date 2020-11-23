// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabelFileBeginning.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The beginning of the label file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.DataFiles
{
    /// <summary>
    /// The beginning of the label file.
    /// </summary>
    public struct LabelFileBeginning
    {
        /// <summary>
        /// The magic number.
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        public int MagicNumber;

        /// <summary>
        /// The number of items.
        /// </summary>
        public int Items;
    }
}