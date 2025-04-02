using dark_cheat;
using Unity.VisualScripting;
using UnityEngine;


namespace dark_cheat
{
    public static class ConfigManager
    {
        public static void SaveToggle(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static bool LoadToggle(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static void SaveAllToggles()
        {
            //Self Tab
            ConfigManager.SaveToggle("God_Mode", Hax2.godModeActive);
            ConfigManager.SaveToggle("inf_health", Hax2.infiniteHealthActive);
            ConfigManager.SaveToggle("No_Clip", NoclipController.noclipActive);
            ConfigManager.SaveToggle("inf_stam", Hax2.stamineState);
            ConfigManager.SaveToggle("rgb_player", playerColor.isRandomizing);
            ConfigManager.SaveToggle("No_Fog", MiscFeatures.NoFogEnabled);
            ConfigManager.SaveToggle("WaterMark_Toggle", Hax2.showWatermark);

            //Visuals Tab
            ConfigManager.SaveToggle("drawEspBool", DebugCheats.drawEspBool);
            ConfigManager.SaveToggle("showEnemyBox", DebugCheats.showEnemyBox);
            ConfigManager.SaveToggle("drawChamsBool", DebugCheats.drawChamsBool);
            ConfigManager.SaveToggle("showEnemyNames", DebugCheats.showEnemyNames);
            ConfigManager.SaveToggle("showEnemyDistance", DebugCheats.showEnemyDistance);
            ConfigManager.SaveToggle("showEnemyHP", DebugCheats.showEnemyHP);

            ConfigManager.SaveToggle("drawItemEspBool", DebugCheats.drawItemEspBool);
            ConfigManager.SaveToggle("showItemBox", DebugCheats.draw3DItemEspBool);
            ConfigManager.SaveToggle("drawItemChamsBool", DebugCheats.drawItemChamsBool);
            ConfigManager.SaveToggle("showItemNames", DebugCheats.showItemNames);
            ConfigManager.SaveToggle("showItemDistance", DebugCheats.showItemDistance);
            ConfigManager.SaveToggle("showItemValue", DebugCheats.showItemValue);
            ConfigManager.SaveToggle("showDeathHeads", DebugCheats.showPlayerDeathHeads);

            ConfigManager.SaveToggle("enable_extract_esp", DebugCheats.drawExtractionPointEspBool);
            ConfigManager.SaveToggle("show_extract_names", DebugCheats.showExtractionNames);
            ConfigManager.SaveToggle("show_extract_distance", DebugCheats.showExtractionDistance);


            ConfigManager.SaveToggle("enable_player_esp", DebugCheats.drawPlayerEspBool);
            ConfigManager.SaveToggle("show_2d_box_player", DebugCheats.draw2DPlayerEspBool);
            ConfigManager.SaveToggle("show_3d_box_player", DebugCheats.draw3DPlayerEspBool);
            ConfigManager.SaveToggle("show_names_player", DebugCheats.showPlayerNames);
            ConfigManager.SaveToggle("show_distance_player", DebugCheats.showPlayerDistance);
            ConfigManager.SaveToggle("show_health_player", DebugCheats.showPlayerHP);
            ConfigManager.SaveToggle("show_alive_dead_list", Hax2.showPlayerStatus);

            //Enemies Tab
            ConfigManager.SaveToggle("blind_enemies", Hax2.blindEnemies);

            PlayerPrefs.Save();
            Debug.Log("Config saved.");
        }

        public static void LoadAllToggles()
        {
            // Self Tab
            Hax2.godModeActive = ConfigManager.LoadToggle("God_Mode", false);
            if (Hax2.godModeActive) PlayerController.GodMode();

            Hax2.infiniteHealthActive = ConfigManager.LoadToggle("inf_health", false);
            if (Hax2.infiniteHealthActive) PlayerController.MaxHealth();

            NoclipController.noclipActive = ConfigManager.LoadToggle("No_Clip", false);
            if (NoclipController.noclipActive) NoclipController.ToggleNoclip();

            Hax2.stamineState = ConfigManager.LoadToggle("inf_stam", false);
            if (Hax2.stamineState) PlayerController.MaxStamina();

            playerColor.isRandomizing = ConfigManager.LoadToggle("rgb_player", false);
            MiscFeatures.NoFogEnabled = ConfigManager.LoadToggle("No_Fog", false);
            Hax2.showWatermark = ConfigManager.LoadToggle("WaterMark_Toggle", true);

            // Visuals Tab
            DebugCheats.drawEspBool = ConfigManager.LoadToggle("drawEspBool", false);
            DebugCheats.showEnemyBox = ConfigManager.LoadToggle("showEnemyBox", true);
            DebugCheats.drawChamsBool = ConfigManager.LoadToggle("drawChamsBool", false);
            DebugCheats.showEnemyNames = ConfigManager.LoadToggle("showEnemyNames", true);
            DebugCheats.showEnemyDistance = ConfigManager.LoadToggle("showEnemyDistance", true);
            DebugCheats.showEnemyHP = ConfigManager.LoadToggle("showEnemyHP", true);

            DebugCheats.drawItemEspBool = ConfigManager.LoadToggle("drawItemEspBool", false);
            DebugCheats.draw3DItemEspBool = ConfigManager.LoadToggle("showItemBox", false);
            DebugCheats.drawItemChamsBool = ConfigManager.LoadToggle("drawItemChamsBool", false);
            DebugCheats.showItemNames = ConfigManager.LoadToggle("showItemNames", true);
            DebugCheats.showItemDistance = ConfigManager.LoadToggle("showItemDistance", true);
            DebugCheats.showItemValue = ConfigManager.LoadToggle("showItemValue", true);
            DebugCheats.showPlayerDeathHeads = ConfigManager.LoadToggle("showDeathHeads", true);

            DebugCheats.drawExtractionPointEspBool = ConfigManager.LoadToggle("enable_extract_esp", false);
            DebugCheats.showExtractionNames = ConfigManager.LoadToggle("show_extract_names", true);
            DebugCheats.showExtractionDistance = ConfigManager.LoadToggle("show_extract_distance", true);

            DebugCheats.drawPlayerEspBool = ConfigManager.LoadToggle("enable_player_esp", false);
            DebugCheats.draw2DPlayerEspBool = ConfigManager.LoadToggle("show_2d_box_player", true);
            DebugCheats.draw3DPlayerEspBool = ConfigManager.LoadToggle("show_3d_box_player", false);
            DebugCheats.showPlayerNames = ConfigManager.LoadToggle("show_names_player", true);
            DebugCheats.showPlayerDistance = ConfigManager.LoadToggle("show_distance_player", true);
            DebugCheats.showPlayerHP = ConfigManager.LoadToggle("show_health_player", true);
            Hax2.showPlayerStatus = ConfigManager.LoadToggle("show_alive_dead_list", true);

            // Enemies Tab
            Hax2.blindEnemies = ConfigManager.LoadToggle("blind_enemies", false);
        }
    }
}