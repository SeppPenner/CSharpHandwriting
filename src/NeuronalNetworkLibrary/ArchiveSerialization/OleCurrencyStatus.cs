// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleCurrencyStatus.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An enumeration for the OLE currency status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization
{
    /// <summary>
    /// An enumeration for the OLE currency status.
    /// </summary>
    public enum OleCurrencyStatus
    {
        /// <summary>
        /// The valid OLE currency status.
        /// </summary>
        Valid = 0,

        /// <summary>
        /// The invalid OLE currency status.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Invalid = 1,

        /// <summary>
        /// The null OLE currency status.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Null = 2
    }
}