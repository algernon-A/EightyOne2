// <copyright file="ImmaterialResourceManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using static ImmaterialResourceManager;
    using static ImmaterialResourceManagerPatches;

    /// <summary>
    /// Harmony patches for the immaterial resource manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class ImmaterialResourceManagerDataPatches
    {
        // Expanded local immaterial resource array size = 900 * 900 = 810000.
        private const int ExpandedImmaterialResourceLocalArraySize = RESOURCE_COUNT * ExpandedImmaterialResourceGridResolution * ExpandedImmaterialResourceGridResolution;

        // Data conversion offset - outer margin of 25-tile data when placed in an 81-tile context.
        private const int CellConversionOffset = (int)ExpandedImmaterialResourceGridHalfResolution - (int)GameImmaterialResourceGridHalfResolution;

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via Postfix as we need the original ImmaterialResourceManager instance (Harmomy Postfix will only give ImmaterialResourceManager.Data instance).
        /// Also need to access original method array references.
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
                    yield return new CodeInstruction(OpCodes.Ldloc, 0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localFinalResources"));
                    yield return new CodeInstruction(OpCodes.Ldloc, 0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ImmaterialResourceManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="m_localFinalResources">ImmaterialResourceManager private array m_localFinalResources.</param>
        /// <param name="m_localTempResources">ImmaterialResourceManager private array m_localTempResources.</param>
        private static void CustomDeserialize(ref ushort[] m_localFinalResources, ref ushort[] m_localTempResources)
        {
            // New area grid for 81 tiles.
            ushort[] newLocalFinalResourcesArray = new ushort[ExpandedImmaterialResourceLocalArraySize];
            ushort[] newLocalTempResourcesArray = new ushort[ExpandedImmaterialResourceLocalArraySize];

            // Convert 25-tile data into 81-tile equivalent locations.
            for (int z = 0; z < GameImmaterialResourceGridResolution; ++z)
            {
                for (int x = 0; x < GameImmaterialResourceGridResolution; ++x)
                {
                    int gameGridIndex = ((z * GameImmaterialResourceGridResolution) + x) * RESOURCE_COUNT;
                    int expandedGridIndex = (((z + CellConversionOffset) * ExpandedImmaterialResourceGridResolution) + x + CellConversionOffset) * RESOURCE_COUNT;

                    // Iterate through all local resources.
                    for (int i = 0; i < RESOURCE_COUNT; ++i)
                    {
                        newLocalFinalResourcesArray[expandedGridIndex + i] = m_localFinalResources[gameGridIndex + i];
                        newLocalTempResourcesArray[expandedGridIndex + i] = m_localTempResources[gameGridIndex + i];
                    }
                }
            }

            // Replace existing array fields with 81 tiles replacements.
            m_localFinalResources = newLocalFinalResourcesArray;
            m_localTempResources = newLocalTempResourcesArray;
        }
    }
}