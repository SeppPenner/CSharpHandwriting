// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OleDateTimeStatus.cs" company="H�mmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An enumeration for the OLE date time status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

/// <summary>
/// An enumeration for the OLE date time status.
/// </summary>
public enum OleDateTimeStatus
{
    /// <summary>
    /// The OLE date time status.
    /// </summary>
    Valid = 0,

    /// <summary>
    /// The invalid OLE date time status.
    /// </summary>
    Invalid = 1,

    /// <summary>
    /// The null OLE date time status.
    /// </summary>
    Null = 2
}
