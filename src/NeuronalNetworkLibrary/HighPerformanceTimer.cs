// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighPerformanceTimer.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The high performance timer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary;

/// <summary>
/// The high performance timer.
/// </summary>
public class HighPerformanceTimer
{
    /// <summary>
    /// The frequency.
    /// </summary>
    private readonly long frequency;

    /// <summary>
    /// The start time.
    /// </summary>
    private long startTime;

    /// <summary>
    /// The stop time.
    /// </summary>
    private long stopTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="HighPerformanceTimer"/> class.
    /// </summary>
    public HighPerformanceTimer()
    {
        this.startTime = 0;
        this.stopTime = 0;

        if (QueryPerformanceFrequency(out this.frequency) == false)
        {
            throw new Win32Exception();
        }
    }

    /// <summary>
    /// Gets the duration of the timer in seconds.
    /// </summary>
    public double Duration => (this.stopTime - this.startTime) / (double)this.frequency;

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        // lets do the waiting threads there work
        Thread.Sleep(0);
        QueryPerformanceCounter(out this.startTime);
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void Stop()
    {
        QueryPerformanceCounter(out this.stopTime);
    }

    /// <summary>
    /// Queries for the performance counter.
    /// </summary>
    /// <param name="time">The time as <see cref="long"/>.</param>
    /// <returns>A value indicating whether whether the timer has elapsed or not.</returns>
    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long time);

    /// <summary>
    /// Queries for the frequency.
    /// </summary>
    /// <param name="frequency">The frequency.</param>
    /// <returns>A value indicating whether the frequency is valid or not.</returns>
    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long frequency);
}
