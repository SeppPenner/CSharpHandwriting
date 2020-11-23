// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMfcArchiveSerialization.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface for the MFC archive serialization.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization
{
    /// <summary>
    /// An interface for the MFC archive serialization.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public interface IMfcArchiveSerialization
    {
        /// <summary>
        /// Serializes the MFC archive.
        /// </summary>
        /// <param name="archive">The archive.</param>
        // ReSharper disable once UnusedMember.Global
        void Serialize(MfcArchive archive);
    }
}