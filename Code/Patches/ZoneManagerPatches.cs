// <copyright file="ZoneManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmomy patches for the game's zone manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager))]
    internal class ZoneManagerPatches
    {
        // Zone grid resolution.
        private const int GameZoneGridResolution = ZoneManager.ZONEGRID_RESOLUTION;
        private const int ExpandedZoneGridResolution = GameZoneGridResolution * 9 / 5;
        private const int GameZoneGridArraySize = GameZoneGridResolution * GameZoneGridResolution;
        private const int ExpandedZoneGridArraySize = ExpandedZoneGridResolution * ExpandedZoneGridResolution;

        // Derived values.
        private const int GameZoneGridMax = GameZoneGridResolution - 1;
        private const int ExpandedZoneGridMax = ExpandedZoneGridResolution - 1;
        private const float GameZoneGridHalfResolution = GameZoneGridResolution / 2f;
        private const float ExpandedZoneGridHalfResolution = ExpandedZoneGridResolution / 2f;

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> ReplaceZoneConstants(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameZoneGridResolution))
                {
                    // Zone grid resolution, i.e. 150 -> 270.
                    instruction.operand = ExpandedZoneGridResolution;
                    Logging.Message("replaced ", GameZoneGridResolution, " with ", ExpandedZoneGridResolution);
                }
                else if (instruction.LoadsConstant(GameZoneGridMax))
                {
                    // Zone grid resolution limit, i.e. 149 -> 269.
                    instruction.operand = ExpandedZoneGridMax;
                    Logging.Message("replaced ", GameZoneGridMax, " with ", ExpandedZoneGridMax);
                }
                else if (instruction.LoadsConstant(GameZoneGridHalfResolution))
                {
                    // Zone grid half resolution, i.e. 75 -> 135.
                    instruction.operand = ExpandedZoneGridHalfResolution;
                    Logging.Message("replaced ", GameZoneGridHalfResolution, " with ", ExpandedZoneGridHalfResolution);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ZoneManager.Awake to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameZoneGridArraySize))
                {
                    instruction.operand = ExpandedZoneGridArraySize;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ZoneManager.CheckSpace to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ZoneManager.CheckSpace),
            new Type[] { typeof(Vector3), typeof(float), typeof(int), typeof(int), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckSpaceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ZoneManager.InitializeBlock to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("InitializeBlock")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> InitializeBlockTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ZoneManager.ReleaseBlockImplementation to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            "ReleaseBlockImplementation",
            new Type[] { typeof(ushort), typeof(ZoneBlock) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ReleaseBlockImplementationTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ZoneManager.TerrainUpdated to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ZoneManager.TerrainUpdated))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ZoneManager.UpdateBlocks to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ZoneManager.UpdateBlocks))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateBlocksTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);
    }
}
