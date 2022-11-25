// <copyright file="ConflictDetection.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Mod conflict detection.
    /// </summary>
    internal class ConflictDetection
    {
        /// <summary>
        /// Checks for mod conflicts and displays a notification when a conflict is detected.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise.</returns>
        internal bool CheckModConflicts()
        {
            List<string> conflictingModNames = CheckConflictingMods();

            bool conflictDetected = conflictingModNames != null && conflictingModNames.Count > 0;
            if (conflictDetected)
            {
                // Mod conflict detected - display warning notification.
                ListNotification modConflictNotification = NotificationBase.ShowNotification<ListNotification>();
                if (modConflictNotification != null)
                {
                    // Key text items.
                    modConflictNotification.AddParas(Translations.Translate("CONFLICT_DETECTED"), Translations.Translate("UNABLE_TO_OPERATE"), Translations.Translate("CONFLICTING_MODS"));

                    // Add conflicting mod name(s).
                    modConflictNotification.AddList(conflictingModNames.ToArray());
                }
            }

            return conflictDetected;
        }

        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>A list of conflicting mod names if a mod conflict was detected, false otherwise.</returns>
        private List<string> CheckConflictingMods()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            List<string> conflictingModNames = new List<string>();

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "EightyOne":
                            // Original 81 tiles mod.
                            conflictDetected = true;
                            conflictingModNames.Add("81 Tiles (broken version)");
                            break;

                        case "BuildAnywhere":
                            // Original WG mod.
                            conflictDetected = true;
                            conflictingModNames.Add("Cross the Line");
                            break;

                        case "EManagersLib":
                            // Extended Managers library - check for Beta.
                            if (assembly.GetName().Version == new Version("1.1.1.0"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Extended Managers Library Beta");
                            }

                            break;

                        case "RemoveNeedForPowerLines":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Remove Need For Power Lines");
                            break;

                        case "RemoveNeedForPipes":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Remove Need For Pipes");
                            break;

                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Garbage Bin Controller");
                            break;

                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Painter");
                            }

                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }
            }

            // If we got here, no conflict was detected; return null.
            return null;
        }
    }
}
