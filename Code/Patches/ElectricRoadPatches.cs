// <copyright file="ElectricRoadPatches.cs" company="algernon (K. Algernon A. Sheppard)">
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
    /// Harmomy patches for the the base road AI to implement electricity transmission color overlays.
    /// </summary>
    [HarmonyPatch(typeof(RoadBaseAI))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class ElectricRoadPatches
    {
        /// <summary>
        /// Harmony postfix patch to RoadBaseAI.GetColor (segments) to provide electricity color overlays for 'electric roads' functionality.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="data">Network segment data.</param>
        /// <param name="infoMode">Current display infomode.</param>
        [HarmonyPatch(nameof(RoadBaseAI.GetColor), new Type[] { typeof(ushort), typeof(NetSegment), typeof(InfoManager.InfoMode) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        private static void GetColor(ref Color __result, ref NetSegment data, InfoManager.InfoMode infoMode)
        {
            // Only doing this if we're in the electricity info mode and electric roads are enabled.
            if (infoMode == InfoManager.InfoMode.Electricity && ExpandedElectricityManager.ElectricRoadsEnabled)
            {
                // Local references.
                NetNode[] netNodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

                // Copied from game's PowerLineAI, with minor adjustments.
                NetNode.Flags startNodeFlags = netNodes[data.m_startNode].m_flags;
                NetNode.Flags endNodeFlags = netNodes[data.m_endNode].m_flags;
                if ((startNodeFlags & endNodeFlags & NetNode.Flags.Electricity) != 0)
                {
                    // Transmitting power.
                    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    return;
                }

                // Not transmitting power.
                Color inactiveColor = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                inactiveColor.a = 0f;
                __result = inactiveColor;
            }
        }

        /// <summary>
        /// Harmony postfix patch to RoadBaseAI.GetColor (nodes) to provide electricity color overlays for 'electric roads' functionality.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="data">Network node data.</param>
        /// <param name="infoMode">Current display infomode.</param>
        [HarmonyPatch(nameof(RoadBaseAI.GetColor), new Type[] { typeof(ushort), typeof(NetNode), typeof(InfoManager.InfoMode) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPostfix]
        private static void GetColorNode(ref Color __result, ref NetNode data, InfoManager.InfoMode infoMode)
        {
            // Only doing this if we're in the electricity info mode and electric roads are enabled.
            if (infoMode == InfoManager.InfoMode.Electricity && ExpandedElectricityManager.ElectricRoadsEnabled)
            {
                // Copied from game's PowerLineAI.
                if ((data.m_flags & NetNode.Flags.Electricity) != 0)
                {
                    // Transmitting power.
                    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    return;
                }

                // Not transmitting power.
                Color inactiveColor = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                inactiveColor.a = 0f;
                __result = inactiveColor;
            }
        }
    }
}