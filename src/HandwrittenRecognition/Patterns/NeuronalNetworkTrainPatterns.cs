// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkTrainPatterns.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network train patterns.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition.Patterns
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    using NeuronalNetworkLibrary;
    using NeuronalNetworkLibrary.ArchiveSerialization;
    using NeuronalNetworkLibrary.DataFiles;
    using NeuronalNetworkLibrary.NeuronalNetwork;
    using NeuronalNetworkLibrary.NeuronalNetworkNeurons;

    /// <summary>
    /// The neuronal network train patterns.
    /// </summary>
    // ReSharper disable ArrangeRedundantParentheses
    public class NeuronalNetworkTrainPatterns : NeuronalNetworkForwardPropagation
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
        private readonly List<Mutex> mutexes;

        /// <summary>
        /// The database.
        /// </summary>
        private readonly NeuronalNetworkDatabase database;

        /// <summary>
        /// The DMSE.
        /// </summary>
        private double dmse;

        /// <summary>
        /// The DMSE 200.
        /// </summary>
        private double dmse200;

        /// <summary>
        /// The number of completed epochs.
        /// </summary>
        private uint epochsCompleted;

        /// <summary>
        /// The next pattern.
        /// </summary>
        private int nextPattern;

        /// <summary>
        /// A value indicating whether Hessian is needed or not.
        /// </summary>
        private bool needHessian;

        /// <summary>
        /// The back properties.
        /// </summary>
        private int backProperties;

        /// <summary>
        /// The recognitions.
        /// </summary>
        private uint recognitions;

        /// <summary>
        /// The number of neuronal networks.
        /// </summary>
        private int neuronalNetworks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkTrainPatterns"/> class.
        /// </summary>
        /// <param name="neuronalNetwork">The neuronal network.</param>
        /// <param name="trainingSet">The training set.</param>
        /// <param name="preferences">The preferences.</param>
        /// <param name="trainingDataReady">A value indicating whether the training data is ready or not.</param>
        /// <param name="eventStop">The event stop event.</param>
        /// <param name="eventStopped">The event stopped event.</param>
        /// <param name="mainForm">The main form.</param>
        /// <param name="mutexes">The mutexes.</param>
        public NeuronalNetworkTrainPatterns(
            NeuronalNetwork neuronalNetwork,
            NeuronalNetworkDatabase trainingSet,
            Preferences preferences,
            bool trainingDataReady,
            ManualResetEvent eventStop,
            ManualResetEvent eventStopped,
            MainForm mainForm,
            List<Mutex> mutexes)
        {
            this.CurrentPatternIndex = 0;
            this.DataReady = trainingDataReady;
            this.NeuronalNetwork = neuronalNetwork;
            this.database = trainingSet;
            this.Preferences = preferences;
            this.mainForm = mainForm;
            this.eventStop = eventStop;
            this.eventStopped = eventStopped;
            this.mutexes = mutexes;
            this.recognitions = 0;
            this.nextPattern = 0;
            this.needHessian = true;
            this.backProperties = 0;
            this.dmse = 0;
            this.neuronalNetworks = 0;
            this.dmse200 = 0;
            this.highPerformanceTimer = new HighPerformanceTimer();
            this.GetGaussianKernel(this.Preferences.ElasticSigma);
        }

        /// <summary>
        /// The estimated current MSE.
        /// </summary>
        public double EstimatedCurrentMse { get; set; }

        /// <summary>
        /// The ETA decay.
        /// </summary>
        public double EtaDecay { get; set; }

        /// <summary>
        /// The minimum ETA.
        /// </summary>
        public double MinimumEta { get; set; }

        /// <summary>
        /// Back propagation and training-related members.
        /// </summary>
        public uint AfterEveryNBackProperties { get; set; }

        /// <summary>
        /// Initializes the network.
        /// </summary>
        /// <param name="afterEveryNBackProperties">The back propagation and training-related members.</param>
        /// <param name="etaDecay">The ETA decay.</param>
        /// <param name="minimumEta">The minimum ETA.</param>
        /// <param name="estimatedCurrentMse">The estimated current MSE.</param>
        /// <param name="distortTrainingPatterns">The distort training patterns.</param>
        // ReSharper disable once UnusedMember.Global
        public void Initialize(
            uint afterEveryNBackProperties,
            double etaDecay,
            double minimumEta,
            double estimatedCurrentMse,
            bool distortTrainingPatterns)
        {
            this.AfterEveryNBackProperties = afterEveryNBackProperties;
            this.EtaDecay = etaDecay;
            this.MinimumEta = minimumEta;
            this.EstimatedCurrentMse = estimatedCurrentMse;
            this.DistortTrainingPatterns = distortTrainingPatterns;
        }

        /// <summary>
        /// Handles the back propagation.
        /// </summary>
        public void BackPropagationThread()
        {
            // Thread for back propagation training of the neuronal network
            // Thread is "owned" by the doc, and accepts a pointer to the doc
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
            while (true)
            {
                this.mutexes[3].WaitOne();

                if (this.nextPattern == 0)
                {
                    this.highPerformanceTimer.Start();
                    this.database.RandomizePatternSequence();
                }

                var grayLevels = new byte[this.Preferences.NumberOfRowImages * this.Preferences.NumberOfColumnImages];
                var pattern = this.database.GetNextPatternNumber(this.database.FromRandomizedPatternSequence);
                this.database.ImagePatterns[pattern].Pattern.CopyTo(grayLevels, 0);
                var label = this.database.ImagePatterns[pattern].Label;
                this.nextPattern++;

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

                // Now back propagate
                this.mutexes[3].ReleaseMutex();

                this.BackPropagateNeuronalNetwork(
                    inputVector,
                    841,
                    targetOutputVector,
                    actualOutputVector,
                    10,
                    memorizedNeuronOutputs,
                    this.DistortTrainingPatterns);

                this.mutexes[3].WaitOne();

                // Calculate error for this pattern and post it to the HWND so it can calculate a running estimate of MSE
                var localDmse = 0.0;

                for (ii = 0; ii < 10; ++ii)
                {
                    localDmse += (actualOutputVector[ii] - targetOutputVector[ii]) * (actualOutputVector[ii] - targetOutputVector[ii]);
                }

                localDmse /= 2.0;
                this.dmse += localDmse;
                this.dmse200 += localDmse;

                // Determine the neuronal network's answer, and compare it to the actual answer.
                // Post a message if the answer was incorrect, so the dialog can display mis-recognition statistics
                this.neuronalNetworks++;
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

                if (bestIndex != label)
                {
                    this.recognitions++;
                }

                // Make step
                string s;

                if (this.neuronalNetworks >= 200)
                {
                    this.dmse200 /= 200;
                    s = "MSE:" + this.dmse200.ToString(CultureInfo.InvariantCulture);
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 4, s);
                    this.dmse200 = 0;
                    this.neuronalNetworks = 0;
                }

                s = $"{Convert.ToString(this.nextPattern)} Miss Number:{this.recognitions}";

                // Make synchronous call to main form.
                // MainForm.AddString function runs in main thread.
                // To make asynchronous call use BeginInvoke
                this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 5, s);

                if (this.nextPattern >= this.database.ImagePatterns.Count - 1)
                {
                    this.highPerformanceTimer.Stop();
                    this.dmse /= this.nextPattern;
                    s =
                        $"Completed Epochs:{Convert.ToString(this.epochsCompleted + 1)}, MisPatterns:{Convert.ToString(this.recognitions)}, MSE:{this.dmse.ToString(CultureInfo.InvariantCulture)}, Ex. time: {this.highPerformanceTimer.Duration}, eta:{this.NeuronalNetwork.EtaLearningRate} ";
                    
                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 3, s);
                    this.recognitions = 0;
                    this.epochsCompleted++;
                    this.nextPattern = 0;
                    this.dmse = 0;
                }
                
                // Check if thread is cancelled
                if (this.eventStop.WaitOne(0, true))
                {
                    // clean-up operations may be placed here
                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 3, $"BackPropagation thread: {Thread.CurrentThread.Name} stopped");

                    // Inform main thread that this thread stopped
                    this.eventStopped.Set();
                    this.mutexes[3].ReleaseMutex();
                    return;
                }

                this.mutexes[3].ReleaseMutex();
            }
        }

        /// <summary>
        /// Calculates the Hessian.
        /// </summary>
        private void CalculateHessian()
        {
            // Controls the neuronal network's calculation if the diagonal Hessian for the neuronal net
            // This will be called from a thread, so although the calculation is lengthy, it should not interfere
            // with the UI

            // We need the neuronal net exclusively during this calculation, so grab it now
            var inputVector = new double[841]; // Note: 29x29, not 28x28

            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];
            this.mutexes[1].WaitOne();

            for (var i = 0; i < 841; i++)
            {
                inputVector[i] = 0.0;
            }

            for (var j = 0; j < 10; j++)
            {
                targetOutputVector[j] = 0.0;
                actualOutputVector[j] = 0.0;
            }

            uint kk;

            // Calculate the diagonal Hessian using 500 random patterns, per Yann LeCun 1998 "Gradient-Based Learning
            // Applied To Document Recognition"
            // Make synchronous call to main form.
            // MainForm.AddString function runs in main thread.
            // To make asynchronous call use BeginInvoke
            this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 3, "Commencing Calculation of Hessian...");

            // Some of this code is similar to the Back propagation thread code
            this.NeuronalNetwork.EraseHessianInformation();

            var numbersOfPatternsSampled = this.Preferences.NumberOfHessianPatterns;

            for (kk = 0; kk < numbersOfPatternsSampled; ++kk)
            {
                var randomPatternNumber = this.database.GetRandomPatternNumber();
                var label = this.database.ImagePatterns[randomPatternNumber].Label;

                if (label > 9)
                {
                    label = 9;
                }

                var grayLevels = this.database.ImagePatterns[randomPatternNumber].Pattern;

                // Pad to 29x29, convert to double precision
                int ii;

                for (ii = 0; ii < 841; ++ii)
                {
                    // One is white, -one is black
                    inputVector[ii] = 1.0;
                }

                // Top row of input vector is left as zero, left-most column is left as zero 
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

                // Apply distortion map to inputVector. It's not certain that this is needed or helpful.
                // The second derivatives do NOT rely on the output of the neuronal net (i.e., because the 
                // second derivative of the MSE function is exactly 1 (one), regardless of the actual output
                // of the net). However, since the back propagated second derivatives rely on the outputs of
                // each neuron, distortion of the pattern might reveal previously-unseen information about the
                // nature of the Hessian. But I am reluctant to give the full distortion, so I set the
                // severityFactor to only 2/3 approximately.
                this.GenerateDistortionMaps(0.65);
                this.ApplyDistortionMap(inputVector);

                // Forward calculate the neuronal network
                this.NeuronalNetwork.Calculate(inputVector, 841, actualOutputVector, 10, null);

                // Back propagate the second derivatives
                this.NeuronalNetwork.BackPropagateSecondDerivatives(actualOutputVector, targetOutputVector, 10);

                // Check if thread is cancelled
                if (!this.eventStop.WaitOne(0, true))
                {
                    continue;
                }

                // clean-up operations may be placed here
                // Make synchronous call to main form.
                // MainForm.AddString function runs in main thread.
                // To make asynchronous call use BeginInvoke
                this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 3, "BackPropagation stopped");

                // Inform main thread that this thread stopped
                this.eventStopped.Set();
                return;
            }

            this.NeuronalNetwork.DivideHessianInformationBy(numbersOfPatternsSampled);
            this.mainForm?.Invoke(this.mainForm.DelegateAddObject, 3, "Calculation of Hessian...completed");
            this.mutexes[1].ReleaseMutex();
        }

        /// <summary>
        /// Back propagates the neuronal network.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="inputCount">The input count.</param>
        /// <param name="targetOutputVector">The target output vector.</param>
        /// <param name="actualOutputVector">The actual output vector.</param>
        /// <param name="outputCount">The output count.</param>
        /// <param name="memorizedNeuronOutputs">The memorized neuronal outputs.</param>
        /// <param name="distort">The distort.</param>
        private void BackPropagateNeuronalNetwork(
            double[] inputVector,
            int inputCount,
            double[] targetOutputVector,
            double[] actualOutputVector,
            int outputCount,
            NeuronalNetworkNeuronOutputsList memorizedNeuronOutputs,
            bool distort)
        {
            // Function to back propagate through the neuronal net. 
            // Determine if it's time to adjust the learning rate
            this.mutexes[2].WaitOne();
            if (this.backProperties % this.AfterEveryNBackProperties == 0 && this.backProperties != 0)
            {
                var eta = this.NeuronalNetwork.EtaLearningRate;
                eta *= this.EtaDecay;

                if (eta < this.MinimumEta)
                {
                    eta = this.MinimumEta;
                }

                this.NeuronalNetwork.PreviousEtaLearningRate = this.NeuronalNetwork.EtaLearningRate;
                this.NeuronalNetwork.EtaLearningRate = eta;
            }

            // Determine if it's time to adjust the Hessian (currently once per epoch)
            if (this.needHessian || this.backProperties % this.Preferences.NumberOfItemsTrainingImages == 0)
            {
                // Adjust the Hessian. This is a lengthy operation, since it must process approximately 500 labels
                this.needHessian = false;
                this.CalculateHessian();
            }

            // Increment counter for tracking number of back properties
            this.backProperties++;

            // Determine if it's time to randomize the sequence of training patterns (currently once per epoch)
            if (this.backProperties % this.Preferences.NumberOfItemsTrainingImages == 0)
            {
                this.database.RandomizePatternSequence();
            }

            this.mutexes[2].ReleaseMutex();

            // Forward calculate through the neuronal network
            this.CalculateNeuronalNetwork(inputVector, inputCount, actualOutputVector, outputCount, memorizedNeuronOutputs, distort);

            this.mutexes[2].WaitOne();

            // Calculate error in the output of the neuronal network
            // note that this code duplicates that found in many other places, and it's probably sensible to 
            // define a (global/static ??) function for it
            var localDmse = 0.0;
            for (var ii = 0; ii < 10; ++ii)
            {
                localDmse += (actualOutputVector[ii] - targetOutputVector[ii])
                             * (actualOutputVector[ii] - targetOutputVector[ii]);
            }

            localDmse /= 2.0;

            var worthWhileBackPropagate = !(localDmse <= 0.10 * this.EstimatedCurrentMse);

            if (worthWhileBackPropagate && memorizedNeuronOutputs == null)
            {
                // The caller has not provided a place to store neuron outputs, so we need to
                // back propagate now, while the neuronal net is still captured. Otherwise, another thread
                // might come along and call CalculateNeuronalNetwork(), which would entirely change the neuron
                // outputs and thereby inject errors into back propagation 
                this.NeuronalNetwork.BackPropagate(actualOutputVector, targetOutputVector, outputCount, null);
                return;
            }

            // If we have reached here, then the mutex for the neuronal net has been released for other 
            // threads. The caller must have provided a place to store neuron outputs, which we can 
            // use to back propagate, even if other threads call CalculateNeuronalNetwork() and change the outputs
            // of the neurons
            if (worthWhileBackPropagate)
            {
                this.NeuronalNetwork.BackPropagate(actualOutputVector, targetOutputVector, outputCount, memorizedNeuronOutputs);
            }

            this.mutexes[2].ReleaseMutex();
        }
    }
}