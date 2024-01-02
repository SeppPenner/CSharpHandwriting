// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition;

/// <summary>
/// The main form.
/// </summary>
public partial class MainForm : DelegateForm
{
    /// <summary>
    /// The testing thread stop event.
    /// </summary>
    private readonly ManualResetEvent testingThreadStop;

    /// <summary>
    /// The testing thread stopped event.
    /// </summary>
    private readonly ManualResetEvent testingThreadStopped;

    /// <summary>
    /// The training thread stop event.
    /// </summary>
    private readonly ManualResetEvent trainingThreadStop;

    /// <summary>
    /// The training thread stopped event.
    /// </summary>
    private readonly ManualResetEvent trainingThreadStopped;

    /// <summary>
    /// The main mutex.
    /// </summary>
    private readonly Mutex mainMutex;

    /// <summary>
    /// The testing database.
    /// </summary>
    private readonly NeuronalNetworkDatabase testingDatabase;

    /// <summary>
    /// The training database.
    /// </summary>
    private readonly NeuronalNetworkDatabase trainingDatabase;

    /// <summary>
    /// The neuronal network.
    /// </summary>
    private readonly NeuronalNetwork neuronalNetwork;

    /// <summary>
    /// The preferences.
    /// </summary>
    private readonly Preferences preferences;

    /// <summary>
    /// The training network.
    /// </summary>
    private readonly NeuronalNetwork trainingNetwork;

    /// <summary>
    /// A value indicating whether the database is ready.
    /// </summary>
    private bool databaseReady;

    /// <summary>
    /// A value indicating whether the testing data is ready.
    /// </summary>
    private bool testingDataReady;

    /// <summary>
    /// A value indicating whether the testing thread is running.
    /// </summary>
    private bool testingThreadRunning;

    /// <summary>
    /// A value indicating whether the training data is ready.
    /// </summary>
    private bool trainingDataReady;

    /// <summary>
    /// A value indicating whether the training thread is running.
    /// </summary>
    private bool trainingThreadRunning;

    /// <summary>
    /// The current pattern.
    /// </summary>
    private int currentPattern;

    /// <summary>
    /// The database.
    /// </summary>
    private NeuronalNetworkDatabase? database;

    /// <summary>
    /// The weights file.
    /// </summary>
    private string weightsFile;

    /// <summary>
    /// The testing threads.
    /// </summary>
    private List<Thread> testingThreads = new();

    /// <summary>
    /// The training threads.
    /// </summary>
    private List<Thread> trainingThreads = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    public MainForm()
    {
        this.InitializeComponent();
        this.preferences = new Preferences();
        this.trainingDatabase = new NeuronalNetworkDatabase();
        this.testingDatabase = new NeuronalNetworkDatabase();
        this.database = this.testingDatabase;
        this.currentPattern = 0;
        this.trainingDataReady = false;
        this.testingDataReady = false;
        this.databaseReady = this.testingDataReady;
        this.radioButtonMnistTestDatabase.Checked = true;
        this.radioButtonMnistTrainDatabase.Checked = false;
        this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;

        // Create the neuronal network
        this.neuronalNetwork = new NeuronalNetwork();
        this.trainingNetwork = new NeuronalNetwork();
        CreateNeuronalNetwork(this.neuronalNetwork);

        // Initialize delegates
        this.DelegateAddObject = this.AddObject;

        // Initialize events
        this.trainingThreadStop = new ManualResetEvent(false);
        this.trainingThreadStopped = new ManualResetEvent(false);
        this.testingThreadStop = new ManualResetEvent(false);
        this.testingThreadStopped = new ManualResetEvent(false);
        this.trainingThreads = new();
        this.mainMutex = new Mutex();
        this.weightsFile = string.Empty;
        this.trainingThreadRunning = false;
        this.testingThreadRunning = false;
    }

    /// <summary>
    /// A delegate to add objects.
    /// </summary>
    public readonly new DelegateAddObject? DelegateAddObject;

    /// <summary>
    /// A delegate to handle the finished thread.
    /// </summary>
    public new DelegateThreadFinished? DelegateThreadFinished;

