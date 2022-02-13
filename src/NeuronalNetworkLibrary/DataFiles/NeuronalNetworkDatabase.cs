// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronalNetworkDatabase.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The neuronal network database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.DataFiles;

/// <summary>
/// The neuronal network database.
/// </summary>
public class NeuronalNetworkDatabase
{
    /// <summary>
    /// A value indicating whether the image file is open or not.
    /// </summary>
    private bool imageFileOpen;

    /// <summary>
    /// A value indicating whether the label file is open or not.
    /// </summary>
    private bool labelFileOpen;

    /// <summary>
    /// The image file beginning.
    /// </summary>
    private ImageFileBeginning imageFileBeginning;

    /// <summary>
    /// The next pattern.
    /// </summary>
    private uint nextPattern;

    /// <summary>
    /// The label file beginning.
    /// </summary>
    private LabelFileBeginning labelFileBeginning;

    /// <summary>
    /// The load image file stream.
    /// </summary>
    private BinaryReader? loadImageFileStream;

    /// <summary>
    /// The load label file stream.
    /// </summary>
    private BinaryReader? loadLabelFileStream;

    /// <summary>
    /// The image file name.
    /// </summary>
    private string? imageFileName;

    /// <summary>
    /// The label file name.
    /// </summary>
    private string? labelFileName;

    /// <summary>
    /// The item count.
    /// </summary>
    private uint itemCount;

    /// <summary>
    /// The randomized pattern sequence.
    /// </summary>
    private int[] randomizedPatternSequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuronalNetworkDatabase"/> class.
    /// </summary>
    public NeuronalNetworkDatabase()
    {
        this.imageFileName = null;
        this.labelFileName = null;
        this.nextPattern = 0;
        this.imageFileOpen = false;
        this.labelFileOpen = false;
        this.ImagePatterns = new();
        this.loadImageFileStream = null;
        this.loadLabelFileStream = null;
        this.DatabaseReady = false;
        this.randomizedPatternSequence = Array.Empty<int>();
        this.FromRandomizedPatternSequence = false;
    }

    /// <summary>
    /// Gets or sets the image patterns.
    /// </summary>
    public List<ImagePattern> ImagePatterns { get; set; }

