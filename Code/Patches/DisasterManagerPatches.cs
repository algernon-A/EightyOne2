// <copyright file="DisasterManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static DisasterManager;

    /// <summary>
    /// Harmony patches for the disaster resource manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(DisasterManager))]
    internal static class DisasterManagerPatches
    {
        /// <summary>
        /// Game evacuation and hazard map grid width and height = 256 (exact 25-tile boundary is 250).
        /// </summary>
        internal const int GameDisasterGridResolution = EVACUATIONMAP_RESOLUTION;

        /// <summary>
        /// Expanded evacuation and hazard map grid width and height = 450 (9-tile width of 17280 divided by grid size of 38.4).
        /// </summary>
        internal const int ExpandedDisasterGridResolution = 450;

        /// <summary>
        /// Game evacuation and hazard mape grid half-resolution = 256 / 2f = 128f.
        /// </summary>
        internal const float GameDisasterGridHalfResolution = GameDisasterGridResolution / 2f;

        /// <summary>
        /// Expanded evacuation and hazard map grid half-resolution = 450 / 2f = 225f.
        /// </summary>
        internal const float ExpandedDisasterGridHalfResolution = ExpandedDisasterGridResolution / 2f;

        /// <summary>
        /// Game evacuation and hazard grid maximum bound (length - 1) = 256 - 1 = 255.
        /// </summary>
        internal const int GameDisasterGridMax = GameDisasterGridResolution - 1;

        /// <summary>
        /// Expanded evacuation and hazard grid maximum bound (length - 1) = 450 - 1 = 449.
        /// </summary>
        internal const int ExpandedDisasterGridMax = ExpandedDisasterGridResolution - 1;

        /// <summary>
        /// Expanded evacuation and hazard grid array size = 450 * 450 = 202500.
        /// </summary>
        internal const int ExpandedDisasterGridArraySize = ExpandedDisasterGridResolution * ExpandedDisasterGridResolution;

        /// <summary>
        /// Harmony transpiler for DisasterManager.AddEvacuationArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DisasterManager.AddEvacuationArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddEvacuationAreaTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceDisasterConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for DisasterManager.Awake to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Logging.Message("transpiling DisasterManager.Awake");

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDisasterGridResolution))
                {
                    // Disaster map resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedDisasterGridResolution;
                }
                else if (instruction.LoadsConstant(GameDisasterGridResolution * GameDisasterGridResolution))
                {
                    // Disaster map resolution squared, i.e. 65536 -> 202500 (m_hazardAmount array).
                    instruction.operand = ExpandedDisasterGridArraySize;
                }
                else if (instruction.LoadsConstant(4096))
                {
                    // Grid area divided by 16.
                    instruction.operand = ExpandedDisasterGridArraySize / 16;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DisasterManager.IsEvacuating to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DisasterManager.IsEvacuating))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> IsEvacuatingTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceDisasterConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for DisasterManager.SampleDisasterHazardMap to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DisasterManager.SampleDisasterHazardMap))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SampleDisasterHazardMapTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            foreach (CodeInstruction instruction in ReplaceDisasterConstants(instructions, original))
            {
                if (instruction.LoadsConstant(1f / GameDisasterGridMax))
                {
                    // Inverse constant - original is 0.003921569f (1 /255).
                    instruction.operand = 1f / ExpandedDisasterGridMax;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.SimulationStepImpl to reframe simulation step processing by calling our custom method.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("SimulationStepImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace with call to our custom method, loading private array fields as arguments.
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DisasterManager), "m_evacuationMap"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisasterManagerPatches), nameof(SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for DisasterManager.UpdateHazardMapping to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateHazardMapping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateHazardMappingTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(1f / (HAZARDMAP_CELL_SIZE * GameDisasterGridResolution)))
                {
                    // Inverse constant - original is 0.000101725258f, new is 0.00005787037f.
                    instruction.operand = 1f / (HAZARDMAP_CELL_SIZE * ExpandedDisasterGridResolution);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DisasterManager.UpdateTexture to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceDisasterConstants(instructions, original);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceDisasterConstants(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDisasterGridResolution))
                {
                    // Grid resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedDisasterGridResolution;
                }
                else if (instruction.LoadsConstant(GameDisasterGridMax))
                {
                    // Maximum iteration value: grid resolution - 1 , i.e. 255 -> 449.
                    instruction.operand = ExpandedDisasterGridMax;
                }
                else if (instruction.LoadsConstant(GameDisasterGridHalfResolution))
                {
                    // Maximum iteration value: grid resolution / 2, i.e. 128f -> 225f.
                    instruction.operand = ExpandedDisasterGridHalfResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Custom SimulationStepImpl code to ensure the larger array is still processed within the simulation frame.
        /// The entire grid needs to be updated every 255 frames.
        /// I tried spreading it out over more, but that just leads to constant fluctuations - the game really does need it all done at once.
        /// And no, changing the statistics calculation framing doesn't change that.
        /// </summary>
        /// <param name="instance">DisasterManager instance.</param>
        /// <param name="subStep">Simulation sub-step.</param>
        /// <param name="m_evacuationMap">DisasterManager private array m_evacuationMap.</param>
        private static void SimulationStepImpl(
            DisasterManager instance,
            int subStep,
            uint[] m_evacuationMap)
        {
            // Based on game code.
            if (subStep != 0 && subStep != 1000)
            {
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                if ((mode & ItemClass.Availability.Game) != 0)
                {
                    int areaCount = Singleton<GameAreaManager>.instance.m_areaCount;
                    float randomDisastersProbability = instance.m_randomDisastersProbability;
                    if (randomDisastersProbability > 0.001f)
                    {
                        randomDisastersProbability *= randomDisastersProbability;
                        randomDisastersProbability *= (float)((areaCount > 1) ? (550 + (areaCount * 50)) : 500);
                        int num = Mathf.Max(1, Mathf.RoundToInt(randomDisastersProbability));
                        if (instance.m_randomDisasterCooldown < 65536 - (49152 * num / 1000))
                        {
                            instance.m_randomDisasterCooldown++;
                        }
                        else
                        {
                            SimulationManager simulationManager = Singleton<SimulationManager>.instance;
                            if (simulationManager.m_randomizer.Int32(67108864u) < num)
                            {
                                if (string.IsNullOrEmpty(simulationManager.m_metaData.m_ScenarioAsset))
                                {
                                    instance.StartRandomDisaster();
                                }

                                instance.m_randomDisasterCooldown = 0;
                            }
                        }
                    }
                }
            }

            /* Start framing change. */

            // Going to process two rows per frame (compared to one in base game).
            // This does mean that all processing will take place over frames 0 - 225 inclusive,
            // With nothing being done on frames 226-254 (255 is the final step calculations).
            // A bit unbalanced, but given the comparatively low workload here, it wasn't worth getting too fancy.
            uint subFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFF;

            if (subStep != 0 && subStep != 1000)
            {
                int startCell = ((int)subFrameIndex << 1) * ExpandedDisasterGridResolution;
                int endCell = ((int)subFrameIndex << 2) * ExpandedDisasterGridResolution;

                // Bounds check, to stop iterating once we've reached the limit.
                if (endCell <= m_evacuationMap.Length)
                {
                    for (int i = startCell; i < endCell; ++i)
                    {
                        m_evacuationMap[i] = (m_evacuationMap[i] & 0x55555555) << 1;
                    }
                }
            }

            /* End framing change. */

            if (subStep != 0)
            {
                int num6 = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFF);
                int num7 = num6;
                int num8 = num6 + 1 - 1;
                for (int j = num7; j <= num8; j++)
                {
                    if (j >= instance.m_disasters.m_size)
                    {
                        continue;
                    }

                    DisasterData.Flags flags = instance.m_disasters.m_buffer[j].m_flags;
                    if ((flags & DisasterData.Flags.Created) != 0)
                    {
                        DisasterInfo info = instance.m_disasters.m_buffer[j].Info;
                        info.m_disasterAI.SimulationStep((ushort)j, ref instance.m_disasters.m_buffer[j]);
                        if ((instance.m_disasters.m_buffer[j].m_flags & (DisasterData.Flags.Finished | DisasterData.Flags.Persistent | DisasterData.Flags.UnReported)) == DisasterData.Flags.Finished)
                        {
                            instance.ReleaseDisaster((ushort)j);
                        }
                    }
                }
            }

            if (subStep <= 1)
            {
                int num9 = (int)(Singleton<SimulationManager>.instance.m_currentTickIndex & 0x3FF);
                int num10 = (num9 * PrefabCollection<DisasterInfo>.PrefabCount()) >> 10;
                int num11 = (((num9 + 1) * PrefabCollection<DisasterInfo>.PrefabCount()) >> 10) - 1;
                for (int k = num10; k <= num11; k++)
                {
                    PrefabCollection<DisasterInfo>.GetPrefab((uint)k)?.CheckUnlocking();
                }
            }
        }
    }
}
