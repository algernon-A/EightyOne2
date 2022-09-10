// <copyright file="NoPipesPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmomy patches for the game's water manager to remove the need for water transmission ('no pipes').
    /// </summary>
    // [HarmonyPatch(typeof(WaterManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class NoPipesPatches
    {
        // Centralised electricity pool.
        private static int s_waterPool = 0;

        // Centralised sewage pool (care for a swim?).
        private static int s_sewagePool = 0;

        // Centralised heating pool.
        private static int s_heatingPool = 0;

        // Water pollution level (global).
        private static byte s_waterPollution = 0;

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.CheckHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="heating">Set to true if heating is available, false otherwise.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.CheckHeating))]
        [HarmonyPrefix]
        public static bool CheckHeatingPrefix(out bool heating)
        {
            heating = s_heatingPool > 0;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.CheckWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="water">Set to true if water is available, false otherwise.</param>
        /// <param name="sewage">Set to true if sewage is available, false otherwise.</param>
        /// <param name="waterPollution">Current water pollution level.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.CheckWater))]
        [HarmonyPrefix]
        public static bool CheckWaterPrefix(out bool water, out bool sewage, out byte waterPollution)
        {
            water = s_waterPool > 0;
            sewage = s_sewagePool > 0;

            // Return zero for water pollution if no water.
            waterPollution = s_waterPool > 0 ? s_waterPollution : byte.MinValue;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.TryDumpHeating))]
        [HarmonyPrefix]
        public static bool TryDumpHeatingPrefix(ref int __result, int rate, int max)
        {
            __result = Mathf.Clamp(rate, 0, max);
            s_heatingPool += __result;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryDumpSewage),
            new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        public static bool TryDumpSewage1Prefix(ref int __result, int rate, int max)
        {
            __result = Mathf.Clamp(rate, 0, max);
            s_sewagePool += __result;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal dumped 'to' grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryDumpSewage),
            new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        public static bool TryDumpSewage2Prefix(ref int __result, int rate, int max)
        {
            __result = Mathf.Clamp(rate, 0, max);
            s_sewagePool += __result;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryDumpWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water dumped to grid).</param>
        /// <param name="rate">Water production rate.</param>
        /// <param name="max">Maximum water production rate.</param>
        /// <param name="waterPollution">Source water pollution level.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.TryDumpWater))]
        [HarmonyPrefix]
        public static bool TryDumpWaterPrefix(ref int __result, int rate, int max, byte waterPollution)
        {
            __result = Mathf.Clamp(rate, 0, max);
            s_waterPool += __result;

            // TODO: implement pollution scaling.
            s_waterPollution = waterPollution;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchHeating to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal 'fetched' from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <param name="connected">Whether or not the building is considered connected to the heating network (controlls complaint display).</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchHeating))]
        [HarmonyPrefix]
        public static bool TryFetchHeatingPrefix(ref int __result, int rate, int max, out bool connected)
        {
            __result = Math.Min(Math.Min(rate, max), s_heatingPool);
            s_heatingPool -= __result;

            // Assign connected status (true if we have any current heating capacity).
            connected = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].GetHeatingCapacity() > 0;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchSewage to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (sewage removal 'fetched' from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchSewage))]
        [HarmonyPrefix]
        public static bool TryFetchSewagePrefix(ref int __result, int rate, int max)
        {
            __result = Math.Min(Math.Min(rate, max), s_sewagePool);
            s_sewagePool -= __result;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water fetched from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryFetchWater),
            new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        public static bool TryFetchWater1Prefix(ref int __result, int rate, int max)
        {
            __result = Math.Min(Math.Min(rate, max), s_waterPool);
            s_waterPool -= __result;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to WaterManager.TryFetchWater to implement 'no pipes' functionality.
        /// </summary>
        /// <param name="__result">Original method result (water fetched from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(
            nameof(WaterManager.TryFetchWater),
            new Type[] { typeof(ushort), typeof(int), typeof(int), typeof(byte) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyPrefix]
        public static bool TryFetchWater2Prefix(ref int __result, int rate, int max)
        {
            __result = Math.Min(Math.Min(rate, max), s_waterPool);
            s_waterPool -= __result;

            // Always pre-empt original method.
            return false;
        }
    }
}