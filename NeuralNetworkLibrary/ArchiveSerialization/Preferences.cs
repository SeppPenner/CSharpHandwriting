using System;
using System.IO;
using NeuralNetworkLibrary.DataFiles;

namespace NeuralNetworkLibrary.ArchiveSerialization
{
    public class Preferences
    {
        private const int GcImageSize = 28;

        // ReSharper disable once UnusedMember.Global
        public const int GcVectorSize = 29;

        private readonly IniFile _mInifile;

        // ReSharper disable once NotAccessedField.Local
        private int _mcNumTestingThreads;

        // for limiting the step size in backpropagation, since we are using second order
        // "Stochastic Diagonal Levenberg-Marquardt" update algorithm.  See Yann LeCun 1998
        // "Gradianet-Based Learning Applied to Document Recognition" at page 41

        // ReSharper disable once NotAccessedField.Local
        private double _mdMicronLimitParameter;

        // ReSharper disable once NotAccessedField.Local
        private int _mnMagicTestingImages;

        // ReSharper disable once NotAccessedField.Local
        private int _mnMagicTestingLabels;

        // ReSharper disable once NotAccessedField.Local
        private uint _mnMagicTrainingImages;

        // ReSharper disable once NotAccessedField.Local
        private uint _mnMagicTrainingLabels;

        // ReSharper disable once NotAccessedField.Local
        private int _mnMagWindowMagnification;

        // ReSharper disable once NotAccessedField.Local
        private int _mnMagWindowSize;

        public int McNumBackpropThreads;
        public double MdElasticScaling; // after-smoohting scale factor for Simard's elastic distortions
        public double MdElasticSigma; // one sigma value for randomness in Simard's elastic distortions

        public double MdInitialEtaLearningRate;
        public double MdLearningRateDecay;
        public double MdMaxRotation; // in degrees, such as 20.0 for plus/minus rotations of 20 degrees

        // for distortions of the input image, in an attempt to improve generalization

        public double MdMaxScaling; // as a percentage, such as 20.0 for plus/minus 20%
        public double MdMinimumEtaLearningRate;
        public uint MnAfterEveryNBackprops;
        public uint MnColsImages;
        public uint MnItemsTestingImages;

        // ReSharper disable once NotAccessedField.Global
        public uint MnItemsTestingLabels;

        public uint MnItemsTrainingImages;

        // ReSharper disable once NotAccessedField.Global
        public uint MnItemsTrainingLabels;

        public uint MnNumHessianPatterns;

        public uint MnRowsImages;

        ////////////
        public Preferences()
        {
            // set default values

            _mnMagicTrainingLabels = 0x00000801;
            _mnMagicTrainingImages = 0x00000803;

            MnItemsTrainingLabels = 60000;
            MnItemsTrainingImages = 60000;

            _mnMagicTestingLabels = 0x00000801;
            _mnMagicTestingImages = 0x00000803;

            MnItemsTestingLabels = 10000;
            MnItemsTestingImages = 10000;

            MnRowsImages = GcImageSize;
            MnColsImages = GcImageSize;

            _mnMagWindowSize = 5;
            _mnMagWindowMagnification = 8;

            MdInitialEtaLearningRate = 0.001;
            MdLearningRateDecay = 0.794328235; // 0.794328235 = 0.001 down to 0.00001 in 20 epochs 
            MdMinimumEtaLearningRate = 0.00001;
            MnAfterEveryNBackprops = 60000;
            McNumBackpropThreads = 2;

            _mcNumTestingThreads = 1;

            // parameters for controlling distortions of input image

            MdMaxScaling = 15.0; // like 20.0 for 20%
            MdMaxRotation = 15.0; // like 20.0 for 20 degrees
            MdElasticSigma = 8.0; // higher numbers are more smooth and less distorted; Simard uses 4.0
            MdElasticScaling = 0.5; // higher numbers amplify the distortions; Simard uses 34 (sic, maybe 0.34 ??)

            // for limiting the step size in backpropagation, since we are using second order
            // "Stochastic Diagonal Levenberg-Marquardt" update algorithm.  See Yann LeCun 1998
            // "Gradient-Based Learning Applied to Document Recognition" at page 41

            _mdMicronLimitParameter = 0.10; // since we divide by this, update can never be more than 10x current eta
            MnNumHessianPatterns = 500; // number of patterns used to calculate the diagonal Hessian
            var path = Directory.GetCurrentDirectory() + "\\Data\\Default-ini.ini";
            _mInifile = new IniFile(path);
            ReadIniFile();
        }

