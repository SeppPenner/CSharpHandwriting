// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleCurrencyStatus.cs" company="H�mmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An enumeration for the OLE currency status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

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
    Invalid = 1,

    /// <summary>
    /// The null OLE currency status.
    /// </summary>
    Null = 2
}
