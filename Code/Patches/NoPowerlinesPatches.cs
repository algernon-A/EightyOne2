// <copyright file="NoPowerlinesPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmomy patches for the game's electricity manager to remove the need for electricity transmission ('no powerlines').
    /// </summary>
    [HarmonyPatch(typeof(ElectricityManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class NoPowerlinesPatches
    {
        // Enabled status.
        private static bool s_noPowerlinesEnabled = false;

        // Centralised electricity pool.
        private static int s_electricityPool = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the 'no powerlines' functionality is enabled.
        /// </summary>
        internal static bool NoPowerlinesEnabled { get => s_noPowerlinesEnabled; set => s_noPowerlinesEnabled = value; }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to ElectricityManager.CheckElectricity to implement 'no powerlines' functionality.
        /// </summary>
        /// <param name="electricity">Set to true if electricity is available, false otherwise.</param>
        /// <returns>False (pre-empt original game method) if no powerlines functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(ElectricityManager.CheckElectricity))]
        private static bool CheckElectricityPrefix(ref bool electricity)
        {
            if (s_noPowerlinesEnabled)
            {
                electricity = s_electricityPool > 0;

                // Always pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to ElectricityManager.TryDumpElectricity to implement 'no powerlines' functionality.
        /// </summary>
        /// <param name="__result">Original method result (electricity dumped to grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no powerlines functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryDumpElectricity), new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        private static bool TryDumpElectricity1Prefix(ref int __result, int rate, int max)
        {
            if (s_noPowerlinesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_electricityPool += __result;

                // Always pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to ElectricityManager.TryDumpElectricity to implement 'no powerlines' functionality.
        /// </summary>
        /// <param name="__result">Original method result (electricity dumped to grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no powerlines functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryDumpElectricity), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        private static bool TryDumpElectricity2Prefix(ref int __result, int rate, int max)
        {
            if (s_noPowerlinesEnabled)
            {
                __result = Mathf.Clamp(rate, 0, max);
                s_electricityPool += __result;

                // Always pre-empt original method.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to ElectricityManager.TryFetchElectricity to implement 'no powerlines' functionality.
        /// </summary>
        /// <param name="__result">Original method result (electricity fetched from grid).</param>
        /// <param name="rate">Electricity production rate.</param>
        /// <param name="max">Maximum electricity production rate.</param>
        /// <returns>False (pre-empt original game method) if no powerlines functionality is enabled, true (continue execution) otherwise.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryFetchElectricity))]
        [HarmonyPrefix]
        private static bool TryFetchElectricityPrefix(ref int __result, int rate, int max)
        {
            if (s_noPowerlinesEnabled)
            {
                __result = Mathf.Clamp(Math.Min(rate, max), 0, s_electricityPool);
                s_electricityPool -= __result;

                // Always pre-empt original method.
                return false;
            }

            return true;
        }
    }
}