        private void ReadIniFile()
        {
            // now read values from the ini file

            // Neural Network parameters

            var tSection = "Neural Network Parameters";

            Get(tSection, "Initial learning rate (eta)", out MdInitialEtaLearningRate);
            Get(tSection, "Minimum learning rate (eta)", out MdMinimumEtaLearningRate);
            Get(tSection, "Rate of decay for learning rate (eta)", out MdLearningRateDecay);
            Get(tSection, "Decay rate is applied after this number of backprops", out MnAfterEveryNBackprops);
            Get(tSection, "Number of backprop threads", out McNumBackpropThreads);
            Get(tSection, "Number of testing threads", out _mcNumTestingThreads);
            Get(tSection, "Number of patterns used to calculate Hessian", out MnNumHessianPatterns);
            Get(tSection, "Limiting divisor (micron) for learning rate amplification (like 0.10 for 10x limit)",
                out _mdMicronLimitParameter);


            // Neural Network Viewer parameters

            tSection = "Neural Net Viewer Parameters";

            Get(tSection, "Size of magnification window", out _mnMagWindowSize);
            Get(tSection, "Magnification factor for magnification window", out _mnMagWindowMagnification);


            // MNIST data collection parameters

            tSection = "MNIST Database Parameters";

            Get(tSection, "Training images magic number", out _mnMagicTrainingImages);
            Get(tSection, "Training images item count", out MnItemsTrainingImages);
            Get(tSection, "Training labels magic number", out _mnMagicTrainingLabels);
            Get(tSection, "Training labels item count", out MnItemsTrainingLabels);

            Get(tSection, "Testing images magic number", out _mnMagicTestingImages);
            Get(tSection, "Testing images item count", out MnItemsTestingImages);
            Get(tSection, "Testing labels magic number", out _mnMagicTestingLabels);
            Get(tSection, "Testing labels item count", out MnItemsTestingLabels);

            // these two are basically ignored

            // ReSharper disable once InlineOutVariableDeclaration
            uint uiCount;
            Get(tSection, "Rows per image", out uiCount);
            MnRowsImages = uiCount;

            Get(tSection, "Columns per image", out uiCount);
            MnColsImages = uiCount;


            // parameters for controlling pattern distortion during backpropagation

            tSection = "Parameters for Controlling Pattern Distortion During Backpropagation";

            Get(tSection, "Maximum scale factor change (percent, like 20.0 for 20%)", out MdMaxScaling);
            Get(tSection, "Maximum rotational change (degrees, like 20.0 for 20 degrees)", out MdMaxRotation);
            Get(tSection,
                "Sigma for elastic distortions (higher numbers are more smooth and less distorted; Simard uses 4.0)",
                out MdElasticSigma);
            Get(tSection, "Scaling for elastic distortions (higher numbers amplify distortions; Simard uses 0.34)",
                out MdElasticScaling);
        }

        private void Get(string lpAppName, string lpKeyName, out int nDefault)
        {
            nDefault = Convert.ToInt32(_mInifile.IniReadValue(lpAppName, lpKeyName));
        }

        private void Get(string lpAppName, string lpKeyName, out uint nDefault)
        {
            nDefault = Convert.ToUInt32(_mInifile.IniReadValue(lpAppName, lpKeyName));
        }

        private void Get(string lpAppName, string lpKeyName, out double nDefault)
        {
            nDefault = Convert.ToDouble(_mInifile.IniReadValue(lpAppName, lpKeyName));
        }

        // ReSharper disable once UnusedMember.Local
        private void Get(string lpAppName, string lpKeyName, out byte nDefault)
        {
            nDefault = Convert.ToByte(_mInifile.IniReadValue(lpAppName, lpKeyName));
        }

        // ReSharper disable once UnusedMember.Local
        private void Get(string lpAppName, string lpKeyName, out string nDefault)
        {
            nDefault = _mInifile.IniReadValue(lpAppName, lpKeyName);
        }

        // ReSharper disable once UnusedMember.Local
        private void Get(string lpAppName, string lpKeyName, out bool nDefault)
        {
            nDefault = Convert.ToBoolean(_mInifile.IniReadValue(lpAppName, lpKeyName));
        }
    }
}