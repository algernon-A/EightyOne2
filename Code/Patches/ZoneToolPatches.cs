// <copyright file="ZoneToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using UnityEngine;
    using static ZoneManagerPatches;

    /// <summary>
    /// Harmomy patches for the game's zone tool to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(ZoneTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony")]
    internal class ZoneToolPatches
    {
        /// <summary>
        /// Harmony transpiler for ZoneTool.ApplyBrush to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ApplyBrush", new Type[] { }, new ArgumentType[] { })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ApplyBrushTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ZoneTool.ApplyFill to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ApplyFill")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ApplyFillTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ZoneTool.ApplyZoning to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ApplyZoning", new Type[] { }, new ArgumentType[] { })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ApplyZoningTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ZoneTool.CalculateFillBuffer to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CalculateFillBuffer", new Type[] { typeof(Vector3), typeof(Vector3), typeof(ItemClass.Zone), typeof(bool), typeof(bool) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateFillBufferTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);
    }
}
