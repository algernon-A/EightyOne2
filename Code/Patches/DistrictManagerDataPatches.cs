// <copyright file="DistrictManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using static DistrictManager;
    using static DistrictManagerPatches;

    /// <summary>
    /// Harmony patches for the district manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class DistrictManagerDataPatches
    {
        // Data conversion offset - outer margin of 25-tile data when placed in an 81-tile context.
        private const int CellConversionOffset = (int)ExpandedDistrictGridHalfResolution - (int)GameDistrictGridHalfResolution;

        /// <summary>
        /// Harmony transpiler for DistrictManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via Postfix as we need the original GameAreaManager instance (Harmomy Postfix will only give GameAreaManager.Data instance).
        /// Also need private GameAreaManager.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    // Insert call to our custom post-deserialize method immediately before the end of the target method (final ret).
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DistrictManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Peforms deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="instance">DistrictManager instance.</param>
        private static void CustomDeserialize(DistrictManager instance)
        {
            // New area grid for 81 tiles.
            Cell[] newDistrictGrid = new Cell[ExpandedDistrictGridArraySize];
            Cell[] newParkGrid = new Cell[ExpandedDistrictGridArraySize];

            // Populate arrays.
            // Yes, this does mean that we'll end up overwriting ~32% of this,
            // but reliability is more important than shaving off a couple of milliseconds on a first-time load.
            for (int i = 0; i < newDistrictGrid.Length; ++i)
            {
                // Initial values per DistrictManager.Awake().
                newDistrictGrid[i] = new Cell
                {
                    m_district1 = 0,
                    m_district2 = 1,
                    m_district3 = 2,
                    m_district4 = 3,
                    m_alpha1 = byte.MaxValue,
                    m_alpha2 = 0,
                    m_alpha3 = 0,
                    m_alpha4 = 0,
                };

                // Initial values per DistrictManager.Awake().
                newParkGrid[i] = new Cell
                {
                    m_district1 = 0,
                    m_district2 = 1,
                    m_district3 = 2,
                    m_district4 = 3,
                    m_alpha1 = byte.MaxValue,
                    m_alpha2 = 0,
                    m_alpha3 = 0,
                    m_alpha4 = 0,
                };
            }

            // Convert 25-tile data into 81-tile equivalent locations.
            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int gameGridIndex = (z * GameDistrictGridResolution) + x;
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;

                    Cell districtGridCell = instance.m_districtGrid[gameGridIndex];
                    newDistrictGrid[expandedGridIndex] = districtGridCell;

                    Cell parkGridCell = instance.m_parkGrid[gameGridIndex];
                    newParkGrid[expandedGridIndex] = parkGridCell;
                }
            }

            // Replace existing fields with 81 tiles replacements.
            instance.m_districtGrid = newDistrictGrid;
            instance.m_parkGrid = newParkGrid;
        }
    }
}