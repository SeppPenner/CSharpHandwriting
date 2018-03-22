using System.Collections.Generic;
using System.Threading;
using NeuralNetworkLibrary;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.DataFiles;
using NeuralNetworkLibrary.NeuralNetwork;
using NeuralNetworkLibrary.NNNeurons;
using NeuralNetworkN = NeuralNetworkLibrary.NeuralNetwork.NeuralNetwork;

namespace HandwrittenRecogniration.NeuralNetwork
{
    public class NnTestPatterns : NnForwardPropagation
    {
        private readonly Mainform _form;
        private readonly ManualResetEvent _mEventStop;
        private readonly ManualResetEvent _mEventStopped;
        private readonly HiPerfTimer _mHiPerfTime;
        private readonly List<Mutex> _mMutexs;
        private readonly MnistDatabase _mnistDataSet;
        private uint _iMisNum;
        private uint _iNextPattern;

        public NnTestPatterns(NeuralNetworkN neuronNet, MnistDatabase testtingSet,
            Preferences preferences, bool testingDataReady,
            ManualResetEvent eventStop,
            ManualResetEvent eventStopped,
            Mainform form, List<Mutex> mutexs)
        {
            _mCurrentPatternIndex = 0;
            BDataReady = testingDataReady;
            Nn = neuronNet;
            _iNextPattern = 0;
            _mEventStop = eventStop;
            _mEventStopped = eventStopped;
            _form = form;
            _mHiPerfTime = new HiPerfTimer();

            //Initialize Gaussian Kernel
            MPreferences = preferences;
            GetGaussianKernel(preferences.MdElasticSigma);
            _mnistDataSet = testtingSet;
            _mMutexs = mutexs;
        }

        // ReSharper disable once UnusedMember.Global
        public NnTestPatterns(NeuralNetworkN neuronNet, Preferences preferences,
            Mainform form, List<Mutex> mutexs)
        {
            _mCurrentPatternIndex = 0;
            BDataReady = true;
            Nn = neuronNet;
            _iNextPattern = 0;
            _mEventStop = null;
            _mEventStopped = null;
            _form = form;
            _mHiPerfTime = new HiPerfTimer();
            _iMisNum = 0;

            //Initialize Gaussian Kernel
            MPreferences = preferences;
            GetGaussianKernel(preferences.MdElasticSigma);
            _mnistDataSet = null;
            _mMutexs = mutexs;
        }

        public void PatternsTestingThread(int iPatternNum)
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

            _mHiPerfTime.Start();

            while (_iNextPattern < iPatternNum)
            {
                _mMutexs[1].WaitOne();

                var grayLevels = new byte[MPreferences.MnRowsImages * MPreferences.MnColsImages];
                //iSequentialNum = m_MnistDataSet.GetCurrentPatternNumber(m_MnistDataSet.m_bFromRandomizedPatternSequence);
                _mnistDataSet.MpImagePatterns[(int) _iNextPattern].PPattern.CopyTo(grayLevels, 0);
                var label = _mnistDataSet.MpImagePatterns[(int) _iNextPattern].NLabel;
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
                // forward calculate through the neural net

                CalculateNeuralNet(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);

                var iBestIndex = 0;
                var maxValue = -99.0;

                for (ii = 0; ii < 10; ++ii)
                {
                    if (!(actualOutputVector[ii] > maxValue)) continue;
                    iBestIndex = ii;
                    maxValue = actualOutputVector[ii];
                }
                string s;
                if (iBestIndex != label)
                {
                    _iMisNum++;
                    s = "Pattern No:" + _iNextPattern + " Recognized value:" + iBestIndex + " Actual value:" + label;
                    _form?.Invoke(_form.DelegateAddObject, 6, s);
                }
                else
                {
                    s = _iNextPattern + ", Mis Nums:" + _iMisNum;
                    _form?.Invoke(_form.DelegateAddObject, 7, s);
                }
                // check if thread is cancelled
                if (_mEventStop.WaitOne(0, true))
                {
                    // clean-up operations may be placed here
                    // ...
                    s = $"Mnist Testing thread: {Thread.CurrentThread.Name} stoped";
                    // Make synchronous call to main form.
                    // MainForm.AddString function runs in main thread.
                    // To make asynchronous call use BeginInvoke
                    _form?.Invoke(_form.DelegateAddObject, 8, s);

                    // inform main thread that this thread stopped
                    _mEventStopped.Set();
                    _mMutexs[1].ReleaseMutex();
                    return;
                }
                _iNextPattern++;
                _mMutexs[1].ReleaseMutex();
            }
            {
                var s = $"Mnist Testing thread: {Thread.CurrentThread.Name} stoped";
                _form?.Invoke(_form.DelegateAddObject, 8, s);
            }
        }

