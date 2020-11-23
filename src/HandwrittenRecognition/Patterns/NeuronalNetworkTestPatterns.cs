// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkTestPatterns.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network test patterns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition.Patterns
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using NeuronalNetworkLibrary;
    using NeuronalNetworkLibrary.ArchiveSerialization;
    using NeuronalNetworkLibrary.DataFiles;
    using NeuronalNetworkLibrary.NeuronalNetwork;
    using NeuronalNetworkLibrary.NeuronalNetworkNeurons;

    /// <summary>
    /// The neuronal network test patterns.
    /// </summary>
    // ReSharper disable ArrangeRedundantParentheses
    public class NeuronalNetworkTestPatterns : NeuronalNetworkForwardPropagation
    {
        /// <summary>
        /// The main form.
        /// </summary>
        private readonly MainForm mainForm;

        /// <summary>
        /// The stop event.
        /// </summary>
        private readonly ManualResetEvent eventStop;

        /// <summary>
        /// The event stopped event.
        /// </summary>
        private readonly ManualResetEvent eventStopped;

        /// <summary>
        /// The high performance timer.
        /// </summary>
        private readonly HighPerformanceTimer highPerformanceTimer;

        /// <summary>
        /// The mutexes.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly List<Mutex> mutexes;

        /// <summary>
        /// The database.
        /// </summary>
        private readonly NeuronalNetworkDatabase database;

        /// <summary>
        /// The number.
        /// </summary>
        private uint number;

        /// <summary>
        /// The next pattern.
        /// </summary>
        private uint nextPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkTestPatterns"/> class.
        /// </summary>
        /// <param name="neuronalNetwork">The neuronal network.</param>
        /// <param name="testingSet">The testing set.</param>
        /// <param name="preferences">The preferences.</param>
        /// <param name="testingDataReady">A value indicating whether the testing data is ready.</param>
        /// <param name="eventStop">The stop event.</param>
        /// <param name="eventStopped">The event stopped event.</param>
        /// <param name="mainForm">The form.</param>
        /// <param name="mutexes">The mutexes.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public NeuronalNetworkTestPatterns(
            NeuronalNetwork neuronalNetwork,
            NeuronalNetworkDatabase testingSet,
            Preferences preferences,
            bool testingDataReady,
            ManualResetEvent eventStop,
            ManualResetEvent eventStopped,
            MainForm mainForm,
            List<Mutex> mutexes)
        {
            this.CurrentPatternIndex = 0;
            this.DataReady = testingDataReady;
            this.NeuronalNetwork = neuronalNetwork;
            this.nextPattern = 0;
            this.eventStop = eventStop;
            this.eventStopped = eventStopped;
            this.mainForm = mainForm;
            this.highPerformanceTimer = new HighPerformanceTimer();

            // Initialize Gaussian Kernel
            this.Preferences = preferences;
            this.GetGaussianKernel(preferences.ElasticSigma);
            this.database = testingSet;
            this.mutexes = mutexes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkTestPatterns"/> class.
        /// </summary>
        /// <param name="neuronalNetwork">The neuronal network.</param>
        /// <param name="preferences">The preferences.</param>
        /// <param name="mainForm">The main form.</param>
        /// <param name="mutexes">The mutexes.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetworkTestPatterns(NeuronalNetwork neuronalNetwork, Preferences preferences, MainForm mainForm, List<Mutex> mutexes)
        {
            this.CurrentPatternIndex = 0;
            this.DataReady = true;
            this.NeuronalNetwork = neuronalNetwork;
            this.nextPattern = 0;
            this.eventStop = null;
            this.eventStopped = null;
            this.mainForm = mainForm;
            this.highPerformanceTimer = new HighPerformanceTimer();
            this.number = 0;

            // Initialize Gaussian Kernel
            this.Preferences = preferences;
            this.GetGaussianKernel(preferences.ElasticSigma);
            this.database = null;
            this.mutexes = mutexes;
        }

        /// <summary>
        /// Handles the pattern testing.
        /// </summary>
        /// <param name="patternNumber">The pattern number.</param>
        public void PatternsTestingThread(int patternNumber)
        {
            // Thread for back propagation training of the neuronal network.
            // The thread is "owned" by the doc, and accepts a pointer to the doc
            // continuously back propagates until threadAbortFlag is set to TRUE
            var inputVector = new double[841]; // Note: 29x29, not 28x28
            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];
            
            for (var i = 0; i < 841; i++)
            {
                inputVector[i] = 0.0;
            }

            for (var i = 0; i < 10; i++)
            {
                targetOutputVector[i] = 0.0;
                actualOutputVector[i] = 0.0;
            }

            var memorizedNeuronOutputs = new NeuronalNetworkNeuronOutputsList();

            // Prepare for training
            this.highPerformanceTimer.Start();

            while (this.nextPattern < patternNumber)
            {
                this.mutexes[1].WaitOne();

                var grayLevels = new byte[this.Preferences.NumberOfRowImages * this.Preferences.NumberOfColumnImages];
                this.database.ImagePatterns[(int)this.nextPattern].Pattern.CopyTo(grayLevels, 0);
                var label = this.database.ImagePatterns[(int)this.nextPattern].Label;

                if (label > 9)
                {
                    label = 9;
                }

                // Pad to 29x29, convert to double precision
                int ii;
                for (ii = 0; ii < 841; ++ii)
                {
                    // One is white, -one is black
                    inputVector[ii] = 1.0;
                }

                // Top row of inputVector is left as zero, left-most column is left as zero
                for (ii = 0; ii < SystemGlobals.ImageSize; ++ii)
                {
                    int jj;
                    for (jj = 0; jj < SystemGlobals.ImageSize; ++jj)
                    {
                        // One is white, -one is black
                        inputVector[1 + jj + (29 * (ii + 1))] = (grayLevels[jj + (SystemGlobals.ImageSize * ii)] / 128.0) - 1.0;
                    }
                }

                // Desired output vector
                for (ii = 0; ii < 10; ++ii)
                {
                    targetOutputVector[ii] = -1.0;
                }

                targetOutputVector[label] = 1.0;

                // Forward calculate through the neuronal network
                this.CalculateNeuronalNetwork(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);

                var bestIndex = 0;
                var maxValue = -99.0;

                for (ii = 0; ii < 10; ++ii)
                {
                    if (!(actualOutputVector[ii] > maxValue))
                    {
                        continue;
                    }

                    bestIndex = ii;
                    maxValue = actualOutputVector[ii];
                }

                string s;
                if (bestIndex != label)
                {
                    this.number++;
                    s = "Pattern No:" + this.nextPattern + " Recognized value:" + bestIndex + " Actual value:"
                        + label;
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 6, s);
                }
                else
                {
                    s = this.nextPattern + ", Mis numbers:" + this.number;
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 7, s);
                }

                // check if thread is cancelled
                if (this.eventStop.WaitOne(0, true))
                {
                    // clean-up operations may be placed here
                    // ...
                    s = $"Testing thread: {Thread.CurrentThread.Name} stopped";

                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 8, s);

                    // Inform main thread that this thread stopped
                    this.eventStopped.Set();
                    this.mutexes[1].ReleaseMutex();
                    return;
                }

                this.nextPattern++;
                this.mutexes[1].ReleaseMutex();
            }

            {
                var s = $"Testing thread: {Thread.CurrentThread.Name} stopped";
                this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 8, s);
            }
        }

        /// <summary>
        /// Runs the pattern recognition.
        /// </summary>
        /// <param name="patternNumber">The pattern number.</param>
        public void PatternRecognizingThread(int patternNumber)
        {
            // Thread for back propagation training of the neuronal network.
            // The thread is "owned" by the doc, and accepts a pointer to the doc
            // continuously back propagates until threadAbortFlag is set to TRUE
            var inputVector = new double[841]; // Note: 29x29, not 28x28
            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];
            
            for (var i = 0; i < 841; i++)
            {
                inputVector[i] = 0.0;
            }

            for (var i = 0; i < 10; i++)
            {
                targetOutputVector[i] = 0.0;
                actualOutputVector[i] = 0.0;
            }

            int ii;

            var memorizedNeuronOutputs = new NeuronalNetworkNeuronOutputsList();

            // Prepare for training
            this.nextPattern = 0;
            this.number = 0;

            this.mutexes[1].WaitOne();

            if (this.nextPattern == 0)
            {
                this.highPerformanceTimer.Start();
            }

            var grayLevels = new byte[this.Preferences.NumberOfRowImages * this.Preferences.NumberOfColumnImages];
            this.database.ImagePatterns[patternNumber].Pattern.CopyTo(grayLevels, 0);
            var label = this.database.ImagePatterns[patternNumber].Label;
            this.nextPattern++;

            if (label > 9)
            {
                label = 9;
            }

            // Pad to 29x29, convert to double precision
            for (ii = 0; ii < 841; ++ii)
            {
                // One is white, -one is black
                inputVector[ii] = 1.0;
            }

            // Top row of inputVector is left as zero, left-most column is left as zero 
            for (ii = 0; ii < SystemGlobals.ImageSize; ++ii)
            {
                int jj;
                for (jj = 0; jj < SystemGlobals.ImageSize; ++jj)
                {
                    // One is white, -one is black
                    inputVector[1 + jj + (29 * (ii + 1))] = (grayLevels[jj + (SystemGlobals.ImageSize * ii)] / 128.0) - 1.0; 
                }
            }

            // desired output vector
            for (ii = 0; ii < 10; ++ii)
            {
                targetOutputVector[ii] = -1.0;
            }

            targetOutputVector[label] = 1.0;

            // Forward calculate through the neuronal net
            this.CalculateNeuronalNetwork(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);
            var bestIndex = 0;
            var maxValue = -99.0;

            for (ii = 0; ii < 10; ++ii)
            {
                if (!(actualOutputVector[ii] > maxValue))
                {
                    continue;
                }

                bestIndex = ii;
                maxValue = actualOutputVector[ii];
            }

            var s = bestIndex.ToString();
            this.mainForm.Invoke(this.mainForm.DelegateAddObject, 2, s);

            // Check if thread is cancelled
            this.mutexes[1].ReleaseMutex();
        }

        /// <summary>
        /// Handles the pattern recognition.
        /// </summary>
        /// <param name="grayLevels">The gray levels.</param>
        // ReSharper disable once UnusedMember.Global
        public void PatternRecognizingThread(byte[] grayLevels)
        {
            // Thread for back propagation training of the neuronal network.
            // The thread is "owned" by the doc, and accepts a pointer to the doc
            // continuously back propagates until threadAbortFlag is set to TRUE
            var inputVector = new double[841]; // Note: 29x29, not 28x28
            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];

            for (var i = 0; i < 841; i++)
            {
                inputVector[i] = 0.0;
            }

            for (var i = 0; i < 10; i++)
            {
                targetOutputVector[i] = 0.0;
                actualOutputVector[i] = 0.0;
            }

            byte label = 0;
            int ii;

            var memorizedNeuronOutputs = new NeuronalNetworkNeuronOutputsList();

            this.mutexes[1].WaitOne();

            if (this.nextPattern == 0)
            {
                this.highPerformanceTimer.Start();
            }

            if (label > 9)
            {
                label = 9;
            }

            // Pad to 29x29, convert to double precision
            for (ii = 0; ii < 841; ++ii)
            {
                // One is white, -one is black
                inputVector[ii] = 1.0;
            }

            // Top row of inputVector is left as zero, left-most column is left as zero 
            for (ii = 0; ii < SystemGlobals.ImageSize; ++ii)
            {
                int jj;
                for (jj = 0; jj < SystemGlobals.ImageSize; ++jj)
                {
                    // One is white, -one is black
                    inputVector[1 + jj + (29 * (ii + 1))] = (grayLevels[jj + (SystemGlobals.ImageSize * ii)] / 128.0) - 1.0;
                }
            }

            // Desired output vector
            for (ii = 0; ii < 10; ++ii)
            {
                targetOutputVector[ii] = -1.0;
            }

            targetOutputVector[label] = 1.0;

            // Forward calculate through the neuronal net
            this.CalculateNeuronalNetwork(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);
            var bestIndex = 0;
            var maxValue = -99.0;

            for (ii = 0; ii < 10; ++ii)
            {
                if (!(actualOutputVector[ii] > maxValue))
                {
                    continue;
                }

                bestIndex = ii;
                maxValue = actualOutputVector[ii];
            }

            var s = bestIndex.ToString();
            this.mainForm.Invoke(this.mainForm.DelegateAddObject, 1, s);

            // Check if thread is cancelled
            this.mutexes[1].ReleaseMutex();
        }
    }
}