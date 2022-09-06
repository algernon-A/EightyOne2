// <copyright file="AreasWrapperPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;

    /// <summary>
    /// Harmomy patches for the game's area wrapper interface handler to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(AreasWrapper))]
    internal static class AreasWrapperPatches
    {
        /// <summary>
        /// Harmony transpiler for AreasWrapper.maxAreaCount setter, to effectively disable the setter and enforce the 81 tile maximum.
        /// </summary>
        /// <param name="instructions">Original ILCode (ignored).</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(AreasWrapper.maxAreaCount), MethodType.Setter)]
        [HarmonyTranspiler]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Ignoring original instructions.")]
        private static IEnumerable<CodeInstruction> SetMaxAreaCountTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(AreasWrapper), "m_gameAreaManager"));
            yield return new CodeInstruction(OpCodes.Ldc_I4, GameAreaManagerPatches.ExpandedMaxAreaCount);
            yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(GameAreaManager), nameof(GameAreaManager.m_maxAreaCount)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for AreasWrapper.GetAreaPrice to replace (probably inlined) method calls to GameAreaManager.GetTileIndex.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(AreasWrapper.GetAreaPrice))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetAreaPriceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileIndex(instructions, original);

        /// <summary>
        /// Harmony transpiler for AreasWrapper.UnlockAreaImpl to replace (probably inlined) method calls to GameAreaManager.GetTileIndex.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UnlockAreaImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UnlockAreaImplTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileIndex(instructions, original);
    }
}
