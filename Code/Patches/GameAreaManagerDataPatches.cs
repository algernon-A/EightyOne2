// <copyright file="GameAreaManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using EightyOne2.Serialization;
    using HarmonyLib;
    using static GameAreaManager;
    using static GameAreaManagerPatches;

    /// <summary>
    /// Harmony patches for the game area manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class GameAreaManagerDataPatches
    {
        /// <summary>
        /// Harmony transpiler for GameAreaManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via PostFix as we need the original GameAreaManager instance (Harmony Postfix will only give GameAreaManager.Data instance).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Insert call to our custom method immediately before the end of the target method (final ret).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameAreaManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.Data.Serialize to insert call to custom serialize method at the correct spot.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo beginWrite = AccessTools.Method(typeof(EncodedArray.Byte), nameof(EncodedArray.Byte.BeginWrite));
            MethodInfo endWrite = AccessTools.Method(typeof(EncodedArray.Byte), nameof(EncodedArray.Byte.EndWrite));
            MethodInfo customSerialize = AccessTools.Method(typeof(GameAreaManagerDataPatches), nameof(CustomSerialize));

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
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
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
        /// Performs deserialization activities when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="instance">GameAreaManager instance.</param>
        private static void CustomDeserialize(GameAreaManager instance)
        {
            // See if this save contains any extended 81 tiles data.
            if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(GameAreaDataSerializer.DataID, out byte[] data))
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    Logging.Message("found expanded area data");
                    DataSerializer.Deserialize<GameAreaDataContainer>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
                }
            }
            else
            {
                Logging.Message("no expanded area data found - converting vanilla data");

                // New area grid for 81 tiles.
                int[] newAreaGrid = new int[ExpandedMaxAreaCount];

                // Convert 25-tile data into 81-tile equivalent locations.
                for (int z = 0; z < GameAreaGridResolution; ++z)
                {
                    for (int x = 0; x < GameAreaGridResolution; ++x)
                    {
                        int gridSquare = instance.m_areaGrid[(z * GameAreaGridResolution) + x];
                        newAreaGrid[((z + 2) * ExpandedAreaGridResolution) + x + 2] = gridSquare;
                    }
                }

                // Replace existing fields with 81 tiles replacements.
                instance.m_areaCount = ExpandedMaxAreaCount;
                instance.m_areaGrid = newAreaGrid;
            }
        }

        /// <summary>
        /// Performs deserialization activates when loading game data.
        /// Saves the 25-tile subset of 81-tile data.
        /// </summary>
        /// <param name="encodedArray">Encoded array to write to.</param>
        /// <param name="instance">GameAreaManager instance.</param>
        private static void CustomSerialize(EncodedArray.Byte encodedArray, GameAreaManager instance)
        {
            // Get 25-tile subset of 81-tile data.
            // Serialization is by field at a time.
            for (int z = 0; z < GameAreaGridResolution; ++z)
            {
                for (int x = 0; x < GameAreaGridResolution; ++x)
                {
                    int expandedGridSquare = ((z + 2) * ExpandedAreaGridResolution) + x + 2;
                    encodedArray.Write((byte)instance.m_areaGrid[expandedGridSquare]);
                }
            }
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>DistrictDataContainer type.</returns>
        private static Type LegacyTypeConverter(string legacyTypeName) => typeof(GameAreaDataContainer);
    }
}