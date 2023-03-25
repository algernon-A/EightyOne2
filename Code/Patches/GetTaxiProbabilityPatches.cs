// <copyright file="GetTaxiProbabilityPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to prevent residents and tourists calling taxis from the map edge.
    /// </summary>
    [HarmonyPatch]
    public static class GetTaxiProbabilityPatches
    {
        // Edge-of-map margins.
        private const float MapEdgeMarginMin = 40f;
        private const float MapEdgeMarginMax = GameAreaManagerPatches.ExpandedAreaGridCells - MapEdgeMarginMin;

        /// <summary>
        /// Custom replacement for GameAreaManager.PointOutOfArea to exclude a margin at the map edge.
        /// To be used in specific places where required (e.g. to ensure that citizens at the map edge don't call for taxis).
        /// </summary>
        /// <param name="location">Location to check.</param>
        /// <returns>True if the given location is near the map edge, false otherwise.</returns>
        public static bool NearMapEdge(Vector3 location) => location.x < MapEdgeMarginMin | location.x > MapEdgeMarginMax | location.y < MapEdgeMarginMin | location.y > MapEdgeMarginMax;

        /// <summary>
        /// Determines list of target methods to patch - in this case, BuildingWorldInfoPanel method OnSetTarget.
        /// </summary>
        /// <returns>List of target methods to patch.</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ResidentAI), "GetTaxiProbability");
            yield return AccessTools.Method(typeof(TouristAI), "GetTaxiProbability");
        }

        /// <summary>
        /// Replaces calls to Singleton[GameAreaManager].instance.PointOutOfArea with a call to our custom replacement NearMapEdge.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            // Want to drop the GameAreaManager singleton call.
            MethodInfo singletonInstance = AccessTools.DeclaredPropertyGetter(typeof(Singleton<GameAreaManager>), nameof(Singleton<GameAreaManager>.instance));
            MethodInfo targetMethod = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3) });
            MethodInfo replacementMethod = AccessTools.Method(typeof(GetTaxiProbabilityPatches), nameof(NearMapEdge));

            // Look for and replace any calls to the target method.
            foreach (CodeInstruction instruction in instructions)
            {
                // Check for any call or callvirt.
                if (instruction.Calls(singletonInstance))
                {
                    Logging.Message("dropping call to Singleton<GameAreaManager>.instance in method ", PatcherBase.PrintMethod(original));

                    // Just drop this.
                    continue;
                }
                else if (instruction.Calls(targetMethod))
                {
                    Logging.Message("replacing call to GameAreaManager.PointOutOfArea in method ", PatcherBase.PrintMethod(original));
                    yield return new CodeInstruction(OpCodes.Call, replacementMethod);
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
