// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using EightyOne2.Patches;
    using UnityEngine;

    /// <summary>
    /// The mod's options panel.
    /// </summary>
    public class OptionsPanel : OptionsPanelBase
    {
        /// <summary>
        /// Performs on-demand panel setup.
        /// </summary>
        protected override void Setup()
        {
            const float HeaderMargin = 50f;
            const float CheckboxMargin = 27f;
            const float LeftMargin = 14f;

            float currentY = 10f;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += 77f;

            // Logging checkbox.
            currentY += 20f;
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
            currentY += CheckboxMargin;

            // Network options.
            currentY += 20f;
            UISpacers.AddTitleSpacer(this, 0f, currentY, OptionsPanelManager<OptionsPanel>.PanelWidth, Translations.Translate("NET_OPTIONS"));
            currentY += HeaderMargin;

            UICheckBox noPowerlineCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("NO_POWERLINES"));
            noPowerlineCheck.isChecked = NoPowerlinesPatches.NoPowerlinesEnabled;
            noPowerlineCheck.eventCheckChanged += (c, isChecked) => NoPowerlinesPatches.NoPowerlinesEnabled = isChecked;
            noPowerlineCheck.tooltip = Translations.Translate("NO_POWERLINES_TIP");
            noPowerlineCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            UICheckBox electricRoadsCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("ELECTRIC_ROADS"));
            electricRoadsCheck.isChecked = ExpandedElectricityManager.ElectricRoadsEnabled;
            electricRoadsCheck.eventCheckChanged += (c, isChecked) => ExpandedElectricityManager.ElectricRoadsEnabled = isChecked;
            electricRoadsCheck.tooltip = Translations.Translate("ELECTRIC_ROADS_TIP");
            electricRoadsCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            UICheckBox noPipesCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("NO_PIPES"));
            noPipesCheck.isChecked = NoPipesPatches.NoPipesEnabled;
            noPipesCheck.eventCheckChanged += (c, isChecked) => NoPipesPatches.NoPipesEnabled = isChecked;
            noPipesCheck.tooltip = Translations.Translate("NO_PIPES_TIP");
            noPipesCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            UICheckBox exceptOriginalCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin + LeftMargin, currentY, Translations.Translate("EXCEPT_ORIGINAL"));
            exceptOriginalCheck.isChecked = WaterFacilityAIPatches.IgnoreOriginal;
            exceptOriginalCheck.eventCheckChanged += (c, isChecked) => WaterFacilityAIPatches.IgnoreOriginal = isChecked;
            exceptOriginalCheck.tooltip = Translations.Translate("EXCEPT_ORIGINAL_TIP");
            exceptOriginalCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            // Unlocking options.
            currentY += 20f;
            UISpacers.AddTitleSpacer(this, 0f, currentY, OptionsPanelManager<OptionsPanel>.PanelWidth, Translations.Translate("UNLOCK"));
            currentY += HeaderMargin;

            UICheckBox ignoreUnlockCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("IGNORE_UNLOCK"));
            ignoreUnlockCheck.isChecked = GameAreaManagerPatches.IgnoreUnlocking;
            ignoreUnlockCheck.eventCheckChanged += (c, isChecked) => GameAreaManagerPatches.IgnoreUnlocking = isChecked;
            ignoreUnlockCheck.tooltip = Translations.Translate("IGNORE_UNLOCK_TIP");
            ignoreUnlockCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            UICheckBox crossTheLineCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("CROSS_THE_LINE"));
            crossTheLineCheck.isChecked = GameAreaManagerPatches.CrossTheLine;
            crossTheLineCheck.eventCheckChanged += (c, isChecked) => GameAreaManagerPatches.CrossTheLine = isChecked;
            crossTheLineCheck.tooltip = Translations.Translate("CROSS_THE_LINE_TIP");
            crossTheLineCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;

            // Unlock buttons (only if in-game).
            if (Loading.IsLoaded)
            {
                UIButton unlock25Button = UIButtons.AddButton(this, LeftMargin, currentY, Translations.Translate("UNLOCK_25"), width: 450f, height: 39.4f, scale: 1.3f, vertPad: 4);
                unlock25Button.eventClicked += (c, p) => Singleton<SimulationManager>.instance.AddAction(() => Unlock(5));
                currentY += 44.4f;

                UIButton unlock81Button = UIButtons.AddButton(this, LeftMargin, currentY, Translations.Translate("UNLOCK_ALL"), width: 450f, height: 39.4f, scale: 1.3f, vertPad: 4);
                unlock81Button.eventClicked += (c, p) => Singleton<SimulationManager>.instance.AddAction(() => Unlock(9));
                currentY += 44.4f;
            }

            // Rescue options.
            currentY += 20f;
            UISpacers.AddTitleSpacer(this, 0f, currentY, OptionsPanelManager<OptionsPanel>.PanelWidth, Translations.Translate("RESCUE_OPTIONS"));
            currentY += HeaderMargin;

            UICheckBox ignoreExpandedCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("IGNORE_EXPANDED"));
            ignoreExpandedCheck.isChecked = ModSettings.IgnoreExpanded;
            ignoreExpandedCheck.eventCheckChanged += (c, isChecked) => ModSettings.IgnoreExpanded = isChecked;
            ignoreExpandedCheck.tooltip = Translations.Translate("IGNORE_EXPANDED_TIP");
            ignoreExpandedCheck.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += CheckboxMargin;
        }

        /// <summary>
        /// Unlocks game tiles.
        /// </summary>
        /// <param name="unlockWidth">Grid with to unlock (centered); e.g. 5 to unlock 25-tile area, 9 to unlock 81.</param>
        private void Unlock(int unlockWidth)
        {
            int tileCount = unlockWidth * unlockWidth;
            Logging.Message("force-unlocking ", tileCount, " tiles");

            // Local references
            GameAreaManager gameAreaManager = Singleton<GameAreaManager>.instance;

            // Calculate margin.
            int tileMargin = (GameAreaManagerPatches.ExpandedAreaGridResolution - unlockWidth) / 2;
            int maxCoord = tileMargin + unlockWidth;

            // Enable forced unlocking while we do this.
            GameAreaManagerPatches.ForceUnlocking = true;

            // Keep going recursively until all tiles have been unlocked.
            bool changingTiles = true;
            int timeoutCounter = 0;
            while (changingTiles)
            {
                // Reset flag.
                changingTiles = false;

                // Iterate through grid and unlock any tiles that already aren't.
                for (int z = tileMargin; z < maxCoord; ++z)
                {
                    for (int x = tileMargin; x < maxCoord; ++x)
                    {
                        // Check if this tile is unlocked.
                        if (!GameAreaManagerPatches.IsUnlocked(gameAreaManager, x, z))
                        {
                            // Not unlocked - record that we're still changing tiles.
                            changingTiles = true;

                            // Attempt to unlock tile (will fail if not unlockable, i.e. no unlocked adjacent areas).
                            gameAreaManager.UnlockArea((z * GameAreaManagerPatches.ExpandedAreaGridResolution) + x);
                        }
                    }
                }

                // Check timeout.
                if (++timeoutCounter > tileCount)
                {
                    break;
                }
            }

            // Disable forced unlocking.
            GameAreaManagerPatches.ForceUnlocking = false;

            // Check result.
            if (changingTiles)
            {
                Logging.Error("unable to properly force-unlock all", tileCount, " tiles");
            }
            else
            {
                Logging.Message("unlocking done");
            }
        }
    }
}