    /// <summary>
    /// Stops the threads.
    /// </summary>
    /// <param name="threads">The threads.</param>
    /// <param name="stopThread">The stop thread event.</param>
    /// <param name="threadStopped">The thread stopped event.</param>
    /// <returns>A value indicating whether the threads have been stopped or not.</returns>
    private static bool StopThreads(IList<Thread> threads, EventWaitHandle stopThread, WaitHandle threadStopped)
    {
        try
        {
            // Thread is active
            if (threads != null && threads.Count > 0 && threads[0].IsAlive)
            {
                // Set stop event
                stopThread.Set();

                // Wait when thread  will stop or finish
                foreach (var thread in threads)
                {
                    while (thread.IsAlive || thread.IsAlive)
                    {
                        // We cannot use here infinite wait because our thread
                        // makes synchronous calls to main form, this will cause deadlock.
                        // Instead of this we wait for event some appropriate time
                        // (and by the way give time to worker thread) and
                        // process events. These events may contain Invoke calls.
                        if (WaitHandle.WaitAll(new[] { threadStopped }, 100, true))
                        {
                            break;
                        }

                        Application.DoEvents();
                    }
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

    /// <summary>
    /// Creates the neuronal network.
    /// </summary>
    /// <param name="network">The network.</param>
    private static void CreateNeuronalNetwork(NeuronalNetwork network)
    {
        int ii, jj, kk;
        var numberOfNeurons = 0;
        const int NumberOfWeights = 0;
        double initWeight;
        string localLabel;
        var random = new Random();

        // Layer zero, the input layer.
        // Create neurons: exactly the same number of neurons as the input
        // vector of 29x29=841 pixels, and no weights/connections
        var firstLayer = new NeuronalNetworkLayer("Layer00", null);
        network.LayersList.Add(firstLayer);

        for (ii = 0; ii < 841; ii++)
        {
            localLabel = $"Layer00_Neuron{ii}_Num{numberOfNeurons}";
            firstLayer.Neurons.Add(new NeuronalNetworkNeuron(localLabel));
            numberOfNeurons++;
        }

        // Layer one:
        // This layer is a convolutional layer that has 6 feature maps. Each feature 
        // map is 13x13, and each unit in the feature maps is a 5x5 convolutional kernel
        // of the input layer.
        // So, there are 13x13x6 = 1014 neurons, (5x5+1)x6 = 156 weights
        firstLayer = new NeuronalNetworkLayer("Layer01", firstLayer);
        network.LayersList.Add(firstLayer);

        for (ii = 0; ii < 1014; ii++)
        {
            localLabel = $"Layer01_Neuron{ii}_Num{numberOfNeurons}";
            firstLayer.Neurons.Add(new NeuronalNetworkNeuron(localLabel));
            numberOfNeurons++;
        }

        for (ii = 0; ii < 156; ii++)
        {
            localLabel = $"Layer01_Weigh{ii}_Num{NumberOfWeights}";
            initWeight = 0.05 * ((2.0 * random.NextDouble()) - 1.0);
            firstLayer.Weights.Add(new NeuronalNetworkWeight(localLabel, initWeight));
        }

        // Interconnections with previous layer: this is difficult
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

        int numberOfWeightsPerFeatureMap;

        int i;

        for (i = 0; i < 6; i++)
        {
            for (ii = 0; ii < 13; ii++)
            {
                for (jj = 0; jj < 13; jj++)
                {
                    // 26 is the number of weights per feature map
                    numberOfWeightsPerFeatureMap = i * 26;
                    var n = firstLayer.Neurons[jj + (ii * 13) + (i * 169)];

                    // Bias weight
                    n.AddConnection(
                        (uint)SystemGlobals.UlongMaximum,
                        (uint)numberOfWeightsPerFeatureMap++);

                    // Note: max val of index == 840, corresponding to 841 neurons in prev layer
                    for (kk = 0; kk < 25; kk++)
                    {
                        n.AddConnection(
                            (uint)((2 * jj) + (58 * ii) + kernelTemplate[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                    }
                }
            }
        }

        // Layer two:
        // This layer is a convolutional layer that has 50 feature maps. Each feature 
        // map is 5x5, and each unit in the feature maps is a 5x5 convolutional kernel
        // of corresponding areas of all 6 of the previous layers, each of which is a 13x13 feature map
        // So, there are 5x5x50 = 1250 neurons, (5x5+1)x6x50 = 7800 weights
        firstLayer = new NeuronalNetworkLayer("Layer02", firstLayer);
        network.LayersList.Add(firstLayer);

        for (ii = 0; ii < 1250; ii++)
        {
            localLabel = $"Layer02_Neuron{ii}_Num{numberOfNeurons}";
            firstLayer.Neurons.Add(new NeuronalNetworkNeuron(localLabel));
            numberOfNeurons++;
        }

        for (ii = 0; ii < 7800; ii++)
        {
            localLabel = $"Layer02_Weight{ii}_Num{NumberOfWeights}";
            initWeight = 0.05 * ((2.0 * random.NextDouble()) - 1.0);
            firstLayer.Weights.Add(new NeuronalNetworkWeight(localLabel, initWeight));
        }

        // Interconnections with previous layer: this is difficult
        // Each feature map in the previous layer is a top-down bitmap image whose size
        // is 13x13, and there are 6 such feature maps. Each neuron in one 5x5 feature map of this 
        // layer is connected to a 5x5 kernel positioned correspondingly in all 6 parent
        // feature maps, and there are individual weights for the six different 5x5 kernels.  As
        // before, we move the kernel by TWO pixels, i.e., we
        // skip every other pixel in the input image. The result is 50 different 5x5 top-down bitmap
        // feature maps
        var kernelTemplate2 = new[]
        {
                0, 1, 2, 3, 4,
                13, 14, 15, 16, 17,
                26, 27, 28, 29, 30,
                39, 40, 41, 42, 43,
                52, 53, 54, 55, 56
            };

        for (i = 0; i < 50; i++)
        {
            for (ii = 0; ii < 5; ii++)
            {
                for (jj = 0; jj < 5; jj++)
                {
                    // 26 is the number of weights per feature map
                    numberOfWeightsPerFeatureMap = i * 156;
                    var n = firstLayer.Neurons[jj + (ii * 5) + (i * 25)];

                    // Bias weight
                    n.AddConnection((uint)SystemGlobals.UlongMaximum, (uint)numberOfWeightsPerFeatureMap++);

                    for (kk = 0; kk < 25; kk++)
                    {
                        // note: max val of index == 1013, corresponding to 1014 neurons in prev layer
                        n.AddConnection(
                            (uint)((2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                        n.AddConnection(
                            (uint)(169 + (2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                        n.AddConnection(
                            (uint)(338 + (2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                        n.AddConnection(
                            (uint)(507 + (2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                        n.AddConnection(
                            (uint)(676 + (2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                        n.AddConnection(
                            (uint)(845 + (2 * jj) + (26 * ii) + kernelTemplate2[kk]),
                            (uint)numberOfWeightsPerFeatureMap++);
                    }
                }
            }
        }

        // Layer three:
        // This layer is a fully-connected layer with 100 units.  Since it is fully-connected,
        // each of the 100 neurons in the layer is connected to all 1250 neurons in
        // the previous layer.
        // So, there are 100 neurons and 100*(1250+1)=125100 weights
        firstLayer = new NeuronalNetworkLayer("Layer03", firstLayer);
        network.LayersList.Add(firstLayer);

        for (ii = 0; ii < 100; ii++)
        {
            localLabel = $"Layer03_Neuron{ii}_Num{numberOfNeurons}";
            firstLayer.Neurons.Add(new NeuronalNetworkNeuron(localLabel));
            numberOfNeurons++;
        }

        for (ii = 0; ii < 125100; ii++)
        {
            localLabel = $"Layer03_Weight{ii}_Num{NumberOfWeights}";
            initWeight = 0.05 * ((2.0 * random.NextDouble()) - 1.0);
            firstLayer.Weights.Add(new NeuronalNetworkWeight(localLabel, initWeight));
        }

        // Interconnections with previous layer: fully-connected
        // Weights are not shared in this layer
        numberOfWeightsPerFeatureMap = 0;

        for (i = 0; i < 100; i++)
        {
            var n = firstLayer.Neurons[i];

            // Bias weight
            n.AddConnection((uint)SystemGlobals.UlongMaximum, (uint)numberOfWeightsPerFeatureMap++);

            for (ii = 0; ii < 1250; ii++)
            {
                n.AddConnection((uint)ii, (uint)numberOfWeightsPerFeatureMap++);
            }
        }

        // Layer four, the final (output) layer:
        // This layer is a fully-connected layer with 10 units. Since it is fully-connected,
        // each of the 10 neurons in the layer is connected to all 100 neurons in
        // the previous layer.
        // So, there are 10 neurons and 10*(100+1)=1010 weights
        firstLayer = new NeuronalNetworkLayer("Layer04", firstLayer);
        network.LayersList.Add(firstLayer);

        for (ii = 0; ii < 10; ii++)
        {
            localLabel = $"Layer04_Neuron{ii}_Num{numberOfNeurons}";
            firstLayer.Neurons.Add(new NeuronalNetworkNeuron(localLabel));
            numberOfNeurons++;
        }

        for (ii = 0; ii < 1010; ii++)
        {
            localLabel = $"Layer04_Weight{ii}_Num{NumberOfWeights}";
            initWeight = 0.05 * ((2.0 * random.NextDouble()) - 1.0);
            firstLayer.Weights.Add(new NeuronalNetworkWeight(localLabel, initWeight));
        }

        // Interconnections with previous layer: fully-connected
        // Weights are not shared in this layer
        numberOfWeightsPerFeatureMap = 0;

        for (i = 0; i < 10; i++)
        {
            var n = firstLayer.Neurons[i];

            // Bias weight
            n.AddConnection((uint)SystemGlobals.UlongMaximum, (uint)numberOfWeightsPerFeatureMap++);

            for (ii = 0; ii < 100; ii++)
            {
                n.AddConnection((uint)ii, (uint)numberOfWeightsPerFeatureMap++);
            }
        }
    }

    /// <summary>
    /// Adds an object to the view.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value.</param>
    private void AddObject(int condition, object value)
    {
        switch (condition)
        {
            case 1:
                this.labelRecognizedValue.Text = (string)value;
                break;
            case 2:
                this.label7.Text = (string)value;
                break;
            case 3:
                this.listBox1.Items.Add((string)value);
                break;
            case 4:
                this.label2.Text = (string)value;
                break;
            case 5:
                this.label3.Text = (string)value;
                break;
            case 6:
                this.listBox2.Items.Add((string)value);
                break;
            case 7:
                this.label14.Text = (string)value;
                break;
            case 8:
                this.listBox2.Items.Add((string)value);
                this.testingThreadRunning = false;
                this.buttonTest.Enabled = true;
                this.radioButtonTestingdatabase.Enabled = true;
                this.radioButtonTrainingdatabase.Enabled = true;
                break;
            case 9:
                this.label7.Text = (string)value;
                break;
        }
    }

    /// <summary>
    /// Handles the next step click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NextClick(object sender, EventArgs e)
    {
        if (!this.databaseReady)
        {
            return;
        }

        if (this.database is null)
        {
            return;
        }

        if (this.currentPattern >= this.database.ImagePatterns.Count - 1)
        {
            return;
        }

        this.currentPattern++;
        var bitmap = new Bitmap(SystemGlobals.ImageSize, SystemGlobals.ImageSize, PixelFormat.Format32bppArgb);
        var array = this.database.ImagePatterns[this.currentPattern].Pattern;
        uint label = this.database.ImagePatterns[this.currentPattern].Label;
        this.label6.Text = label.ToString();
        var colors = new byte[4];

        for (var i = 0; i < 28; i++)
        {
            for (var j = 0; j < 28; j++)
            {
                colors[0] = 255;
                colors[1] = Convert.ToByte(array[(i * 28) + j]);
                colors[2] = Convert.ToByte(array[(i * 28) + j]);
                colors[3] = Convert.ToByte(array[(i * 28) + j]);
                var argbColor = BitConverter.ToInt32(colors, 0);
                bitmap.SetPixel(j, i, Color.FromArgb(argbColor));
            }
        }

        this.pictureBox2.Image = bitmap;
        this.ImagePatternRecognition(this.currentPattern);
        this.label10.Text = this.currentPattern.ToString();
    }

    /// <summary>
    /// Handles the image pattern recognition.
    /// </summary>
    /// <param name="index">The index.</param>
    private void ImagePatternRecognition(int index)
    {
        var mutexes = new List<Mutex>(2);

        for (var i = 0; i < 2; i++)
        {
            var mutex = new Mutex();
            mutexes.Add(mutex);
        }

        var testPatterns = new NeuronalNetworkTestPatterns(
            this.neuronalNetwork,
            this.database,
            this.preferences,
            this.databaseReady,
            null,
            null,
            this,
            mutexes);
        var thread = new Thread(() => testPatterns.PatternRecognizingThread(index));
        thread.Start();
    }

    /// <summary>
    /// Handles the previous step click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void PreviousClick(object sender, EventArgs e)
    {
        if (!this.databaseReady)
        {
            return;
        }

        if (this.database is null)
        {
            return;
        }

        if (this.currentPattern <= 1)
        {
            return;
        }

        this.currentPattern -= 1;
        var bitmap = new Bitmap(SystemGlobals.ImageSize, SystemGlobals.ImageSize, PixelFormat.Format32bppArgb);
        var array = this.database.ImagePatterns[this.currentPattern].Pattern;
        uint label = this.database.ImagePatterns[this.currentPattern].Label;
        this.label6.Text = label.ToString();
        var colors = new byte[4];

        for (var i = 0; i < 28; i++)
        {
            for (var j = 0; j < 28; j++)
            {
                colors[0] = 255;
                colors[1] = Convert.ToByte(array[(i * 28) + j]);
                colors[2] = Convert.ToByte(array[(i * 28) + j]);
                colors[3] = Convert.ToByte(array[(i * 28) + j]);
                var argbColor = BitConverter.ToInt32(colors, 0);
                bitmap.SetPixel(j, i, Color.FromArgb(argbColor));
            }
        }

        this.pictureBox2.Image = bitmap;
        this.ImagePatternRecognition(this.currentPattern);
        this.label10.Text = this.currentPattern.ToString();
    }

    /// <summary>
    /// Handles the start back propagation click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void StartBackPropagationButtonClick(object sender, EventArgs e)
    {
        if (this.trainingDataReady)
        {
            this.OnStartBackPropagation();
        }
    }

    /// <summary>
    /// Starts the back propagation.
    /// </summary>
    private void OnStartBackPropagation()
    {
        if (!this.trainingDataReady || this.trainingThreadRunning || this.testingThreadRunning)
        {
            return;
        }

        using var dialog = new BackPropagationParametersForm();

        var parameters = new BackPropagationParameters
        {
            NumberOfThreads = (uint)this.preferences.NumberOfBackPropagationThreads,
            InitialEta = this.preferences.InitialEtaLearningRate,
            MinimumEta = this.preferences.MinimumEtaLearningRate,
            EtaDecay = this.preferences.LearningRateDecay,
            AfterEvery = this.preferences.AfterEveryNBackPropagationItems,
            StartingPattern = 0,
            EstimatedCurrentMse = 0.10,
            DistortPatterns = true
        };

        var eta = parameters.InitialEta;
        parameters.InitialEtaMessage = $"Initial Learning Rate eta (currently, eta = {eta})";
        const int LocalPattern = 0;
        parameters.StartingPatternNumber = $"Starting Pattern Number (currently at {LocalPattern})";
        dialog.SetBackPropagationParameters(parameters);
        var dialogResult = dialog.ShowDialog();

        if (dialogResult != DialogResult.OK)
        {
            return;
        }

        parameters = dialog.GetBackProParameters();
        var propagationStarted = this.StartBackPropagation(
            parameters.NumberOfThreads,
            parameters.InitialEta,
            parameters.MinimumEta,
            parameters.EtaDecay,
            parameters.AfterEvery,
            parameters.DistortPatterns,
            parameters.EstimatedCurrentMse);

        if (propagationStarted)
        {
            this.trainingThreadRunning = true;
        }
    }

    /// <summary>
    /// Starts the back propagation.
    /// </summary>
    /// <param name="numberOfThreads">The number of threads.</param>
    /// <param name="initialEta">The initial ETA.</param>
    /// <param name="minimumEta">The minimum ETA.</param>
    /// <param name="etaDecay">The ETA decay.</param>
    /// <param name="afterEveryNValues">Do a propagation after every n values.</param>
    /// <param name="distortPatterns">A value indicating whether distort patterns should be used or not.</param>
    /// <param name="estimatedCurrentMse">The estimated current MSE.</param>
    /// <returns>A value indicating whether the propagation has started or not.</returns>
    private bool StartBackPropagation(uint numberOfThreads, double initialEta, double minimumEta, double etaDecay, uint afterEveryNValues, bool distortPatterns, double estimatedCurrentMse)
    {
        if (numberOfThreads < 1)
        {
            numberOfThreads = 1;
        }

        // 10 is arbitrary upper limit
        if (numberOfThreads > 10)
        {
            numberOfThreads = 10;
        }

        // Initialize the back propagation before process
        this.neuronalNetwork.EtaLearningRate = initialEta;
        this.neuronalNetwork.PreviousEtaLearningRate = initialEta;

        // Run the thread here
        this.trainingThreadStop.Reset();
        this.trainingThreadStopped.Reset();
        this.trainingThreads = new List<Thread>(2);
        this.trainingDatabase.RandomizePatternSequence();

        // Clear the mutexes before run threads.
        var mutexes = new List<Mutex>(2);

        for (var i = 0; i < 4; i++)
        {
            var mutex = new Mutex();
            mutexes.Add(mutex);
        }

        // Create the neuronal network
        try
        {
            CreateNeuronalNetwork(this.trainingNetwork);

            // Initialize the weight parameters to the network
            if (this.weightsFile != string.Empty)
            {
                this.mainMutex.WaitOne();
                var fileStreamInput = new FileStream(this.weightsFile, FileMode.Open);
                var archiveInput = new Archive(fileStreamInput, ArchiveOperation.Load);
                this.trainingNetwork.Serialize(archiveInput);
                fileStreamInput.Close();
                this.mainMutex.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            return false;
        }

        var trainingPatterns = new NeuronalNetworkTrainPatterns(this.trainingNetwork, this.trainingDatabase, this.preferences, this.trainingDataReady, this.trainingThreadStop, this.trainingThreadStopped, this, mutexes)
        {
            MinimumEta = minimumEta,
            EtaDecay = etaDecay,
            AfterEveryNBackProperties = afterEveryNValues,
            DistortTrainingPatterns = distortPatterns,
            EstimatedCurrentMse = estimatedCurrentMse
        };

        for (var i = 0; i < numberOfThreads; i++)
        {
            var trainerThread = new Thread(trainingPatterns.BackPropagationThread) { Name = $"Thread{i + 1}" };
            this.trainingThreads.Add(trainerThread);
            trainerThread.Start();
        }

        return true;
    }

    /// <summary>
    /// Handles the stop back propagation click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void StopBackPropagationButtonClick(object sender, EventArgs e)
    {
        if (!this.trainingThreadRunning)
        {
            return;
        }

        if (StopThreads(this.trainingThreads, this.trainingThreadStop, this.trainingThreadStopped))
        {
            this.BackPropagationThreadsFinished();
        }
    }

    /// <summary>
    /// Finish the back propagation threads.
    /// </summary>
    private void BackPropagationThreadsFinished()
    {
        if (!this.trainingThreadRunning)
        {
            return;
        }

        var result = MessageBox.Show(@"Do you want to save the neuronal network data?", @"Save neuronal network data", MessageBoxButtons.OKCancel);
        if (result == DialogResult.OK)
        {
            using var saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"Neuronal network file (*.nnt)|*.nnt",
                Title = @"Save neuronal network file"
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var fileStreamInput = saveFileDialog1.OpenFile();
                var archiveInput = new Archive(fileStreamInput, ArchiveOperation.Store);
                this.trainingNetwork.Serialize(archiveInput);
                fileStreamInput.Close();
            }
        }

        this.trainingThreadRunning = false;
    }

    /// <summary>
    /// Handles the network parameter menu item click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NetworkParametersToolStripMenuItemClick(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = @"Neuronal network file (*.nnt)|*.nnt",
            Title = @"Open neuronal network File"
        };
        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        this.mainMutex.WaitOne();
        this.weightsFile = openFileDialog.FileName;
        var fileStreamInput = openFileDialog.OpenFile();
        var archiveInput = new Archive(fileStreamInput, ArchiveOperation.Load);
        this.neuronalNetwork.Serialize(archiveInput);
        fileStreamInput.Close();
        this.mainMutex.ReleaseMutex();
    }

    /// <summary>
    /// Handles the database menu item click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void DatabaseToolStripMenuItemClick(object sender, EventArgs e)
    {
        this.trainingDataReady = this.trainingDatabase.LoadDatabaseFiles();
        if (this.trainingDataReady)
        {
            // Update preferences parameters
            if (this.trainingDatabase.ImagePatterns.Count != this.preferences.NumberOfItemsTrainingImages)
            {
                this.preferences.NumberOfItemsTrainingImages = (uint)this.trainingDatabase.ImagePatterns.Count;
                this.preferences.NumberOfItemsTrainingLabels = (uint)this.trainingDatabase.ImagePatterns.Count;
            }

            this.radioButtonMnistTrainDatabase.Enabled = true;
            this.radioButtonTrainingdatabase.Enabled = true;
            this.buttonMnistNext.Enabled = true;
            this.buttonMnistPrevious.Enabled = true;
            this.databaseReady = this.trainingDataReady;
            this.database = this.trainingDatabase;
        }
        else
        {
            this.radioButtonMnistTrainDatabase.Enabled = false;
            return;
        }

        this.testingDataReady = this.testingDatabase.LoadDatabaseFiles();

        if (this.testingDataReady)
        {
            // Update preferences parameters
            if (this.testingDatabase.ImagePatterns.Count != this.preferences.NumberOfItemsTestingImages)
            {
                this.preferences.NumberOfItemsTestingImages = (uint)this.testingDatabase.ImagePatterns.Count;
                this.preferences.NumberOfItemsTestingLabels = (uint)this.testingDatabase.ImagePatterns.Count;
            }

            this.radioButtonMnistTestDatabase.Enabled = true;
            this.radioButtonMnistTestDatabase.Checked = true;
            this.radioButtonTestingdatabase.Enabled = true;
            this.radioButtonTestingdatabase.Checked = true;
            this.buttonMnistNext.Enabled = true;
            this.buttonMnistPrevious.Enabled = true;
            this.databaseReady = this.testingDataReady;
            this.database = this.testingDatabase;
        }
        else
        {
            this.radioButtonMnistTestDatabase.Enabled = false;
        }
    }

    /// <summary>
    /// Handles the test button click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void TestClick(object sender, EventArgs e)
    {
        if (this.testingThreadRunning || this.trainingThreadRunning)
        {
            return;
        }

        var mutexes = new List<Mutex>(2);
        var numberOfThreads = (int)this.numericUpDownThreads.Value;
        NeuronalNetworkTestPatterns testingPatterns;
        var neuronalNetworkLocal = new NeuronalNetwork();
        bool databaseForTest;

        // Create the neuronal network
        try
        {
            CreateNeuronalNetwork(neuronalNetworkLocal);

            // Initialize weight parameters to the network
            if (this.weightsFile != string.Empty)
            {
                this.mainMutex.WaitOne();
                var fileStreamInput = new FileStream(this.weightsFile, FileMode.Open);
                var archiveInput = new Archive(fileStreamInput, ArchiveOperation.Load);
                neuronalNetworkLocal.Serialize(archiveInput);
                fileStreamInput.Close();
                this.mainMutex.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            return;
        }

        if (this.radioButtonTestingdatabase.Checked)
        {
            if (this.testingDataReady)
            {
                testingPatterns = new NeuronalNetworkTestPatterns(
                    neuronalNetworkLocal,
                    this.testingDatabase,
                    this.preferences,
                    this.testingDataReady,
                    this.testingThreadStop,
                    this.testingThreadStopped,
                    this,
                    mutexes);
                databaseForTest = this.testingDataReady;
            }
            else
            {
                return;
            }
        }
        else
        {
            if (this.trainingDataReady)
            {
                testingPatterns = new NeuronalNetworkTestPatterns(
                    neuronalNetworkLocal,
                    this.trainingDatabase,
                    this.preferences,
                    this.trainingDataReady,
                    this.testingThreadStop,
                    this.testingThreadStopped,
                    this,
                    mutexes);
                databaseForTest = this.trainingDataReady;
            }
            else
            {
                return;
            }
        }

        if (!databaseForTest)
        {
            return;
        }

        this.listBox2.Items.Clear();
        for (var i = 0; i < 2; i++)
        {
            var mutex = new Mutex();
            mutexes.Add(mutex);
        }

        this.testingThreadStop.Reset();
        this.testingThreadStopped.Reset();
        this.testingThreads = new List<Thread>(2);

        try
        {
            for (var i = 0; i < numberOfThreads; i++)
            {
                var thread = new Thread(delegate ()
                {
                    testingPatterns.PatternsTestingThread((int)this.numericUpDownNumberofTestPattern.Value);
                });

                this.testingThreads.Add(thread);
                thread.Start();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            return;
        }

        this.testingThreadRunning = true;
        this.radioButtonTestingdatabase.Enabled = false;
        this.radioButtonTrainingdatabase.Enabled = false;
        this.buttonTest.Enabled = false;
    }

    /// <summary>
    /// Handles the stop test click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void StopTestClick(object sender, EventArgs e)
    {
        if (!this.testingThreadRunning)
        {
            return;
        }

        if (!StopThreads(this.testingThreads, this.testingThreadStop, this.testingThreadStopped))
        {
            return;
        }

        this.testingThreadRunning = false;
        this.radioButtonTestingdatabase.Enabled = true;
        this.radioButtonTrainingdatabase.Enabled = true;
        this.buttonTest.Enabled = true;
    }

    /// <summary>
    /// Handles the testing database checked changed event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void TestingDatabaseCheckedChanged(object sender, EventArgs e)
    {
        this.numericUpDownNumberofTestPattern.Maximum = this.radioButtonTestingdatabase.Checked ? 9999 : 59999;
    }

    /// <summary>
    /// Handles the database checked changed event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void DatabaseCheckedChanged(object sender, EventArgs e)
    {
        if (this.radioButtonMnistTestDatabase.Checked)
        {
            this.database = this.testingDatabase;
            this.databaseReady = this.testingDataReady;
            this.currentPattern = 0;
        }
        else
        {
            this.database = this.testingDatabase;
            this.databaseReady = this.trainingDataReady;
            this.currentPattern = 0;
        }
    }

    /// <summary>
    /// Handles the main form closing event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void MainFormFormClosing(object sender, FormClosingEventArgs e)
    {
        if (!this.testingThreadRunning && !this.trainingThreadRunning)
        {
            return;
        }

        MessageBox.Show(@"Sorry, some threads are running. Please stop them before  you can close the program", string.Empty, MessageBoxButtons.OK);
        e.Cancel = true;
    }

    /// <summary>
    /// Handles the help menu item click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void ViewHelpToolStripMenuItemClick(object sender, EventArgs e)
    {
        MessageBox.Show(
            @"Handwritten character recognition program Version",
            @"About Handwritten character recognition program",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
