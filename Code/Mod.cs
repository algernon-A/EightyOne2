// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.Globalization;
    using ColossalFramework.Plugins;
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
            // Perform conflict detection.
            ConflictDetection conflictDetection = new ConflictDetection();
            if (conflictDetection.CheckModConflicts())
            {
                Logging.Error("aborting activation due to conflicting mods");

                // Load mod settings to ensure that correct language is selected for notification display.
                LoadSettings();

                // Disable mod.
                if (AssemblyUtils.ThisPlugin is PluginManager.PluginInfo plugin)
                {
                    Logging.KeyMessage("disabling mod");
                    plugin.isEnabled = false;
                }

                // Don't do anything further.
                return;
            }

            base.OnEnabled();

            // Disable the map editor button.
            // First, check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - disable the button now.
                DisableMapEditorButton();

                // Ensure button is re-disabled on locale change.
                LocaleManager.eventLocaleChanged += DisableMapEditorButton;
            }
            else
            {
                // Otherwise, queue the button disablement for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += () =>
                {
                    DisableMapEditorButton();

                    // Ensure button is re-disabled on locale change.
                    LocaleManager.eventLocaleChanged += DisableMapEditorButton;
                };
            }

            // Also disable the map editor button on locale change.
        }

        /// <summary>
        /// Disables the map editor button.
        /// </summary>
        private void DisableMapEditorButton()
        {
            Logging.Message("disabling map editor buttons");

            // Disable map editor button.
            UIButton mapEditorButton = GameObject.FindObjectOfType<ToolsMenu>()?.Find<UIPanel>("Panel Layout")?.Find<UIButton>("MapEditor");
            if (mapEditorButton != null && mapEditorButton.enabled)
            {
                mapEditorButton.tooltip = Translations.Translate("EDITOR_DISABLED");
                mapEditorButton.tooltipBox = UIToolTips.WordWrapToolTip;
                mapEditorButton.Disable();
            }

            UIButton editorsButton = GameObject.Find("MenuContainer")?.GetComponent<UIPanel>().Find<UISlicedSprite>("CenterPart")?.Find<UIPanel>("MenuArea")?.Find<UIPanel>("Menu")?.Find<UIButton>("Tools");
            if (editorsButton != null && editorsButton.enabled)
            {
                editorsButton.tooltip = Translations.Translate("EDITOR_DISABLED");
                editorsButton.tooltipBox = UIToolTips.WordWrapToolTip;
                editorsButton.Disable();
            }
        }
    }
}
