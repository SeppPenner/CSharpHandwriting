// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MfcStringReader.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The MFC string reader class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.ArchiveSerialization
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// The MFC string reader class.
    /// </summary>
    public static class MfcStringReader
    {
        /// <summary>
        /// Reads a C string.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The C <see cref="string"/>.</returns>
        public static string ReadCString(BinaryReader reader)
        {
            // If we get ANSI, convert
            var convert = 1; 

            var length = ReadStringLength(reader);

            if (length == unchecked((uint)-1))
            {
                convert = 1 - convert;
                length = ReadStringLength(reader);

                if (length == unchecked((uint)-1))
                {
                    return string.Empty;
                }
            }

            // Set length of string to new length
            var byteLength = length;

            // Bytes to read
            byteLength += (uint)(byteLength * (1 - convert));

            // Read in the characters
            if (length == 0)
            {
                return string.Empty;
            }

            // Read new data
            var byteBuf = reader.ReadBytes((int)byteLength);

            // Convert the data if as necessary
            var sb = new StringBuilder();
            if (convert != 0)
            {
                for (var i = 0; i < length; i++)
                {
                    sb.Append((char)byteBuf[i]);
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    sb.Append((char)(byteBuf[i * 2] + (byteBuf[(i * 2) + 1] * 256)));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reads the string length.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The string length as <see cref="uint"/>.</returns>
        private static uint ReadStringLength(BinaryReader reader)
        {
            // Attempt byte length first
            var byteLength = reader.ReadByte();

            if (byteLength < 0xff)
            {
                return byteLength;
            }

            // Attempt WORD length
            var wordLength = reader.ReadUInt16();

            if (wordLength == 0xfffe)
            {
                return unchecked((uint)-1);
            }

            // Read DWORD of length
            return wordLength != 0xffff ? wordLength : reader.ReadUInt32();
        }
    }
}