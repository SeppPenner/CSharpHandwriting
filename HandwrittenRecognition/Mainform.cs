using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using HandwrittenRecogniration.NeuralNetwork;
using NeuralNetworkLibrary;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.DataFiles;
using NeuralNetworkLibrary.NNLayers;
using NeuralNetworkLibrary.NNNeurons;
using NeuralNetworkLibrary.NNWeights;
using NeuralNetworkN = NeuralNetworkLibrary.NeuralNetwork.NeuralNetwork;

namespace HandwrittenRecogniration
{
    // delegates used to call MainForm functions from worker thread
    public delegate void DelegateAddObject(int i, object s);

    public delegate void DelegateThreadFinished();

    public partial class Mainform : Form
    {
        private readonly ManualResetEvent _eventTestingStopThread;

        private readonly ManualResetEvent _eventTestingThreadStopped;
        //static uint _iBackpropThreadIdentifier;  // static member used by threads to identify themselves


        //
        //Thread

        // events used to stop worker thread
        private readonly ManualResetEvent _eventTrainingStopThread;

        private readonly ManualResetEvent _eventTrainingThreadStopped;

        //    
        private readonly Mutex _mainMutex;

        private readonly MnistDatabase _minstTestingDatabase;

        //MNIST Data set
        private readonly MnistDatabase _mnistTrainingDatabase;

        private readonly NeuralNetworkN _nn;
        private readonly Preferences _preference;

        private readonly NeuralNetworkN _trainingNn;

        // Delegate instances used to cal user interface functions 
        // from worker thread:
        public readonly DelegateAddObject DelegateAddObject;

        private bool _bDatabaseReady;
        private bool _bTestingDataReady;
        private bool _bTestingThreadRuning;

        private bool _bTrainingDataReady;
        private bool _bTrainingThreadRuning;

        /// <summary>
        /// </summary>
        private int _icurrentMnistPattern;

        private MnistDatabase _mnistdatabase;

        /// <summary>
        ///     My Defines
        /// </summary>
        private string _mnistWeightsFile;

        private List<Thread> _testingThreads;
        private List<Thread> _trainerThreads;

        // ReSharper disable once UnusedMember.Global
        public DelegateThreadFinished DelegateThreadFinished;

        public Mainform()
        {
            InitializeComponent();
            _preference = new Preferences();
            _mnistTrainingDatabase = new MnistDatabase();
            _minstTestingDatabase = new MnistDatabase();
            _mnistdatabase = _minstTestingDatabase;
            _icurrentMnistPattern = 0;
            _bTrainingDataReady = false;
            _bTestingDataReady = false;
            _bDatabaseReady = _bTestingDataReady;
            radioButtonMnistTestDatabase.Checked = true;
            radioButtonMnistTrainDatabase.Checked = false;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

            //Create Neural net work
            _nn = new NeuralNetworkN();
            _trainingNn = new NeuralNetworkN();
            CreateNnNetWork(_nn);
            // initialize delegates
            DelegateAddObject = AddObject;

            // initialize events
            _eventTrainingStopThread = new ManualResetEvent(false);
            _eventTrainingThreadStopped = new ManualResetEvent(false);
            _eventTestingStopThread = new ManualResetEvent(false);
            _eventTestingThreadStopped = new ManualResetEvent(false);
            _trainerThreads = null;
            _mainMutex = new Mutex();
            _mnistWeightsFile = "";
            _bTrainingThreadRuning = false;
            _bTestingThreadRuning = false;
        }

        private void AddObject(int iCondition, object value)
        {
            switch (iCondition)
            {
                case 1:
                    labelRecognizedValue.Text = (string) value;
                    break;
                case 2:
                    label7.Text = (string) value;
                    break;
                case 3:
                    listBox1.Items.Add((string) value);
                    break;
                case 4:
                    label2.Text = (string) value;
                    break;
                case 5:
                    label3.Text = (string) value;
                    break;
                case 6:
                    listBox2.Items.Add((string) value);
                    break;
                case 7:
                    label14.Text = (string) value;
                    break;
                case 8:
                    listBox2.Items.Add((string) value);
                    _bTestingThreadRuning = false;
                    buttonMnistTest.Enabled = true;
                    radioButtonTestingdatabase.Enabled = true;
                    radioButtonTrainingdatabase.Enabled = true;
                    break;
                case 9:
                    label7.Text = (string) value;
                    break;
            }
        }

