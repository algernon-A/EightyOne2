// <copyright file="TsunamiAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
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
    using UnityEngine;
    using static DisasterManagerPatches;

    /// <summary>
    /// Harmony patches for tsunami AIs to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(TsunamiAI), nameof(TsunamiAI.UpdateHazardMap))]
    internal static class TsunamiAIPatches
    {
        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
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
    }
}
