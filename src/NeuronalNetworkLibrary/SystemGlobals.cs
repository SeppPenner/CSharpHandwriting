// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemGlobals.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class for global constant definitions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NeuronalNetworkLibrary
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A class for global constant definitions.
    /// </summary>
    public static class SystemGlobals
    {
        /// <summary>
        /// The PI constant.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const double Pi = 3.14159;

        /// <summary>
        /// The speed of light constant.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const int SpeedOfLight = 300000; // km per sec.

        /// <summary>
        /// The image size constant.
        /// </summary>
        public const int ImageSize = 28;

        /// <summary>
        /// The vector size constant.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const int VectorSize = 29;

        /// <summary>
        /// The random maximum.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const uint RandomMaximum = 0x7fff;

        /// <summary>
        /// The ulong maximum.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public const ulong UlongMaximum = 0xffffffff;

        /// <summary>
        /// The integer maximum.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public const int IntegerMaximum = 0x7fffffff;

        /// <summary>
        /// The Gaussian field size constant.
        /// </summary>
        public const int GaussianFieldSize = 21;
    }
}