    /// <summary>
    /// Gets a value indicating whether the from randomizer pattern sequence is set or not.
    /// </summary>
    public bool FromRandomizedPatternSequence { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the database is ready or not.
    /// </summary>
    private bool DatabaseReady { get; set; }

    /// <summary>
    /// Loads the database files.
    /// </summary>
    /// <returns>A value indicating whether the database files are loaded or not.</returns>
    public bool LoadDatabaseFiles()
    {
        // Clear the image pattern list
        this.ImagePatterns?.Clear();

        // Close the files if opened
        if (this.imageFileOpen)
        {
            this.loadImageFileStream?.Close();
            this.imageFileOpen = false;
        }

        if (this.labelFileOpen)
        {
            this.loadLabelFileStream?.Close();
            this.labelFileOpen = false;
        }

        // Load the image files
        if (!this.GetImageFileHeader())
        {
            MessageBox.Show("Can not open image file");
            this.imageFileName = null;
            this.imageFileOpen = false;
            this.DatabaseReady = false;
            return false;
        }

        if (!this.GetLabelFileHeader())
        {
            MessageBox.Show("Can not open label file");
            this.labelFileName = null;
            this.labelFileOpen = false;
            this.DatabaseReady = false;
            return false;
        }

        // Check the value if the image file and label file have been opened successfully
        if (this.labelFileBeginning.Items != this.imageFileBeginning.Items)
        {
            MessageBox.Show("Item numbers are different");
            this.CloseDatabaseFiles();
            this.DatabaseReady = false;
            return false;
        }

        this.ImagePatterns = new List<ImagePattern>(this.imageFileBeginning.Items);
        this.randomizedPatternSequence = new int[this.imageFileBeginning.Items];

        for (var i = 0; i < this.imageFileBeginning.Items; i++)
        {
            var patternArray = new byte[SystemGlobals.ImageSize * SystemGlobals.ImageSize];
            var imagePattern = new ImagePattern();
            this.GetNextPattern(patternArray, out var label, true);
            imagePattern.Pattern = patternArray;
            imagePattern.Label = label;
            this.ImagePatterns.Add(imagePattern);
        }

        this.DatabaseReady = true;
        this.CloseDatabaseFiles();
        return true;
    }

    /// <summary>
    /// Gets the current pattern number.
    /// </summary>
    /// <param name="randomizedPatternSequenceParameter">The randomized pattern sequence.</param>
    /// <returns>The current pattern number.</returns>
    public int GetCurrentPatternNumber(bool randomizedPatternSequenceParameter)
    {
        // returns the current number of the training pattern, either from the straight sequence, or from
        // the randomized sequence
        if (randomizedPatternSequenceParameter == false)
        {
            return (int)this.nextPattern;
        }

        return this.randomizedPatternSequence[this.nextPattern];
    }

    /// <summary>
    /// Gets the next pattern number.
    /// </summary>
    /// <param name="fromRandomizedPatternSequence">A value indicating whether the from randomizer pattern sequence is set or not.</param>
    /// <returns>The next pattern number.</returns>
    public int GetNextPatternNumber(bool fromRandomizedPatternSequence)
    {
        // Returns the current number of the training pattern, either from the straight sequence, or from
        // the randomized sequence
        if (this.nextPattern < this.itemCount - 1)
        {
            this.nextPattern++;
        }
        else
        {
            this.nextPattern = 0;
        }

        if (fromRandomizedPatternSequence == false)
        {
            return (int)this.nextPattern;
        }

        return this.randomizedPatternSequence[this.nextPattern];
    }

    /// <summary>
    /// Gets a random pattern number.
    /// </summary>
    /// <returns>The random pattern number.</returns>
    public int GetRandomPatternNumber()
    {
        var rdm = new Random();
        var patternNum = (int)(rdm.NextDouble() * (this.itemCount - 1));
        return patternNum;
    }

    /// <summary>
    /// Randomizes the pattern sequence.
    /// </summary>
    public void RandomizePatternSequence()
    {
        // Randomizes the order of randomizedPatternSequence, which is an UINT array
        // holding the numbers 0..59999 in random order
        // Reset nextPattern to 0
        this.nextPattern = 0;
        int ii;

        var maximum = (int)this.itemCount;

        // Initialize array in sequential order
        for (ii = 0; ii < maximum; ii++)
        {
            this.randomizedPatternSequence[ii] = ii;
        }

        // Now at each position, swap with a random position
        var rdm = new Random();

        for (ii = 0; ii < maximum; ii++)
        {
            // Gives a uniformly-distributed number between zero (inclusive) and one (exclusive):(uint)(rdm.Next() / (0x7fff + 1))
            var jj = (int)(rdm.NextDouble() * maximum);
            var tempValue = this.randomizedPatternSequence[ii];
            this.randomizedPatternSequence[ii] = this.randomizedPatternSequence[jj];
            this.randomizedPatternSequence[jj] = tempValue;
        }

        this.FromRandomizedPatternSequence = true;
    }

    /// <summary>
    /// Closes the database files.
    /// </summary>
    private void CloseDatabaseFiles()
    {
        this.loadLabelFileStream?.Close();
        this.loadImageFileStream?.Close();
        this.imageFileOpen = false;
        this.labelFileOpen = false;
    }

    /// <summary>
    /// Gets the image file header.
    /// </summary>
    /// <returns>A value indicating whether the image file header is present or not.</returns>
    private bool GetImageFileHeader()
    {
        if (this.imageFileOpen)
        {
            return true;
        }

        var bytes = new byte[4];

        var openFileDialog1 = new OpenFileDialog
        {
            Filter = "Image file (*.idx3-ubyte)|*.idx3-ubyte",
            Title = "Open image file"
        };

        if (openFileDialog1.ShowDialog() != DialogResult.OK)
        {
            return false;
        }

        this.imageFileName = openFileDialog1.FileName;

        try
        {
            this.loadImageFileStream = new BinaryReader(openFileDialog1.OpenFile());

            // The magic number 
            this.loadImageFileStream.Read(bytes, 0, 4);
            Array.Reverse(bytes, 0, 4);
            this.imageFileBeginning.MagicNumber = BitConverter.ToInt32(bytes, 0);

            // The number of images 
            this.loadImageFileStream.Read(bytes, 0, 4);

            // Convert big endian to little endian
            Array.Reverse(bytes, 0, 4);
            this.imageFileBeginning.Items = BitConverter.ToInt32(bytes, 0);
            this.itemCount = (uint)this.imageFileBeginning.Items;

            // The number of rows 
            this.loadImageFileStream.Read(bytes, 0, 4);
            Array.Reverse(bytes, 0, 4);
            this.imageFileBeginning.Rows = BitConverter.ToInt32(bytes, 0);

            // The number of columns 
            this.loadImageFileStream.Read(bytes, 0, 4);
            Array.Reverse(bytes, 0, 4);
            this.imageFileBeginning.Columns = BitConverter.ToInt32(bytes, 0);
            this.imageFileOpen = true;
            return true;
        }
        catch
        {
            this.imageFileOpen = false;
            return false;
        }
    }

    /// <summary>
    /// Gets the label file header.
    /// </summary>
    /// <returns>A value indicating whether the label file header is present or not.</returns>
    private bool GetLabelFileHeader()
    {
        if (this.labelFileOpen)
        {
            return true;
        }

        var bytes = new byte[4];
        var openFileDialog1 = new OpenFileDialog
        {
            Filter = "Label file (*.idx1-ubyte)|*.idx1-ubyte",
            Title = "Open label file"
        };

        if (openFileDialog1.ShowDialog() != DialogResult.OK)
        {
            return false;
        }

        try
        {
            this.labelFileName = openFileDialog1.FileName;
            this.loadLabelFileStream = new BinaryReader(openFileDialog1.OpenFile());

            // Magic number 
            this.loadLabelFileStream.Read(bytes, 0, 4);
            Array.Reverse(bytes, 0, 4);
            this.labelFileBeginning.MagicNumber = BitConverter.ToInt32(bytes, 0);

            // number of images 
            this.loadLabelFileStream.Read(bytes, 0, 4);

            // High-Endian format to Low-Endian format
            Array.Reverse(bytes, 0, 4);
            this.labelFileBeginning.Items = BitConverter.ToInt32(bytes, 0);
            this.labelFileOpen = true;
            return true;
        }
        catch
        {
            this.labelFileOpen = false;
            return false;
        }
    }

    /// <summary>
    /// Gets the pattern array values.
    /// </summary>
    /// <param name="patternLabel">The pattern label.</param>
    /// <param name="patternArray">The pattern array.</param>
    /// <param name="flipGrayscale">A value indicating whether the bytes should be flipped with Grayscale algorithm.</param>
    private void GetPatternArrayValues(out byte patternLabel, byte[]? patternArray = null, bool flipGrayscale = true)
    {
        if (this.loadImageFileStream is null)
        {
            throw new ArgumentNullException(nameof(this.loadImageFileStream), "The load image file stream wasn't initialized properly.");
        }

        if (this.loadLabelFileStream is null)
        {
            throw new ArgumentNullException(nameof(this.loadLabelFileStream), "The load label file stream wasn't initialized properly.");
        }

        const uint ImageSize = SystemGlobals.ImageSize * SystemGlobals.ImageSize;

        if (this.imageFileOpen)
        {
            if (patternArray != null)
            {
                this.loadImageFileStream.Read(patternArray, 0, (int)ImageSize);

                if (flipGrayscale)
                {
                    for (var ii = 0; ii < ImageSize; ++ii)
                    {
                        patternArray[ii] = Convert.ToByte(255 - Convert.ToInt32(patternArray[ii]));
                    }
                }
            }
        }
        else
        {
            // No files are open: Return a simple gray wedge
            if (patternArray != null)
            {
                for (var ii = 0; ii < ImageSize; ++ii)
                {
                    patternArray[ii] = Convert.ToByte(ii * 255 / ImageSize);
                }
            }
        }

        // Read the label
        if (this.labelFileOpen)
        {
            var bytes = new byte[1];
            this.loadLabelFileStream.Read(bytes, 0, 1);
            patternLabel = bytes[0];
        }
        else
        {
            patternLabel = 255;
        }
    }

    /// <summary>
    /// Gets the next pattern.
    /// </summary>
    /// <param name="patternArray">The pattern array.</param>
    /// <param name="patternLabel">The pattern label.</param>
    /// <param name="flipGrayscale">A value indicating whether the bytes should be flipped with Grayscale algorithm.</param>
    private void GetNextPattern(byte[] patternArray, out byte patternLabel, bool flipGrayscale)
    {
        // Returns the number of the pattern corresponding to the pattern stored in patternArray
        this.GetPatternArrayValues(out patternLabel, patternArray, flipGrayscale);
        this.nextPattern++;

        if (this.nextPattern >= this.itemCount)
        {
            this.nextPattern = 0;
        }
    }
}
