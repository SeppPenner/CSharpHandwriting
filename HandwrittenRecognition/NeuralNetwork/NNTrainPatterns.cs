using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NeuralNetworkLibrary;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.DataFiles;
using NeuralNetworkLibrary.NeuralNetwork;
using NeuralNetworkLibrary.NNNeurons;
using NeuralNetworkN = NeuralNetworkLibrary.NeuralNetwork.NeuralNetwork;

namespace HandwrittenRecogniration.NeuralNetwork
{
    public class NnTrainPatterns : NnForwardPropagation
    {
        private readonly Mainform _form;

        private readonly ManualResetEvent _mEventStop;
        private readonly ManualResetEvent _mEventStopped;
        private readonly HiPerfTimer _mHiPerfTime;
        private readonly List<Mutex> _mMutexs;
        private readonly MnistDatabase _mnistDataSet;
        private double _dMse;
        private double _dMse200;

        /// <summary>
        /// </summary>
        private uint _iEpochsCompleted;

        private int _iNextPattern;
        private bool _mbNeedHessian;
        private int _mcBackprops;
        private uint _mcMisrecognitions;
        private int _nn;
        public double MdEstimatedCurrentMse; // this number will be changed by one thread and used by others
        public double MdEtaDecay;
        public double MdMinimumEta;

        /// <summary>
        /// </summary>

        //backpropagation and training-related members
        public uint MnAfterEveryNBackprops;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public NnTrainPatterns(NeuralNetworkN neuronNet, MnistDatabase trainingSet, Preferences preferences,
            bool trainingDataReady,
            ManualResetEvent eventStop,
            ManualResetEvent eventStopped,
            Mainform form, List<Mutex> mutexs)
        {
            _mCurrentPatternIndex = 0;
            BDataReady = trainingDataReady;
            Nn = neuronNet;
            _mnistDataSet = trainingSet;
            MPreferences = preferences;
            _form = form;
            _mEventStop = eventStop;
            _mEventStopped = eventStopped;
            _mMutexs = mutexs;
            _mcMisrecognitions = 0;
            _iNextPattern = 0;
            _mbNeedHessian = true;
            _mcBackprops = 0;
            _dMse = 0;
            _nn = 0;
            _dMse200 = 0;
            _mHiPerfTime = new HiPerfTimer();
            GetGaussianKernel(MPreferences.MdElasticSigma);
        }

        // ReSharper disable once UnusedMember.Global
        public void Initialize(uint nAfterEveryNBackprops,
            double dEtaDecay, double dMinimumEta,
            double dEstimatedCurrentMse, // this number will be changed by one thread and used by others
            bool bDistortTrainingPatterns)
        {
            MnAfterEveryNBackprops = nAfterEveryNBackprops;
            MdEtaDecay = dEtaDecay;
            MdMinimumEta = dMinimumEta;
            MdEstimatedCurrentMse =
                dEstimatedCurrentMse; // this number will be changed by one thread and used by others
            MbDistortPatterns = bDistortTrainingPatterns;
        }


        private void CalculateHessian()
        {
            // controls the Neural network's calculation if the diagonal Hessian for the Neural net
            // This will be called from a thread, so although the calculation is lengthy, it should not interfere
            // with the UI

            // we need the neural net exclusively during this calculation, so grab it now

            var inputVector = new double[841]; // note: 29x29, not 28x28

            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];
            _mMutexs[1].WaitOne();

            for (var i = 0; i < 841; i++)
                inputVector[i] = 0.0;
            for (var j = 0; j < 10; j++)
            {
                targetOutputVector[j] = 0.0;
                actualOutputVector[j] = 0.0;
            }

            uint kk;

            // calculate the diagonal Hessian using 500 random patterns, per Yann LeCun 1998 "Gradient-Based Learning
            // Applied To Document Recognition"
            var s = "Commencing Caculation of Hessian...";
            // Make synchronous call to main form.
            // MainForm.AddString function runs in main thread.
            // To make asynchronous call use BeginInvoke
            _form?.Invoke(_form.DelegateAddObject, 3, s);

            // some of this code is similar to the BackpropagationThread() code

