// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMfcArchiveSerialization.cs" company="H�mmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface for the MFC archive serialization.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

/// <summary>
/// An interface for the MFC archive serialization.
/// </summary>
public interface IMfcArchiveSerialization
{
    /// <summary>
    /// Serializes the MFC archive.
    /// </summary>
    /// <param name="archive">The archive.</param>
    void Serialize(MfcArchive archive);
}