        //draw training pattern to picturebox
        private void Next_Click(object sender, EventArgs e)
        {
            if (!_bDatabaseReady) return;
            if (_icurrentMnistPattern >= _mnistdatabase.MpImagePatterns.Count - 1) return;
            _icurrentMnistPattern++;
            var bitmap = new Bitmap(MyDefinitions.GcImageSize, MyDefinitions.GcImageSize, PixelFormat.Format32bppArgb);
            var pArray = _mnistdatabase.MpImagePatterns[_icurrentMnistPattern].PPattern;
            uint label = _mnistdatabase.MpImagePatterns[_icurrentMnistPattern].NLabel;
            label6.Text = label.ToString();
            var colors = new byte[4];
            for (var i = 0; i < 28; i++)
            for (var j = 0; j < 28; j++)
            {
                colors[0] = 255;
                colors[1] = Convert.ToByte(pArray[i * 28 + j]);
                colors[2] = Convert.ToByte(pArray[i * 28 + j]);
                colors[3] = Convert.ToByte(pArray[i * 28 + j]);
                var mArgb = BitConverter.ToInt32(colors, 0);
                bitmap.SetPixel(j, i, Color.FromArgb(mArgb));
            }
            pictureBox2.Image = bitmap;
            ImagePatternRecognization(_icurrentMnistPattern);
            label10.Text = _icurrentMnistPattern.ToString();
        }

        private void ImagePatternRecognization(int index)
        {
            var mutexs = new List<Mutex>(2);
            for (var i = 0; i < 2; i++)
            {
                var mutex = new Mutex();
                mutexs.Add(mutex);
            }

            var nnTessing = new NnTestPatterns(_nn, _mnistdatabase, _preference, _bDatabaseReady, null, null, this,
                mutexs);
            var thread = new Thread(() => nnTessing.PatternRecognizingThread(index));
            thread.Start();
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            if (!_bDatabaseReady) return;
            if (_icurrentMnistPattern <= 1) return;
            _icurrentMnistPattern -= 1;
            var bitmap = new Bitmap(MyDefinitions.GcImageSize, MyDefinitions.GcImageSize,
                PixelFormat.Format32bppArgb);
            var pArray = _mnistdatabase.MpImagePatterns[_icurrentMnistPattern].PPattern;
            uint ulabel = _mnistdatabase.MpImagePatterns[_icurrentMnistPattern].NLabel;
            label6.Text = ulabel.ToString();
            var colors = new byte[4];
            for (var i = 0; i < 28; i++)
            for (var j = 0; j < 28; j++)
            {
                colors[0] = 255;
                colors[1] = Convert.ToByte(pArray[i * 28 + j]);
                colors[2] = Convert.ToByte(pArray[i * 28 + j]);
                colors[3] = Convert.ToByte(pArray[i * 28 + j]);
                var mArgb = BitConverter.ToInt32(colors, 0);
                bitmap.SetPixel(j, i, Color.FromArgb(mArgb));
            }
            pictureBox2.Image = bitmap;
            ImagePatternRecognization(_icurrentMnistPattern);
            label10.Text = _icurrentMnistPattern.ToString();
        }

        private void StartBackPropagationbutton_Click(object sender, EventArgs e)
        {
            if (_bTrainingDataReady)
                OnStartBackpropagation();
        }

