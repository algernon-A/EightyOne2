// <copyright file="BuildingPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using HarmonyLib;
    using static ZoneManagerPatches;

    /// <summary>
    /// Harmomy patches for the game's building AI to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Building))]
    internal class BuildingPatches
    {
        /// <summary>
        /// Harmony transpiler for Building.CheckZoning to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Building.CheckZoning), new Type[] { typeof(ItemClass.Zone), typeof(ItemClass.Zone), typeof(bool) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckZoningTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceZoneConstants(instructions, original);
    }
}
