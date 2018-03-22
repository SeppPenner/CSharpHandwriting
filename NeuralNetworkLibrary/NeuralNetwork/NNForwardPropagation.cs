using System;
using System.Collections.Generic;
using System.Threading;
using NeuralNetworkLibrary.ArchiveSerialization;
using NeuralNetworkLibrary.NNNeurons;

namespace NeuralNetworkLibrary.NeuralNetwork
{
    public class NnForwardPropagation
    {
        private readonly int _cCols; // size of the distortion maps
        private readonly int _cCount;

        private readonly int _cRows;

        //double m_GaussianKernel[ GAUSSIAN_FIELD_SIZE ] [ GAUSSIAN_FIELD_SIZE ];
        private readonly double[,] _gaussianKernel =
            new double[MyDefinitions.GaussianFieldSize, MyDefinitions.GaussianFieldSize];

        private readonly double[] _mDispH; // horiz distortion map array
        private readonly double[] _mDispV; // vert distortion map array

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly List<Mutex> _mMutexs;
#pragma warning disable 414
        public int _mCurrentPatternIndex;
#pragma warning restore 414

        /// <summary>
        /// </summary>

        // Main thread sets this event to stop worker thread:
#pragma warning disable 414
        private ManualResetEvent _mEventStop;
#pragma warning restore 414

        // Worker thread sets this event when it is stopped:
#pragma warning disable 414
        private ManualResetEvent _mEventStopped;
#pragma warning restore 414

        // ReSharper disable once NotAccessedField.Local
        private HiPerfTimer _mHiPerfTime;

#pragma warning disable 414
        private uint _mnImages;
#pragma warning restore 414

        /// <summary>
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        protected bool BDataReady;

        public bool MbDistortPatterns;

        //backpropagation and training-related members
        protected NeuralNetwork Nn;

