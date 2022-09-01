// <copyright file="ImmaterialResourceManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
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
        private const int GameImmaterialResourceGridResolution = RESOURCEGRID_RESOLUTION;

        /// <summary>
        /// Expanded immaterial resource grid width and height = 450 (9-tile width of 17280 divided by grid size of 38.4).
        /// </summary>
        private const int ExpandedImmaterialResourceGridResolution = 450;

        /// <summary>
        /// Game district grid maximum bound (length - 1) = 256 - 1 = 255.
        /// </summary>
        private const int GameImmaterialResourceGridMax = GameImmaterialResourceGridResolution - 1;

        /// <summary>
        /// Expanded district grid maximum bound (length - 1) = 450 - 1 = 449.
        /// </summary>
        private const int ExpandedImmaterialResourceGridMax = ExpandedImmaterialResourceGridResolution - 1;

        // Derived constants.
        private const float GameImmaterialResourceGridHalfResolution = GameImmaterialResourceGridResolution / 2f;
        private const float ExpandedImmaterialResourceGridHalfResolution = ExpandedImmaterialResourceGridResolution / 2f;

        // Simulation step counter.
        private static int s_simulationStep = 0;

        /// <summary>
        /// Gets the current simulation step counter, resetting it to zero if needed.
        /// </summary>
        private static int SimulationStepCounter
        {
            get
            {
                if (s_simulationStep > ExpandedImmaterialResourceGridMax)
                {
                    s_simulationStep = 0;
                }

                return s_simulationStep++;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("AddLocalResource")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddLocalResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddObstructedResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddObstructedResourceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            while (instructionsEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionsEnumerator.Current;

                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 255 -> 449.
                    instruction.operand = ExpandedImmaterialResourceGridMax;
                    yield return instruction;

                    // Check for any '& 0xFF' that need to be converted.
                    instructionsEnumerator.MoveNext();
                    instruction = instructionsEnumerator.Current;
                    if (instruction.opcode == OpCodes.And)
                    {
                        instruction.opcode = OpCodes.Rem;
                    }
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Immaterial resource half-resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax - 2))
                {
                    // Maximum iteration value: immaterial resource resolution - 3 , i.e. 253 -> 447.
                    instruction.operand = ExpandedImmaterialResourceGridMax - 2;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddParkResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddParkResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddParkResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AddResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AddResource), new Type[] { typeof(Resource), typeof(int), typeof(Vector3), typeof(float) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AddResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceLocalImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.AreaModified to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.AreaModified))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AreaModifiedTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

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

            // Array sizes.
            const int gameArraySize = RESOURCE_COUNT * GameImmaterialResourceGridResolution * GameImmaterialResourceGridResolution;
            const int expandedArraySize = RESOURCE_COUNT * ExpandedImmaterialResourceGridResolution * ExpandedImmaterialResourceGridResolution;

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
                else if (instruction.LoadsConstant(gameArraySize))
                {
                    // Resource array size. i.e. 1900544 -> 5872500
                    instruction.operand = expandedArraySize;
                }

                yield return instruction;
            }
        }
        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CalculateLocalResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CalculateLocalResources")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Need to avoid false positive with 255 (used as resource rate).
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Immaterial resource half-resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax - 1))
                {
                    // Maximum iteration value: immaterial resource resolution - 2 , i.e. 254 -> 448.
                    instruction.operand = ExpandedImmaterialResourceGridMax - 1;
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
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ImmaterialResourceManager.CheckLocalResource),
            new Type[] { typeof(Resource), typeof(Vector3), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResource1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckLocalResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ImmaterialResourceManager.CheckLocalResource),
            new Type[] { typeof(Resource), typeof(Vector3), typeof(float), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResource2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceLocalImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckLocalResources to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.CheckLocalResources))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.CheckResource to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ImmaterialResourceManager.CheckResource))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ImmaterialResourceManager.SimulationStepImpl to reframe simulation step processing.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("SimulationStepImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace 256, and override num2 (z), num3 (min x), num 4 (max x)

            // Look for and update any relevant constants.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            while (instructionsEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionsEnumerator.Current;

                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                }
                else if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return instruction;

                    // Insert custom code here.
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ImmaterialResourceManagerPatches), nameof(SimulationStepCounter)));
                    yield return new CodeInstruction(OpCodes.Stloc_2);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, ExpandedImmaterialResourceGridMax);

                    // Then skip everything until next stloc.s (which will be stloc.s 4).
                    do
                    {
                        instructionsEnumerator.MoveNext();
                        instruction = instructionsEnumerator.Current;
                    }
                    while (instruction.opcode != OpCodes.Stloc_S);
                }

                yield return instruction;
            }
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
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceImmaterialResourceConstants(instructions);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceImmaterialResourceConstants(IEnumerable<CodeInstruction> instructions)
        {
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
                    instruction.operand = ExpandedImmaterialResourceGridMax;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Maximum iteration value: immaterial resource resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values (version for methods with bounds - 2).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceLocalImmaterialResourceConstants(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameImmaterialResourceGridResolution))
                {
                    // Immaterial resource resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedImmaterialResourceGridResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridHalfResolution))
                {
                    // Immaterial resource half-resolution - 1 , i.e. 128f -> 225f.
                    instruction.operand = ExpandedImmaterialResourceGridHalfResolution;
                }
                else if (instruction.LoadsConstant(GameImmaterialResourceGridMax - 2))
                {
                    // Maximum iteration value: immaterial resource resolution - 3 , i.e. 253 -> 447.
                    instruction.operand = ExpandedImmaterialResourceGridMax - 2;
                }

                yield return instruction;
            }
        }
    }
}
