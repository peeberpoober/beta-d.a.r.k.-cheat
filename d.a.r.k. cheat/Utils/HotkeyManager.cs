using System;
using System.Collections.Generic;
using UnityEngine;

namespace dark_cheat
{
    public class HotkeyManager
    {
        private Dictionary<KeyCode, Action> hotkeyBindings = new Dictionary<KeyCode, Action>();
        private List<HotkeyAction> availableActions = new List<HotkeyAction>();
        private KeyCode[] defaultHotkeys = new KeyCode[12];
        private string keyAssignmentError = "";
        private float errorMessageTime = 0f;
        public const float ERROR_MESSAGE_DURATION = 3f; // how long to show duplicate hotkey error message

        // System keys
        private KeyCode _menuToggleKey = KeyCode.Delete;
        private KeyCode _reloadKey = KeyCode.F5;
        private KeyCode _unloadKey = KeyCode.F10;

        // Properties to access system keys
        public KeyCode MenuToggleKey => _menuToggleKey;
        public KeyCode ReloadKey => _reloadKey;
        public KeyCode UnloadKey => _unloadKey;

        // Properties for configuration state
        public bool ConfiguringHotkey { get; private set; }
        public bool ConfiguringSystemKey { get; private set; }
        public bool WaitingForAnyKey { get; private set; }
        public int SystemKeyConfigIndex { get; private set; } = -1; // 0=menu, 1=reload, 2=unload
        public int SelectedHotkeySlot { get; private set; }
        public KeyCode CurrentHotkeyKey { get; private set; } = KeyCode.None;
        public string KeyAssignmentError => keyAssignmentError;
        public float ErrorMessageTime => errorMessageTime;

