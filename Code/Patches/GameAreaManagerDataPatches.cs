// <copyright file="GameAreaManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for the game area manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(GameAreaManager.Data))]
    internal static class GameAreaManagerDataPatches
    {
        /// <summary>
        /// Harmony transpiler for GameAreaManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via PostFix as we need the original GameAreaManager instance (Harmomy Postfix will only give GameAreaManager.Data instance).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.Data.Deserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Insert call to our custom method immediately before the end of the target method (final ret).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameAreaManagerDataPatches), nameof(GameAreaManagerDataPatches.CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Peforms deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="instance">GameAreaManager instance.</param>
        private static void CustomDeserialize(GameAreaManager instance)
        {
            // New area grid for 81 tiles.
            int[] newAreaGrid = new int[GameAreaManagerPatches.ExpandedMaxAreaCount];

            // Convert 25-tile data into 81-tile equivalent locations.
            for (int z = 0; z < GameAreaManagerPatches.GameAreaGridResolution; ++z)
            {
                for (int x = 0; x < GameAreaManagerPatches.GameAreaGridResolution; ++x)
                {
                    int gridSquare = instance.m_areaGrid[(z * GameAreaManagerPatches.GameAreaGridResolution) + x];
                    newAreaGrid[((z + 2) * GameAreaManagerPatches.ExpandedAreaGridResolution) + x + 2] = gridSquare;
                }
            }

            // Replace existing fields with 81 tiles replacements.
            instance.m_areaCount = GameAreaManagerPatches.ExpandedMaxAreaCount;
            instance.m_areaGrid = newAreaGrid;
        }
    }
}