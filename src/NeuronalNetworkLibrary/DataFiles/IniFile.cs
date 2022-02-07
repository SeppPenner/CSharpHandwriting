// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IniFile.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The ini file class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary.DataFiles
{
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// The ini file class.
    /// </summary>
    public class IniFile
    {
        /// <summary>
        /// The path.
        /// </summary>
        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public IniFile(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Writes a value to the ini file.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        // ReSharper disable once UnusedMember.Global
        public void IniWriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.path);
        }

        /// <summary>
        /// Reads a value from the ini file.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <returns>The read value.</returns>
        public string IniReadValue(string section, string key)
        {
            var temp = new StringBuilder(255);
            // ReSharper disable once UnusedVariable
            _ = GetPrivateProfileString(section, key, string.Empty, temp, 255, this.path);
            return temp.ToString();
        }

        /// <summary>
        /// Writes the private profile string.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="filePath">The filePath.</param>
        /// <returns>The address of the profile string.</returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        /// <summary>
        /// Gets the profile string.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="definition">The definition</param>
        /// <param name="returnValue">The return value.</param>
        /// <param name="size">The size.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>The profile string.</returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string definition,
            StringBuilder returnValue,
            int size,
            string filePath);
    }
}