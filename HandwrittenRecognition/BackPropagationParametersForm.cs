using System;
using System.Globalization;
using System.Windows.Forms;

namespace HandwrittenRecogniration
{
    public partial class BackPropagationParametersForm : Form
    {
        private BackPropagationParameters _mParameters;

        public BackPropagationParametersForm()
        {
            InitializeComponent();
            _mParameters.MAfterEvery = 0;
            _mParameters.MbDistortPatterns = true;
            _mParameters.McNumThreads = 0;
            _mParameters.MEstimatedCurrentMse = 0;
            _mParameters.MEtaDecay = 0;
            _mParameters.MInitialEta = 0;
            _mParameters.MMinimumEta = 0;
            _mParameters.MStartingPattern = 0;
            _mParameters.MStrInitialEtaMessage = "";
            _mParameters.MStrStartingPatternNum = "";
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        public void SetBackProParameters(BackPropagationParameters value)
        {
            _mParameters = value;
            textBoxAfterEveryNBackPropagations.Text = _mParameters.MAfterEvery.ToString();
            textBoxBackThreads.Text = _mParameters.McNumThreads.ToString();
            textBoxEstimateofCurrentMSE.Text = _mParameters.MEstimatedCurrentMse.ToString(CultureInfo.InvariantCulture);
            textBoxILearningRateEta.Text = _mParameters.MInitialEta.ToString(CultureInfo.InvariantCulture);
            textBoxLearningRateDecayRate.Text = _mParameters.MEtaDecay.ToString(CultureInfo.InvariantCulture);
            textBoxMinimumLearningRate.Text = _mParameters.MMinimumEta.ToString(CultureInfo.InvariantCulture);
            textBoxStartingPatternNumber.Text = _mParameters.MStartingPattern.ToString();
            checkBoxDistortPatterns.Checked = _mParameters.MbDistortPatterns;
        }

        public BackPropagationParameters GetBackProParameters()
        {
            return _mParameters;
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            _mParameters.MAfterEvery = Convert.ToUInt32(textBoxAfterEveryNBackPropagations.Text);
            _mParameters.McNumThreads = Convert.ToUInt32(textBoxBackThreads.Text);
            _mParameters.MEstimatedCurrentMse = Convert.ToDouble(textBoxEstimateofCurrentMSE.Text);
            _mParameters.MInitialEta = Convert.ToDouble(textBoxILearningRateEta.Text);
            _mParameters.MEtaDecay = Convert.ToDouble(textBoxLearningRateDecayRate.Text);
            _mParameters.MMinimumEta = Convert.ToDouble(textBoxMinimumLearningRate.Text);
            _mParameters.MStartingPattern = Convert.ToUInt32(textBoxStartingPatternNumber.Text);
            _mParameters.MbDistortPatterns = checkBoxDistortPatterns.Checked;
        }
    }
}