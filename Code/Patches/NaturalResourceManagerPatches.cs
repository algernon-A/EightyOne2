// <copyright file="NaturalResourceManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using HarmonyLib;

    /// <summary>
    /// Harmomy patches for the game's natural resource manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(NaturalResourceManager))]
    internal static class NaturalResourceManagerPatches
    {
        /// <summary>
        /// Harmony transpiler for NaturalResourceManager.GetTileResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(NaturalResourceManager.GetTileResources))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => GameAreaManagerPatches.Replace2with0(instructions);

        /// <summary>
        /// Harmony transpiler for NaturalResourceManager.CalculateUnlockedResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(NaturalResourceManager.CalculateUnlockedResources))]
        [HarmonyTranspiler]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateUnlockedResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => GameAreaManagerPatches.Replace2with0(instructions);

        /// <summary>
        /// Harmony transpiler for NaturalResourceManager.CalculateUnlockableResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(NaturalResourceManager.CalculateUnlockableResources))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateUnlockableResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => GameAreaManagerPatches.Replace2with0(instructions);
    }
}
