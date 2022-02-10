// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorsList.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The errors list class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetwork;

/// <inheritdoc cref="List{T}"/>
/// <summary>
/// The errors list class.
/// </summary>
/// <seealso cref="List{T}"/>
public class ErrorsList : List<double>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorsList"/> class.
    /// </summary>
    public ErrorsList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorsList"/> class.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    public ErrorsList(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorsList"/> class.
    /// </summary>
    /// <param name="collection">The collection.s</param>
    public ErrorsList(IEnumerable<double> collection) : base(collection)
    {
    }
}
