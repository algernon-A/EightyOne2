// <copyright file="GameAreaInfoPanelPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for the game area manager.
    /// </summary>
    [HarmonyPatch(typeof(GameAreaInfoPanel))]
    internal static class GameAreaInfoPanelPatches
    {
        /// <summary>
        /// Harmony transpiler for GameAreaInfoPanel.ShowInternal to replace calls to GameAreaManager.GetTileXZ.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ShowInternal")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> ShowInternalTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileXZ(instructions, original);

        /// <summary>
        /// Harmony transpiler for GameAreaInfoPanel.UpdatePanel to replace calls to GameAreaManager.GetTileXZ.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdatePanel")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> UpdatePanelTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileXZ(instructions, original);
    }
}