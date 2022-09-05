﻿// <copyright file="ImmaterialResourceManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using HarmonyLib;
    using UnityEngine;
    using static ImmaterialResourceManager;

    /// <summary>
    /// Harmony patches for the immaterial resource manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(ImmaterialResourceManager))]
    internal static class ImmaterialResourceManagerPatches
    {
        /// <summary>
        /// Game immaterial resource grid width and height = 256 (exact 25-tile boundary is 250).
        /// </summary>
        internal const int GameImmaterialResourceGridResolution = RESOURCEGRID_RESOLUTION;

        /// <summary>
        /// Expanded immaterial resource grid width and height = 450 (9-tile width of 17280 divided by grid size of 38.4).
        /// </summary>
        internal const int ExpandedImmaterialResourceGridResolution = 450;

        /// <summary>
        /// Game immaterial resource grid half-resolution = 256 / 2f = 128f.
        /// </summary>
        internal const float GameImmaterialResourceGridHalfResolution = GameImmaterialResourceGridResolution / 2f;

        /// <summary>
        /// Expanded immaterial resource grid half-resolution = 450 / 2f = 225f.
        /// </summary>
        internal const float ExpandedImmaterialResourceGridHalfResolution = ExpandedImmaterialResourceGridResolution / 2f;

        /// <summary>
        /// Game immaterial resource grid maximum bound (length - 1) = 256 - 1 = 255.
        /// </summary>
        private const int GameImmaterialResourceGridMax = GameImmaterialResourceGridResolution - 1;

        /// <summary>
        /// Expanded immaterial resource grid maximum bound (length - 1) = 450 - 1 = 449.
        /// </summary>
        private const int ExpandedImmaterialResourceGridMax = ExpandedImmaterialResourceGridResolution - 1;

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("AddLocalResource")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddLocalResourceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddObstructedResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddObstructedResourceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceLocalImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddParkResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddParkResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddParkResourceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddResource), new Type[] { typeof(Resource), typeof(int), typeof(Vector3), typeof(float) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddResourceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceLocalImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AreaModified to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AreaModified))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AreaModifiedTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.Awake to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool replacing255 = false;

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 255 -> 449.
                    // But, skip first 255, which is Resource.None.
                    if (replacing255)
                    {
                        instruction.operand = ExpandedImmaterialResourceGridMax;
                    }
                    else
                    {
                        replacing255 = true;
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CalculateLocalResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CalculateLocalResources")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Need to avoid false positive with 255 (used as resource rate).
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridResolution, " with ", ExpandedImmaterialResourceGridResolution);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Immaterial resource half-resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridHalfResolution, " with ", ExpandedImmaterialResourceGridHalfResolution);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax - 1))
                {
                    // Maximum iteration value: immaterial resource resolution - 2 , i.e. 254 -> 448.
                    instruction.operand = ExpandedImmaterialResourceGridMax - 1;
                    Logging.Message("replaced ", GameImmaterialResourceGridMax - 1, " with ", ExpandedImmaterialResourceGridMax - 1);
                }

                // TOOD: may need to have custom code for this, ushort[] buffer needs to be uint[] maybe?
                // NO! (yay!) - BUT need to atch 540/1080 bounds - 540* 16 /38.54 = 225, i.e. half-square - CONFIRMED, bounds is for full map so can leave
                // BUT, 64f?  Quarter grid? NO - height (y).
                // Watch 0xFF
                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ImmaterialResourceManager.CheckLocalResource),
            new Type[] { typeof(Resource), typeof(Vector3), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResource1Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ImmaterialResourceManager.CheckLocalResource),
            new Type[] { typeof(Resource), typeof(Vector3), typeof(float), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResource2Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceLocalImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckLocalResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.CheckLocalResources))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.CheckResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckResourceTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

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
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_globalTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_globalFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalTempResourcesMul"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ImmaterialResourceManagerPatches), nameof(SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.UpdateResourceMapping to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateResourceMapping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateResourceMappingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool replacing255 = false;

            // Need to avoid false positive with 255 (used as resource rate).
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridMax))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 255 -> 449.
                    // But, skip first 255, which is Resource.None.
                    if (replacing255)
                    {
                        instruction.operand = ExpandedImmaterialResourceGridMax;
                    }
                    else
                    {
                        replacing255 = true;
                    }
                }
                else if (instruction.LoadsConstant(1f / (RESOURCEGRID_CELL_SIZE * GameImmaterialResourceGridResolution)))
                {
                    // Inverse constant - original is 0.000101725258f, new is 0.00005787037f.
                    instruction.operand = 1f / (RESOURCEGRID_CELL_SIZE * ExpandedImmaterialResourceGridResolution);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.UpdateTexture to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => ReplaceImmaterialResourceConstants(instructions, original);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceImmaterialResourceConstants(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridResolution, " with ", ExpandedImmaterialResourceGridResolution);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 255 -> 449.
                    instruction.operand = ExpandedImmaterialResourceGridMax;
                    Logging.Message("replaced ", GameImmaterialResourceGridMax, " with ", ExpandedImmaterialResourceGridMax);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridHalfResolution, " with ", ExpandedImmaterialResourceGridHalfResolution);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values (version for methods with bounds - 2).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being transpiled.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceLocalImmaterialResourceConstants(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.Message("transpiling ", PatcherBase.PrintMethod(original));

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridResolution, " with ", ExpandedImmaterialResourceGridResolution);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Immaterial resource half-resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                    Logging.Message("replaced ", GameImmaterialResourceGridHalfResolution, " with ", ExpandedImmaterialResourceGridHalfResolution);
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax - 2))
                {
                    // Maximum iteration value: immaterial resource resolution - 3 , i.e. 253 -> 447.
                    instruction.operand = ExpandedImmaterialResourceGridMax - 2;
                    Logging.Message("replaced ", GameImmaterialResourceGridMax, " with ", ExpandedImmaterialResourceGridMax);
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
        /// <param name="instance">ImmagterialResourceManager instance.</param>
        /// <param name="subStep">Simulation sub-step.</param>
        /// <param name="m_localTempResources">ImmagterialResourceManager private array m_localTempResources.</param>
        /// <param name="m_localFinalResources">ImmagterialResourceManager private array m_localFinalResources.</param>
        /// <param name="m_globalTempResources">ImmagterialResourceManager private array m_globalTempResources.</param>
        /// <param name="m_globalFinalResources">ImmagterialResourceManager private array m_globalFinalResources.</param>
        /// <param name="m_totalTempResources">ImmagterialResourceManager private array m_totalTempResources.</param>
        /// <param name="m_totalFinalResources">ImmagterialResourceManager private array m_totalFinalResources.</param>
        /// <param name="m_totalTempResourcesMul">ImmagterialResourceManager private array m_totalTempResourcesMul.</param>
        private static void SimulationStepImpl(
            ImmaterialResourceManager instance,
            int subStep,
            ushort[] m_localTempResources,
            ushort[] m_localFinalResources,
            int[] m_globalTempResources,
            int[] m_globalFinalResources,
            int[] m_totalTempResources,
            int[] m_totalFinalResources,
            long[] m_totalTempResourcesMul)
        {
            // Based on game code.
            if (subStep == 0 || subStep == 1000)
            {
                return;
            }

            // Going to process two rows per frame (compared to one in base game).
            // This does mean that all processing will take place over frames 0 - 225 inclusive,
            // With nothing being done on frames 226-254 (255 is the final step calculations).
            // A bit unbalanced, but given the comparatively low workload here, it wasn't worth getting too fancy.
            uint subFrameIndex = ColossalFramework.Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFF;
            int startZ = (int)subFrameIndex * 2;
            int endZ = startZ + 1;

            // For recording changed areas.
            int minX = -1;
            int maxX = -1;

            // Bounds check to stop processing past frame 225.
            if (endZ < ExpandedImmaterialResourceGridResolution)
            {
                // Iterate through all selected Z rows.
                for (int z = startZ; z <= endZ; ++z)
                {
                    // Iterate through each cell in row.
                    for (int x = 0; x < ExpandedImmaterialResourceGridResolution; ++x)
                    {
                        int gridIndex = ((z * ExpandedImmaterialResourceGridResolution) + x) * RESOURCE_COUNT;
                        if (CalculateLocalResources(x, z, m_localTempResources, m_globalFinalResources, m_localFinalResources, gridIndex))
                        {
                            // Local resource levels have been changed.
                            if (minX == -1)
                            {
                                // Store the starting X value of the changed area if no change has been flagged.
                                minX = x;
                            }

                            // Update the ending X value of the changed area.
                            maxX = x;
                        }

                        // Update temp arrays.
                        int mulIndex = m_localFinalResources[gridIndex + 16];
                        for (int i = 0; i < RESOURCE_COUNT; ++i)
                        {
                            int finalResourceTotal = m_localFinalResources[gridIndex + i];
                            m_totalTempResources[i] += finalResourceTotal;
                            m_totalTempResourcesMul[i] += finalResourceTotal * mulIndex;
                            m_localTempResources[gridIndex + i] = 0;
                        }
                    }
                }
            }

            // Calculate statistics on final frame of step.
            if (subFrameIndex == 255)
            {
                CalculateTotalResources(m_totalTempResources, m_totalTempResourcesMul, m_totalFinalResources);
                StatisticsManager statisticsManager = ColossalFramework.Singleton<StatisticsManager>.instance;
                StatisticBase statisticBase = statisticsManager.Acquire<StatisticArray>(StatisticType.ImmaterialResource);
                for (int i = 0; i < RESOURCE_COUNT; ++i)
                {
                    m_globalFinalResources[i] = m_globalTempResources[i];
                    m_globalTempResources[i] = 0;
                    m_totalTempResources[i] = 0;
                    m_totalTempResourcesMul[i] = 0L;
                    statisticBase.Acquire<StatisticInt32>(i, 29).Set(m_totalFinalResources[i]);
                }
            }

            // If any local resource levels were changed, update the relevant areas.
            if (minX != -1)
            {
                instance.AreaModified(minX, startZ, maxX, endZ);
            }
        }

        /// <summary>
        /// Harmony reverse patch for ImmaterialResouceMananger.CalculateLocalResources to access private method of original instance.
        /// </summary>
        /// <param name="x">ImmaterialResouceMananger grid X-coordinate.</param>
        /// <param name="z">ImmaterialResouceMananger grid Z-coordinate.</param>
        /// <param name="buffer">Local resource buffer.</param>
        /// <param name="global">Global resource buffer.</param>
        /// <param name="target">Target resource buffer.</param>
        /// <param name="index">ImmaterialResouceMananger grid index.</param>
        /// <returns>True if local resources have changed, false otherwise.</returns>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Original)]
        [HarmonyPatch("CalculateLocalResources")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CalculateLocalResources(int x, int z, ushort[] buffer, int[] global, ushort[] target, int index)
        {
            // Transpile original code with our transpiler.
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => CalculateLocalResourcesTranspiler(instructions, original);
            _ = Transpiler(null, AccessTools.Method(typeof(ImmaterialResourceManager), "CalculateLocalResources"));

            string message = "CalculateLocalResources reverse Harmony patch wasn't applied";
            Logging.Error(message, x, z, buffer, global, target, index);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Harmony reverse patch for ImmaterialResouceMananger.CalculateTotalResources to access private method of original instance.
        /// </summary>
        /// <param name="buffer">Resource buffer.</param>
        /// <param name="bufferMul">Buffer multipliers.</param>
        /// <param name="target">Target resource buffer.</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("CalculateTotalResources")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CalculateTotalResources(int[] buffer, long[] bufferMul, int[] target)
        {
            string message = "CalculateTotalResources reverse Harmony patch wasn't applied";
            Logging.Error(message, buffer, bufferMul, target);
            throw new NotImplementedException(message);
        }
    }
}