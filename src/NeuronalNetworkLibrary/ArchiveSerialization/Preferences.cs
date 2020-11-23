// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Preferences.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The preferences class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using NeuronalNetworkLibrary.DataFiles;

    /// <summary>
    /// The preferences class.
    /// </summary>
    public class Preferences
    {
        /// <summary>
        /// The vector size.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const int VectorSize = 29;

        /// <summary>
        /// The image size.
        /// </summary>
        private const int ImageSize = 28;

        /// <summary>
        /// The ini file.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IniFile iniFile;

        /// <summary>
        /// The maximum number of testing threads.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int maximumNumberOfTestingThreads;

        /// <summary>
        /// The micron limit parameter.
        /// For limiting the step size in back propagation, since we are using second order
        /// "Stochastic Diagonal Levenberg-Marquardt" update algorithm. See Yann LeCun 1998
        /// "Gradient-Based Learning Applied to Document Recognition" at page 41
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private double micronLimitParameter;

        /// <summary>
        /// The magic testing images.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int magicTestingImages;

        /// <summary>
        /// The magic testing labels.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int magicTestingLabels;

        /// <summary>
        /// The magic training images.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private uint magicTrainingImages;

        /// <summary>
        /// The magic training labels.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private uint magicTrainingLabels;

        /// <summary>
        /// The magnification factor.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int magnificationFactor;

        /// <summary>
        /// The magic window size.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int magicWindowSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Preferences"/> class.
        /// </summary>
        public Preferences()
        {
            // Sets default values
            this.magicTrainingLabels = 0x00000801;
            this.magicTrainingImages = 0x00000803;

            this.NumberOfItemsTrainingLabels = 60000;
            this.NumberOfItemsTrainingImages = 60000;

            this.magicTestingLabels = 0x00000801;
            this.magicTestingImages = 0x00000803;

            this.NumberOfItemsTestingLabels = 10000;
            this.NumberOfItemsTestingImages = 10000;

            this.NumberOfRowImages = ImageSize;
            this.NumberOfColumnImages = ImageSize;

            this.magicWindowSize = 5;
            this.magnificationFactor = 8;

            this.InitialEtaLearningRate = 0.001;
            this.LearningRateDecay = 0.794328235; // 0.794328235 = 0.001 down to 0.00001 in 20 epochs 
            this.MinimumEtaLearningRate = 0.00001;
            this.AfterEveryNBackPropagationItems = 60000;
            this.NumberOfBackPropagationThreads = 2;

            this.maximumNumberOfTestingThreads = 1;

            // parameters for controlling distortions of input image
            this.MaximumScaling = 15.0; // like 20.0 for 20%
            this.MaximumRotation = 15.0; // like 20.0 for 20 degrees
            this.ElasticSigma = 8.0; // higher numbers are more smooth and less distorted; Simard uses 4.0
            this.ElasticScaling = 0.5; // higher numbers amplify the distortions; Simard uses 34 (sic, maybe 0.34 ??)

            // For limiting the step size in back propagation, since we are using second order
            // "Stochastic Diagonal Levenberg-Marquardt" update algorithm. See Yann LeCun 1998
            // "Gradient-Based Learning Applied to Document Recognition" at page 41
            this.micronLimitParameter = 0.10; // since we divide by this, update can never be more than 10x current eta
            this.NumberOfHessianPatterns = 500; // number of patterns used to calculate the diagonal Hessian
            var path = Directory.GetCurrentDirectory() + "\\Settings.ini";
            this.iniFile = new IniFile(path);
            this.ReadIniFile();
        }

        /// <summary>
        /// Gets or sets the after every n back propagation items.
        /// </summary>
        public uint AfterEveryNBackPropagationItems { get; set; }

        /// <summary>
        /// Gets or sets the minimum ETA learning rate.
        /// </summary>
        public double MinimumEtaLearningRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum scaling for distortions of the input image,
        /// in an attempt to improve generalization as a percentage, such as 20.0 for plus/minus 20%
        /// </summary>
        public double MaximumScaling { get; set; }

        /// <summary>
        /// Gets or sets the maximum rotation in degrees, such as 20.0 for plus/minus rotations of 20 degrees.
        /// </summary>
        public double MaximumRotation { get; set; }

        /// <summary>
        /// Gets or sets the initial ETA learning rate.
        /// </summary>
        public double InitialEtaLearningRate { get; set; }

        /// <summary>
        /// Gets or sets the learning rate decay.
        /// </summary>
        public double LearningRateDecay { get; set; }

        /// <summary>
        /// Gets or sets the elastic sigma for randomness in Simard's elastic distortions. 
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public double ElasticSigma { get; set; }

        /// <summary>
        /// Gets or sets the elastic scaling factor for after-smoothing scale factor for Simard's elastic distortions.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public double ElasticScaling { get; set; }

        /// <summary>
        /// Gets or sets the number of column images.
        /// </summary>
        public uint NumberOfColumnImages { get; set; }

        /// <summary>
        /// Gets or sets the number of back propagation threads.
        /// </summary>
        public int NumberOfBackPropagationThreads { get; set; }

        /// <summary>
        ///  Gets or sets the number of items testing images.
        /// </summary>
        public uint NumberOfItemsTestingImages { get; set; }

        /// <summary>
        /// Gets or sets the number of Hessian patterns.
        /// </summary>
        public uint NumberOfHessianPatterns { get; set; }

        /// <summary>
        /// Gets or sets the number of rows images.
        /// </summary>
        public uint NumberOfRowImages { get; set; }

        /// <summary>
        /// Gets or sets the number of items testing labels.
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        public uint NumberOfItemsTestingLabels { get; set; }

        /// <summary>
        /// Gets or sets the number of items training images.
        /// </summary>
        public uint NumberOfItemsTrainingImages { get; set; }

        /// <summary>
        /// Gets or sets the number of items training labels.
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        public uint NumberOfItemsTrainingLabels { get; set; }

        /// <summary>
        /// Reads the ini file.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void ReadIniFile()
        {
            // Now read values from the ini file

            // The neuronal network parameters
            var section = "Neuronal network parameters";

            this.InitialEtaLearningRate = this.Get(section, "Initial learning rate (ETA)", this.InitialEtaLearningRate);
            this.MinimumEtaLearningRate = this.Get(section, "Minimum learning rate (ETA)", this.MinimumEtaLearningRate);
            this.LearningRateDecay = this.Get(section, "Rate of decay for learning rate (ETA)", this.LearningRateDecay);
            this.AfterEveryNBackPropagationItems = this.Get(section, "Decay rate is applied after this number of propagation", this.AfterEveryNBackPropagationItems);
            this.NumberOfBackPropagationThreads = this.Get(section, "Number of back propagation threads", this.NumberOfBackPropagationThreads);
            this.maximumNumberOfTestingThreads = this.Get(section, "Number of testing threads", this.maximumNumberOfTestingThreads);
            this.NumberOfHessianPatterns = this.Get(section, "Number of patterns used to calculate Hessian", this.NumberOfHessianPatterns);
            this.micronLimitParameter = this.Get(section, "Limiting divisor (micron) for learning rate amplification (like 0.10 for 10x limit)", this.micronLimitParameter);

            // The neuronal network viewer parameters
            section = "Neuronal network viewer parameters";

            this.magicWindowSize = this.Get(section, "Size of magnification window", this.magicWindowSize);
            this.magnificationFactor = this.Get(section, "Magnification factor for magnification window", this.magnificationFactor);

            // The data collection parameters
            section = "Database parameters";

            this.magicTrainingImages = this.Get(section, "Training images magic number", this.magicTrainingImages);
            this.NumberOfItemsTrainingImages = this.Get(section, "Training images item count", this.NumberOfItemsTrainingImages);
            this.magicTrainingLabels = this.Get(section, "Training labels magic number", this.magicTrainingLabels);
            this.NumberOfItemsTrainingLabels = this.Get(section, "Training labels item count", this.NumberOfItemsTrainingLabels);

            this.magicTestingImages = this.Get(section, "Testing images magic number", this.magicTestingImages);
            this.NumberOfItemsTestingImages = this.Get(section, "Testing images item count", this.NumberOfItemsTestingImages);
            this.magicTestingLabels = this.Get(section, "Testing labels magic number", this.magicTestingLabels);
            this.NumberOfItemsTestingLabels = this.Get(section, "Testing labels item count", this.NumberOfItemsTestingLabels);

            // These two are basically ignored
            uint imageCount = 0;
            imageCount = this.Get(section, "Rows per image", imageCount);
            this.NumberOfRowImages = imageCount;

            imageCount = this.Get(section, "Columns per image", imageCount);
            this.NumberOfColumnImages = imageCount;

            // Parameters for controlling pattern distortion during back propagation
            section = "Parameters for controlling pattern distortion during back propagation";

            this.MaximumScaling = this.Get(section, "Maximum scale factor change (percent, like 20.0 for 20%)", this.MaximumScaling);
            this.MaximumRotation = this.Get(section, "Maximum rotational change (degrees, like 20.0 for 20 degrees)", this.MaximumRotation);
            this.ElasticSigma = this.Get(section, "Sigma for elastic distortions (higher numbers are more smooth and less distorted, Simard uses 4.0)", this.ElasticSigma);
            this.ElasticScaling = this.Get(section, "Scaling for elastic distortions (higher numbers amplify distortions, Simard uses 0.34)", this.ElasticScaling);
        }

        /// <summary>
        /// Gets the value as <see cref="int"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedParameter.Local
        private int Get(string appName, string keyName, int defaultValue)
        {
            return Convert.ToInt32(this.iniFile.IniReadValue(appName, keyName));
        }

        /// <summary>
        /// Gets the value as <see cref="uint"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedParameter.Local
        private uint Get(string appName, string keyName, uint defaultValue)
        {
            return Convert.ToUInt32(this.iniFile.IniReadValue(appName, keyName));
        }

        /// <summary>
        /// Gets the value as <see cref="double"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedParameter.Local
        private double Get(string appName, string keyName, double defaultValue)
        {
            return Convert.ToDouble(this.iniFile.IniReadValue(appName, keyName));
        }

        /// <summary>
        /// Gets the value as <see cref="byte"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private byte Get(string appName, string keyName, byte defaultValue)
        {
            return Convert.ToByte(this.iniFile.IniReadValue(appName, keyName));
        }

        /// <summary>
        /// Gets the value as <see cref="string"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private string Get(string appName, string keyName, string defaultValue)
        {
            return this.iniFile.IniReadValue(appName, keyName);
        }

        /// <summary>
        /// Gets the value as <see cref="bool"/>.
        /// </summary>
        /// <param name="appName">The application name.</param>
        /// <param name="keyName">The key name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private bool Get(string appName, string keyName, bool defaultValue)
        {
            return Convert.ToBoolean(this.iniFile.IniReadValue(appName, keyName));
        }
    }
}