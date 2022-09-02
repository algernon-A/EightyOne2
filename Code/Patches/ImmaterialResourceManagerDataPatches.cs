// <copyright file="ImmaterialResourceManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using ColossalFramework.IO;
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
        /// Harmony transpiler for ImmaterialResourceManager.Data.Serialize to insert call to custom serialize method at the correct spot.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Tag methods.
            MethodInfo beginWrite = AccessTools.Method(typeof(EncodedArray.UShort), nameof(EncodedArray.UShort.BeginWrite));
            MethodInfo endWrite = AccessTools.Method(typeof(EncodedArray.UShort), nameof(EncodedArray.UShort.EndWrite));
            MethodInfo customSerialize = AccessTools.Method(typeof(ImmaterialResourceManagerDataPatches), nameof(CustomSerialize));

            // Transpiling counter.
            int beginWriteCount = 0;

            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Skip first call, which is varsity colors.
                if (instruction.Calls(beginWrite))
                {
                    yield return instruction;

                    // Insert call to custom method, keeping EncodedArray.Byte instance on top of stack.
                    yield return new CodeInstruction(OpCodes.Dup);

                    // First pass is localFinalResources (local 1), second is localTempResources (local 5).
                    if (beginWriteCount++ == 0)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    }

                    yield return new CodeInstruction(OpCodes.Call, customSerialize);

                    // Skip forward until we find the call to EndWrite.
                    do
                    {
                        instructionEnumerator.MoveNext();
                        instruction = instructionEnumerator.Current;
                    }
                    while (!instruction.Calls(endWrite));
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

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Saves the 25-tile subset of 81-tile data.
        /// </summary>
        /// <param name="encodedArray">Encoded array to write to.</param>
        /// <param name="localResourceArray">Local resource array to write.</param>
        private static void CustomSerialize(EncodedArray.UShort encodedArray, ushort[] localResourceArray)
        {
            // Get 25-tile subset of 81-tile data.
            // Serialization is by field at a time.
            for (int z = 0; z < GameImmaterialResourceGridResolution; ++z)
            {
                for (int x = 0; x < GameImmaterialResourceGridResolution; ++x)
                {
                    int expandedGridIndex = (((z + CellConversionOffset) * ExpandedImmaterialResourceGridResolution) + x + CellConversionOffset) * RESOURCE_COUNT;

                    // Iterate through all local resources.
                    for (int i = 0; i < RESOURCE_COUNT; ++i)
                    {
                        encodedArray.Write(localResourceArray[expandedGridIndex + i]);
                    }
                }
            }
        }
    }
}