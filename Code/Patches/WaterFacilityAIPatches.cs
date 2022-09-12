// <copyright file="WaterFacilityAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
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
    [HarmonyPatch(typeof(WaterFacilityAI))]
    internal class WaterFacilityAIPatches
    {
        /// <summary>
        /// Harmony transpiler for WaterFacilityAI.ProduceGoods to implement segment checking skipping for 'no pipes' functionality.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ProduceGoods")]
        [HarmonyTranspiler]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony")]
        private static IEnumerable<CodeInstruction> ProduceGoodsTranspiler(IEnumerable<CodeInstruction> instructions) => NoPipesPatches.NoSegmentsTranspiler(instructions);
    }
}
