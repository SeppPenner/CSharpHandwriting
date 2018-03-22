using System;
using System.IO;

namespace NeuralNetworkLibrary.ArchiveSerialization
{
    /// <summary>
    ///     Class allows reading objects serialize using MFC CArchive
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MfcArchive : Archive
    {
        public MfcArchive(Stream stream, ArchiveOp op)
            : base(stream, op)
        {
            if (op == ArchiveOp.Store)
                throw new NotImplementedException("Writing to MFC compatible serialization is not supported.");
        }

        // ReSharper disable once UnusedMember.Global
        public new void Read(out decimal d)
        {
            // MFC stores decimal as 32-bit status value, 32-bit high value, and 32-bit low value
            // ReSharper disable InlineOutVariableDeclaration
            int status, high;
            // ReSharper disable once InlineOutVariableDeclaration
            uint low;
            Read(out status);
            Read(out high);
            Read(out low);

            if (status != (int) OleCurrencyStatus.Valid)
            {
                d = 0;
            }
            else
            {
                var final = MakeInt64((int) low, high);
                d = decimal.FromOACurrency(final);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public new void Read(out bool b)
        {
            // MFC stores bools as 32-bit "long"
            // ReSharper disable once InlineOutVariableDeclaration
            int l;
            Read(out l);
            b = l != 0;
        }


        // ReSharper disable once UnusedMember.Global
        public new void Read(out DateTime dt)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            uint status;
            Read(out status); // status is a 32-bit "long" in C++

            // MFC stores dates as 8-byte double
            // ReSharper disable once InlineOutVariableDeclaration
            double l;
            Read(out l);
            dt = DateTime.FromOADate(l);

            if (status == (uint) OleDateTimeStatus.Null ||
                status == (uint) OleDateTimeStatus.Invalid)
                dt = DateTime.FromOADate(0.0);
        }

        // ReSharper disable once UnusedMember.Global
        public void Read(out DateTime? dt)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            uint status;
            Read(out status); // status is a 32-bit "long" in C++
            // ReSharper disable once InlineOutVariableDeclaration
            double l;
            Read(out l);
            dt = DateTime.FromOADate(l);

            // read in nullable type
            if (status == (uint) OleDateTimeStatus.Null ||
                status == (uint) OleDateTimeStatus.Invalid)
                dt = null;
        }

        // ReSharper disable once UnusedMember.Global
        public new void Read(out string s)
        {
            s = MfcStringReader.ReadCString(Reader);
        }

        // ReSharper disable once UnusedMember.Global
        public void ReadUnicodeString(out string s)
        {
            s = Reader.ReadString();
        }

        // Convert current low and high to 8-Byte C++ CURRENCY structure 
        private static long MakeInt64(int l1, int l2)
        {
            return (uint) l1 | ((uint) l2 << 32);
        }
    }
}