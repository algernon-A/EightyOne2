// <copyright file="ZoneManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;
    using static ZoneManager;
    using static ZoneManagerPatches;

    /// <summary>
    /// Harmony patches for the game zone manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class ZoneManagerDataPatches
    {
        /// <summary>
        /// Harmony transpiler for ZoneManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via PostFix as we need the original ZoneManager instance (Harmomy Postfix will only give GameAreaManager.Data instance).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Insert call to our custom method immediately before the storing of the ZoneManager instance in local 0.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZoneManagerDataPatches), nameof(EnsureGridSize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Ensures correct m_zoneGrid array size on commencement of deserialization.
        /// As the zone grid isn't read directly from file, we do this before any data is read (so array will be properly initialized by the game as deserialization occurs).
        /// Required here in case the ZoneManger was activated (another mod called the Singleton) before our patches were applied.
        /// Zoning Adjuster is a case in point where this can happen.
        /// </summary>
        /// <param name="instance">ZoneManager instance.</param>
        private static void EnsureGridSize(ZoneManager instance)
        {
            int gridArraySize = instance.m_zoneGrid.Length;
            if (gridArraySize != ExpandedZoneGridArraySize)
            {
                Logging.Message("m_zoneGrid found with length ", gridArraySize, "; increasing to ", ExpandedZoneGridArraySize);
                instance.m_zoneGrid = new ushort[ExpandedZoneGridArraySize];
            }
        }
    }
}