            Nn.EraseHessianInformation();

            var numPatternsSampled = MPreferences.MnNumHessianPatterns;

            for (kk = 0; kk < numPatternsSampled; ++kk)
            {
                var iRandomPatternNum = _mnistDataSet.GetRandomPatternNumber();
                var label = _mnistDataSet.MpImagePatterns[iRandomPatternNum].NLabel;

                if (label > 9) label = 9;
                var grayLevels = _mnistDataSet.MpImagePatterns[iRandomPatternNum].PPattern;

                // pad to 29x29, convert to double precision

                int ii;
                for (ii = 0; ii < 841; ++ii)
                    inputVector[ii] = 1.0; // one is white, -one is black

                // top row of inputVector is left as zero, left-most column is left as zero 

                for (ii = 0; ii < MyDefinitions.GcImageSize; ++ii)
                {
                    int jj;
                    for (jj = 0; jj < MyDefinitions.GcImageSize; ++jj)
                        inputVector[1 + jj + 29 * (ii + 1)] =
                            grayLevels[jj + MyDefinitions.GcImageSize * ii] / 128.0 -
                            1.0; // one is white, -one is black
                }

                // desired output vector

                for (ii = 0; ii < 10; ++ii)
                    targetOutputVector[ii] = -1.0;
                targetOutputVector[label] = 1.0;


                // apply distortion map to inputVector.  It's not certain that this is needed or helpful.
                // The second derivatives do NOT rely on the output of the neural net (i.e., because the 
                // second derivative of the MSE function is exactly 1 (one), regardless of the actual output
                // of the net).  However, since the backpropagated second derivatives rely on the outputs of
                // each neuron, distortion of the pattern might reveal previously-unseen information about the
                // nature of the Hessian.  But I am reluctant to give the full distortion, so I set the
                // severityFactor to only 2/3 approx

                GenerateDistortionMap(0.65);
                ApplyDistortionMap(inputVector);
                // forward calculate the neural network

                Nn.Calculate(inputVector, 841, actualOutputVector, 10, null);


                // backpropagate the second derivatives

                Nn.BackpropagateSecondDervatives(actualOutputVector, targetOutputVector, 10);

                //
                // check if thread is cancelled
                if (!_mEventStop.WaitOne(0, true)) continue;
                // clean-up operations may be placed here
                // ...
                const string ss = "BackPropagation stoped";
                // Make synchronous call to main form.
                // MainForm.AddString function runs in main thread.
                // To make asynchronous call use BeginInvoke
                _form?.Invoke(_form.DelegateAddObject, 3, ss);
                // inform main thread that this thread stopped
                _mEventStopped.Set();

                return;
            }

            Nn.DivideHessianInformationBy(numPatternsSampled);
            s = " Caculation of Hessian...completed";
            _form?.Invoke(_form.DelegateAddObject, 3, s);
            _mMutexs[1].ReleaseMutex();
        }

