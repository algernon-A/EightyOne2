// <copyright file="DistrictDataContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Serialization
{
    using ColossalFramework;
    using ColossalFramework.IO;
    using EightyOne2.Patches;
    using static DistrictManager;

    /// <summary>
    /// Savegame data container for expanded district data.
    /// Mirroring the original 81 tiles to make backwards-compatibility easier and because no particular reason to change.
    /// </summary>
    public class DistrictDataContainer : IDataContainer
    {
        /// <summary>
        /// Saves data to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            // Local reference.
            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            // Straightforward array write.
            EncodedArray.Byte encodedArray = EncodedArray.Byte.BeginWrite(serializer);
            WriteDistrictGrid(encodedArray, districtManager.m_districtGrid);
            WriteDistrictGrid(encodedArray, districtManager.m_parkGrid);
            encodedArray.EndWrite();
        }

        /// <summary>
        /// Reads data from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // New expanded grid arrays.
            Cell[] expandedDistrictGrid = new Cell[DistrictManagerPatches.ExpandedDistrictGridArraySize];
            Cell[] expandedParkGrid = new Cell[DistrictManagerPatches.ExpandedDistrictGridArraySize];

            // Read expanded grids from savegame data.
            EncodedArray.Byte encodedArray = EncodedArray.Byte.BeginRead(serializer);
            ReadDistrictGrid(encodedArray, expandedDistrictGrid);

            // Earlier versions of 81 tiles data don't have the park grid; need to initialize it.
            if (serializer.version < 2)
            {
                DistrictManagerDataPatches.InitializeDistrictCellArray(expandedParkGrid);
            }
            else
            {
                ReadDistrictGrid(encodedArray, expandedParkGrid);

                // Legacy data repairer - version 2 of original 81 tiles mod didn't properly initialize grid cell alphas.
                if (serializer.version == 2)
                {
                    RepairCells(expandedDistrictGrid);
                    RepairCells(expandedParkGrid);
                }
            }

            encodedArray.EndRead();

            // Populate district manager with read expanded data.
            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            districtManager.m_districtGrid = expandedDistrictGrid;
            districtManager.m_parkGrid = expandedParkGrid;
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }

        /// <summary>
        /// Reads a game district grid array.
        /// </summary>
        /// <param name="encodedArray">Encoded byte array to write to.</param>
        /// <param name="districtCellArray">District cell array to write.</param>
        private void ReadDistrictGrid(EncodedArray.Byte encodedArray, Cell[] districtCellArray)
        {
            // Mimics game deserialization, as used in original 81 tiles.
            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_district1 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_district2 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_district3 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_district4 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_alpha1 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_alpha2 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_alpha3 = encodedArray.Read();
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                districtCellArray[i].m_alpha4 = encodedArray.Read();
            }
        }

        /// <summary>
        /// Writes a game district grid array.
        /// </summary>
        /// <param name="encodedArray">Encoded byte array to write to.</param>
        /// <param name="districtCellArray">District cell array to write.</param>
        private void WriteDistrictGrid(EncodedArray.Byte encodedArray, Cell[] districtCellArray)
        {
            // Mimics game serialization, as used in original 81 tiles.
            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_district1);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_district2);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_district3);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_district4);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_alpha1);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_alpha2);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_alpha3);
            }

            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                encodedArray.Write(districtCellArray[i].m_alpha4);
            }
        }

        /// <summary>
        /// Repairer for earlier versions of legacy 81 tiles data, where cell alphas weren't properly intialized.
        /// </summary>
        /// <param name="districtCellArray">Cell array to repair.</param>
        private void RepairCells(Cell[] districtCellArray)
        {
            // Iterate through all cells in array.
            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                // If no district is assigned to this cell, reset the alphas to game defaults.
                if (districtCellArray[i].m_district1 == 0)
                {
                    districtCellArray[i].m_alpha1 = byte.MaxValue;
                    districtCellArray[i].m_alpha2 = 0;
                    districtCellArray[i].m_alpha3 = 0;
                    districtCellArray[i].m_alpha4 = 0;
                }
            }
        }
    }
}