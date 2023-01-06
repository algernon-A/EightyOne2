// <copyright file="DisasterAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;
    using UnityEngine;
    using static DisasterManagerPatches;
    using static GameAreaManagerPatches;

    /// <summary>
    /// Harmony patches for disaster AIs to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch]
    internal static class DisasterAIPatches
    {
        /// <summary>
        /// Harmony transpiler for DisasterAI.FindRandomTarget to spread disasters evenly across the 81 tile area.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(DisasterAI), nameof(DisasterAI.FindRandomTarget))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameAreaGridResolution))
                {
                    // Grid width, i.e. 5 -> 9.  Need to replace opcode here as well due to larger constant.
                    instruction.opcode = OpCodes.Ldc_I4_S;
                    instruction.operand = ExpandedAreaGridResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for EarthquakeAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(EarthquakeAI), nameof(EarthquakeAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EarthquakeAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ForestFireAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(ForestFireAI), nameof(ForestFireAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ForestFireAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for MeteorStrikeAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(MeteorStrikeAI), nameof(MeteorStrikeAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MeteorStrikeAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for SinkholeAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(SinkholeAI), nameof(SinkholeAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SinkholeAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ThunderStormAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(ThunderStormAI), nameof(ThunderStormAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ThunderStormAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for TornadoAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(TornadoAI), nameof(TornadoAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TornadoAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceHazardMapConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for TsunamiAI.UpdateHazardMap to update the hazard map.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(typeof(TsunamiAI), nameof(TsunamiAI.UpdateHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TsunamiAITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // We're only transpiling the last chunk of code in this method, where it deals with the texture (most of the code is calculating tsunami risk relative to sea level height).
            // The trigger is the call to Mathf.CeilToInt.
            bool transpiling = false;
            MethodInfo ceilToInt = AccessTools.Method(typeof(Mathf), nameof(Mathf.CeilToInt));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for trigger if we haven't already found it.
                if (!transpiling)
                {
                    if (instruction.Calls(ceilToInt))
                    {
                        // Found it.
                        transpiling = true;
                    }
                }
                else
                {
                    // We're past the trigger, so transpile the relevant constants.
                    // Note: we're NOT transpiling 255 here (no relevant use; it's all tsunami risk).
                    if (instruction.LoadsConstant(GameDisasterGridResolution))
                    {
                        // Grid resolution, i.e. 256 -> 450.
                        instruction.operand = ExpandedDisasterGridResolution;
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
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceHazardMapConstants(IEnumerable<CodeInstruction> instructions, MethodBase original)
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