        /// <summary>
        /// </summary>
        private void OnStartBackpropagation()
        {
            if (_bTrainingDataReady && _bTrainingThreadRuning != true && _bTestingThreadRuning != true)
                using (var dlg = new BackPropagationParametersForm())
                {
                    var parameters = new BackPropagationParameters
                    {
                        McNumThreads = (uint) _preference.McNumBackpropThreads,
                        MInitialEta = _preference.MdInitialEtaLearningRate,
                        MMinimumEta = _preference.MdMinimumEtaLearningRate,
                        MEtaDecay = _preference.MdLearningRateDecay,
                        MAfterEvery = _preference.MnAfterEveryNBackprops,
                        MStartingPattern = 0,
                        MEstimatedCurrentMse = 0.10,
                        MbDistortPatterns = true
                    };
                    var eta = parameters.MInitialEta;
                    parameters.MStrInitialEtaMessage = $"Initial Learning Rate eta (currently, eta = {eta})";
                    var curPattern = 0;
                    parameters.MStrStartingPatternNum = $"Starting Pattern Number (currently at {curPattern})";
                    dlg.SetBackProParameters(parameters);
                    var mResult = dlg.ShowDialog();
                    if (mResult != DialogResult.OK) return;
                    parameters = dlg.GetBackProParameters();
                    var bRet = StartBackpropagation(parameters.McNumThreads, parameters.MInitialEta,
                        parameters.MMinimumEta, parameters.MEtaDecay, parameters.MAfterEvery,
                        parameters.MbDistortPatterns, parameters.MEstimatedCurrentMse);
                    if (bRet)
                        _bTrainingThreadRuning = true;
                }
        }

