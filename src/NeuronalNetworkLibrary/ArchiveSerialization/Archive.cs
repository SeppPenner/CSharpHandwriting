// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Archive.cs" company="H�mmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The archive class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization;

/// <summary>
/// The archive class.
/// </summary>
public class Archive
{
    /// <summary>
    /// The reader.
    /// </summary>
    protected readonly BinaryReader? Reader;

    /// <summary>
    /// The index.
    /// </summary>
    private const int Index = 0;

    /// <summary>
    /// The operator.
    /// </summary>
    private readonly ArchiveOperation archiveOperation;

    /// <summary>
    /// The writer.
    /// </summary>
    private readonly BinaryWriter? writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Archive"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="archiveOperation">The archive operator.</param>
    public Archive(Stream stream, ArchiveOperation archiveOperation)
    {
        this.archiveOperation = archiveOperation;

        if (archiveOperation == ArchiveOperation.Load)
        {
            this.Reader = new BinaryReader(stream);
        }
        else
        {
            this.writer = new BinaryWriter(stream);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the archive operator is storing at the moment or not.
    /// </summary>
    /// <returns>A value indicating whether the archive operator is storing at the moment or not.</returns>
    public bool IsStoring()
    {
        return this.archiveOperation == ArchiveOperation.Store;
    }

    /// <summary>
    /// Serializes the archive serialization.
    /// </summary>
    /// <param name="obj">The object.</param>
    public void Serialize(IArchiveSerialization obj)
    {
        obj.Serialize(this);
    }

    /// <summary>
    /// Writes a char value.
    /// </summary>
    /// <param name="ch">The char value.</param>
    public void Write(char ch)
    {
        this.writer?.Write(Convert.ToInt16(ch));
    }

    /// <summary>
    /// Writes the ushort value.
    /// </summary>
    /// <param name="n">The ushort value value.</param>
    public void Write(ushort n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the short value.
    /// </summary>
    /// <param name="n">The short value.</param>
    public void Write(short n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the uint value.
    /// </summary>
    /// <param name="n">The uint value.</param>
    public void Write(uint n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the int value.
    /// </summary>
    /// <param name="n">The int value.</param>
    public void Write(int n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the ulong value.
    /// </summary>
    /// <param name="n">The ulong value.</param>
    public void Write(ulong n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the long value.
    /// </summary>
    /// <param name="n">The long value.</param>
    public void Write(long n)
    {
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the float value.
    /// </summary>
    /// <param name="d">The float value.</param>
    public void Write(float d)
    {
        this.writer?.Write(d);
    }

    /// <summary>
    /// Writes the double value.
    /// </summary>
    /// <param name="d">The double value.</param>
    public void Write(double d)
    {
        this.writer?.Write(d);
    }

    /// <summary>
    /// Writes the decimal value.
    /// </summary>
    /// <param name="d">The decimal value.</param>
    public void Write(decimal d)
    {
        // store decimals as Int64
        var n = decimal.ToOACurrency(d);
        this.writer?.Write(n);
    }

    /// <summary>
    /// Writes the date time value.
    /// </summary>
    /// <param name="dt">The date time value.</param>
    public void Write(DateTime dt)
    {
        this.writer?.Write(dt.ToBinary());
    }

    /// <summary>
    /// Writes the bool value.
    /// </summary>
    /// <param name="b">The bool value.</param>
    public void Write(bool b)
    {
        this.writer?.Write(b);
    }

    /// <summary>
    /// Writes the string value.
    /// </summary>
    /// <param name="s">The string value.</param>
    public void Write(string s)
    {
        this.writer?.Write(Convert.ToInt32(s.Length));
        this.writer?.Write(s.ToCharArray());
    }

    /// <summary>
    /// Writes the GUID value.
    /// </summary>
    /// <param name="guid">The GUID value.</param>
    public void Write(Guid guid)
    {
        var bytes = guid.ToByteArray();
        this.Write(bytes);
    }

    /// <summary>
    /// Reads the string value.
    /// </summary>
    /// <param name="s">The string value.</param>
    public void Read(out string s)
    {
        this.Read(out int length);

        var ch = new char[length];

        this.Reader?.Read(ch, Index, length);

        var sb = new StringBuilder();
        sb.Append(ch);
        s = sb.ToString();
    }

    /// <summary>
    /// Reads the ushort value.
    /// </summary>
    /// <param name="n">The ushort value.</param>
    public void Read(out ushort n)
    {
        var bytes = new byte[2];
        this.Reader?.Read(bytes, Index, 2);
        n = BitConverter.ToUInt16(bytes, 0);
    }

    /// <summary>
    /// Reads the uint value.
    /// </summary>
    /// <param name="n">The uint value.</param>
    public void Read(out uint n)
    {
        var bytes = new byte[4];
        this.Reader?.Read(bytes, Index, 4);
        n = BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>
    /// Reads the int value.
    /// </summary>
    /// <param name="n">The int value.</param>
    public void Read(out int n)
    {
        var bytes = new byte[4];
        this.Reader?.Read(bytes, Index, 4);
        n = BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Reads the ulong value.
    /// </summary>
    /// <param name="n">The ulong value.</param>
    public void Read(out ulong n)
    {
        var bytes = new byte[8];
        this.Reader?.Read(bytes, Index, 8);
        n = BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// Reads the char value.
    /// </summary>
    /// <param name="ch">The char value.</param>
    public void Read(out char ch)
    {
        this.Read(out short n);
        ch = Convert.ToChar(n);
    }

    /// <summary>
    /// Reads the float value.
    /// </summary>
    /// <param name="d">The float value.</param>
    public void Read(out float d)
    {
        var bytes = new byte[4];
        this.Reader?.Read(bytes, Index, 4);
        d = BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// Reads the double value.
    /// </summary>
    /// <param name="d">The double value.</param>
    public void Read(out double d)
    {
        var bytes = new byte[8];
        this.Reader?.Read(bytes, Index, 8);
        d = BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// Reads the decimal value.
    /// </summary>
    /// <param name="d">The decimal value.</param>
    public void Read(out decimal d)
    {
        var bytes = new byte[8];
        this.Reader?.Read(bytes, Index, 8);

        // BitConverter does not support direct conversion to Decimal so use Int64
        var n = BitConverter.ToInt64(bytes, 0);
        d = decimal.FromOACurrency(n);
    }

    /// <summary>
    /// Reads the date time value.
    /// </summary>
    /// <param name="dt">The date time value.</param>
    public void Read(out DateTime dt)
    {
        this.Read(out long l);
        dt = DateTime.FromBinary(l);
    }

    /// <summary>
    /// Reads the bool value.
    /// </summary>
    /// <param name="b">The bool value.</param>
    public void Read(out bool b)
    {
        var bytes = new byte[1];
        this.Reader?.Read(bytes, Index, 1);
        b = BitConverter.ToBoolean(bytes, 0);
    }

    /// <summary>
    /// Reads the GUID value.
    /// </summary>
    /// <param name="guid">The GUID value.</param>
    public void Read(out Guid guid)
    {
        this.Read(out var bytes, 16);
        guid = new Guid(bytes);
    }

    /// <summary>
    /// Writes the byte array.
    /// </summary>
    /// <param name="buffer">The byte array.</param>
    private void Write(byte[] buffer)
    {
        this.writer?.Write(buffer);
    }

    /// <summary>
    /// Reads the short value.
    /// </summary>
    /// <param name="n">The short value.</param>
    private void Read(out short n)
    {
        var bytes = new byte[2];
        this.Reader?.Read(bytes, Index, 2);
        n = BitConverter.ToInt16(bytes, 0);
    }

    /// <summary>
    /// Reads the long value.
    /// </summary>
    /// <param name="n">The long value.</param>
    private void Read(out long n)
    {
        var bytes = new byte[8];
        this.Reader?.Read(bytes, Index, 8);
        n = BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Reads the byte array.
    /// </summary>
    /// <param name="buffer">The byte array.</param>
    /// <param name="bufferSize">The byte array size.</param>
    private void Read(out byte[] buffer, int bufferSize)
    {
        buffer = new byte[bufferSize];
        this.Reader?.Read(buffer, Index, bufferSize);
    }
}
