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
    using ICities;

    /// <summary>
    /// The mod's options panel.
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        public OptionsPanel()
        {
            // Auto layout.
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            UIHelper helper = new UIHelper(this);

            // Language options.
            UIHelperBase languageGroup = helper.AddGroup(Translations.Translate("LANGUAGE_CHOICE"));
            UIDropDown languageDropDown = (UIDropDown)languageGroup.AddDropdown(Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index, (value) =>
            {
                Translations.Index = value;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            });
            languageDropDown.autoSize = false;
            languageDropDown.width = 270f;

            // Network options.
            UIHelperBase netGroup = helper.AddGroup(Translations.Translate("NET_OPTIONS"));
            UICheckBox noPowerlineCheck = netGroup.AddCheckbox(Translations.Translate("NO_POWERLINES"), NoPowerlinesPatches.NoPowerlinesEnabled, (isChecked) => NoPowerlinesPatches.NoPowerlinesEnabled = isChecked) as UICheckBox;
            noPowerlineCheck.tooltip = Translations.Translate("NO_POWERLINES_TIP");
            noPowerlineCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            UICheckBox electricRoadsCheck = netGroup.AddCheckbox(Translations.Translate("ELECTRIC_ROADS"), ExpandedElectricityManager.ElectricRoadsEnabled, (isChecked) => ExpandedElectricityManager.ElectricRoadsEnabled = isChecked) as UICheckBox;
            electricRoadsCheck.tooltip = Translations.Translate("ELECTRIC_ROADS_TIP");
            electricRoadsCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            UICheckBox noPipesCheck = netGroup.AddCheckbox(Translations.Translate("NO_PIPES"), NoPipesPatches.NoPipesEnabled, (isChecked) => NoPipesPatches.NoPipesEnabled = isChecked) as UICheckBox;
            noPipesCheck.tooltip = Translations.Translate("NO_PIPES_TIP");
            noPipesCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            // Unlocking options.
            UIHelperBase unlockGroup = helper.AddGroup(Translations.Translate("UNLOCK"));

            UICheckBox ignoreUnlockCheck = unlockGroup.AddCheckbox(Translations.Translate("IGNORE_UNLOCK"), GameAreaManagerPatches.IgnoreUnlocking, (isChecked) => GameAreaManagerPatches.IgnoreUnlocking = isChecked) as UICheckBox;
            ignoreUnlockCheck.tooltip = Translations.Translate("IGNORE_UNLOCK_TIP");
            ignoreUnlockCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            UICheckBox crossTheLineCheck = unlockGroup.AddCheckbox(Translations.Translate("CROSS_THE_LINE"), GameAreaManagerPatches.CrossTheLine, (isChecked) => GameAreaManagerPatches.CrossTheLine = isChecked) as UICheckBox;
            crossTheLineCheck.tooltip = Translations.Translate("CROSS_THE_LINE_TIP");
            crossTheLineCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            // Unlock buttons (only if in-game).
            if (Loading.IsLoaded)
            {
                unlockGroup.AddButton(Translations.Translate("UNLOCK_25"), () => Singleton<SimulationManager>.instance.AddAction(() => Unlock(5)));
                unlockGroup.AddButton(Translations.Translate("UNLOCK_ALL"), () => Singleton<SimulationManager>.instance.AddAction(() => Unlock(9)));
            }

            // Rescue options.
            UIHelperBase rescueGroup = helper.AddGroup(Translations.Translate("RESCUE_OPTIONS"));
            UICheckBox ignoreExpandedCheck = rescueGroup.AddCheckbox(Translations.Translate("IGNORE_EXPANDED"), ModSettings.IgnoreExpanded, (isChecked) => ModSettings.IgnoreExpanded = isChecked) as UICheckBox;
            ignoreExpandedCheck.tooltip = Translations.Translate("IGNORE_EXPANDED_TIP");
            ignoreExpandedCheck.tooltipBox = UIToolTips.WordWrapToolTip;
        }

        /// <summary>
        /// Unlocks game tiles.
        /// </summary>
        /// <param name="unlockWidth">Grid with to unlock (centered); e.g. 5 to unock 25-tile area, 9 to unlock 81.</param>
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