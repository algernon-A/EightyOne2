// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.XML;
    using ColossalFramework.IO;
    using EightyOne2.Patches;

    /// <summary>
    /// The mod's XML settings file.
    /// </summary>
    [XmlRoot("EightyOneTiles")]
    public class ModSettings : SettingsXMLBase
    {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(DataLocation.localApplicationData, "EightyOneTiles.xml");

        /// <summary>
        /// Gets or sets a value indicating whether area unlocking progression is ignored.
        /// </summary>
        [XmlElement("IgnoreUnlocking")]
        public bool XMLIgnoreUnlocking { get => GameAreaManagerPatches.IgnoreUnlocking; set => GameAreaManagerPatches.IgnoreUnlocking = value; }

        /// <summary>
        /// Gets or sets a value indicating whether building outside of owned tiles is permitted.
        /// </summary>
        [XmlElement("CrossTheLine")]
        public bool XMLCrossTheLine { get => GameAreaManagerPatches.CrossTheLine; set => GameAreaManagerPatches.CrossTheLine = value; }

        /// <summary>
        /// Gets or sets a value indicating whether 'no powerlines' functionality is enabled.
        /// </summary>
        [XmlElement("NoPowerlines")]
        public bool XMLNoPowerlines { get => NoPowerlinesPatches.NoPowerlinesEnabled; set => NoPowerlinesPatches.NoPowerlinesEnabled = value; }

        /// <summary>
        /// Gets or sets a value indicating whether 'electric roads' functionality is enabled.
        /// </summary>
        [XmlElement("ElectricRoads")]
        public bool XMLElectricRoads { get => ExpandedElectricityManager.ElectricRoadsEnabled; set => ExpandedElectricityManager.ElectricRoadsEnabled = value; }

        /// <summary>
        /// Gets or sets a value indicating whether 'no powerlines' functionality is enabled.
        /// </summary>
        [XmlElement("NoPipes")]
        public bool XMLNoPipes { get => NoPipesPatches.NoPipesEnabled; set => NoPipesPatches.NoPipesEnabled = value; }

        /// <summary>
        /// Gets or sets a value indicating whether expanded data should be ignored on next load.
        /// </summary>
        [XmlElement("IgnoreExpanded")]
        public bool XMLIgnoreExpanded { get => IgnoreExpanded; set => IgnoreExpanded = value; }

        /// <summary>
        /// Gets or sets a value indicating whether expanded data should be ignored on next load.
        /// </summary>
        internal static bool IgnoreExpanded { get; set; } = false;

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);
    }
}