        private bool StartBackpropagation(uint iNumThreads /* =2 */, double initialEta /* =0.005 */,
            double minimumEta /* =0.000001 */, double etaDecay /* =0.990 */,
            uint nAfterEvery /* =1000 */, bool bDistortPatterns /* =TRUE */, double estimatedCurrentMse /* =1.0 */)
        {
            if (iNumThreads < 1)
                iNumThreads = 1;
            if (iNumThreads > 10) // 10 is arbitrary upper limit
                iNumThreads = 10;
            //initialize BackPropagation before process
            _nn.MEtaLearningRate = initialEta;
            _nn.MEtaLearningRatePrevious = initialEta;

            //run thread here
            _eventTrainingStopThread.Reset();
            _eventTrainingThreadStopped.Reset();
            _trainerThreads = new List<Thread>(2);
            _mnistTrainingDatabase.RandomizePatternSequence();
            //cleare mutex before run threads.
            var mutexs = new List<Mutex>(2);
            for (var i = 0; i < 4; i++)
            {
                var mutex = new Mutex();
                mutexs.Add(mutex);
            }

            //create neural network
            try
            {
                CreateNnNetWork(_trainingNn);
                //initialize weight parameters to the network
                if (_mnistWeightsFile != "")
                {
                    _mainMutex.WaitOne();
                    var fsIn = new FileStream(_mnistWeightsFile, FileMode.Open);
                    var arIn = new Archive(fsIn, ArchiveOp.Load);
                    _trainingNn.Serialize(arIn);
                    fsIn.Close();
                    _mainMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
            //
            var ntraining = new NnTrainPatterns(_trainingNn, _mnistTrainingDatabase, _preference, _bTrainingDataReady,
                _eventTrainingStopThread,
                _eventTrainingThreadStopped, this, mutexs)
            {
                MdMinimumEta = minimumEta,
                MdEtaDecay = etaDecay,
                MnAfterEveryNBackprops = nAfterEvery,
                MbDistortPatterns = bDistortPatterns,
                MdEstimatedCurrentMse = estimatedCurrentMse
                /* estimated number that will define whether a forward calculation's error is significant enough to warrant backpropagation*/
            };

            for (var i = 0; i < iNumThreads; i++)
            {
                var trainerThread = new Thread(ntraining.BackpropagationThread) {Name = $"Thread{i + 1}"};
                _trainerThreads.Add(trainerThread);
                trainerThread.Start();
            }

            return true;
        }

        /////////////////////////
        private void CreateNnNetWork(NeuralNetworkN network)
        {
            int ii, jj, kk;
            var icNeurons = 0;
            var icWeights = 0;
            double initWeight;
            string sLabel;
            var mRdm = new Random();
            // layer zero, the input layer.
            // Create neurons: exactly the same number of neurons as the input
            // vector of 29x29=841 pixels, and no weights/connections

            var pLayer = new NnLayer("Layer00", null);
            network.MLayers.Add(pLayer);

            for (ii = 0; ii < 841; ii++)
            {
                sLabel = $"Layer00_Neuro{ii}_Num{icNeurons}";
                pLayer.MNeurons.Add(new NnNeuron(sLabel));
                icNeurons++;
            }

            //double UNIFORM_PLUS_MINUS_ONE= (double)(2.0 * m_rdm.Next())/Constants.RAND_MAX - 1.0 ;

            // layer one:
            // This layer is a convolutional layer that has 6 feature maps.  Each feature 
            // map is 13x13, and each unit in the feature maps is a 5x5 convolutional kernel
            // of the input layer.
            // So, there are 13x13x6 = 1014 neurons, (5x5+1)x6 = 156 weights

            pLayer = new NnLayer("Layer01", pLayer);
            network.MLayers.Add(pLayer);

            for (ii = 0; ii < 1014; ii++)
            {
                sLabel = $"Layer01_Neuron{ii}_Num{icNeurons}";
                pLayer.MNeurons.Add(new NnNeuron(sLabel));
                icNeurons++;
            }

            for (ii = 0; ii < 156; ii++)
            {
                sLabel = $"Layer01_Weigh{ii}_Num{icWeights}";
                initWeight = 0.05 * (2.0 * mRdm.NextDouble() - 1.0);
                pLayer.MWeights.Add(new NnWeight(sLabel, initWeight));
            }

            // interconnections with previous layer: this is difficult
            // The previous layer is a top-down bitmap image that has been padded to size 29x29
            // Each neuron in this layer is connected to a 5x5 kernel in its feature map, which 
            // is also a top-down bitmap of size 13x13.  We move the kernel by TWO pixels, i.e., we
            // skip every other pixel in the input image

            var kernelTemplate = new[]
            {
                0, 1, 2, 3, 4,
                29, 30, 31, 32, 33,
                58, 59, 60, 61, 62,
                87, 88, 89, 90, 91,
                116, 117, 118, 119, 120
            };

            int iNumWeight;

            int fm;

            for (fm = 0; fm < 6; fm++)
            for (ii = 0; ii < 13; ii++)
            for (jj = 0; jj < 13; jj++)
            {
                iNumWeight = fm * 26; // 26 is the number of weights per feature map
                var n = pLayer.MNeurons[jj + ii * 13 + fm * 169];

                n.AddConnection((uint) MyDefinitions.UlongMax, (uint) iNumWeight++); // bias weight

                for (kk = 0; kk < 25; kk++)
                    // note: max val of index == 840, corresponding to 841 neurons in prev layer
                    n.AddConnection((uint) (2 * jj + 58 * ii + kernelTemplate[kk]), (uint) iNumWeight++);
            }


            // layer two:
            // This layer is a convolutional layer that has 50 feature maps.  Each feature 
            // map is 5x5, and each unit in the feature maps is a 5x5 convolutional kernel
            // of corresponding areas of all 6 of the previous layers, each of which is a 13x13 feature map
            // So, there are 5x5x50 = 1250 neurons, (5x5+1)x6x50 = 7800 weights

            pLayer = new NnLayer("Layer02", pLayer);
            network.MLayers.Add(pLayer);

            for (ii = 0; ii < 1250; ii++)
            {
                sLabel = $"Layer02_Neuron{ii}_Num{icNeurons}";
                pLayer.MNeurons.Add(new NnNeuron(sLabel));
                icNeurons++;
            }

            for (ii = 0; ii < 7800; ii++)
            {
                sLabel = $"Layer02_Weight{ii}_Num{icWeights}";
                initWeight = 0.05 * (2.0 * mRdm.NextDouble() - 1.0);
                pLayer.MWeights.Add(new NnWeight(sLabel, initWeight));
            }

            // Interconnections with previous layer: this is difficult
            // Each feature map in the previous layer is a top-down bitmap image whose size
            // is 13x13, and there are 6 such feature maps.  Each neuron in one 5x5 feature map of this 
            // layer is connected to a 5x5 kernel positioned correspondingly in all 6 parent
            // feature maps, and there are individual weights for the six different 5x5 kernels.  As
            // before, we move the kernel by TWO pixels, i.e., we
            // skip every other pixel in the input image.  The result is 50 different 5x5 top-down bitmap
            // feature maps

            var kernelTemplate2 = new[]
            {
                0, 1, 2, 3, 4,
                13, 14, 15, 16, 17,
                26, 27, 28, 29, 30,
                39, 40, 41, 42, 43,
                52, 53, 54, 55, 56
            };


            for (fm = 0; fm < 50; fm++)
            for (ii = 0; ii < 5; ii++)
            for (jj = 0; jj < 5; jj++)
            {
                iNumWeight = fm * 156; // 26 is the number of weights per feature map
                var n = pLayer.MNeurons[jj + ii * 5 + fm * 25];

                n.AddConnection((uint) MyDefinitions.UlongMax, (uint) iNumWeight++); // bias weight

                for (kk = 0; kk < 25; kk++)
                {
                    // note: max val of index == 1013, corresponding to 1014 neurons in prev layer
                    n.AddConnection((uint) (2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                    n.AddConnection((uint) (169 + 2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                    n.AddConnection((uint) (338 + 2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                    n.AddConnection((uint) (507 + 2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                    n.AddConnection((uint) (676 + 2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                    n.AddConnection((uint) (845 + 2 * jj + 26 * ii + kernelTemplate2[kk]), (uint) iNumWeight++);
                }
            }


            // layer three:
            // This layer is a fully-connected layer with 100 units.  Since it is fully-connected,
            // each of the 100 neurons in the layer is connected to all 1250 neurons in
            // the previous layer.
            // So, there are 100 neurons and 100*(1250+1)=125100 weights

            pLayer = new NnLayer("Layer03", pLayer);
            network.MLayers.Add(pLayer);

            for (ii = 0; ii < 100; ii++)
            {
                sLabel = $"Layer03_Neuron{ii}_Num{icNeurons}";
                pLayer.MNeurons.Add(new NnNeuron(sLabel));
                icNeurons++;
            }

            for (ii = 0; ii < 125100; ii++)
            {
                sLabel = $"Layer03_Weight{ii}_Num{icWeights}";
                initWeight = 0.05 * (2.0 * mRdm.NextDouble() - 1.0);
                pLayer.MWeights.Add(new NnWeight(sLabel, initWeight));
            }

            // Interconnections with previous layer: fully-connected

            iNumWeight = 0; // weights are not shared in this layer

            for (fm = 0; fm < 100; fm++)
            {
                var n = pLayer.MNeurons[fm];
                n.AddConnection((uint) MyDefinitions.UlongMax, (uint) iNumWeight++); // bias weight

                for (ii = 0; ii < 1250; ii++)
                    n.AddConnection((uint) ii, (uint) iNumWeight++);
            }


            // layer four, the final (output) layer:
            // This layer is a fully-connected layer with 10 units.  Since it is fully-connected,
            // each of the 10 neurons in the layer is connected to all 100 neurons in
            // the previous layer.
            // So, there are 10 neurons and 10*(100+1)=1010 weights

            pLayer = new NnLayer("Layer04", pLayer);
            network.MLayers.Add(pLayer);

            for (ii = 0; ii < 10; ii++)
            {
                sLabel = $"Layer04_Neuron{ii}_Num{icNeurons}";
                pLayer.MNeurons.Add(new NnNeuron(sLabel));
                icNeurons++;
            }

            for (ii = 0; ii < 1010; ii++)
            {
                sLabel = $"Layer04_Weight{ii}_Num{icWeights}";
                initWeight = 0.05 * (2.0 * mRdm.NextDouble() - 1.0);
                pLayer.MWeights.Add(new NnWeight(sLabel, initWeight));
            }

            // Interconnections with previous layer: fully-connected

            iNumWeight = 0; // weights are not shared in this layer

            for (fm = 0; fm < 10; fm++)
            {
                var n = pLayer.MNeurons[fm];
                n.AddConnection((uint) MyDefinitions.UlongMax, (uint) iNumWeight++); // bias weight

                for (ii = 0; ii < 100; ii++)
                    n.AddConnection((uint) ii, (uint) iNumWeight++);
            }
        }

        private void Mainform_Load(object sender, EventArgs e)
        {
        }

        //stop threads.
        private void StopBackPropagationbutton_Click(object sender, EventArgs e)
        {
            if (!_bTrainingThreadRuning) return;
            if (StopTheads(_trainerThreads, _eventTrainingStopThread, _eventTrainingThreadStopped))
                BackPropagationThreadsFinished(); // set initial state of buttons
        }

        private void BackPropagationThreadsFinished()
        {
            if (!_bTrainingThreadRuning) return;
            var msResult = MessageBox.Show(@"Do you want to save Neural Network data ?", @"Save Neural Network Data",
                MessageBoxButtons.OKCancel);
            if (msResult == DialogResult.OK)
                using (var saveFileDialog1 = new SaveFileDialog
                {
                    Filter = @"Mnist Neural network file (*.nnt)|*.nnt",
                    Title = @"Save Neural network File"
                })
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var fsIn = saveFileDialog1.OpenFile();
                        var arIn = new Archive(fsIn, ArchiveOp.Store);
                        _trainingNn.Serialize(arIn);
                        fsIn.Close();
                    }
                }
            _bTrainingThreadRuning = false;
        }

        // Load Image from file
        // ReSharper disable once UnusedMember.Local
        private Bitmap CreateNonIndexedImage(Image src)
        {
            var newBmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (var gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }
            return newBmp;
        }

        private void NetworkParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog
            {
                Filter = @"Mnist Neural network file (*.nnt)|*.nnt",
                Title = @"Open Neural network File"
            })
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                _mainMutex.WaitOne();
                _mnistWeightsFile = openFileDialog1.FileName;
                var fsIn = openFileDialog1.OpenFile();
                var arIn = new Archive(fsIn, ArchiveOp.Load);
                _nn.Serialize(arIn);
                fsIn.Close();
                _mainMutex.ReleaseMutex();
            }
        }

        private void MNISTDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _bTrainingDataReady = _mnistTrainingDatabase.LoadMinstFiles();
            if (_bTrainingDataReady)
            {
                //update Preferences parametters
                if (_mnistTrainingDatabase.MpImagePatterns.Count != _preference.MnItemsTrainingImages)
                {
                    _preference.MnItemsTrainingImages = (uint) _mnistTrainingDatabase.MpImagePatterns.Count;
                    _preference.MnItemsTrainingLabels = (uint) _mnistTrainingDatabase.MpImagePatterns.Count;
                }
                radioButtonMnistTrainDatabase.Enabled = true;
                radioButtonTrainingdatabase.Enabled = true;
                buttonMnistNext.Enabled = true;
                buttonMnistPrevious.Enabled = true;
                _bDatabaseReady = _bTrainingDataReady;
                _mnistdatabase = _mnistTrainingDatabase;
            }
            else
            {
                radioButtonMnistTrainDatabase.Enabled = false;
                return;
            }
            _bTestingDataReady = _minstTestingDatabase.LoadMinstFiles();
            if (_bTestingDataReady)
            {
                //update Preferences parametters
                if (_minstTestingDatabase.MpImagePatterns.Count != _preference.MnItemsTestingImages)
                {
                    _preference.MnItemsTestingImages = (uint) _minstTestingDatabase.MpImagePatterns.Count;
                    _preference.MnItemsTestingLabels = (uint) _minstTestingDatabase.MpImagePatterns.Count;
                }
                radioButtonMnistTestDatabase.Enabled = true;
                radioButtonMnistTestDatabase.Checked = true;
                radioButtonTestingdatabase.Enabled = true;
                radioButtonTestingdatabase.Checked = true;
                buttonMnistNext.Enabled = true;
                buttonMnistPrevious.Enabled = true;
                _bDatabaseReady = _bTestingDataReady;
                _mnistdatabase = _minstTestingDatabase;
            }
            else
            {
                radioButtonMnistTestDatabase.Enabled = false;
            }
        }

        private void ButtonMnistTest_Click(object sender, EventArgs e)
        {
            if (_bTestingThreadRuning || _bTrainingThreadRuning) return;
            var mutexs = new List<Mutex>(2);
            var theadsNum = (int) numericUpDownThreads.Value;
            NnTestPatterns nnTesting;
            var nnNetwork = new NeuralNetworkN();
            bool bDatabaseforTest;
            //create neural network
            try
            {
                CreateNnNetWork(nnNetwork);
                //initialize weight parameters to the network
                if (_mnistWeightsFile != "")
                {
                    _mainMutex.WaitOne();
                    var fsIn = new FileStream(_mnistWeightsFile, FileMode.Open);
                    var arIn = new Archive(fsIn, ArchiveOp.Load);
                    nnNetwork.Serialize(arIn);
                    fsIn.Close();
                    _mainMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            //
            if (radioButtonTestingdatabase.Checked)
            {
                if (_bTestingDataReady)
                {
                    nnTesting = new NnTestPatterns(nnNetwork, _minstTestingDatabase, _preference, _bTestingDataReady,
                        _eventTestingStopThread, _eventTestingThreadStopped, this, mutexs);
                    bDatabaseforTest = _bTestingDataReady;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (_bTrainingDataReady)
                {
                    nnTesting = new NnTestPatterns(nnNetwork, _mnistTrainingDatabase, _preference, _bTrainingDataReady,
                        _eventTestingStopThread, _eventTestingThreadStopped, this, mutexs);
                    bDatabaseforTest = _bTrainingDataReady;
                }
                else
                {
                    return;
                }
            }
            if (!bDatabaseforTest) return;
            {
                //
                listBox2.Items.Clear();
                for (var i = 0; i < 2; i++)
                {
                    var mutex = new Mutex();
                    mutexs.Add(mutex);
                }
                _eventTestingStopThread.Reset();
                _eventTestingThreadStopped.Reset();
                _testingThreads = new List<Thread>(2);

                try
                {
                    for (var i = 0; i < theadsNum; i++)
                    {
                        var thread = new Thread(delegate()
                        {
                            nnTesting.PatternsTestingThread((int) numericUpDownNumberofTestPattern.Value);
                        });
                        _testingThreads.Add(thread);
                        thread.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return;
                }
                _bTestingThreadRuning = true;
                radioButtonTestingdatabase.Enabled = false;
                radioButtonTrainingdatabase.Enabled = false;
                buttonMnistTest.Enabled = false;
            }
        }

        private static bool StopTheads(IList<Thread> threads, ManualResetEvent eventStopThread,
            ManualResetEvent eventThreadStopped)
        {
            try
            {
                if (threads != null && threads.Count > 0 && threads[0].IsAlive) // thread is active
                {
                    // set event "Stop"
                    eventStopThread.Set();
                    foreach (var thread in threads)
                        // wait when thread  will stop or finish

                        while (thread.IsAlive || thread.IsAlive)
                        {
                            // We cannot use here infinite wait because our thread
                            // makes syncronous calls to main form, this will cause deadlock.
                            // Instead of this we wait for event some appropriate time
                            // (and by the way give time to worker thread) and
                            // process events. These events may contain Invoke calls.
                            if (WaitHandle.WaitAll(
                                new WaitHandle[] {eventThreadStopped},
                                100,
                                true))
                                break;

                            Application.DoEvents();
                        }
                }
                threads?.Clear();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        private void ButtonStopMnistTest_Click(object sender, EventArgs e)
        {
            if (!_bTestingThreadRuning) return;
            if (!StopTheads(_testingThreads, _eventTestingStopThread, _eventTestingThreadStopped)) return;
            _bTestingThreadRuning = false;
            radioButtonTestingdatabase.Enabled = true;
            radioButtonTrainingdatabase.Enabled = true;
            buttonMnistTest.Enabled = true;
            //grayscale bitmap
        }

        private void RadioButtonTestingdatabaseCheckedChanged(object sender, EventArgs e)
        {
            numericUpDownNumberofTestPattern.Maximum = radioButtonTestingdatabase.Checked ? 9999 : 59999;
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonMnistTestDatabase.Checked)
            {
                _mnistdatabase = _minstTestingDatabase;
                _bDatabaseReady = _bTestingDataReady;
                _icurrentMnistPattern = 0;
            }
            else
            {
                _mnistdatabase = _minstTestingDatabase;
                _bDatabaseReady = _bTrainingDataReady;
                _icurrentMnistPattern = 0;
            }
        }

        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_bTestingThreadRuning && !_bTrainingThreadRuning) return;
            // ReSharper disable once UnusedVariable
            var result = MessageBox.Show(
                @"Sorry, some threads are running. Please stop them before  you can close the program",
                "", MessageBoxButtons.OK);
            e.Cancel = true;
        }

        private void ViewHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                @"Handwritten character recognition program Vesion 0.1,\nCopyright (C) 2010-2011, \nPham Viet Dung, Vietnam Maritime University" +
                @"\nEmail:vietdungiitb@vimaru.edu.vn",
                @"About Handwritten character recognition program", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}