        /// <summary>
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public NnForwardPropagation()
        {
            _mCurrentPatternIndex = 0;
            BDataReady = false;
            Nn = null;
            _mEventStop = null;
            _mEventStopped = null;
            _mMutexs = new List<Mutex>(4);
            _mHiPerfTime = new HiPerfTimer();
            _mnImages = 0;
            // allocate memory to store the distortion maps

            _cCols = 29;
            _cRows = 29;

            _cCount = _cCols * _cRows;

            _mDispH = new double[_cCount];
            _mDispV = new double[_cCount];
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Preferences MPreferences { get; set; }

        // ReSharper disable once UnusedMember.Global
        public NeuralNetwork MNeuralNetwork
        {
            get => Nn;
            set => Nn = value;
        }

        protected void GetGaussianKernel(double dElasticSigma)
        {
            // create a gaussian kernel, which is constant, for use in generating elastic distortions

            const int iiMid = 21 / 2; // GAUSSIAN_FIELD_SIZE is strictly odd

            var twoSigmaSquared = 2.0 * dElasticSigma * dElasticSigma;
            twoSigmaSquared = 1.0 / twoSigmaSquared;
            var twoPiSigma = 1.0 / dElasticSigma * Math.Sqrt(2.0 * 3.1415926535897932384626433832795);

            for (var col = 0; col < 21; ++col)
            for (var row = 0; row < 21; ++row)
                _gaussianKernel[row, col] = twoPiSigma *
                                            Math.Exp(-(((row - iiMid) * (row - iiMid) + (col - iiMid) * (col - iiMid)) *
                                                       twoSigmaSquared));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public double GetCurrentEta()
        {
            return Nn?.MEtaLearningRate ?? 0.0;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public double GetPreviousEta()
        {
            // provided because threads might change the current eta before we are able to read it
            return Nn?.MEtaLearningRatePrevious ?? 0.0;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected void CalculateNeuralNet(double[] inputVector, int count,
            double[] outputVector /* =NULL */, int oCount /* =0 */,
            NnNeuronOutputsList pNeuronOutputs /* =NULL */,
            bool bDistort /* =FALSE */)
        {
            // wrapper function for neural net's Calculate() function, needed because the NN is a protected member
            // waits on the neural net mutex (using the CAutoMutex object, which automatically releases the
            // mutex when it goes out of scope) so as to restrict access to one thread at a time
            _mMutexs[0].WaitOne();
            {
                if (bDistort)
                {
                    GenerateDistortionMap(1.0);
                    ApplyDistortionMap(inputVector);
                }


                Nn.Calculate(inputVector, count, outputVector, oCount, pNeuronOutputs);
            }
            _mMutexs[0].ReleaseMutex();
        }

        /// <summary>
        ///     Distortion Pattern
        /// </summary>
        /// <param name="inputVector"></param>
        protected void ApplyDistortionMap(double[] inputVector)
        {
            // applies the current distortion map to the input vector

            // For the mapped array, we assume that 0.0 == background, and 1.0 == full intensity information
            // This is different from the input vector, in which +1.0 == background (white), and 
            // -1.0 == information (black), so we must convert one to the other

            var mappedVector = new List<List<double>>(_cRows);
            for (var i = 0; i < _cRows; i++)
            {
                var mVector = new List<double>(_cCols);

                for (var j = 0; j < _cCols; j++)
                    mVector.Add(0.0);
                mappedVector.Add(mVector);
            }

            int row, col;

            for (row = 0; row < _cRows; ++row)
            for (col = 0; col < _cCols; ++col)
            {
                // the pixel at sourceRow, sourceCol is an "phantom" pixel that doesn't really exist, and
                // whose value must be manufactured from surrounding real pixels (i.e., since 
                // sourceRow and sourceCol are floating point, not ints, there's not a real pixel there)
                // The idea is that if we can calculate the value of this phantom pixel, then its 
                // displacement will exactly fit into the current pixel at row, col (which are both ints)

                var sourceRow = row - _mDispV[row * _cCols + col];
                var sourceCol = col - _mDispH[row * _cCols + col];

                // weights for bi-linear interpolation

                var fracRow = sourceRow - (int) sourceRow;
                var fracCol = sourceCol - (int) sourceCol;


                var w1 = (1.0 - fracRow) * (1.0 - fracCol);
                var w2 = (1.0 - fracRow) * fracCol;
                var w3 = fracRow * (1 - fracCol);
                var w4 = fracRow * fracCol;


                // limit indexes

                /*
                                while (sourceRow >= m_cRows ) sourceRow -= m_cRows;
                                while (sourceRow < 0 ) sourceRow += m_cRows;
			
                                while (sourceCol >= m_cCols ) sourceCol -= m_cCols;
                                while (sourceCol < 0 ) sourceCol += m_cCols;
                    */
                var bSkipOutOfBounds = sourceRow + 1.0 >= _cRows || sourceRow < 0 || sourceCol + 1.0 >= _cCols ||
                                       sourceCol < 0;

                double sourceValue;
                if (bSkipOutOfBounds == false)
                {
                    // the supporting pixels for the "phantom" source pixel are all within the 
                    // bounds of the character grid.
                    // Manufacture its value by bi-linear interpolation of surrounding pixels

                    var sRow = (int) sourceRow;
                    var sCol = (int) sourceCol;

                    var sRowp1 = sRow + 1;
                    var sColp1 = sCol + 1;

                    while (sRowp1 >= _cRows) sRowp1 -= _cRows;
                    while (sRowp1 < 0) sRowp1 += _cRows;

                    while (sColp1 >= _cCols) sColp1 -= _cCols;
                    while (sColp1 < 0) sColp1 += _cCols;

                    // perform bi-linear interpolation

                    sourceValue = w1 * inputVector[sRow * _cCols + sCol] +
                                  w2 * w1 * inputVector[sRow * _cCols + sColp1] +
                                  w3 * w1 * inputVector[sRowp1 * _cCols + sCol] +
                                  w4 * w1 * inputVector[sRowp1 * _cCols + sColp1];
                }
                else
                {
                    // At least one supporting pixel for the "phantom" pixel is outside the
                    // bounds of the character grid. Set its value to "background"

                    sourceValue = 1.0; // "background" color in the -1 -> +1 range of inputVector
                }

                mappedVector[row][col] =
                    0.5 * (1.0 - sourceValue); // conversion to 0->1 range we are using for mappedVector
            }

            // now, invert again while copying back into original vector

            for (row = 0; row < _cRows; ++row)
            for (col = 0; col < _cCols; ++col)
                inputVector[row * _cCols + col] = 1.0 - 2.0 * mappedVector[row][col];
        }

        /// <summary>
        /// </summary>
        /// <param name="severityFactor"></param>
        protected void GenerateDistortionMap(double severityFactor /* =1.0 */)
        {
            // generates distortion maps in each of the horizontal and vertical directions
            // Three distortions are applied: a scaling, a rotation, and an elastic distortion
            // Since these are all linear tranformations, we can simply add them together, after calculation
            // one at a time

            // The input parameter, severityFactor, let's us control the severity of the distortions relative
            // to the default values.  For example, if we only want half as harsh a distortion, set
            // severityFactor == 0.5

            // First, elastic distortion, per Patrice Simard, "Best Practices For Convolutional Neural Networks..."
            // at page 2.
            // Three-step process: seed array with uniform randoms, filter with a gaussian kernel, normalize (scale)

            int row, col;
            var uniformH = new double[_cCount];
            var uniformV = new double[_cCount];
            var rdm = new Random();

            for (col = 0; col < _cCols; ++col)
            for (row = 0; row < _cRows; ++row)
            {
                uniformH[row * _cCols + col] = 2.0 * rdm.NextDouble() - 1.0;
                uniformV[row * _cCols + col] = 2.0 * rdm.NextDouble() - 1.0;
            }

            // filter with gaussian

            var elasticScale = severityFactor * MPreferences.MdElasticScaling;
            const int iiMid = 21 / 2; // GAUSSIAN_FIELD_SIZE (21) is strictly odd

            for (col = 0; col < _cCols; ++col)
            for (row = 0; row < _cRows; ++row)
            {
                var fConvolvedH = 0.0;
                var fConvolvedV = 0.0;

                int xxx;
                for (xxx = 0; xxx < 21; ++xxx)
                {
                    int yyy;
                    for (yyy = 0; yyy < 21; ++yyy)
                    {
                        var xxxDisp = col - iiMid + xxx;
                        var yyyDisp = row - iiMid + yyy;

                        double fSampleH;
                        double fSampleV;
                        if (xxxDisp < 0 || xxxDisp >= _cCols || yyyDisp < 0 || yyyDisp >= _cRows)
                        {
                            fSampleH = 0.0;
                            fSampleV = 0.0;
                        }
                        else
                        {
                            fSampleH = uniformH[yyyDisp * _cCols + xxxDisp];
                            fSampleV = uniformV[yyyDisp * _cCols + xxxDisp];
                        }

                        fConvolvedH += fSampleH * _gaussianKernel[yyy, xxx];
                        fConvolvedV += fSampleV * _gaussianKernel[yyy, xxx];
                    }
                }

                _mDispH[row * _cCols + col] = elasticScale * fConvolvedH;
                _mDispV[row * _cCols + col] = elasticScale * fConvolvedV;
            }

            // next, the scaling of the image by a random scale factor
            // Horizontal and vertical directions are scaled independently

            var dSfHoriz =
                severityFactor * MPreferences.MdMaxScaling / 100.0 *
                (2.0 * rdm.NextDouble() - 1.0); // m_dMaxScaling is a percentage
            var dSfVert =
                severityFactor * MPreferences.MdMaxScaling / 100.0 *
                (2.0 * rdm.NextDouble() - 1.0); // m_dMaxScaling is a percentage


            var iMid = _cRows / 2;

            for (row = 0; row < _cRows; ++row)
            for (col = 0; col < _cCols; ++col)
            {
                _mDispH[row * _cCols + col] = _mDispH[row * _cCols + col] + dSfHoriz * (col - iMid);
                _mDispV[row * _cCols + col] =
                    _mDispV[row * _cCols + col] - dSfVert * (iMid - row); // negative because of top-down bitmap
            }


            // finally, apply a rotation

            var angle = severityFactor * MPreferences.MdMaxRotation * (2.0 * rdm.NextDouble() - 1.0);
            angle = angle * 3.1415926535897932384626433832795 / 180.0; // convert from degrees to radians

            var cosAngle = Math.Cos(angle);
            var sinAngle = Math.Sin(angle);

            for (row = 0; row < _cRows; ++row)
            for (col = 0; col < _cCols; ++col)
            {
                _mDispH[row * _cCols + col] =
                    _mDispH[row * _cCols + col] + (col - iMid) * (cosAngle - 1) - (iMid - row) * sinAngle;
                _mDispV[row * _cCols + col] =
                    _mDispV[row * _cCols + col] - (iMid - row) * (cosAngle - 1) +
                    (col - iMid) * sinAngle; // negative because of top-down bitmap
            }
        }
    }
}