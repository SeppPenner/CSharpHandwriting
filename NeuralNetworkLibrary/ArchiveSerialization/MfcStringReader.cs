using System.IO;
using System.Text;

namespace NeuralNetworkLibrary.ArchiveSerialization
{
    public static class MfcStringReader
    {
        public static string ReadCString(BinaryReader reader)
        {
            var str = "";
            var nConvert = 1; // if we get ANSI, convert

            var nNewLen = ReadStringLength(reader);
            if (nNewLen == unchecked((uint) -1))
            {
                nConvert = 1 - nConvert;
                nNewLen = ReadStringLength(reader);
                if (nNewLen == unchecked((uint) -1))
                    return str;
            }

            // set length of string to new length
            var nByteLen = nNewLen;
            nByteLen += (uint) (nByteLen * (1 - nConvert)); // bytes to read

            // read in the characters
            if (nNewLen == 0) return str;
            // read new data
            var byteBuf = reader.ReadBytes((int) nByteLen);

            // convert the data if as necessary
            var sb = new StringBuilder();
            if (nConvert != 0)
                for (var i = 0; i < nNewLen; i++)
                    sb.Append((char) byteBuf[i]);
            else
                for (var i = 0; i < nNewLen; i++)
                    sb.Append((char) (byteBuf[i * 2] + byteBuf[i * 2 + 1] * 256));

            str = sb.ToString();

            return str;
        }

        private static uint ReadStringLength(BinaryReader reader)
        {
            // attempt BYTE length first
            var bLen = reader.ReadByte();

            if (bLen < 0xff)
                return bLen;

            // attempt WORD length
            var wLen = reader.ReadUInt16();
            if (wLen == 0xfffe)
                return unchecked((uint) -1);
            if (wLen != 0xffff) return wLen;
            // read DWORD of length
            var nNewLen = reader.ReadUInt32();
            return nNewLen;
        }
    }
}