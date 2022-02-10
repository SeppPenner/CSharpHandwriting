// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BackPropagationParametersForm.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The back propagation form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandwrittenRecognition;

/// <summary>
/// The back propagation form.
/// </summary>
public partial class BackPropagationParametersForm : Form
{
    /// <summary>
    /// The back propagation parameters.
    /// </summary>
    private BackPropagationParameters backPropagationParameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackPropagationParametersForm"/> class.
    /// </summary>
    public BackPropagationParametersForm()
    {
        this.InitializeComponent();
        this.backPropagationParameters.AfterEvery = 0;
        this.backPropagationParameters.DistortPatterns = true;
        this.backPropagationParameters.NumberOfThreads = 0;
        this.backPropagationParameters.EstimatedCurrentMse = 0;
        this.backPropagationParameters.EtaDecay = 0;
        this.backPropagationParameters.InitialEta = 0;
        this.backPropagationParameters.MinimumEta = 0;
        this.backPropagationParameters.StartingPattern = 0;
        this.backPropagationParameters.InitialEtaMessage = string.Empty;
        this.backPropagationParameters.StartingPatternNumber = string.Empty;
    }

    /// <summary>
    /// Sets back the <see cref="BackPropagationParameters"/>.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public void SetBackPropagationParameters(BackPropagationParameters parameters)
    {
        this.backPropagationParameters = parameters;
        this.textBoxAfterEveryNBackPropagations.Text = this.backPropagationParameters.AfterEvery.ToString();
        this.textBoxBackThreads.Text = this.backPropagationParameters.NumberOfThreads.ToString();
        this.textBoxEstimateofCurrentMSE.Text = this.backPropagationParameters.EstimatedCurrentMse.ToString(CultureInfo.InvariantCulture);
        this.textBoxILearningRateEta.Text = this.backPropagationParameters.InitialEta.ToString(CultureInfo.InvariantCulture);
        this.textBoxLearningRateDecayRate.Text = this.backPropagationParameters.EtaDecay.ToString(CultureInfo.InvariantCulture);
        this.textBoxMinimumLearningRate.Text = this.backPropagationParameters.MinimumEta.ToString(CultureInfo.InvariantCulture);
        this.textBoxStartingPatternNumber.Text = this.backPropagationParameters.StartingPattern.ToString();
        this.checkBoxDistortPatterns.Checked = this.backPropagationParameters.DistortPatterns;
    }

    /// <summary>
    /// Gets the <see cref="BackPropagationParameters"/>.
    /// </summary>
    /// <returns>The found <see cref="BackPropagationParameters"/>.</returns>
    public BackPropagationParameters GetBackProParameters()
    {
        return this.backPropagationParameters;
    }

    /// <summary>
    /// Starts the back propagation process.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void Start(object sender, EventArgs e)
    {
        this.backPropagationParameters.AfterEvery = Convert.ToUInt32(this.textBoxAfterEveryNBackPropagations.Text);
        this.backPropagationParameters.NumberOfThreads = Convert.ToUInt32(this.textBoxBackThreads.Text);
        this.backPropagationParameters.EstimatedCurrentMse = Convert.ToDouble(this.textBoxEstimateofCurrentMSE.Text);
        this.backPropagationParameters.InitialEta = Convert.ToDouble(this.textBoxILearningRateEta.Text);
        this.backPropagationParameters.EtaDecay = Convert.ToDouble(this.textBoxLearningRateDecayRate.Text);
        this.backPropagationParameters.MinimumEta = Convert.ToDouble(this.textBoxMinimumLearningRate.Text);
        this.backPropagationParameters.StartingPattern = Convert.ToUInt32(this.textBoxStartingPatternNumber.Text);
        this.backPropagationParameters.DistortPatterns = this.checkBoxDistortPatterns.Checked;
    }
}
