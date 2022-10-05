// <copyright file="ServicePointAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static TransferManager;

    /// <summary>
    /// Patches for the game's ServicePointAI to fix the game's inability to properly locate transfers outside of the 25-tile zone.
    /// </summary>
    [HarmonyPatch(typeof(ServicePointAI))]
    internal static class ServicePointAIPatches
    {
        /// <summary>
        /// Harmony transpiler for ServicePointAI.StartTransfer to use building position instead of offer position (to compensate for game inaccuracies with TransferOffer.Position).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ServicePointAI.StartTransfer))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> OfferTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Logging.Message("transpiling ServicePointAI.StartTransfer");

            // Iterate through all instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for call of TransferOffer.GetPosition.
                if (instruction.Calls(AccessTools.PropertyGetter(typeof(TransferOffer), nameof(TransferOffer.Position))))
                {
                    Logging.Message("found call to get TransferOffer.Position");

                    // Replace it with a call to our custom method.
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ServicePointAIPatches), nameof(OfferPosition)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Calculates the accurate position of the given offer, using the offer building position (if available) instead of the offer position itself.
        /// This overcomes the inaccuracies that happen when using TransferOffer.Position, especially outside the 25 tile area.
        /// </summary>
        /// <param name="offer">Transfer offer.</param>
        /// <returns>Building position if available, otherwise the raw offer position.</returns>
        private static Vector3 OfferPosition(ref TransferOffer offer)
        {
            if (offer.Building != 0)
            {
                return Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building].m_position;
            }

            return offer.Position;
        }
    }
}
