// <copyright file="GameAreaDataContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Serialization
{
    using ColossalFramework;
    using ColossalFramework.IO;
    using EightyOne2.Patches;

    /// <summary>
    /// Savegame data container for expanded area data.
    /// Mirroring the original 81 tiles to make backwards-compatibility easier and because no particular reason to change.
    /// </summary>
    public sealed class GameAreaDataContainer : IDataContainer
    {
        /// <summary>
        /// Saves data to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            // Local reference (extended grid).
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;

            // Straightforward array write.
            EncodedArray.Byte encodedArray = EncodedArray.Byte.BeginWrite(serializer);
            for (int i = 0; i < areaGrid.Length; ++i)
            {
                encodedArray.Write((byte)areaGrid[i]);
            }

            encodedArray.EndWrite();
        }

        /// <summary>
        /// Reads data from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // Count of unlocked areas.
            int unlockedAreaCount = 0;

            // Read expanded grid from savegame data.
            int[] expandedAreaGrid = new int[GameAreaManagerPatches.ExpandedMaxAreaCount];
            EncodedArray.Byte encodedArray = EncodedArray.Byte.BeginRead(serializer);
            for (int i = 0; i < expandedAreaGrid.Length; ++i)
            {
                expandedAreaGrid[i] = encodedArray.Read();

                // Increment unlocked areas count if this area is unlocked.
                if (expandedAreaGrid[i] > 0)
                {
                    ++unlockedAreaCount;
                }
            }

            encodedArray.EndRead();

            // Populate game area manager with read expanded data.
            GameAreaManager gameAreaManager = Singleton<GameAreaManager>.instance;
            gameAreaManager.m_areaGrid = expandedAreaGrid;
            gameAreaManager.m_areaCount = unlockedAreaCount;
            gameAreaManager.m_maxAreaCount = GameAreaManagerPatches.ExpandedMaxAreaCount;
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}