// <copyright file="GameAreaToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for the game area manager.
    /// </summary>
    [HarmonyPatch(typeof(GameAreaTool))]
    internal static class GameAreaToolPatches
    {
        /// <summary>
        /// Harmony transpiler for GameAreaTool.OnToolGUI to replace calls to GameAreaManager.GetTileXZ.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("OnToolGUI")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> OnToolGUITranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => GameAreaManagerPatches.ReplaceGetTileXZ(instructions, original);
    }
}