using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace NeuralNetworkLibrary.DataFiles
{
    /// <summary>
    ///     MNIST Data Class (Image+Label)
    /// </summary>
    public class MnistDatabase
    {
        private bool _bImageFileOpen;
        private bool _bLabelFileOpen;
        private ImageFileBegining _imageFileBegin;
        private uint _iNextPattern;
        private LabelFileBegining _labelFileBegin;
        private BinaryReader _loadImageFileStream;
        private BinaryReader _loadLabelFileStream;

        // ReSharper disable once NotAccessedField.Local
        private string _mnistImageFileName;

        // ReSharper disable once NotAccessedField.Local
        private string _mnistLabelFileName;

        private uint _nItems;
        private int[] _randomizedPatternSequence;

        public List<ImagePattern> MpImagePatterns;

        public MnistDatabase()
        {
            _mnistImageFileName = null;
            _mnistLabelFileName = null;
            _iNextPattern = 0;
            _bImageFileOpen = false;
            _bLabelFileOpen = false;
            MpImagePatterns = null;
            _loadImageFileStream = null;
            _loadLabelFileStream = null;
            MbDatabaseReady = false;
            _randomizedPatternSequence = null;
            MbFromRandomizedPatternSequence = false;
        }

        public bool MbFromRandomizedPatternSequence { get; private set; }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool MbDatabaseReady { get; set; }

        public bool LoadMinstFiles()
        {
            //clear Image Pattern List
            MpImagePatterns?.Clear();
            //close files if opened
            if (_bImageFileOpen)
            {
                _loadImageFileStream.Close();
                _bImageFileOpen = false;
            }
            if (_bLabelFileOpen)
            {
                _loadLabelFileStream.Close();
                _bLabelFileOpen = false;
            }
            //load Mnist Images files.
            if (!MnistImageFileHeader())
            {
                MessageBox.Show("Can not open Image file");
                _mnistImageFileName = null;
                _bImageFileOpen = false;
                MbDatabaseReady = false;
                return false;
            }
            if (!MnistLabelFileHeader())
            {
                MessageBox.Show("Can not open label file");
                _mnistLabelFileName = null;
                _bLabelFileOpen = false;
                MbDatabaseReady = false;
                return false;
            }
            //check the value if image file and label file have been opened successfully
            if (_labelFileBegin.NItems != _imageFileBegin.NItems)
            {
                MessageBox.Show("Item numbers are different");
                CloseMinstFiles();
                MbDatabaseReady = false;
                return false;
            }
            MpImagePatterns = new List<ImagePattern>(_imageFileBegin.NItems);
            _randomizedPatternSequence = new int[_imageFileBegin.NItems];
            for (var i = 0; i < _imageFileBegin.NItems; i++)
            {
                // ReSharper disable once InlineOutVariableDeclaration
                byte mNlabel;
                var mPPatternArray = new byte[MyDefinitions.GcImageSize * MyDefinitions.GcImageSize];
                var mImagePattern = new ImagePattern();
                GetNextPattern(mPPatternArray, out mNlabel, true);
                mImagePattern.PPattern = mPPatternArray;
                mImagePattern.NLabel = mNlabel;
                MpImagePatterns.Add(mImagePattern);
            }
            MbDatabaseReady = true;
            CloseMinstFiles();
            return true;
        }

        private void CloseMinstFiles()
        {
            _loadLabelFileStream.Close();
            _loadImageFileStream.Close();
            _bImageFileOpen = false;
            _bLabelFileOpen = false;
        }

        /// <summary>
        ///     //Get MNIST Image file'header
        /// </summary>
        private bool MnistImageFileHeader()
        {
            if (_bImageFileOpen) return true;
            var mByte = new byte[4];
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = "Mnist Image file (*.idx3-ubyte)|*.idx3-ubyte",
                Title = "Open Minist Image File"
            };
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return false;
            _mnistImageFileName = openFileDialog1.FileName;

            try
            {
                _loadImageFileStream = new BinaryReader(openFileDialog1.OpenFile());
                //Magic number 
                _loadImageFileStream.Read(mByte, 0, 4);
                Array.Reverse(mByte, 0, 4);
                _imageFileBegin.NMagic = BitConverter.ToInt32(mByte, 0);
                //number of images 
                _loadImageFileStream.Read(mByte, 0, 4);
                //High-Endian format to Low-Endian format
                Array.Reverse(mByte, 0, 4);
                _imageFileBegin.NItems = BitConverter.ToInt32(mByte, 0);
                _nItems = (uint) _imageFileBegin.NItems;
                //number of rows 
                _loadImageFileStream.Read(mByte, 0, 4);
                Array.Reverse(mByte, 0, 4);
                _imageFileBegin.NRows = BitConverter.ToInt32(mByte, 0);
                //number of columns 
                _loadImageFileStream.Read(mByte, 0, 4);
                Array.Reverse(mByte, 0, 4);
                _imageFileBegin.NCols = BitConverter.ToInt32(mByte, 0);
                _bImageFileOpen = true;
                return true;
            }
            catch
            {
                _bImageFileOpen = false;
                return false;
            }
        }

        /// <summary>
        ///     Get MNIST Label file's header
        /// </summary>
        private bool MnistLabelFileHeader()
        {
            if (_bLabelFileOpen) return true;
            var mByte = new byte[4];
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = "Mnist Label file (*.idx1-ubyte)|*.idx1-ubyte",
                Title = "Open MNIST Label file"
            };
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return false;
            try
            {
                _mnistLabelFileName = openFileDialog1.FileName;
                _loadLabelFileStream = new BinaryReader(openFileDialog1.OpenFile());
                //Magic number 
                _loadLabelFileStream.Read(mByte, 0, 4);
                Array.Reverse(mByte, 0, 4);
                _labelFileBegin.NMagic = BitConverter.ToInt32(mByte, 0);
                //number of images 
                _loadLabelFileStream.Read(mByte, 0, 4);
                //High-Endian format to Low-Endian format
                Array.Reverse(mByte, 0, 4);
                _labelFileBegin.NItems = BitConverter.ToInt32(mByte, 0);
                _bLabelFileOpen = true;
                return true;
            }
            catch
            {
                _bLabelFileOpen = false;
                return false;
            }
        }

        /// <summary>
        ///     // get current pattern number
        /// </summary>
        /// <param name="bFromRandomizedPatternSequence"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public int GetCurrentPatternNumber(bool bFromRandomizedPatternSequence /* =FALSE */)
        {
            // returns the current number of the training pattern, either from the straight sequence, or from
            // the randomized sequence

            int iRet;

            if (bFromRandomizedPatternSequence == false)
                iRet = (int) _iNextPattern;
            else
                iRet = _randomizedPatternSequence[_iNextPattern];

            return iRet;
        }

        public int GetNextPatternNumber(bool bFromRandomizedPatternSequence /* =FALSE */)
        {
            // returns the current number of the training pattern, either from the straight sequence, or from
            // the randomized sequence
            if (_iNextPattern < _nItems - 1)
                _iNextPattern++;
            else
                _iNextPattern = 0;
            int iRet;

            if (bFromRandomizedPatternSequence == false)
                iRet = (int) _iNextPattern;
            else
                iRet = _randomizedPatternSequence[_iNextPattern];

            return iRet;
        }

        public int GetRandomPatternNumber()
        {
            var rdm = new Random();
            var patternNum = (int) (rdm.NextDouble() * (_nItems - 1));
            return patternNum;
        }

        public void RandomizePatternSequence()
        {
            // randomizes the order of m_iRandomizedTrainingPatternSequence, which is a UINT array
            // holding the numbers 0..59999 in random order
            //reset iNextPattern to 0
            _iNextPattern = 0;
            int ii;

            var iiMax = (int) _nItems;
            // initialize array in sequential order

            for (ii = 0; ii < iiMax; ii++)
                _randomizedPatternSequence[ii] = ii;


            // now at each position, swap with a random position
            var rdm = new Random();
            for (ii = 0; ii < iiMax; ii++)
            {
                //gives a uniformly-distributed number between zero (inclusive) and one (exclusive):(uint)(rdm.Next() / (0x7fff + 1))

                var jj = (int) (rdm.NextDouble() * iiMax);

                var iiTemp = _randomizedPatternSequence[ii];
                _randomizedPatternSequence[ii] = _randomizedPatternSequence[jj];
                _randomizedPatternSequence[jj] = iiTemp;
            }
            MbFromRandomizedPatternSequence = true;
        }

        /// <summary>
        ///     //get value of pattern
        /// </summary>
        /// <param name="pArray"></param>
        /// <param name="pLabel"></param>
        /// <param name="bFlipGrayscale"></param>
        private void GetPatternArrayValues(out byte pLabel, byte[] pArray = null, bool bFlipGrayscale = true)
        {
            ////////
            uint cCount = MyDefinitions.GcImageSize * MyDefinitions.GcImageSize;
            //
            if (_bImageFileOpen)
            {
                if (pArray != null)
                {
                    //load_ImageFile_stream.Read(pArray,(int)fPos,(int)cCount);
                    _loadImageFileStream.Read(pArray, 0, (int) cCount);
                    if (bFlipGrayscale)
                        for (var ii = 0; ii < cCount; ++ii)
                            pArray[ii] = Convert.ToByte(255 - Convert.ToInt32(pArray[ii]));
                }
            }
            else // no files are open: return a simple gray wedge
            {
                if (pArray != null)
                    for (var ii = 0; ii < cCount; ++ii)
                        pArray[ii] = Convert.ToByte(ii * 255 / cCount);
            }
            //read label
            if (_bLabelFileOpen)
            {
                var mByte = new byte[1];
                _loadLabelFileStream.Read(mByte, 0, 1);
                pLabel = mByte[0];
            }
            else
            {
                pLabel = 255;
            }
        }

        private void GetNextPattern(byte[] pArray, out byte pLabel, bool bFlipGrayscale /* =TRUE */)
        {
            // returns the number of the pattern corresponding to the pattern stored in pArray
            GetPatternArrayValues(out pLabel, pArray, bFlipGrayscale);
            _iNextPattern++;
            if (_iNextPattern >= _nItems)
                _iNextPattern = 0;
        }
    }
}