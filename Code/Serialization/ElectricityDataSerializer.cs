﻿// <copyright file="ElectricityDataSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Serialization
{
    using System.IO;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Serialization for expanded district data.
    /// </summary>
    public class ElectricityDataSerializer : SerializableDataExtensionBase
    {
        /// <summary>
        /// Legacy 81 tiles data ID.
        /// </summary>
        internal const string DataID = "fakeEM";

        // Data version (last legacy 81 tiles version was 114012u).
        private const uint DataVersion = 114013u;

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise extended electricity data.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new ElectricityDataContainer());

                // Write to savegame.
                serializableDataManager.SaveData(DataID, stream.ToArray());
                Logging.Message("wrote expanded electricity data length ", stream.Length);
            }
        }

        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            // Deserialization is done at ElectricityManager.Deserialize (inserted by transpiler).
        }
    }
}