        // Singleton instance
        private static HotkeyManager _instance;
        public static HotkeyManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HotkeyManager();
                return _instance;
            }
        }

        private HotkeyManager()
        {
            availableActions.Clear(); // Clear existing actions before re-initializing
            InitializeHotkeyActions();
            LoadHotkeySettings();
        }

        public class HotkeyAction
        {
            public string Name { get; set; }
            public Action Action { get; set; }
            public string Description { get; set; }
            public HotkeyAction(string name, Action action, string description = "")
            {
                Name = name;
                Action = action;
                Description = description;
            }
        }

        private void InitializeHotkeyActions()
        {
            availableActions.Add(new HotkeyAction("God Mode", () =>
            {
                bool newGodModeState = !Hax2.godModeActive;
                PlayerController.GodMode();
                Hax2.godModeActive = newGodModeState;
                DLog.Log("god mode toggled: " + Hax2.godModeActive);
            }, "toggles god mode on/off"));

            availableActions.Add(new HotkeyAction("Noclip", () =>
            {
                bool newNoclipState = !NoclipController.noclipActive;
                NoclipController.ToggleNoclip();
                NoclipController.noclipActive = newNoclipState;
                DLog.Log("noclip toggled: " + NoclipController.noclipActive);
            }, "toggles noclip on/off"));

            availableActions.Add(new HotkeyAction("Tumble Guard", () =>
            {
                bool newTumbleGuardState = !Hax2.debounce;
                PlayerTumblePatch.ToggleTumbleGuard();
                Hax2.debounce = newTumbleGuardState;
                DLog.Log("tumble tuard toggled: " + Hax2.debounce);
            }, "toggles tumble guard on/off"));

            availableActions.Add(new HotkeyAction("Infinite Health", () =>
            {
                bool newHealState = !Hax2.infiniteHealthActive;
                Hax2.infiniteHealthActive = newHealState;
                PlayerController.MaxHealth();
                DLog.Log("infinite health toggled: " + Hax2.infiniteHealthActive);
            }, "toggles infinite health on/off"));

            availableActions.Add(new HotkeyAction("Infinite Stamina", () =>
            {
                bool newStaminaState = !Hax2.stamineState;
                Hax2.stamineState = newStaminaState;
                PlayerController.MaxStamina();
                DLog.Log("infinite stamina toggled: " + Hax2.stamineState);
            }, "toggles infinite stamina on/off"));

            availableActions.Add(new HotkeyAction("RGB Player", () =>
            {
                playerColor.isRandomizing = !playerColor.isRandomizing;
                DLog.Log("rgb player toggled: " + playerColor.isRandomizing);
            }, "toggles rgb player effect"));

            availableActions.Add(new HotkeyAction("Spawn Money", () =>
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer != null)
                {
                    Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                    // Use the SpawnMoney method instead of SpawnItem
                    ItemSpawner.SpawnMoney(targetPosition);
                    DLog.Log("money spawned.");
                }
                else
                {
                    DLog.Log("local player not found!");
                }
            }, "spawns money at your position"));

            availableActions.Add(new HotkeyAction("Kill All Enemies", () =>
            {
                Enemies.KillAllEnemies();
                DLog.Log("all enemies killed.");
            }, "kills all enemies on the map"));

            availableActions.Add(new HotkeyAction("Enemy ESP Toggle", () =>
            {
                DebugCheats.drawEspBool = !DebugCheats.drawEspBool;
                DLog.Log("enemy esp toggled: " + DebugCheats.drawEspBool);
            }, "toggles enemy esp on/off"));

            availableActions.Add(new HotkeyAction("Item ESP Toggle", () =>
            {
                DebugCheats.drawItemEspBool = !DebugCheats.drawItemEspBool;
                DLog.Log("item esp toggled: " + DebugCheats.drawItemEspBool);
            }, "toggles item esp on/off"));

            availableActions.Add(new HotkeyAction("Player ESP Toggle", () =>
            {
                DebugCheats.drawPlayerEspBool = !DebugCheats.drawPlayerEspBool;
                DLog.Log("player esp toggled: " + DebugCheats.drawPlayerEspBool);
            }, "toggles player esp on/off"));

            availableActions.Add(new HotkeyAction("Heal Self", () =>
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer != null)
                {
                    Players.HealPlayer(localPlayer, 100, "Self");
                    DLog.Log("healed self by 100 hp.");
                }
                else
                {
                    DLog.Log("local player not found!");
                }
            }, "heals yourself by 100 hp"));

            availableActions.Add(new HotkeyAction("Max Speed", () =>
            {
                Hax2.sliderValueStrength = 30f;
                PlayerController.SetSprintSpeed(Hax2.sliderValueStrength);
                DLog.Log("speed set to maximum (30)");
            }, "sets speed to maximum value"));

            availableActions.Add(new HotkeyAction("Normal Speed", () =>
            {
                Hax2.sliderValueStrength = 5f;
                PlayerController.SetSprintSpeed(Hax2.sliderValueStrength);
                DLog.Log("speed set to normal (5)");
            }, "sets speed to normal value"));

            availableActions.Add(new HotkeyAction("Unlimited Battery", () =>
            {
                Hax2.unlimitedBatteryActive = !Hax2.unlimitedBatteryActive;
                if (Hax2.unlimitedBatteryComponent != null)
                    Hax2.unlimitedBatteryComponent.unlimitedBatteryEnabled = Hax2.unlimitedBatteryActive;
            }, "toggles unlimited battery on/off"));


            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                defaultHotkeys[i] = KeyCode.None;
            }
        }

        public void SaveHotkeySettings()
        {
            PlayerPrefs.SetInt("MenuToggleKey", (int)_menuToggleKey);
            PlayerPrefs.SetInt("ReloadKey", (int)_reloadKey);
            PlayerPrefs.SetInt("UnloadKey", (int)_unloadKey);

            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                PlayerPrefs.SetInt($"HotkeyKey_{i}", (int)defaultHotkeys[i]);

                int actionIndex = -1;
                if (defaultHotkeys[i] != KeyCode.None && hotkeyBindings.ContainsKey(defaultHotkeys[i]))
                {
                    var action = hotkeyBindings[defaultHotkeys[i]];
                    if (action != null)
                    {
                        for (int j = 0; j < availableActions.Count; j++)
                        {
                            if (availableActions[j].Action == action)
                            {
                                actionIndex = j;
                                break;
                            }
                        }
                    }
                }
                PlayerPrefs.SetInt($"HotkeyAction_{i}", actionIndex);
            }

            PlayerPrefs.Save();
            DLog.Log("Hotkey settings saved");
        }

        private void LoadHotkeySettings()
        {
            _menuToggleKey = (KeyCode)PlayerPrefs.GetInt("MenuToggleKey", (int)KeyCode.Delete);
            _reloadKey = (KeyCode)PlayerPrefs.GetInt("ReloadKey", (int)KeyCode.F5);
            _unloadKey = (KeyCode)PlayerPrefs.GetInt("UnloadKey", (int)KeyCode.F10);

            hotkeyBindings.Clear();

            for (int i = 0; i < defaultHotkeys.Length; i++)
            {
                defaultHotkeys[i] = (KeyCode)PlayerPrefs.GetInt($"HotkeyKey_{i}", (int)KeyCode.None);
                int actionIndex = PlayerPrefs.GetInt($"HotkeyAction_{i}", -1);

                if (defaultHotkeys[i] != KeyCode.None && actionIndex >= 0 && actionIndex < availableActions.Count)
                {
                    hotkeyBindings[defaultHotkeys[i]] = availableActions[actionIndex].Action;
                }
            }

            DLog.Log("Hotkey settings loaded");
        }

        public void StartConfigureSystemKey(int index)
        {
            ConfiguringSystemKey = true;
            SystemKeyConfigIndex = index;
            WaitingForAnyKey = true;
            DLog.Log($"Press any key to set {GetSystemKeyName(index)}...");
        }

        public string GetSystemKeyName(int index)
        {
            switch (index)
            {
                case 0: return "Menu Toggle";
                case 1: return "Reload";
                case 2: return "Unload";
                default: return "Unknown";
            }
        }

        public void ShowActionSelector(int slotIndex, KeyCode key)
        {
            CurrentHotkeyKey = key;
            // The actual UI display is handled in the Hax2 class
        }

        public void AssignActionToHotkey(int actionIndex)
        {
            if (CurrentHotkeyKey != KeyCode.None && actionIndex >= 0 && actionIndex < availableActions.Count)
            {
                hotkeyBindings[CurrentHotkeyKey] = availableActions[actionIndex].Action;
                DLog.Log("assigned " + availableActions[actionIndex].Name + " to " + CurrentHotkeyKey);
                SaveHotkeySettings();
            }
        }

        public void ClearHotkeyBinding(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < defaultHotkeys.Length)
            {
                KeyCode key = defaultHotkeys[slotIndex];
                if (key != KeyCode.None && hotkeyBindings.ContainsKey(key))
                {
                    hotkeyBindings.Remove(key);
                }
                defaultHotkeys[slotIndex] = KeyCode.None;
                DLog.Log("cleared hotkey binding for slot " + (slotIndex + 1));
                SaveHotkeySettings();
            }
        }

        public void ProcessHotkeyConfiguration(KeyCode key)
        {
            if (key != KeyCode.Escape)
            {
                if (key == _menuToggleKey || key == _reloadKey || key == _unloadKey)
                {
                    keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used as a system key!";
                    errorMessageTime = Time.time;
                    DLog.Log(keyAssignmentError);
                    ConfiguringHotkey = false;
                }
                else if (hotkeyBindings.ContainsKey(key))
                {
                    keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used for another action!";
                    errorMessageTime = Time.time;
                    DLog.Log(keyAssignmentError);
                    ConfiguringHotkey = false;
                }
                else
                {
                    KeyCode oldKey = defaultHotkeys[SelectedHotkeySlot];
                    if (oldKey != KeyCode.None && hotkeyBindings.ContainsKey(oldKey))
                    {
                        Action oldAction = hotkeyBindings[oldKey];
                        hotkeyBindings.Remove(oldKey);

                        hotkeyBindings[key] = oldAction;
                    }
                    else
                    {
                        hotkeyBindings[key] = null;
                    }

                    defaultHotkeys[SelectedHotkeySlot] = key;

                    DLog.Log($"Hotkey set to: {key}");
                    ConfiguringHotkey = false;
                }
            }
            else
            {
                ConfiguringHotkey = false;
                DLog.Log("Hotkey configuration canceled");
            }
        }

        public void ProcessSystemKeyConfiguration(KeyCode key)
        {
            if (key != KeyCode.Escape)
            {
                bool isUsed = false;
                string conflictType = "";
                WaitingForAnyKey = false;

                if (key == _menuToggleKey && SystemKeyConfigIndex != 0)
                {
                    isUsed = true;
                    conflictType = "Menu Toggle";
                }
                else if (key == _reloadKey && SystemKeyConfigIndex != 1)
                {
                    isUsed = true;
                    conflictType = "Reload";
                }
                else if (key == _unloadKey && SystemKeyConfigIndex != 2)
                {
                    isUsed = true;
                    conflictType = "Unload";
                }
                else if (hotkeyBindings.ContainsKey(key))
                {
                    isUsed = true;
                    conflictType = "action hotkey";
                }

                if (isUsed)
                {
                    keyAssignmentError = $"Cannot assign {key} - already used as {conflictType}!";
                    errorMessageTime = Time.time;
                    DLog.Log(keyAssignmentError);
                    ConfiguringSystemKey = false;
                }
                else
                {
                    switch (SystemKeyConfigIndex)
                    {
                        case 0: _menuToggleKey = key; break;
                        case 1: _reloadKey = key; break;
                        case 2: _unloadKey = key; break;
                    }
                    DLog.Log($"{GetSystemKeyName(SystemKeyConfigIndex)} key set to: {key}");
                    ConfiguringSystemKey = false;
                    SaveHotkeySettings();
                }
            }
            else
            {
                ConfiguringSystemKey = false;
                DLog.Log($"{GetSystemKeyName(SystemKeyConfigIndex)} key configuration canceled");
            }
        }

        public void StartHotkeyConfiguration(int slotIndex)
        {
            SelectedHotkeySlot = slotIndex;
            ConfiguringHotkey = true;
            DLog.Log("configuring hotkey for slot " + (slotIndex + 1));
        }

        public void CheckAndExecuteHotkeys()
        {
            foreach (var kvp in hotkeyBindings)
            {
                if (Input.GetKeyDown(kvp.Key) && kvp.Value != null)
                {
                    kvp.Value.Invoke();
                    break;
                }
            }
        }

        public KeyCode GetHotkeyForSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < defaultHotkeys.Length)
            {
                return defaultHotkeys[slotIndex];
            }
            return KeyCode.None;
        }

        public string GetActionNameForKey(KeyCode key)
        {
            if (key != KeyCode.None && hotkeyBindings.ContainsKey(key))
            {
                var action = hotkeyBindings[key];
                if (action != null)
                {
                    for (int j = 0; j < availableActions.Count; j++)
                    {
                        if (availableActions[j].Action == action)
                        {
                            return availableActions[j].Name;
                        }
                    }
                }
            }
            return "Not assigned";
        }

        public List<HotkeyAction> GetAvailableActions()
        {
            return availableActions;
        }
    }
}
