// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkForwardPropagation.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network forward propagation class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.NeuronalNetwork
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using NeuronalNetworkLibrary.ArchiveSerialization;
    using NeuronalNetworkLibrary.NeuronalNetworkNeurons;

    /// <summary>
    /// The neuronal network forward propagation class.
    /// </summary>
    public class NeuronalNetworkForwardPropagation
    {
        /// <summary>
        /// The number of columns.
        /// </summary>
        private readonly int numberOfColumns;

        /// <summary>
        /// The count.
        /// </summary>
        private readonly int count;

        /// <summary>
        /// The number of rows.
        /// </summary>
        private readonly int numberOfRows;

        /// <summary>
        /// The Gaussian kernel.
        /// </summary>
        private readonly double[,] gaussianKernel = new double[SystemGlobals.GaussianFieldSize, SystemGlobals.GaussianFieldSize];

        /// <summary>
        /// The horizontal distortion map array.
        /// </summary>
        private readonly double[] horizontalDistortion;

        /// <summary>
        /// The vertical distortion map array.
        /// </summary>
        private readonly double[] verticalDistortion;

        /// <summary>
        /// The mutexes.
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Local
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly List<Mutex> mutexes;

        /// <summary>
        /// The stop event event.
        /// </summary>
        private ManualResetEvent eventStop;

        /// <summary>
        /// The event stopped event.
        /// </summary>
        private ManualResetEvent eventStopped;

        /// <summary>
        /// The high performance timer.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private HighPerformanceTimer highPerformanceTimer;

        /// <summary>
        /// The number of images.
        /// </summary>
        private uint numberOfImages;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuronalNetworkForwardPropagation"/> class.
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public NeuronalNetworkForwardPropagation()
        {
            this.CurrentPatternIndex = 0;
            this.DataReady = false;
            this.NeuronalNetwork = null;
            this.eventStop = null;
            this.eventStopped = null;
            this.mutexes = new List<Mutex>(4);
            this.highPerformanceTimer = new HighPerformanceTimer();
            this.numberOfImages = 0;
            this.numberOfColumns = 29;
            this.numberOfRows = 29;
            this.count = this.numberOfColumns * this.numberOfRows;
            this.horizontalDistortion = new double[this.count];
            this.verticalDistortion = new double[this.count];
        }

        /// <summary>
        /// Gets or sets a value indicating whether distort training patterns are used or not.
        /// </summary>
        public bool DistortTrainingPatterns { get; set; }

        /// <summary>
        /// Gets or sets the current pattern index.
        /// </summary>
        public int CurrentPatternIndex { get; set; }

        /// <summary>
        /// Gets or sets the preferences.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Preferences Preferences { get; set; }

        /// <summary>
        /// Gets or sets the neuronal network.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public NeuronalNetwork NeuronalNetwork { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data is ready or not.
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        protected bool DataReady { get; set; }

        /// <summary>
        /// Gets the current ETA.
        /// </summary>
        /// <returns>The current ETA.</returns>
        // ReSharper disable once UnusedMember.Global
        public double GetCurrentEta()
        {
            return this.NeuronalNetwork?.EtaLearningRate ?? 0.0;
        }

        /// <summary>
        /// Gets the previous ETA.
        /// </summary>
        /// <returns>The previous ETA.</returns>
        // ReSharper disable once UnusedMember.Global
        public double GetPreviousEta()
        {
            // Provided because threads might change the current ETA before we are able to read it
            return this.NeuronalNetwork?.PreviousEtaLearningRate ?? 0.0;
        }

        /// <summary>
        /// Gets the Gaussian sigma.
        /// </summary>
        /// <param name="elasticSigma">The elastic sigma.</param>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:StatementMustNotUseUnnecessaryParenthesis", Justification = "Reviewed. Suppression is OK here.")]
        protected void GetGaussianKernel(double elasticSigma)
        {
            // Create a Gaussian kernel, which is constant, for use in generating elastic distortions
            const int FieldSize = 21 / 2;

            var twoSigmaSquared = 2.0 * elasticSigma * elasticSigma;
            twoSigmaSquared = 1.0 / twoSigmaSquared;
            var twoPiSigma = 1.0 / elasticSigma * Math.Sqrt(2.0 * 3.1415926535897932384626433832795);

            for (var col = 0; col < 21; ++col)
            {
                for (var row = 0; row < 21; ++row)
                {
                    this.gaussianKernel[row, col] = twoPiSigma * Math.Exp(-1 * ((((row - FieldSize) * (row - FieldSize)) + ((col - FieldSize) * ((col - FieldSize)) * twoSigmaSquared))));
                }
            }
        }

        /// <summary>
        /// Calculates the neuronal network.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="inputCount">The input count.</param>
        /// <param name="outputVector">The output vector.</param>
        /// <param name="outputCount">The output count.</param>
        /// <param name="outputList">The output list.</param>
        /// <param name="distort">A value indicating whether distort is used or not.</param>
        protected void CalculateNeuralNet(double[] inputVector, int inputCount, double[] outputVector, int outputCount, NeuronalNetworkNeuronOutputsList outputList, bool distort)
        {
            // Wrapper function for neuronal network's Calculate() function, needed because the neuronal network is a protected member
            // Waits on the neural net mutex (using the CAutoMutex object, which automatically releases the
            // mutex when it goes out of scope) so as to restrict access to one thread at a time
            this.mutexes[0].WaitOne();
            {
                if (distort)
                {
                    this.GenerateDistortionMaps(1.0);
                    this.ApplyDistortionMap(inputVector);
                }

                this.NeuronalNetwork.Calculate(inputVector, inputCount, outputVector, outputCount, outputList);
            }

            this.mutexes[0].ReleaseMutex();
        }

        /// <summary>
        /// Applies the distortion map.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        protected void ApplyDistortionMap(double[] inputVector)
        {
            // Applies the current distortion map to the input vector

            // For the mapped array, we assume that 0.0 == background, and 1.0 == full intensity information
            // This is different from the input vector, in which +1.0 == background (white), and 
            // -1.0 == information (black), so we must convert one to the other
            var mappedVector = new List<List<double>>(this.numberOfRows);

            for (var i = 0; i < this.numberOfRows; i++)
            {
                var vector = new List<double>(this.numberOfColumns);

                for (var j = 0; j < this.numberOfColumns; j++)
                {
                    vector.Add(0.0);
                }

                mappedVector.Add(vector);
            }

            int row, column;

            for (row = 0; row < this.numberOfRows; ++row)
            {
                for (column = 0; column < this.numberOfColumns; ++column)
                {
                    // The pixel at sourceRow, sourceCol is an "phantom" pixel that doesn't really exist, and
                    // whose value must be manufactured from surrounding real pixels (i.e., since 
                    // sourceRow and sourceCol are floating point, not integers, there's not a real pixel there)
                    // The idea is that if we can calculate the value of this phantom pixel, then its 
                    // displacement will exactly fit into the current pixel at row, col (which are both integers)
                    var sourceRow = row - this.verticalDistortion[(row * this.numberOfColumns) + column];
                    var sourceCol = column - this.horizontalDistortion[(row * this.numberOfColumns) + column];

                    // Weights for bi-linear interpolation
                    var fractionRow = sourceRow - (int)sourceRow;
                    var fractionColumn = sourceCol - (int)sourceCol;

                    var w1 = (1.0 - fractionRow) * (1.0 - fractionColumn);
                    var w2 = (1.0 - fractionRow) * fractionColumn;
                    var w3 = fractionRow * (1 - fractionColumn);
                    var w4 = fractionRow * fractionColumn;

                    var skipOutOfBounds = sourceRow + 1.0 >= this.numberOfRows || sourceRow < 0 || sourceCol + 1.0 >= this.numberOfColumns || sourceCol < 0;

                    double sourceValue;

                    if (skipOutOfBounds == false)
                    {
                        // The supporting pixels for the "phantom" source pixel are all within the 
                        // bounds of the character grid.
                        // Manufacture its value by bi-linear interpolation of surrounding pixels
                        var localRow = (int)sourceRow;
                        var localColumn = (int)sourceCol;

                        var localRow2 = localRow + 1;
                        var localColumn2 = localColumn + 1;

                        while (localRow2 >= this.numberOfRows)
                        {
                            localRow2 -= this.numberOfRows;
                        }

                        while (localRow2 < 0)
                        {
                            localRow2 += this.numberOfRows;
                        }

                        while (localColumn2 >= this.numberOfColumns)
                        {
                            localColumn2 -= this.numberOfColumns;
                        }

                        while (localColumn2 < 0)
                        {
                            localColumn2 += this.numberOfColumns;
                        }

                        // Perform bi-linear interpolation
                        sourceValue = (w1 * inputVector[(localRow * this.numberOfColumns) + localColumn])
                                      + (w2 * (w1 * inputVector[(localRow * this.numberOfColumns) + localColumn2]))
                                      + (w3 * (w1 * inputVector[(localRow2 * this.numberOfColumns) + localColumn]))
                                      + (w4 * (w1 * inputVector[(localRow2 * this.numberOfColumns) + localColumn2]));
                    }
                    else
                    {
                        // At least one supporting pixel for the "phantom" pixel is outside the
                        // bounds of the character grid. Set its value to "background"
                        // "background" color in the -1 -> +1 range of inputVector
                        sourceValue = 1.0; 
                    }

                    // Conversion to 0->1 range we are using for mappedVector
                    mappedVector[row][column] = 0.5 * (1.0 - sourceValue); 
                }
            }

            // Now, invert again while copying back into original vector
            for (row = 0; row < this.numberOfRows; ++row)
            {
                for (column = 0; column < this.numberOfColumns; ++column)
                {
                    inputVector[(row * this.numberOfColumns) + column] = 1.0 - (2.0 * mappedVector[row][column]);
                }
            }
        }

        /// <summary>
        /// Generates the distortion maps.
        /// </summary>
        /// <param name="severityFactor">The severity factor.</param>
        protected void GenerateDistortionMaps(double severityFactor)
        {
            // Generates the distortion maps in each of the horizontal and vertical directions
            // Three distortions are applied: a scaling, a rotation, and an elastic distortion
            // Since these are all linear transformations, we can simply add them together, after calculation
            // one at a time

            // The input parameter, severityFactor, let's us control the severity of the distortions relative
            // to the default values.  For example, if we only want half as harsh a distortion, set
            // severityFactor == 0.5

            // First, elastic distortion, per Patrice Simard, "Best Practices For Convolutional Neural Networks..."
            // at page 2.
            // Three-step process: seed array with uniform randoms, filter with a Gaussian kernel, normalize (scale)
            int row, column;
            var uniformHorizontal = new double[this.count];
            var uniformVertical = new double[this.count];
            var random = new Random();

            for (column = 0; column < this.numberOfColumns; ++column)
            {
                for (row = 0; row < this.numberOfRows; ++row)
                {
                    uniformHorizontal[(row * this.numberOfColumns) + column] = (2.0 * random.NextDouble()) - 1.0;
                    uniformVertical[(row * this.numberOfColumns) + column] = (2.0 * random.NextDouble()) - 1.0;
                }
            }

            // Filter with Gaussian
            var elasticScale = severityFactor * this.Preferences.ElasticScaling;
            const int FieldSize = 21 / 2;

            for (column = 0; column < this.numberOfColumns; ++column)
            {
                for (row = 0; row < this.numberOfRows; ++row)
                {
                    var convolvedHorizontal = 0.0;
                    var convolvedVertical = 0.0;

                    int xxx;

                    for (xxx = 0; xxx < 21; ++xxx)
                    {
                        int yyy;
                        for (yyy = 0; yyy < 21; ++yyy)
                        {
                            var indexX = column - FieldSize + xxx;
                            var indexY = row - FieldSize + yyy;

                            double sampleHorizontal;
                            double sampleVertical;

                            if (indexX < 0 || indexX >= this.numberOfColumns || indexY < 0 || indexY >= this.numberOfRows)
                            {
                                sampleHorizontal = 0.0;
                                sampleVertical = 0.0;
                            }
                            else
                            {
                                sampleHorizontal = uniformHorizontal[(indexY * this.numberOfColumns) + indexX];
                                sampleVertical = uniformVertical[(indexY * this.numberOfColumns) + indexX];
                            }

                            convolvedHorizontal += sampleHorizontal * this.gaussianKernel[yyy, xxx];
                            convolvedVertical += sampleVertical * this.gaussianKernel[yyy, xxx];
                        }
                    }

                    this.horizontalDistortion[(row * this.numberOfColumns) + column] = elasticScale * convolvedHorizontal;
                    this.verticalDistortion[(row * this.numberOfColumns) + column] = elasticScale * convolvedVertical;
                }
            }

            // Next, the scaling of the image by a random scale factor
            // Horizontal and vertical directions are scaled independently
            var horizontalScalingFactor = severityFactor * (this.Preferences.MaximumScaling / 100.0) * ((2.0 * random.NextDouble()) - 1.0);
            var verticalScalingFactor = severityFactor * (this.Preferences.MaximumScaling / 100.0) * ((2.0 * random.NextDouble()) - 1.0);

            var fieldSize = this.numberOfRows / 2;

            for (row = 0; row < this.numberOfRows; ++row)
            {
                for (column = 0; column < this.numberOfColumns; ++column)
                {
                    this.horizontalDistortion[(row * this.numberOfColumns) + column] =
                        this.horizontalDistortion[(row * this.numberOfColumns) + column]
                        + (horizontalScalingFactor * (column - fieldSize));
                    this.verticalDistortion[(row * this.numberOfColumns) + column] =
                        this.verticalDistortion[(row * this.numberOfColumns) + column]
                        - (verticalScalingFactor * (fieldSize - row));
                }
            }

            // Finally, apply a rotation
            var angle = severityFactor * (this.Preferences.MaximumRotation * ((2.0 * random.NextDouble()) - 1.0));

            // Convert from degrees to radians
            angle = angle * SystemGlobals.Pi / 180.0;

            var cosAngle = Math.Cos(angle);
            var sinAngle = Math.Sin(angle);

            for (row = 0; row < this.numberOfRows; ++row)
            {
                for (column = 0; column < this.numberOfColumns; ++column)
                {
                    this.horizontalDistortion[(row * this.numberOfColumns) + column] =
                        this.horizontalDistortion[(row * this.numberOfColumns) + column]
                        + ((column - fieldSize) * (cosAngle - 1)) - ((fieldSize - row) * sinAngle);
                    this.verticalDistortion[(row * this.numberOfColumns) + column] =
                        this.verticalDistortion[(row * this.numberOfColumns) + column]
                        - ((fieldSize - row) * (cosAngle - 1)) + ((column - fieldSize) * sinAngle);
                }
            }
        }
    }
}