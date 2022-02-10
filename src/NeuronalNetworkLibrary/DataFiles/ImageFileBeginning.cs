// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageFileBeginning.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The beginning of the label file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.DataFiles;

/// <summary>
/// The beginning of the label file.
/// </summary>
public struct ImageFileBeginning
{
    /// <summary>
    /// The magic number.
    /// </summary>
    public int MagicNumber;

    /// <summary>
    /// The number of items.
    /// </summary>
    public int Items;

    /// <summary>
    /// The number of rows.
    /// </summary>
    public int Rows;

    /// <summary>
    /// The number of columns.
    /// </summary>
    public int Columns;
}
