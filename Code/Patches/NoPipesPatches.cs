// <copyright file="NoPipesPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmomy patches for the game's water manager to remove the need for water transmission ('no pipes').
    /// </summary>
    [HarmonyPatch(typeof(WaterManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class NoPipesPatches
    {
        // Enabled status.
        private static bool s_noPipesEnabled = false;

        // Centralised electricity pool.
        private static int s_waterPool = 0;

        // Centralised sewage pool (care for a swim?).
        private static int s_sewagePool = 0;

        // Centralised heating pool.
        private static int s_heatingPool = 0;

        // Water pollution level (global).
        private static byte s_waterPollution = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the 'no pipes' functionality is enabled.
        /// </summary>
        internal static bool NoPipesEnabled
        {
            get => s_noPipesEnabled;

            set
            {
                s_noPipesEnabled = value;

                // If game is loaded, we need to update all water nodes attached to water facility buildings, to ensure that 'no pipe connection' messages are displayed/cleared as appropriate.
                if (Loading.IsLoaded)
                {
                    // Watch the threading!
                    Singleton<SimulationManager>.instance.AddAction(() =>
                    {
                        // Local references.
                        Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                        NetNode[] nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

                        // Iterate through all buildings, looking for valid targets.
                        for (ushort i = 0; i < buildings.Length; ++i)
                        {
                            // Extant buildings only.
                            if ((buildings[i].m_flags & Building.Flags.Created) != 0)
                            {
                                // WaterFacilityAIs only.
                                if (buildings[i].Info?.m_buildingAI is WaterFacilityAI)
                                {
                                    // If there's an attached node, update it.
                                    ushort nodeID = buildings[i].m_netNode;
                                    if (nodeID != 0)
                                    {
                                        nodes[nodeID].UpdateNode(nodeID);
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Patches methods to suppress 'no connected segments' check.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> NoSegmentsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            foreach (CodeInstruction instruction in instructions)
            {
                // Replace initial 'false' allocation to flag with custom setting.
                if (!patched && instruction.opcode == OpCodes.Ldc_I4_0)
                {
                    instruction.opcode = OpCodes.Ldsfld;
                    instruction.operand = AccessTools.Field(typeof(NoPipesPatches), nameof(s_noPipesEnabled));
                    patched = true;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.CheckHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="heating">Set to true if heating is available, false otherwise.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.CheckHeating))]
        [HarmonyPrefix]
        private static bool CheckHeatingPrefix(ref bool heating)
        {
            if (s_noPipesEnabled)
            {
                heating = s_heatingPool > 0;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.CheckWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="water">Set to true if water is available, false otherwise.</param>
        /// <param name="sewage">Set to true if sewage is available, false otherwise.</param>
        /// <param name="waterPollution">Current water pollution level.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.CheckWater))]
        [HarmonyPrefix]
        private static bool CheckWaterPrefix(ref bool water, ref bool sewage, ref byte waterPollution)
        {
            if (s_noPipesEnabled)
            {
                water = s_waterPool > 0;
                sewage = s_sewagePool > 0;

                // Return zero for water pollution if no water.
                waterPollution = s_waterPool > 0 ? s_waterPollution : byte.MinValue;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.TryDumpHeating))]
        [HarmonyPrefix]
        private static bool TryDumpHeatingPrefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_heatingPool += __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryDumpSewage),
            new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        private static bool TryDumpSewage1Prefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_sewagePool += __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryDumpSewage),
            new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        private static bool TryDumpSewage2Prefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_sewagePool += __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water dumped to grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <param name="waterPollution">Source water pollution level.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.TryDumpWater))]
        [HarmonyPrefix]
        private static bool TryDumpWaterPrefix(ref int __result, int rate, int max, byte waterPollution)
        {
            if (s_noPipesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_waterPool += __result;

                // TODO: implement pollution scaling.
                s_waterPollution = waterPollution;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal 'fetched' from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <param name="connected">Whether or not the building is considered connected to the heating network (controlls complaint display).</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchHeating))]
        [HarmonyPrefix]
        private static bool TryFetchHeatingPrefix(ref int __result, int rate, int max, ref bool connected)
        {
            if (s_noPipesEnabled)
            {
                __result = Math.Min(Math.Min(rate, max), s_heatingPool);
                s_heatingPool -= __result;

                // Assign connected status (true if we have any current heating capacity).
                connected = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].GetHeatingCapacity() > 0;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal 'fetched' from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchSewage))]
        [HarmonyPrefix]
        private static bool TryFetchSewagePrefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Math.Min(Math.Min(rate, max), s_sewagePool);
                s_sewagePool -= __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water fetched from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryFetchWater),
            new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        private static bool TryFetchWater1Prefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Math.Min(Math.Min(rate, max), s_waterPool);
                s_waterPool -= __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water fetched from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no pipes functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryFetchWater),
            new Type[] { typeof(ushort), typeof(int), typeof(int), typeof(byte) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        private static bool TryFetchWater2Prefix(ref int __result, int rate, int max)
        {
            if (s_noPipesEnabled)
            {
                __result = Math.Min(Math.Min(rate, max), s_waterPool);
                s_waterPool -= __result;

                // Pre-empt original method.
                return false;
            }

            return true;
        }
    }
}