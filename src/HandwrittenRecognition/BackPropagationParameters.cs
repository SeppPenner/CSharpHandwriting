// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BackPropagationParameters.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The back propagation parameters.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition;

/// <summary>
/// The back propagation parameters.
/// </summary>
public struct BackPropagationParameters
{
    /// <summary>
    /// After every.
    /// </summary>
    public uint AfterEvery;

    /// <summary>
    /// The ETA decay.
    /// </summary>
    public double EtaDecay;

    /// <summary>
    /// The initial ETA.
    /// </summary>
    public double InitialEta;

    /// <summary>
    /// The minimum ETA.
    /// </summary>
    public double MinimumEta;

    /// <summary>
    /// The initial ETA message.
    /// </summary>
    public string InitialEtaMessage;

    /// <summary>
    /// The starting pattern number.
    /// </summary>
    public string StartingPatternNumber;

    /// <summary>
    /// The starting pattern.
    /// </summary>
    public uint StartingPattern;

    /// <summary>
    /// The number of threads.
    /// </summary>
    public uint NumberOfThreads;

    /// <summary>
    /// A value indicating whether pattern distort should be done.
    /// </summary>
    public bool DistortPatterns;

    /// <summary>
    /// The estimated current MSE.
    /// </summary>
    public double EstimatedCurrentMse;
}
