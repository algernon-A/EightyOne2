// <copyright file="NetManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using HarmonyLib;

    /// <summary>
    /// Harmomy patches for the game's net manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(NetManager))]
    internal class NetManagerPatches
    {
        /// <summary>
        /// Harmony transpiler for NetManager.Awake to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Need to make sure m_tileNodesCount is initialized using a larger array for 81 tiles.
            // Looking for the hardcoded constant 975, being 39 * 25 (original game area grid count), and replacing it with 39 * 81 (new area grid count).
            const int OriginalValue = 39 * GameAreaManagerPatches.GameGridArea;
            const int NewValue = 39 * GameAreaManagerPatches.ExpandedMaxAreaCount;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(OriginalValue))
                {
                    instruction.operand = NewValue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for NetManager.GetTileNodeCount to replace calls to GameAreaManager.GetTileXZ.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(NetManager.GetTileNodeCount))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileNodeCount(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileXZ(instructions, original);
    }
}
