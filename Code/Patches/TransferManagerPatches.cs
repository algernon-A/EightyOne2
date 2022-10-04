// <copyright file="TransferManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;
    using UnityEngine;
    using static TransferManager;

    /// <summary>
    /// Patches for the game's TransferManager to fix the game's inability to properly locate transfers outside of the 25-tile zone.
    /// </summary>
    [HarmonyPatch(typeof(TransferManager))]
    internal static class TransferManagerPatches
    {
        /// <summary>
        /// Harmony transpiler for TransferManager.AddIncomingOffer to use building position instead of offer position (to compensate for game inaccuracies with TransferOffer.Position).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TransferManager.AddIncomingOffer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddIncomingOfferTranspiler(IEnumerable<CodeInstruction> instructions) => OfferTranspiler(instructions);

        /// <summary>
        /// Harmony transpiler for TransferManager.AddOutgoingOffer to use building position instead of offer position (to compensate for game inaccuracies with TransferOffer.Position).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TransferManager.AddOutgoingOffer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddOutgoingOfferTranspiler(IEnumerable<CodeInstruction> instructions) => OfferTranspiler(instructions);

        /// <summary>
        /// Harmony transpiler for TransferManager methods to use building position for determining park area instead of offer position.
        /// This overcomes the inaccuracies that happen when using TransferOffer.Position, especially outside the 25 tile area.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> OfferTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // State flag.
            bool transpiled = false;

            // Iterate through all instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for intitial 'brfalse' to flag start of modifications.
                if (!transpiled && instruction.opcode == OpCodes.Brfalse)
                {
                    Logging.Message("found Brfalse");
                    transpiled = true;

                    yield return instruction;

                    // Drop `park = Singleton<DistrictManager>.instance.GetPark(offer.Position);`.
                    do
                    {
                        if (!instructionEnumerator.MoveNext())
                        {
                            Logging.Error("Stloc.0 not found");
                            break;
                        }

                        instruction = instructionEnumerator.Current;
                    }
                    while (instruction.opcode != OpCodes.Stloc_0);

                    // Retain `Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;`.
                    do
                    {
                        if (!instructionEnumerator.MoveNext())
                        {

                            Logging.Error("Stloc.2 not found");
                            break;
                        }

                        instruction = instructionEnumerator.Current;
                        yield return instruction;
                    }
                    while (instruction.opcode != OpCodes.Stloc_2);

                    // Insert `park = Singleton<DistrictManager>.instance.GetPark(buffer[offer.Building].m_position;`.
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DistrictManager), nameof(DistrictManager.instance)));
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(TransferOffer), nameof(TransferOffer.Building)));
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(Building));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Building), nameof(Building.m_position)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetPark), new Type[] { typeof(Vector3) }));
                    instruction = new CodeInstruction(OpCodes.Stloc_0);
                }

                yield return instruction;
            }
        }
    }
}
