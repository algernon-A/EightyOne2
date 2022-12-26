// <copyright file="DisasterAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;
    using static DisasterManagerPatches;

    /// <summary>
    /// Harmony patches for disaster AIs to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch]
    internal static class DisasterAIPatches
    {
        /// <summary>
        /// Target methods.
        /// </summary>
        /// <returns>List of methods to transpile.</returns>
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Need to do this for each disaster type.
            yield return AccessTools.Method(typeof(EarthquakeAI), nameof(EarthquakeAI.UpdateHazardMap));
            yield return AccessTools.Method(typeof(ForestFireAI), nameof(ForestFireAI.UpdateHazardMap));
            yield return AccessTools.Method(typeof(MeteorStrikeAI), nameof(MeteorStrikeAI.UpdateHazardMap));
            yield return AccessTools.Method(typeof(SinkholeAI), nameof(SinkholeAI.UpdateHazardMap));
            yield return AccessTools.Method(typeof(ThunderStormAI), nameof(ThunderStormAI.UpdateHazardMap));
            yield return AccessTools.Method(typeof(TornadoAI), nameof(TornadoAI.UpdateHazardMap));
        }

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDisasterGridResolution))
                {
                    // Grid resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedDisasterGridResolution;
                }
                else if (instruction.LoadsConstant(GameDisasterGridMax))
                {
                    // Maximum iteration value: grid resolution - 1 , i.e. 255 -> 449.
                    instruction.operand = ExpandedDisasterGridMax;
                }
                else if (instruction.LoadsConstant(GameDisasterGridHalfResolution))
                {
                    // Maximum iteration value: grid resolution / 2, i.e. 128f -> 225f.
                    instruction.operand = ExpandedDisasterGridHalfResolution;
                }
                else if (instruction.LoadsConstant(GameDisasterGridResolution - 3))
                {
                    // Resolution - 3, i.e. 253 -> 447.
                    instruction.operand = ExpandedDisasterGridResolution - 3;
                }

                yield return instruction;
            }
        }
    }
}
