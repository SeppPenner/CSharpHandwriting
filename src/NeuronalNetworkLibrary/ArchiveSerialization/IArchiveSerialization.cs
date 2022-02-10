// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IArchiveSerialization.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface for the archive serialization.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

/// <summary>
/// An interface for the archive serialization.
/// </summary>
public interface IArchiveSerialization
{
    /// <summary>
    /// Serializes the archive.
    /// </summary>
    /// <param name="archive">The archive.</param>
    void Serialize(Archive archive);
}
