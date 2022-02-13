// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MfcArchive.cs" company="H�mmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The MFC archive.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

/// <summary>
/// The MFC archive.
/// </summary>
public class MfcArchive : Archive
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MfcArchive"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="archiveOperation">The archive operation.</param>
    public MfcArchive(Stream stream, ArchiveOperation archiveOperation)
        : base(stream, archiveOperation)
    {
        if (archiveOperation == ArchiveOperation.Store)
        {
            throw new NotImplementedException("Writing to MFC compatible serialization is not supported.");
        }
    }

    /// <summary>
    /// Reads a decimal value.
    /// </summary>
    /// <param name="d">The decimal value.</param>
    public new void Read(out decimal d)
    {
        // MFC stores decimal as 32-bit status value, 32-bit high value, and 32-bit low value.
        this.Read(out int status);
        this.Read(out int high);
        this.Read(out uint low);

        if (status != (int)OleCurrencyStatus.Valid)
        {
            d = 0;
        }
        else
        {
            var final = MakeInt64((int)low, high);
            d = decimal.FromOACurrency(final);
        }
    }

    /// <summary>
    /// Reads a bool value.
    /// </summary>
    /// <param name="b">The bool value.</param>
    public new void Read(out bool b)
    {
        // MFC stores bools as 32-bit "long"
        this.Read(out int l);
        b = l != 0;
    }

    /// <summary>
    /// Reads the date time value.
    /// </summary>
    /// <param name="dt">The date time value.</param>
    public new void Read(out DateTime dt)
    {
        // Status is a 32-bit "long" in C++
        this.Read(out uint status);

        // MFC stores dates as 8-byte double
        this.Read(out double l);
        dt = DateTime.FromOADate(l);

        if (status == (uint)OleDateTimeStatus.Null || status == (uint)OleDateTimeStatus.Invalid)
        {
            dt = DateTime.FromOADate(0.0);
        }
    }

    /// <summary>
    /// Reads the nullable date time value.
    /// </summary>
    /// <param name="dt">The nullable date time value.</param>
    public void Read(out DateTime? dt)
    {
        // Status is a 32-bit "long" in C++
        this.Read(out uint status);

        this.Read(out double l);
        dt = DateTime.FromOADate(l);

        // Read in nullable type
        if (status == (uint)OleDateTimeStatus.Null || status == (uint)OleDateTimeStatus.Invalid)
        {
            dt = null;
        }
    }

    /// <summary>
    /// Reads the string.
    /// </summary>
    /// <param name="s">The string.</param>
    public new void Read(out string s)
    {
        if (this.Reader is null)
        {
            throw new ArgumentNullException(nameof(this.Reader), "The reader wasn't initialized properly.");
        }

        s = MfcStringReader.ReadCString(this.Reader);
    }

    /// <summary>
    /// Reads the string as unicode string.
    /// </summary>
    /// <param name="s">The unicode string.</param>
    public void ReadUnicodeString(out string s)
    {
        if (this.Reader is null)
        {
            throw new ArgumentNullException(nameof(this.Reader), "The reader wasn't initialized properly.");
        }

        s = this.Reader.ReadString();
    }

    /// <summary>
    /// Converts a current low and high to 8-Byte C++ CURRENCY structure.
    /// </summary>
    /// <param name="l1">The first int value.</param>
    /// <param name="l2">The second int value.</param>
    /// <returns>The long value.</returns>
    private static long MakeInt64(int l1, int l2)
    {
        return (uint)l1 | ((uint)l2 << 32);
    }
}
