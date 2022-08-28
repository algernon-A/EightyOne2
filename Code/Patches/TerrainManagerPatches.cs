// <copyright file="TerrainManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmomy patches for the game's terrain manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(TerrainManager))]
    internal static class TerrainManagerPatches
    {
        /// <summary>
        /// Modified version of TerrainManager.GetTileFlatness to work with 81 tiles.
        /// Avoids inlining in original.
        /// </summary>
        /// <param name="patches">Terrain patch array.</param>
        /// <param name="x">Tile x-coordinate.</param>
        /// <param name="z">Tile y-coordinate.</param>
        /// <returns>Tile flatness.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float GetTileFlatness(TerrainPatch[] patches, int x, int z) => patches[(z * GameAreaManagerPatches.ExpandedAreaGridResolution) + x].m_flatness;

        /// <summary>
        /// Modified version of TerrainManager.GetUnlockableTerrainFlatness to work with 81 tiles.
        /// Avoids inlining in original.
        /// </summary>
        /// <param name="patches">TerrainManager.m_patches array.</param>
        /// <returns>Calculated average terrain flatness.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static float GetUnlockableTerrainFlatness(TerrainPatch[] patches)
        {
            float flatness = 0f;

            // Add flatness of all individual game areas.
            for (int z = 0; z < GameAreaManagerPatches.ExpandedAreaGridResolution; ++z)
            {
                for (int x = 0; x < GameAreaManagerPatches.ExpandedAreaGridResolution; ++x)
                {
                    // Just a copy of GetTileFlatness above.  Not worth a function call, and not worth removing the 'NoInlining' tag.
                    flatness += patches[(z * GameAreaManagerPatches.ExpandedAreaGridResolution) + x].m_flatness;
                }
            }

            // Divide total by number of game areas to get the average.
            return flatness / GameAreaManagerPatches.ExpandedMaxAreaCount;
        }

        /// <summary>
        /// Modified version of TerrainManager.GetSurfaceCell to work with 81 tiles.
        /// </summary>
        /// <param name="terrainManager">TerrainManager instance.</param>
        /// <param name="patches">Terrain patch array.</param>
        /// <param name="x">Cell x-coordinate.</param>
        /// <param name="z">Cell y-coordinate.</param>
        /// <returns>Specified surface cell.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static TerrainManager.SurfaceCell GetSurfaceCell(TerrainManager terrainManager, TerrainPatch[] patches, int x, int z)
        {
            // Calculate patch coordinates and index.
            int patchX = Mathf.Min(x / 480, 8);
            int patchZ = Mathf.Min(z / 480, 8);
            int patchIndex = (patchZ * 9) + patchX;

            // Check detail level.
            int simDetailIndex = patches[patchIndex].m_simDetailIndex;
            if (simDetailIndex != 0)
            {
                // Detail is greater than zero - calculate detail coordinates and index.
                int detailOffset = (simDetailIndex - 1) * 480 * 480;
                int detailX = x - (patchX * 480);
                int detailZ = z - (patchZ * 480);

                // Minimum x or z value - clip result.
                if ((detailX == 0 && patchZ != 0 && patches[patchIndex - 1].m_simDetailIndex == 0) || (detailZ == 0 && patchZ != 0 && patches[patchIndex - 9].m_simDetailIndex == 0))
                {
                    TerrainManager.SurfaceCell result = terrainManager.SampleRawSurface(x * 0.25f, z * 0.25f);
                    result.m_clipped = terrainManager.m_detailSurface[Mathf.Clamp(detailOffset + (detailZ * 480) + detailX, 0, terrainManager.m_detailSurface.Length - 1)].m_clipped;
                    return result;
                }

                // Maximum X or z value - clip result.
                if ((detailX == 479 && patchX != 8 && patches[patchIndex + 1].m_simDetailIndex == 0) || (detailZ == 479 && patchZ != 8 && patches[patchIndex + 9].m_simDetailIndex == 0))
                {
                    TerrainManager.SurfaceCell result2 = terrainManager.SampleRawSurface(x * 0.25f, z * 0.25f);
                    result2.m_clipped = terrainManager.m_detailSurface[Mathf.Clamp(detailOffset + (detailZ * 480) + detailX, 0, terrainManager.m_detailSurface.Length - 1)].m_clipped;
                    return result2;
                }

                // Normal range (not at edge of map).
                return terrainManager.m_detailSurface[Mathf.Clamp(detailOffset + (detailZ * 480) + detailX, 0, terrainManager.m_detailSurface.Length - 1)];
            }

            return terrainManager.SampleRawSurface(x * 0.25f, z * 0.25f);
        }

        /// <summary>
        /// Harmony transpiler for TerrainManager.GetUnlockableTerrainFlatness.
        /// Replaces original code with a call to our custom method (avoids issues with inlining), accessing the private array m_patches.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TerrainManager.GetUnlockableTerrainFlatness))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetUnlockableTerrainFlatnessTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_patches)));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TerrainManagerPatches), nameof(TerrainManagerPatches.GetUnlockableTerrainFlatness)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for TerrainManager.GetTileFlatness.
        /// Replaces original code with a call to our custom method (avoids issues with inlining), accessing the private array m_patches.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TerrainManager.GetTileFlatness))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetTileFlatnessTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_patches)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TerrainManagerPatches), nameof(TerrainManagerPatches.GetTileFlatness)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for TerrainManager.GetSurfaceCell.
        /// Replaces original code with a call to our custom method, accessing the private array m_patches.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TerrainManager.GetSurfaceCell))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetSurfaceCellTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_patches)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TerrainManagerPatches), nameof(TerrainManagerPatches.GetSurfaceCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