        private void BackpropagateNeuralNet(double[] inputVector, int iCount, double[] targetOutputVector,
            double[] actualOutputVector, int oCount,
            NnNeuronOutputsList pMemorizedNeuronOutputs,
            bool bDistort)
        {
            // function to backpropagate through the neural net. 

            //ASSERT( (inputVector != NULL) && (targetOutputVector != NULL) && (actualOutputVector != NULL) );

            ///////////////////////////////////////////////////////////////////////
            //
            // CODE REVIEW NEEDED:
            //
            // It does not seem worthwhile to backpropagate an error that's very small.  "Small" needs to be defined
            // and for now, "small" is set to a fixed size of pattern error ErrP <= 0.10 * MSE, then there will
            // not be a backpropagation of the error.  The current MSE is updated from the neural net dialog CDlgNeuralNet


            // local scope for capture of the neural net, only during the forward calculation step,
            // i.e., we release neural net for other threads after the forward calculation, and after we
            // have stored the outputs of each neuron, which are needed for the backpropagation step


            // determine if it's time to adjust the learning rate
            _mMutexs[2].WaitOne();
            if (_mcBackprops % MnAfterEveryNBackprops == 0 && _mcBackprops != 0)
            {
                var eta = Nn.MEtaLearningRate;
                eta *= MdEtaDecay;
                if (eta < MdMinimumEta)
                    eta = MdMinimumEta;
                Nn.MEtaLearningRatePrevious = Nn.MEtaLearningRate;
                Nn.MEtaLearningRate = eta;
            }


            // determine if it's time to adjust the Hessian (currently once per epoch)

            if (_mbNeedHessian || _mcBackprops % MPreferences.MnItemsTrainingImages == 0)
            {
                // adjust the Hessian.  This is a lengthy operation, since it must process approx 500 labels
                _mbNeedHessian = false;
                CalculateHessian();
            }
            // increment counter for tracking number of backprops

            _mcBackprops++;

            // determine if it's time to randomize the sequence of training patterns (currently once per epoch)

            if (_mcBackprops % MPreferences.MnItemsTrainingImages == 0)
                _mnistDataSet.RandomizePatternSequence();

            _mMutexs[2].ReleaseMutex();


            // forward calculate through the neural net

            CalculateNeuralNet(inputVector, iCount, actualOutputVector, oCount, pMemorizedNeuronOutputs, bDistort);

            _mMutexs[2].WaitOne();
            // calculate error in the output of the neural net
            // note that this code duplicates that found in many other places, and it's probably sensible to 
            // define a (global/static ??) function for it

            var dMse = 0.0;
            for (var ii = 0; ii < 10; ++ii)
                dMse += (actualOutputVector[ii] - targetOutputVector[ii]) *
                        (actualOutputVector[ii] - targetOutputVector[ii]);
            dMse /= 2.0;

            var bWorthwhileToBackpropagate = !(dMse <= 0.10 * MdEstimatedCurrentMse);

            if (bWorthwhileToBackpropagate && pMemorizedNeuronOutputs == null)
            {
                // the caller has not provided a place to store neuron outputs, so we need to
                // backpropagate now, while the neural net is still captured.  Otherwise, another thread
                // might come along and call CalculateNeuralNet(), which would entirely change the neuron
                // outputs and thereby inject errors into backpropagation 

                Nn.Backpropagate(actualOutputVector, targetOutputVector, oCount, null);
                // we're done, so return

                return;
            }


            // if we have reached here, then the mutex for the neural net has been released for other 
            // threads.  The caller must have provided a place to store neuron outputs, which we can 
            // use to backpropagate, even if other threads call CalculateNeuralNet() and change the outputs
            // of the neurons

            if (bWorthwhileToBackpropagate)
                Nn.Backpropagate(actualOutputVector, targetOutputVector, oCount, pMemorizedNeuronOutputs);
            _mMutexs[2].ReleaseMutex();
        }

