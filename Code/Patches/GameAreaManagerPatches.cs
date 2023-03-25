// <copyright file="GameAreaManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches for the game area manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(GameAreaManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class GameAreaManagerPatches
    {
        /// <summary>
        /// Game area grid width and height.
        /// </summary>
        internal const int GameAreaGridResolution = GameAreaManager.AREAGRID_RESOLUTION;

        /// <summary>
        /// Expanded area grid width (and height).
        /// </summary>
        internal const int ExpandedAreaGridResolution = 9;

        /// <summary>
        /// Game area maximum count.
        /// </summary>
        internal const int GameGridArea = 25;

        /// <summary>
        /// Expanded area maximum count.
        /// </summary>
        internal const int ExpandedMaxAreaCount = 81;

        /// <summary>
        /// Game area map (whole-map) resolution.
        /// </summary>
        internal const int GameAreaMapResolution = GameAreaManager.AREA_MAP_RESOLUTION;

        /// <summary>
        /// Expanded area map (whole-map) resolution.
        /// </summary>
        internal const int ExpandedAreaMapResolution = 10;

        /// <summary>
        /// Whole-map resolution in grid cells (9 * 1,920 = 17,280).
        /// </summary>
        internal const float ExpandedAreaGridCells = ExpandedAreaGridResolution * AreaGridCellResolution;

        // Half-widths.
        private const float GameGridHalfWidth = GameAreaGridResolution / 2f;
        private const float ExpandedGridHalfWidth = ExpandedAreaGridResolution / 2f;

        // Cell resolution.
        private const float AreaGridCellResolution = GameAreaManager.AREAGRID_CELL_SIZE;
        private const float GameAreaGridCells = GameAreaGridResolution * AreaGridCellResolution;
        private const float GameAreaGridHalfCells = GameGridHalfWidth * AreaGridCellResolution;
        private const float ExpandedAreaGridHalfCells = ExpandedGridHalfWidth * AreaGridCellResolution;

        // Original game area limit.
        private const int GameMaxAreaCount = 9;

        // Ignore game area unlocking progression.
        private static bool s_ignoreUnlocking = false;

        // Restrict building to owned tiles only.
        private static bool s_restrictToOwned = true;

        /// <summary>
        /// Gets or sets a value indicating whether area unlocking progression is ignored.
        /// </summary>
        internal static bool IgnoreUnlocking { get => s_ignoreUnlocking; set => s_ignoreUnlocking = value; }

        /// <summary>
        /// Gets or sets a value indicating whether building outside of owned tiles is permitted.
        /// </summary>
        internal static bool CrossTheLine { get => !s_restrictToOwned; set => s_restrictToOwned = !value; }

        /// <summary>
        /// Gets or sets a value indicating whether forced unlocking is enabled..
        /// </summary>
        internal static bool ForceUnlocking { get; set; }

        /// <summary>
        /// Replication ofGameAreaManager.GetStartTile to implement new area grid size.
        /// Needed as a separate method due to JITter inlining of original method.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance (from original instance call).</param>
        /// <param name="x">Tile x-position.</param>
        /// <param name="z">Tile z-position.</param>
        /// <param name="startTile">Start tile index.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Existing parameters")]
        public static void GetStartTile(GameAreaManager __instance, out int x, out int z, int startTile)
        {
            x = startTile % ExpandedAreaGridResolution;
            z = startTile / ExpandedAreaGridResolution;
        }

        /// <summary>
        /// Replication of GameAreaManager.GetTileIndex to implement new area grid size.
        /// Needed as a separate method due to JITter inlining of original method.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance (from original instance call).</param>
        /// <param name="x">Tile x-position.</param>
        /// <param name="z">Tile z-position.</param>
        /// <returns>Area tile index.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Existing parameters")]
        public static int GetTileIndex(GameAreaManager __instance, int x, int z)
        {
            return (z * ExpandedAreaGridResolution) + x;
        }

        /// <summary>
        /// Replication of GameAreaManager.GetTileXZ that implements new area grid size.
        /// Needed as a separate method due to JITter inlining of original method.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance (from original instance call).</param>
        /// <param name="tile">Tile index.</param>
        /// <param name="x">Tile x-position.</param>
        /// <param name="z">Tile z-position.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Existing parameters")]
        public static void GetTileXZ(GameAreaManager __instance, int tile, out int x, out int z)
        {
            x = tile % ExpandedAreaGridResolution;
            z = tile / ExpandedAreaGridResolution;
        }

        /// <summary>
        /// Replication of GameAreaManager.GetTileXZ that implements new area grid size.
        /// Needed as a separate method due to JITter inlining of original method.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance (from original instance call).</param>
        /// <param name="p">Position.</param>
        /// <param name="x">Tile x-position.</param>
        /// <param name="z">Tile z-position.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Existing parameters")]
        public static void GetTileXZ(GameAreaManager __instance, Vector3 p, out int x, out int z)
        {
            x = Mathf.Clamp(Mathf.FloorToInt((p.x / AreaGridCellResolution) + ExpandedGridHalfWidth), 0, 5);
            z = Mathf.Clamp(Mathf.FloorToInt((p.z / AreaGridCellResolution) + ExpandedGridHalfWidth), 0, 5);
        }

        /// <summary>
        /// Replication of GameAreaManager.IsUnlocked that implements new area grid size.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance.</param>
        /// <param name="x">Area tile x-position.</param>
        /// <param name="z">Area tile z-position.</param>
        /// <returns>True if the area tile is unlocked, false otherwise.</returns>
        public static bool IsUnlocked(GameAreaManager __instance, int x, int z)
        {
            // Bounds check.
            if (x < 0 || z < 0 || x >= ExpandedAreaGridResolution || z >= ExpandedAreaGridResolution)
            {
                return false;
            }

            return __instance.m_areaGrid[(z * ExpandedAreaGridResolution) + x] != 0;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix patch to GameAreaManager.MaxAreaCount to implement new area grid size.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance (from original instance call).</param>
        /// <param name="__result">Method result.</param>
        /// <returns>Always false (pre-empt original game method).</returns>
        [HarmonyPatch(nameof(GameAreaManager.MaxAreaCount), MethodType.Getter)]
        [HarmonyPrefix]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool MaxAreaCountPrefix(GameAreaManager __instance, out int __result)
        {
            // Assign result.
            __instance.m_maxAreaCount = ExpandedMaxAreaCount;
            __result = ExpandedMaxAreaCount;

            // Always pre-empt original method.
            return false;
        }

        /// <summary>
        /// Replaces ldc.i4.2 with ldc.i4.0; used to correct area grid offset counts in code (change +2 to +0).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> Replace2with0(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            // Look for and replace any calls to the target method.
            foreach (CodeInstruction instruction in instructions)
            {
                // Check for ldc.i4.2.
                if (instruction.opcode == OpCodes.Ldc_I4_2)
                {
                    // Convert to ldc.i4.0.
                    instruction.opcode = OpCodes.Ldc_I4_0;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any calls to GameAreaManager.ReplaceGetTileIndex with a call to our custom replacement.
        /// Needed due to JITter inlining of original method.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> ReplaceGetTileIndex(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return ReplaceCall(
                instructions,
                AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileIndex)),
                AccessTools.Method(typeof(GameAreaManagerPatches), nameof(GameAreaManagerPatches.GetTileIndex)),
                original);
        }

        /// <summary>
        /// Replaces any calls to GameAreaManager.GetTileXZ with a call to our custom replacement.
        /// Needed due to JITter inlining of original method.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> ReplaceGetTileXZ(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            return ReplaceCall(
                instructions,
                AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileXZ), new Type[] { typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }),
                AccessTools.Method(typeof(GameAreaManagerPatches), nameof(GameAreaManagerPatches.GetTileXZ), new Type[] { typeof(GameAreaManager), typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }),
                original);
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.Awake to update texture size constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameAreaMapResolution))
                {
                    // Need ldc.i4.s here.
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, ExpandedAreaMapResolution);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.BeginOverlayImpl to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("BeginOverlayImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BeginOverlayImplTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Inverse floating-point constants.
            foreach (CodeInstruction instruction in ReplaceAreaConstants(instructions))
            {
                if (instruction.LoadsConstant(6.510417E-05f))
                {
                    instruction.operand = 1f / (AreaGridCellResolution * ExpandedAreaMapResolution);
                }
                else if (instruction.LoadsConstant(0.4375f))
                {
                    instruction.operand = ExpandedAreaGridResolution / (ExpandedAreaMapResolution * 2f);
                }
                else if (instruction.LoadsConstant(0.125f))
                {
                    instruction.operand = 1f / ExpandedAreaMapResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.CalculateTilePrice to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.CalculateTilePrice), new Type[] { typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateTilePriceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony Prefix for GameAreaManager.CanUnlock to ensure all tiles can be unlocked.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance.</param>
        /// <param name="__result">Original method result.</param>
        /// <param name="x">Area tile x-coordinate.</param>
        /// <param name="z">Area tile y-coordinate.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(GameAreaManager.CanUnlock))]
        [HarmonyPrefix]
        private static bool CanUnlockPrefix(GameAreaManager __instance, out bool __result, int x, int z)
        {
            // Bounds check.
            if (x < 0 || z < 0 || x >= ExpandedAreaGridResolution || z >= ExpandedAreaGridResolution)
            {
                __result = false;

                // Don't execute original method.
                return false;
            }

            // Already unlocked check.
            if (__instance.m_areaGrid[(z * ExpandedAreaGridResolution) + x] != 0)
            {
                __result = false;

                // Don't execute original method.
                return false;
            }

            // Adjacency check.
            __result = IsUnlocked(__instance, x, z - 1) || IsUnlocked(__instance, x - 1, z) || IsUnlocked(__instance, x + 1, z) || IsUnlocked(__instance, x, z + 1);

            // Game checks that are overriden by forced unlocking.
            if (!ForceUnlocking)
            {
                // Milestone unlock check.
                if (!s_ignoreUnlocking && !Singleton<UnlockManager>.instance.Unlocked(__instance.m_areaCount))
                {
                    __result = false;

                    // Don't execute original method.
                    return false;
                }

                // Invoke area wrappers.
                __instance.m_AreasWrapper.OnCanUnlockArea(x, z, ref __result);
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.ClampPoint to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.ClampPoint))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ClampPointTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => Replace2with0(ReplaceAreaConstants(instructions), original);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetAreaBounds to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetAreaBounds))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetAreaBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetAreaIndex to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetAreaIndex))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetAreaIndexTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetAreaPositionSmooth to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetAreaPositionSmooth))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetAreaPositionSmoothTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetFreeBounds to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("GetFreeBounds")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetFreeBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetTileXZ to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetTileXZ), new Type[] { typeof(Vector3), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileXZ2Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in ReplaceAreaConstants(instructions))
            {
                // Change clamping from maximum 4 to maximum 8.
                if (instruction.opcode == OpCodes.Ldc_I4_4)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, ExpandedAreaGridResolution - 1);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.IsUnlocked to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.IsUnlocked))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> IsUnlockedTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.OnLevelLoaded to update code constants and calls to GetStartTile.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("OnLevelLoaded")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> OnLevelLoadedTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            // First update all constants, then iterate though all instructions.
            foreach (CodeInstruction instruction in ReplaceGetStartTile(instructions, original))
            {
                // Replace ldc.i4.2 with ldc.i4.4 (account for offset of 4, not 2, from edge of grid).
                if (instruction.opcode == OpCodes.Ldc_I4_2)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.PointOutOfArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PointOutOfArea1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.PointOutOfArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3), typeof(float) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PointOutOfArea2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.QuadOutOfArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.QuadOutOfArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> QuadOutOfAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony prefix for GameAreaManager.QuadOutOfArea to permit building outside owned tiles.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if building outside owned areas is permitted, true otherwise.</returns>
        [HarmonyPatch(nameof(GameAreaManager.QuadOutOfArea))]
        [HarmonyPrefix]
        private static bool QuadOutOfAreaPrefix(ref bool __result)
        {
            __result = s_restrictToOwned;
            return s_restrictToOwned;
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.SetStartTile to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.SetStartTile), new Type[] { typeof(int), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SetStartTileTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.UpdateAreaMapping to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateAreaMapping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateAreaMappingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(GameAreaGridCells))
                {
                    // Zone cells per half-grid, i.e. 9600 -> 17280.
                    instruction.operand = ExpandedAreaGridCells;
                }
                else if (instruction.LoadsConstant(200))
                {
                    // Center adjustment - change to 30.
                    instruction.operand = 30;
                }
                else if (instruction.LoadsConstant(60))
                {
                    // Camera angle adjustment - increase by 20.
                    instruction.operand = 80;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for GameAreaManager.UpdateAreaTexture due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__instance">GameAreaManager instance.</param>
        /// <param name="___m_areasUpdated">GameAreaManager private field - m_areasUpdated (updated indicator flag).</param>
        /// <param name="___m_highlightAreaIndex">GameAreaManager private field - m_highlightAreaIndex (highlighted area index).</param>
        /// <param name="___m_areaTex">GameAreaManager private field - m_areaTex (area texture).</param>
        /// <param name="___m_startTile">GameAreaManager private field - m_startTile (start tile).</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("UpdateAreaTexture")]
        [HarmonyPrefix]
        private static bool UpdateAreaTexturePrefix(GameAreaManager __instance, ref bool ___m_areasUpdated, int ___m_highlightAreaIndex, Texture2D ___m_areaTex, int ___m_startTile)
        {
            ___m_areasUpdated = false;
            ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((mode & ItemClass.Availability.MapEditor) != 0)
            {
                GetStartTile(__instance, out int startX, out int startZ, ___m_startTile);
                Color color = default;
                for (int z = 0; z <= ExpandedAreaMapResolution; ++z)
                {
                    for (int x = 0; x <= ExpandedAreaMapResolution; ++x)
                    {
                        bool isStart = x == startX && z == startZ;
                        bool withinBounds = !isStart && x >= 0 && z >= 0 && x < ExpandedAreaGridResolution && z < ExpandedAreaGridResolution;
                        color.r = (!isStart) ? 0f : 1f;
                        color.g = (!withinBounds) ? 0f : 1f;
                        color.b = (!withinBounds || ___m_highlightAreaIndex != (z * ExpandedAreaGridResolution) + x) ? 0f : 1f;
                        color.a = 1f;
                        ___m_areaTex.SetPixel(x, z, color);
                    }
                }
            }
            else
            {
                Color color2 = default;
                for (int z = 0; z <= ExpandedAreaMapResolution; ++z)
                {
                    for (int x = 0; x <= ExpandedAreaMapResolution; ++x)
                    {
                        bool isUnlocked = IsUnlocked(__instance, x, z);
                        CanUnlockPrefix(__instance, out bool canUnlock, x, z);
                        color2.r = (!isUnlocked) ? 0f : 1f;
                        color2.g = (!canUnlock) ? 0f : 1f;
                        if (___m_highlightAreaIndex == (z * ExpandedAreaGridResolution) + x)
                        {
                            if (canUnlock)
                            {
                                color2.b = 0.5f;
                            }
                            else if (isUnlocked)
                            {
                                color2.b = 0.5f;
                            }
                            else
                            {
                                color2.b = 0f;
                            }
                        }
                        else
                        {
                            color2.b = 0f;
                        }

                        color2.a = 1f;
                        ___m_areaTex.SetPixel(x, z, color2);
                    }
                }
            }

            ___m_areaTex.Apply(updateMipmaps: false);

            // Never execute original method.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.UpdateData to update code constants and starting tile conversion.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.UpdateData))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Logging.Message("Transpiling GameAreaManager.UpdateData");

            // Need to skip initial mode check to avoid false positives in update codes.
            bool transpiling = false;

            MethodInfo getStartTile = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetStartTile));
            MethodInfo replacementMethod = AccessTools.Method(typeof(GameAreaManagerPatches), nameof(GameAreaManagerPatches.GetStartTile));
            FieldInfo startTileField = AccessTools.Field(typeof(GameAreaManager), "m_startTile");

            // Iterate through all instructions in original method.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Skip 'header'.
                if (!transpiling)
                {
                    // Trigger is first stfld instruction at 0042.
                    if (instruction.opcode == OpCodes.Stfld)
                    {
                        transpiling = true;

                        // At this point also insert a call to our start tile adjustment method, and store the result.
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Ldfld, startTileField);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameAreaManagerPatches), nameof(AdjustStartingTile)));
                        yield return new CodeInstruction(OpCodes.Stfld, startTileField);
                        continue;
                    }
                }
                else
                {
                    // Look for and update any relevant constants.
                    if (instruction.opcode == OpCodes.Ldc_I4_5)
                    {
                        // Grid width, i.e. 5 -> 9.  Need to replace opcode here as well due to larger constant.
                        instruction.opcode = OpCodes.Ldc_I4;
                        instruction.operand = ExpandedAreaGridResolution;
                    }
                    else if (instruction.opcode == OpCodes.Call && instruction.operand == getStartTile)
                    {
                        // Append m_startTile field value to call.
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, startTileField);

                        // Replace target method call.
                        instruction.operand = replacementMethod;
                    }
                    else if (instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 14)
                    {
                        // Skip ldloc.s 14 and following add.
                        instructionEnumerator.MoveNext();
                        continue;
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for GameAreaManager.UnlockArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.UnlockArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UnlockAreaTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => Replace2with0(ReplaceAreaConstants(instructions), original);

        /*
        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetTileIndex to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Patched ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetTileIndex))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileIndex(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);

        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetStartTile to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Patched ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetStartTile))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetStartTileTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);


        /// <summary>
        /// Harmony transpiler for GameAreaManager.GetTileXZ to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Patched ILCode.</returns>
        [HarmonyPatch(nameof(GameAreaManager.GetTileXZ), new Type[] { typeof(int), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileXZ1(IEnumerable<CodeInstruction> instructions) => ReplaceAreaConstants(instructions);
        */

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceAreaConstants(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(GameMaxAreaCount))
                {
                    // Default game maximum, i.e. 9 -> 81.
                    instruction.operand = ExpandedMaxAreaCount;
                }
                else if (instruction.LoadsConstant(GameAreaGridResolution))
                {
                    // Grid width, i.e. 5 -> 9.  Need to replace opcode here as well due to larger constant.
                    instruction.opcode = OpCodes.Ldc_I4_S;
                    instruction.operand = ExpandedAreaGridResolution;
                }
                else if (instruction.LoadsConstant(GameGridHalfWidth))
                {
                    // Grid half-width, i.e. 2.5 -> 4.5.
                    instruction.operand = ExpandedGridHalfWidth;
                }

                /*
                else if (instruction.LoadsConstant(GameAreaGridCells))
                {
                    // Zone cells per half-grid, i.e. 9600 -> 17280.
                    instruction.operand = ExpandedAreaGridCells;
                }
                else if (instruction.LoadsConstant(-GameAreaGridCells))
                {
                    // Zone cells per half-grid, i.e. 9600 -> 17280.
                    instruction.operand = -ExpandedAreaGridCells;
                }
                else if (instruction.LoadsConstant(GameAreaGridHalfCells))
                {
                    // Zone cells per half-grid, i.e. 4800 -> 8640.
                    instruction.operand = ExpandedAreaGridHalfCells;
                }
                else if (instruction.LoadsConstant(-GameAreaGridHalfCells))
                {
                    // Zone cells per half-grid, i.e. 4800 -> 8640.
                    instruction.operand = -ExpandedAreaGridHalfCells;
                }
                */

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any calls to the original method with calls to the target method instead.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="targetMethod">Target method (calls to this will be replaced).</param>
        /// <param name="replacementMethod">Replacment method.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceCall(IEnumerable<CodeInstruction> instructions, MethodInfo targetMethod, MethodInfo replacementMethod, MethodBase original)
        {
            // Look for and replace any calls to the target method.
            foreach (CodeInstruction instruction in instructions)
            {
                // Check for any call or callvirt.
                if ((instruction.opcode == OpCodes.Callvirt | instruction.opcode == OpCodes.Call) && instruction.operand == targetMethod)
                {
                    Logging.Message("replacing call to ", PatcherBase.PrintMethod(targetMethod), " with ", PatcherBase.PrintMethod(replacementMethod), " in method ", PatcherBase.PrintMethod(original));

                    // Ensure opcode is call (calling a static replacement method).
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacementMethod;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any calls to GameAreaManager.ReplaceGetStartTile with a call to our custom replacement.
        /// Needed due to JITter inlining of original method.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceGetStartTile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            MethodInfo targetMethod = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetStartTile));
            MethodInfo replacementMethod = AccessTools.Method(typeof(GameAreaManagerPatches), nameof(GameAreaManagerPatches.GetStartTile));
            FieldInfo startTileField = AccessTools.Field(typeof(GameAreaManager), "m_startTile");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand == targetMethod)
                {
                    Logging.Message("replacing call to ", PatcherBase.PrintMethod(targetMethod), " with ", PatcherBase.PrintMethod(replacementMethod), " in method ", PatcherBase.PrintMethod(original));

                    // Append m_startTile field value to call.
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, startTileField);

                    // Replace target method call.
                    instruction.operand = replacementMethod;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Adjusts the starting tile coordinates from 25 to 81 tile cooredinates.
        /// </summary>
        /// <param name="startTile">Original (25-tile) starting coordinates.</param>
        /// <returns>81 tile coordinates.</returns>
        private static int AdjustStartingTile(int startTile)
        {
            // Convert original tile number to (original) x and z.
            int x = startTile % GameAreaGridResolution;
            int z = startTile / GameAreaGridResolution;

            // Adjust for margin.
            startTile = ((z + 2) * ExpandedAreaGridResolution) + x + 2;
            return startTile;
        }
    }
}