        public void PatternRecognizingThread(int iPatternNo)
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


            int ii;


            var memorizedNeuronOutputs = new NnNeuronOutputsList();
            //prepare for training
            _iNextPattern = 0;
            _iMisNum = 0;


            _mMutexs[1].WaitOne();
            if (_iNextPattern == 0)
                _mHiPerfTime.Start();
            var grayLevels = new byte[MPreferences.MnRowsImages * MPreferences.MnColsImages];
            _mnistDataSet.MpImagePatterns[iPatternNo].PPattern.CopyTo(grayLevels, 0);
            var label = _mnistDataSet.MpImagePatterns[iPatternNo].NLabel;
            _iNextPattern++;

            if (label > 9) label = 9;

            // pad to 29x29, convert to double precision

            for (ii = 0; ii < 841; ++ii)
                inputVector[ii] = 1.0; // one is white, -one is black

            // top row of inputVector is left as zero, left-most column is left as zero 

            for (ii = 0; ii < MyDefinitions.GcImageSize; ++ii)
            {
                int jj;
                for (jj = 0; jj < MyDefinitions.GcImageSize; ++jj)
                    inputVector[1 + jj + 29 * (ii + 1)] =
                        grayLevels[jj + MyDefinitions.GcImageSize * ii] / 128.0 - 1.0; // one is white, -one is black
            }

            // desired output vector

            for (ii = 0; ii < 10; ++ii)
                targetOutputVector[ii] = -1.0;
            targetOutputVector[label] = 1.0;
            // forward calculate through the neural net

            CalculateNeuralNet(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);
            var iBestIndex = 0;
            var maxValue = -99.0;

            for (ii = 0; ii < 10; ++ii)
            {
                if (!(actualOutputVector[ii] > maxValue)) continue;
                iBestIndex = ii;
                maxValue = actualOutputVector[ii];
            }

            var s = iBestIndex.ToString();
            _form.Invoke(_form.DelegateAddObject, 2, s);
            // check if thread is cancelled
            _mMutexs[1].ReleaseMutex();
        }

        // ReSharper disable once UnusedMember.Global
        public void PatternRecognizingThread(byte[] grayLevels)
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

            byte label = 0;
            int ii;


            var memorizedNeuronOutputs = new NnNeuronOutputsList();


            _mMutexs[1].WaitOne();
            if (_iNextPattern == 0)
                _mHiPerfTime.Start();
            if (label > 9) label = 9;

            // pad to 29x29, convert to double precision

            for (ii = 0; ii < 841; ++ii)
                inputVector[ii] = 1.0; // one is white, -one is black

            // top row of inputVector is left as zero, left-most column is left as zero 

            for (ii = 0; ii < MyDefinitions.GcImageSize; ++ii)
            {
                int jj;
                for (jj = 0; jj < MyDefinitions.GcImageSize; ++jj)
                    inputVector[1 + jj + 29 * (ii + 1)] =
                        grayLevels[jj + MyDefinitions.GcImageSize * ii] / 128.0 - 1.0; // one is white, -one is black
            }

            // desired output vector

            for (ii = 0; ii < 10; ++ii)
                targetOutputVector[ii] = -1.0;
            targetOutputVector[label] = 1.0;
            // forward calculate through the neural net

            CalculateNeuralNet(inputVector, 841, actualOutputVector, 10, memorizedNeuronOutputs, false);
            var iBestIndex = 0;
            var maxValue = -99.0;

            for (ii = 0; ii < 10; ++ii)
            {
                if (!(actualOutputVector[ii] > maxValue)) continue;
                iBestIndex = ii;
                maxValue = actualOutputVector[ii];
            }

            var s = iBestIndex.ToString();
            _form.Invoke(_form.DelegateAddObject, 1, s);
            // check if thread is cancelled

            _mMutexs[1].ReleaseMutex();
        }
    }
}