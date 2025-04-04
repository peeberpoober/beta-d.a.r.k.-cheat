using dark_cheat;
using Photon.Pun;
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

        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float CurrentSpreadMultiplier = 1.0f;

        public static bool NoWeaponCooldownEnabled = false;

        public static float LoadFloat(string key, float defaultValue = 1.0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static bool LoadToggle(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SaveColor(string key, Color color)
        {
            PlayerPrefs.SetFloat(key + "_r", color.r);
            PlayerPrefs.SetFloat(key + "_g", color.g);
            PlayerPrefs.SetFloat(key + "_b", color.b);
            PlayerPrefs.SetFloat(key + "_a", color.a);
        }

        public static Color LoadColor(string key, Color defaultColor)
        {
            float r = PlayerPrefs.GetFloat(key + "_r", defaultColor.r);
            float g = PlayerPrefs.GetFloat(key + "_g", defaultColor.g);
            float b = PlayerPrefs.GetFloat(key + "_b", defaultColor.b);
            float a = PlayerPrefs.GetFloat(key + "_a", defaultColor.a);
            return new Color(r, g, b, a);
        }

        public static void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
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
            ConfigManager.SaveToggle("no_weapon_recoil", Patches.NoWeaponRecoil._isEnabledForConfig);
            ConfigManager.SaveToggle("no_weapon_cooldown", NoWeaponCooldownEnabled);

            //Sliders
            ConfigManager.SaveFloat("strength", Hax2.sliderValueStrength);
            ConfigManager.SaveFloat("throw_strength", Hax2.throwStrength);
            ConfigManager.SaveFloat("speed", Hax2.sliderValue);
            ConfigManager.SaveFloat("grab_Range", Hax2.grabRange);
            ConfigManager.SaveFloat("stam_Recharge_Delay", Hax2.staminaRechargeDelay);
            ConfigManager.SaveFloat("stam_Recharge_Rate", Hax2.staminaRechargeRate);
            ConfigManager.SaveInt("extra_jumps", Hax2.extraJumps);
            ConfigManager.SaveFloat("tumble_launch", Hax2.tumbleLaunch);
            ConfigManager.SaveFloat("jump_force", Hax2.jumpForce);
            ConfigManager.SaveFloat("gravity", Hax2.customGravity);
            ConfigManager.SaveFloat("crouch_delay", Hax2.crouchDelay);
            ConfigManager.SaveFloat("crouch_speed", Hax2.crouchSpeed);
            ConfigManager.SaveFloat("slide_decay", Hax2.slideDecay);
            ConfigManager.SaveFloat("flashlight_intensity", Hax2.flashlightIntensity);
            ConfigManager.SaveFloat("field_of_view", Hax2.fieldOfView);
            ConfigManager.SaveFloat("max_item_distance", DebugCheats.maxItemEspDistance);
            ConfigManager.SaveFloat("weapon_spread_multiplier", CurrentSpreadMultiplier);
            ConfigManager.SaveInt("min_item_value", DebugCheats.minItemValue);

            //Visuals Tab
            ConfigManager.SaveToggle("drawEspBool", DebugCheats.drawEspBool);
            ConfigManager.SaveToggle("showEnemyBox", DebugCheats.showEnemyBox);
            ConfigManager.SaveToggle("drawChamsBool", DebugCheats.drawChamsBool);
            ConfigManager.SaveColor("enemy_visible_color", DebugCheats.enemyVisibleColor);
            ConfigManager.SaveColor("enemy_invisible_color", DebugCheats.enemyHiddenColor);
            ConfigManager.SaveToggle("showEnemyNames", DebugCheats.showEnemyNames);
            ConfigManager.SaveToggle("showEnemyDistance", DebugCheats.showEnemyDistance);
            ConfigManager.SaveToggle("showEnemyHP", DebugCheats.showEnemyHP);

            ConfigManager.SaveToggle("drawItemEspBool", DebugCheats.drawItemEspBool);
            ConfigManager.SaveToggle("showItemBox", DebugCheats.draw3DItemEspBool);
            ConfigManager.SaveToggle("drawItemChamsBool", DebugCheats.drawItemChamsBool);
            ConfigManager.SaveColor("item_visible_color", DebugCheats.itemVisibleColor);
            ConfigManager.SaveColor("item_invisible_color", DebugCheats.itemHiddenColor);
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
            Hax2.debounce = ConfigManager.LoadToggle("grab_guard", true);

            //Sliders
            Hax2.sliderValueStrength = ConfigManager.LoadFloat("strength", Hax2.sliderValueStrength);
            Hax2.throwStrength = ConfigManager.LoadFloat("throw_strength", Hax2.throwStrength);
            Hax2.sliderValue = ConfigManager.LoadFloat("speed", Hax2.sliderValue);
            Hax2.grabRange = ConfigManager.LoadFloat("grab_Range", Hax2.grabRange);
            Hax2.staminaRechargeDelay = ConfigManager.LoadFloat("stam_Recharge_Delay", Hax2.staminaRechargeDelay);
            Hax2.staminaRechargeRate = ConfigManager.LoadFloat("stam_Recharge_Rate", Hax2.staminaRechargeRate);
            Hax2.extraJumps = ConfigManager.LoadInt("extra_jumps", Hax2.extraJumps);
            Hax2.tumbleLaunch = ConfigManager.LoadFloat("tumble_launch", Hax2.tumbleLaunch);
            Hax2.jumpForce = ConfigManager.LoadFloat("jump_force", Hax2.jumpForce);
            Hax2.customGravity = ConfigManager.LoadFloat("gravity", Hax2.customGravity);
            Hax2.crouchDelay = ConfigManager.LoadFloat("crouch_delay", Hax2.crouchDelay);
            Hax2.crouchSpeed = ConfigManager.LoadFloat("crouch_speed", Hax2.crouchSpeed);
            Hax2.slideDecay = ConfigManager.LoadFloat("slide_decay", Hax2.slideDecay);
            Hax2.flashlightIntensity = ConfigManager.LoadFloat("flashlight_intensity", Hax2.flashlightIntensity);
            Hax2.fieldOfView = ConfigManager.LoadFloat("field_of_view", Hax2.fieldOfView);
            DebugCheats.maxItemEspDistance = ConfigManager.LoadFloat("max_item_distance", DebugCheats.maxItemEspDistance);
            DebugCheats.minItemValue = ConfigManager.LoadInt("min_item_value", DebugCheats.minItemValue);

            // Visuals Tab
            DebugCheats.drawEspBool = ConfigManager.LoadToggle("drawEspBool", false);
            DebugCheats.showEnemyBox = ConfigManager.LoadToggle("showEnemyBox", true);
            DebugCheats.drawChamsBool = ConfigManager.LoadToggle("drawChamsBool", false);
            DebugCheats.enemyVisibleColor = ConfigManager.LoadColor("enemy_visible_color", DebugCheats.enemyVisibleColor);
            DebugCheats.enemyHiddenColor = ConfigManager.LoadColor("enemy_invisible_color", DebugCheats.enemyHiddenColor);
            DebugCheats.showEnemyNames = ConfigManager.LoadToggle("showEnemyNames", true);
            DebugCheats.showEnemyDistance = ConfigManager.LoadToggle("showEnemyDistance", true);
            DebugCheats.showEnemyHP = ConfigManager.LoadToggle("showEnemyHP", true);

            DebugCheats.drawItemEspBool = ConfigManager.LoadToggle("drawItemEspBool", false);
            DebugCheats.draw3DItemEspBool = ConfigManager.LoadToggle("showItemBox", false);
            DebugCheats.drawItemChamsBool = ConfigManager.LoadToggle("drawItemChamsBool", false);
            DebugCheats.itemVisibleColor = ConfigManager.LoadColor("item_visible_color", DebugCheats.itemVisibleColor);
            DebugCheats.itemHiddenColor = ConfigManager.LoadColor("item_invisible_color", DebugCheats.itemHiddenColor);
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
            if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
            {
                ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable();
                initialProps["isBlindEnabled"] = Hax2.blindEnemies;
                PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            }
        }
    }
}