// <copyright file="WaterPipeAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using HarmonyLib;

    /// <summary>
    /// Harmomy patches for the game's water pipe AI to suppress pipe connection problems when using 'no pipes' functionality..
    /// </summary>
    [HarmonyPatch(typeof(WaterPipeAI))]
    internal class WaterPipeAIPatches
    {
        /// <summary>
        /// Harmony transpiler for WaterPipeAI.UpdateNode to implement segment checking skipping for 'no pipes' functionality.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterPipeAI.UpdateNode))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateNodeTranspiler(IEnumerable<CodeInstruction> instructions) => NoPipesPatches.NoSegmentsTranspiler(instructions);
    }
}
