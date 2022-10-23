// <copyright file="BusAIPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to enable intercity busses 
    /// </summary>
    [HarmonyPatch(typeof(BusAI), "ArriveAtDestination")]
    internal static class BusAIPatch
    {
        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(4800f))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = 8640f;
                }

                yield return instruction;
            }
        }
    }
}