        /// <summary>
        ///     StopBackpropagation function
        /// </summary>
        /// <summary>
        /// </summary>
        public void BackpropagationThread()
        {
            // thread for backpropagation training of NN
            //
            // thread is "owned" by the doc, and accepts a pointer to the doc
            // continuously backpropagates until m_bThreadAbortFlag is set to TRUE  	
            var inputVector = new double[841]; // note: 29x29, not 28x28
            var targetOutputVector = new double[10];
            var actualOutputVector = new double[10];
            //
            for (var i = 0; i < 841; i++)
                inputVector[i] = 0.0;
            for (var i = 0; i < 10; i++)
            {
                targetOutputVector[i] = 0.0;
                actualOutputVector[i] = 0.0;
            }
            //
            var memorizedNeuronOutputs = new NnNeuronOutputsList();
            //prepare for training

            while (true)
            {
                _mMutexs[3].WaitOne();
                if (_iNextPattern == 0)
                {
                    _mHiPerfTime.Start();
                    _mnistDataSet.RandomizePatternSequence();
                }
                var grayLevels = new byte[MPreferences.MnRowsImages * MPreferences.MnColsImages];
                var ipattern = _mnistDataSet.GetNextPatternNumber(_mnistDataSet.MbFromRandomizedPatternSequence);
                _mnistDataSet.MpImagePatterns[ipattern].PPattern.CopyTo(grayLevels, 0);
                var label = _mnistDataSet.MpImagePatterns[ipattern].NLabel;
                _iNextPattern++;
                if (label > 9) label = 9;

                // pad to 29x29, convert to double precision

                int ii;
                for (ii = 0; ii < 841; ++ii)
                    inputVector[ii] = 1.0; // one is white, -one is black

                // top row of inputVector is left as zero, left-most column is left as zero 

                for (ii = 0; ii < MyDefinitions.GcImageSize; ++ii)
                {
                    int jj;
                    for (jj = 0; jj < MyDefinitions.GcImageSize; ++jj)
                        inputVector[1 + jj + 29 * (ii + 1)] =
                            grayLevels[jj + MyDefinitions.GcImageSize * ii] / 128.0 -
                            1.0; // one is white, -one is black
                }

                // desired output vector

                for (ii = 0; ii < 10; ++ii)
                    targetOutputVector[ii] = -1.0;
                targetOutputVector[label] = 1.0;

                // now backpropagate
                _mMutexs[3].ReleaseMutex();

                BackpropagateNeuralNet(inputVector, 841, targetOutputVector, actualOutputVector, 10,
                    memorizedNeuronOutputs, MbDistortPatterns);

                _mMutexs[3].WaitOne();
                // calculate error for this pattern and post it to the hwnd so it can calculate a running 
                // estimate of MSE

                var dMse = 0.0;
                for (ii = 0; ii < 10; ++ii)
                    dMse += (actualOutputVector[ii] - targetOutputVector[ii]) *
                            (actualOutputVector[ii] - targetOutputVector[ii]);
                dMse /= 2.0;
                _dMse += dMse;
                _dMse200 += dMse;
                // determine the neural network's answer, and compare it to the actual answer.
                // Post a message if the answer was incorrect, so the dialog can display mis-recognition
                // statistics
                _nn++;
                var iBestIndex = 0;
                var maxValue = -99.0;

                for (ii = 0; ii < 10; ++ii)
                    if (actualOutputVector[ii] > maxValue)
                    {
                        iBestIndex = ii;
                        maxValue = actualOutputVector[ii];
                    }

                if (iBestIndex != label)
                    _mcMisrecognitions++;
                //
                // make step
                string s;
                if (_nn >= 200)
                {
                    _dMse200 /= 200;
                    s = "MSE:" + _dMse200.ToString(CultureInfo.InvariantCulture);
                    _form.Invoke(_form.DelegateAddObject, 4, s);
                    _dMse200 = 0;
                    _nn = 0;
                }

                s = $"{Convert.ToString(_iNextPattern)} Miss Number:{_mcMisrecognitions}";
                // Make synchronous call to main form.
                // MainForm.AddString function runs in main thread.
                // To make asynchronous call use BeginInvoke
                _form?.Invoke(_form.DelegateAddObject, 5, s);

                if (_iNextPattern >= _mnistDataSet.MpImagePatterns.Count - 1)
                {
                    _mHiPerfTime.Stop();
                    _dMse /= _iNextPattern;
                    s =
                        $"Completed Epochs:{Convert.ToString(_iEpochsCompleted + 1)}, MisPatterns:{Convert.ToString(_mcMisrecognitions)}, MSE:{_dMse.ToString(CultureInfo.InvariantCulture)}, Ex. time: {_mHiPerfTime.Duration}, eta:{Nn.MEtaLearningRate} ";
                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    _form?.Invoke(_form.DelegateAddObject, 3, s);
                    _mcMisrecognitions = 0;
                    _iEpochsCompleted++;
                    _iNextPattern = 0;
                    _dMse = 0;
                }
                //
                // check if thread is cancelled
                if (_mEventStop.WaitOne(0, true))
                {
                    // clean-up operations may be placed here

                    // ...
                    s = $"BackPropagation thread: {Thread.CurrentThread.Name} stoped";
                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    _form?.Invoke(_form.DelegateAddObject, 3, s);
                    // inform main thread that this thread stopped
                    _mEventStopped.Set();
                    _mMutexs[3].ReleaseMutex();
                    return;
                }
                _mMutexs[3].ReleaseMutex();
            } // end of main "while not abort flag" loop
        }
    }
}