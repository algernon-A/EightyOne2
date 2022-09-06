// <copyright file="GameAreaDataSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
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
    /// Serialization for expanded game area data.
    /// </summary>
    public class GameAreaDataSerializer : SerializableDataExtensionBase
    {
        // Legacy 81 tiles data ID.
        private const string DataID = "fakeGAM";

        // Data version (last legacy 81 tiles version was 1).
        private const int DataVersion = 2;

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise extended district data..
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new GameAreaDataContainer());

                // Write to savegame.
                serializableDataManager.SaveData(DataID, stream.ToArray());
                Logging.Message("wrote expanded game area data length ", stream.Length);
            }
        }

        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            // Read data from savegame.
            byte[] data = serializableDataManager.LoadData(DataID);

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    // Deserialise extended district data..
                    DataSerializer.Deserialize<GameAreaDataContainer>(stream, DataSerializer.Mode.Memory);
                    Logging.Message("read expanded game area data length ", stream.Length);
                }
            }
            else
            {
                // No data read.
                Logging.Message("no expanded game area data read");
            }
        }
    }
}