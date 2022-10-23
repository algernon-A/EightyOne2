// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : PatcherMod<OptionsPanel, Patcher>, IUserMod
    {
        /// <summary>
        /// Gets the mod's base display name (name only).
        /// </summary>
        public override string BaseName => "81 tiles 2";

        /// <summary>
        /// Gets the mod's unique Harmony identfier.
        /// </summary>
        public override string HarmonyID => "com.github.algernon-A.csl.eightyone2";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => Translations.Translate("MOD_DESCRIPTION");

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings() => ModSettings.Save();

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings() => ModSettings.Load();

        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            base.OnEnabled();

            // Disable the map editor button.
            // First, check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - disable the button now.
                DisableMapEditorButton();
            }
            else
            {
                // Otherwise, queue the button disablement for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += DisableMapEditorButton;
            }
        }

        /// <summary>
        /// Disables the map editor button.
        /// </summary>
        private void DisableMapEditorButton()
        {
            // Disable map editor button.
            UIButton mapEditorButton = GameObject.FindObjectOfType<ToolsMenu>()?.Find<UIPanel>("Panel Layout")?.Find<UIButton>("MapEditor");
            if (mapEditorButton != null && mapEditorButton.enabled)
            {
                mapEditorButton.tooltip = Translations.Translate("EDITOR_DISABLED");
                mapEditorButton.tooltipBox = UIToolTips.WordWrapToolTip;
                mapEditorButton.Disable();
            }

            UIButton editorsButton = GameObject.Find("Tools")?.GetComponent<UIButton>();
            if (editorsButton != null && editorsButton.enabled)
            {
                editorsButton.tooltip = Translations.Translate("EDITOR_DISABLED");
                editorsButton.tooltipBox = UIToolTips.WordWrapToolTip;
                editorsButton.Disable();
            }
        }
    }
}
