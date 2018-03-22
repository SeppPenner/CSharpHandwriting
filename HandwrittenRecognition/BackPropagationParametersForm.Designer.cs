namespace HandwrittenRecogniration
{
    partial class BackPropagationParametersForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxBackThreads = new System.Windows.Forms.TextBox();
            this.textBoxILearningRateEta = new System.Windows.Forms.TextBox();
            this.textBoxMinimumLearningRate = new System.Windows.Forms.TextBox();
            this.textBoxLearningRateDecayRate = new System.Windows.Forms.TextBox();
            this.textBoxAfterEveryNBackPropagations = new System.Windows.Forms.TextBox();
            this.textBoxStartingPatternNumber = new System.Windows.Forms.TextBox();
            this.textBoxEstimateofCurrentMSE = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxDistortPatterns = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBoxBackThreads
            // 
            this.textBoxBackThreads.Location = new System.Drawing.Point(292, 17);
            this.textBoxBackThreads.Name = "textBoxBackThreads";
            this.textBoxBackThreads.Size = new System.Drawing.Size(100, 20);
            this.textBoxBackThreads.TabIndex = 0;
            // 
            // textBoxILearningRateEta
            // 
            this.textBoxILearningRateEta.Location = new System.Drawing.Point(292, 53);
            this.textBoxILearningRateEta.Name = "textBoxILearningRateEta";
            this.textBoxILearningRateEta.Size = new System.Drawing.Size(100, 20);
            this.textBoxILearningRateEta.TabIndex = 0;
            // 
            // textBoxMinimumLearningRate
            // 
            this.textBoxMinimumLearningRate.Location = new System.Drawing.Point(292, 91);
            this.textBoxMinimumLearningRate.Name = "textBoxMinimumLearningRate";
            this.textBoxMinimumLearningRate.Size = new System.Drawing.Size(100, 20);
            this.textBoxMinimumLearningRate.TabIndex = 0;
            // 
            // textBoxLearningRateDecayRate
            // 
            this.textBoxLearningRateDecayRate.Location = new System.Drawing.Point(292, 131);
            this.textBoxLearningRateDecayRate.Name = "textBoxLearningRateDecayRate";
            this.textBoxLearningRateDecayRate.Size = new System.Drawing.Size(100, 20);
            this.textBoxLearningRateDecayRate.TabIndex = 0;
            // 
            // textBoxAfterEveryNBackPropagations
            // 
            this.textBoxAfterEveryNBackPropagations.Location = new System.Drawing.Point(292, 169);
            this.textBoxAfterEveryNBackPropagations.Name = "textBoxAfterEveryNBackPropagations";
            this.textBoxAfterEveryNBackPropagations.Size = new System.Drawing.Size(100, 20);
            this.textBoxAfterEveryNBackPropagations.TabIndex = 0;
            // 
            // textBoxStartingPatternNumber
            // 
            this.textBoxStartingPatternNumber.Location = new System.Drawing.Point(292, 206);
            this.textBoxStartingPatternNumber.Name = "textBoxStartingPatternNumber";
            this.textBoxStartingPatternNumber.Size = new System.Drawing.Size(100, 20);
            this.textBoxStartingPatternNumber.TabIndex = 0;
            // 
            // textBoxEstimateofCurrentMSE
            // 
            this.textBoxEstimateofCurrentMSE.Location = new System.Drawing.Point(292, 243);
            this.textBoxEstimateofCurrentMSE.Name = "textBoxEstimateofCurrentMSE";
            this.textBoxEstimateofCurrentMSE.Size = new System.Drawing.Size(100, 20);
            this.textBoxEstimateofCurrentMSE.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(246, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Number of Backprop threads (one per CPU is best)";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(52, 328);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(131, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Start BackPropagation";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(221, 328);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(140, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel BackPropagation";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(258, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Initial Learning Rate eta (currently, eta = 0.00000001)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Minimum Learning Rate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(209, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Learning Rate Decay Rate (multiply eta by)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(194, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "After Every N Backpropagations: N =    ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 213);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(220, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Starting Pattern Number (currently at 100000)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 250);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(233, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Estimate of current MSE (enter 0.10 if uncertain)";
            // 
            // checkBoxDistortPatterns
            // 
            this.checkBoxDistortPatterns.AutoSize = true;
            this.checkBoxDistortPatterns.Location = new System.Drawing.Point(27, 287);
            this.checkBoxDistortPatterns.Name = "checkBoxDistortPatterns";
            this.checkBoxDistortPatterns.Size = new System.Drawing.Size(303, 17);
            this.checkBoxDistortPatterns.TabIndex = 3;
            this.checkBoxDistortPatterns.Text = "Distort Patterns (recommended for improved generalization)";
            this.checkBoxDistortPatterns.UseVisualStyleBackColor = true;
            // 
            // BackPropagationParametersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 357);
            this.Controls.Add(this.checkBoxDistortPatterns);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxEstimateofCurrentMSE);
            this.Controls.Add(this.textBoxStartingPatternNumber);
            this.Controls.Add(this.textBoxAfterEveryNBackPropagations);
            this.Controls.Add(this.textBoxLearningRateDecayRate);
            this.Controls.Add(this.textBoxMinimumLearningRate);
            this.Controls.Add(this.textBoxILearningRateEta);
            this.Controls.Add(this.textBoxBackThreads);
            this.Name = "BackPropagationParametersForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BackPropagationParametersForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBackThreads;
        private System.Windows.Forms.TextBox textBoxILearningRateEta;
        private System.Windows.Forms.TextBox textBoxMinimumLearningRate;
        private System.Windows.Forms.TextBox textBoxLearningRateDecayRate;
        private System.Windows.Forms.TextBox textBoxAfterEveryNBackPropagations;
        private System.Windows.Forms.TextBox textBoxStartingPatternNumber;
        private System.Windows.Forms.TextBox textBoxEstimateofCurrentMSE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxDistortPatterns;
    }
}