using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using SingularityGroup.HotReload;
using System.Runtime.CompilerServices;
using dark_cheat.Utils;
using static UnityEngine.Rendering.DebugUI.Table;

namespace dark_cheat
{
    public static class UIHelper
    {
        private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();
        private static float x, y, width, height, margin, controlHeight, controlDist, nextControlY;
        private static int columns = 1;
        private static int currentColumn = 0;
        private static int currentRow = 0;

        private static GUIStyle sliderStyle;
        private static GUIStyle thumbStyle;

        public static bool ButtonBool(string text, bool value, float? customX = null, float? customY = null)
        {
            Rect rect = NextControlRect(customX, customY);
            string displayText = $"{text} {(value ? "✔" : " ")}";
            GUIStyle style = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, normal = { textColor = value ? Color.green : Color.red } };
            return GUI.Button(rect, displayText, style) ? !value : value;
        }

        public static bool Checkbox(string text, bool value, float? customX = null, float? customY = null)
        {
            Rect rect = NextControlRect(customX, customY);
            rect.height = 20f;
            rect.width = 200f;
            return GUI.Toggle(rect, value, text);
        }

        public static void Begin(string text, float _x, float _y, float _width, float _height, float InstructionHeight, float _controlHeight, float _controlDist)
        {
            x = _x; y = _y; width = _width; height = _height; margin = InstructionHeight; controlHeight = _controlHeight; controlDist = _controlDist;
            nextControlY = y + margin + 60;
            GUI.Box(new Rect(x, y, width, height), text);
            ResetGrid();
        }

        private static Rect NextControlRect(float? customX = null, float? customY = null)
        {
            float controlX = customX ?? (x + margin + currentColumn * ((width - (columns + 1) * margin) / columns));
            float controlY = customY ?? nextControlY;
            float controlWidth = customX == null ? ((width - (columns + 1) * margin) / columns) : width - 2 * margin;

            Rect rect = new Rect(controlX, controlY, controlWidth, controlHeight);

            if (customX == null && customY == null)
            {
                currentColumn++;
                if (currentColumn >= columns)
                {
                    currentColumn = 0;
                    currentRow++;
                    nextControlY += controlHeight + controlDist;
                }
            }

            return rect;
        }

        public static bool Button(string text, float? customX = null, float? customY = null)
        {
            return GUI.Button(NextControlRect(customX, customY), text);
        }

        public static bool Button(string text, float customX, float customY, float width, float height)
        {
            Rect rect = new Rect(customX, customY, width, height);
            return GUI.Button(rect, text);
        }

        public static void InitSliderStyles()
        {
            if (sliderStyle == null) // Custom style for the slider
            {
                sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
                {
                    normal = { background = MakeSolidBackground(new Color(0.7f, 0.7f, 0.7f), 1f) },
                    hover = { background = MakeSolidBackground(new Color(0.8f, 0.8f, 0.8f), 1f) },
                    active = { background = MakeSolidBackground(new Color(0.9f, 0.9f, 0.9f), 1f) }
                };
            }
            if (thumbStyle == null)
            {
                thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
                {
                    normal = { background = MakeSolidBackground(Color.white, 1f) },
                    hover = { background = MakeSolidBackground(new Color(0.9f, 0.9f, 0.9f), 1f) },
                    active = { background = MakeSolidBackground(Color.green, 1f) }
                };
            }
        }

        public static string MakeEnable(string text, bool state) => $"{text}{(state ? "ON" : "OFF")}";
        public static void Label(string text, float? customX = null, float? customY = null) => GUI.Label(NextControlRect(customX, customY), text);
        public static float Slider(float val, float min, float max, float? customX = null, float? customY = null)
        {
            Rect rect = NextControlRect(customX, customY); // Get control rect, but reduce height to 12px for better hitbox management
            rect.height = 12f;

            return Mathf.Round(GUI.HorizontalSlider(rect, val, min, max, sliderStyle, thumbStyle));
        } // Round value after interacting

        private static Texture2D MakeSolidBackground(Color color, float alpha)
        {
            Color key = new Color(color.r, color.g, color.b, alpha);

            if (!solidTextures.ContainsKey(key))
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.SetPixel(0, 0, key);
                texture.Apply();
                solidTextures[key] = texture;
            }
            return solidTextures[key];
        }

        public static void ResetGrid() { currentColumn = 0; currentRow = 0; nextControlY = y + margin + 60; }
    }

    public class Hax2 : MonoBehaviour
    {
        private static List<EnemySetup> cachedFilteredEnemySetups = null;
        private static List<string> cachedEnemySetupNames = null;
        public string spawnCountText = "1";  // Default value for number of enemies to spawn.
        public int spawnEnemyIndex = 0;
        public bool showSpawnDropdown = false;
        public Vector2 spawnDropdownScrollPosition = Vector2.zero;
        public static bool ChatDropdownVisible = false;
        public static string ChatDropdownVisibleName = "All";
        public static float fieldOfView = 70f;
        public static float oldFieldOfView = 70f;
        private float nextUpdateTime = 0f;
        private const float updateInterval = 10f;

        string[] availableLevels = new[] {
            "Level - Wizard",
            "Level - Shop",
            "Level - Manor",
            "Level - Arctic",
            "Level - Lobby",
            "Level - Recording"
        };
        private Vector2 chatDropdownScroll = Vector2.zero;
        private bool showChatDropdown = false;
        private int ChatSelectedPlayerIndex = 0;
        private string chatMessageText = "D4RK CHEATS :3"; // Default message
        int selectedLevelIndex = 0;
        bool showLevelDropdown = false;
        Vector2 levelDropdownScroll = Vector2.zero;
        public static bool hasInitializedDefaults = false;
        private bool sliderDragging = false;
        private bool dragTargetIsMin = false;
        private Vector2 sourceDropdownScrollPosition = Vector2.zero;
        private Vector2 destDropdownScrollPosition = Vector2.zero;
        private Vector2 enemyTeleportDropdownScrollPosition = Vector2.zero;
        private float levelCheckTimer = 0f;
        private const float LEVEL_CHECK_INTERVAL = 5.0f;
        private string previousLevelName = "";
        private bool pendingLevelUpdate = false;
        private float levelChangeDetectedTime = 0f;
        private const float LEVEL_UPDATE_DELAY = 3.0f;
        public static int selectedPlayerIndex = 0;
        public static List<string> playerNames = new List<string>();
        public static List<object> playerList = new List<object>();
        private int selectedEnemyIndex = 0;
        private List<string> enemyNames = new List<string>();
        private List<Enemy> enemyList = new List<Enemy>();
        public static float oldSliderValue = 0.5f;
        public static float oldSliderValueStrength = 0.5f;
        public static float sliderValue = 0.5f;
        public static float sliderValueStrength = 0.5f;
        public static float offsetESp = 0.5f;
        public static bool showMenu = true;
        public static bool godModeActive = false;
        public static bool debounce = false;
        public static bool infiniteHealthActive = false;
        public static bool stamineState = false;
        public static bool unlimitedBatteryActive = false;
        public static UnlimitedBattery unlimitedBatteryComponent;
        public static bool blindEnemies = false;
        public static bool forceMuteActivated = false;
        public static bool forceUnmute = false;
        private Vector2 playerScrollPosition = Vector2.zero;
        private Vector2 enemyScrollPosition = Vector2.zero;
        private int teleportPlayerSourceIndex = 0;  // Default to first player in list
        private int teleportPlayerDestIndex = 0;  // Default to first player or void
        private string[] teleportPlayerSourceOptions;  // Will contain only player names
        private string[] teleportPlayerDestOptions;    // Will contain player names + "Void"
        private bool showTeleportUI = false;
        private bool showSourceDropdown = false;  // Track source dropdown visibility
        private bool showDestDropdown = false;  // Track destination dropdown visibility
        private bool showEnemyTeleportUI = false;
        private bool showEnemyTeleportDropdown = false;
        private int enemyTeleportDestIndex = 0;
        private string[] enemyTeleportDestOptions;
        private float enemyTeleportLabelWidth = 70f;
        private float enemyTeleportToWidth = 20f;
        private float enemyTeleportDropdownWidth = 200f;
        private float enemyTeleportTotalWidth;
        private float enemyTeleportStartX;

        public static string[] levelsToSearchItems = { "Level - Manor", "Level - Wizard", "Level - Arctic", "Level - Shop", "Level - Lobby", "Level - Recording" };

        private GUIStyle menuStyle;
        private bool initialized = false;
        private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();

        private bool showColorPicker = false;
        private int selectedColorOption = 0; // 0: Enemy Visible, 1: Enemy Hidden, 2: Item Visible, 3: Item Hidden
        private enum MenuCategory { Self, ESP, Combat, Misc, Enemies, Items, Hotkeys }
        private MenuCategory currentCategory = MenuCategory.Self;

        public static float staminaRechargeDelay = 1f;
        public static float staminaRechargeRate = 1f;
        public static float oldStaminaRechargeDelay = 1f;
        public static float oldStaminaRechargeRate = 1f;

        // Spoof Name feature UI state
        private bool spoofNameEnabled = false;
        private string spoofTargetVisibleName = "All";
        private bool spoofDropdownVisible = false;
        public static string spoofedNameText = "";
        private string originalSteamName = Steamworks.SteamClient.Name; // Store real name at startup
        public static bool spoofNameActive = false;
        private float lastSpoofTime = 0f;
        private const float NAME_SPOOF_DELAY = 3f;
        static public bool hasAlreadySpoofed = false;
        private Dictionary<int, bool> playerMuteStates = new Dictionary<int, bool>();

        // Color change UI variables
        private string colorTargetVisibleName = "All";
        private bool colorDropdownVisible = false;
        private string colorIndexText = "1"; // Default color index
        private bool showColorIndexDropdown = false;
        private Vector2 colorIndexScrollPosition = Vector2.zero;
        private Dictionary<int, string> colorNameMapping = new Dictionary<int, string>()
        {
            {0, "White"}, {1, "Grey"}, {2, "Black"},
            {3, "Light Red"}, {4, "Red"}, {5, "Dark Red 1"}, {6, "Dark Red 2"},
            {7, "Hot Pink 1"}, {8, "Hot Pink 2"}, {9, "Bright Purple"}, {10, "Light Purple 1"},
            {11, "Light Purple 2"}, {12, "Purple"}, {13, "Dark Purple 1"}, {14, "Dark Purple 2"},
            {15, "Dark Blue"}, {16, "Blue"}, {17, "Light Blue 1"}, {18, "Light Blue 2"},
            {19, "Cyan"}, {20, "Light Green 1"}, {21, "Light Green 2"}, {22, "Light Green 3"},
            {23, "Green"}, {24, "Green 2"}, {25, "Dark Green 1"}, {26, "Dark Green 2"},
            {27, "Dark Green 3"}, {28, "Light Yellow"}, {29, "Yellow"}, {30, "Dark Yellow"},
            {31, "Orange"}, {32, "Dark Orange"}, {33, "Brown"}, {34, "Olive"},
            {35, "Skin"}
        };
        private int previousItemCount = 0;

        public static float jumpForce = 1f;
        public static float customGravity = 1f;
        public static int extraJumps = 0;
        public static float flashlightIntensity = 1f;
        public static float crouchDelay = 1f;
        public static float crouchSpeed = 1f;
        public static float grabRange = 1f;
        public static float throwStrength = 1f;
        public static float slideDecay = 1f;

        public static float OldflashlightIntensity = 1f;
        public static float OldcrouchDelay = 1f;
        public static float OldjumpForce = 1f;
        public static float OldcustomGravity = 1f;
        public static float OldextraJumps = 1f;
        public static float OldcrouchSpeed = 1f;
        public static float OldgrabRange = 1f;
        public static float OldthrowStrength = 1f;
        public static float OldslideDecay = 1f;

        private List<ItemTeleport.GameItem> itemList = new List<ItemTeleport.GameItem>();
        private int selectedItemIndex = 0;
        private Vector2 itemScrollPosition = Vector2.zero;

        private List<string> availableItemsList = new List<string>();
        private int selectedItemToSpawnIndex = 0;
        private Vector2 itemSpawnerScrollPosition = Vector2.zero;
        private int itemSpawnValue = 45000;
        private bool isChangingItemValue = false;
        private float itemValueSliderPos = 4.0f;
        private bool showItemSpawner = false;
        private bool isHost = false;

        private bool showingActionSelector = false;
        private Vector2 actionSelectorScroll = Vector2.zero;

        private Vector2 selfScrollPosition = Vector2.zero;
        private Vector2 espScrollPosition = Vector2.zero;
        private Vector2 combatScrollPosition = Vector2.zero;
        private Vector2 miscScrollPosition = Vector2.zero;
        private Vector2 enemiesScrollPosition = Vector2.zero;
        private Vector2 itemsScrollPosition = Vector2.zero;
        private Vector2 hotkeyScrollPosition = Vector2.zero;

        private HotkeyManager hotkeyManager; // Reference to the HotkeyManager
        public bool showWatermark = true;
        private float actionSelectorX = 300f;
        private float actionSelectorY = 200f;
        private bool isDraggingActionSelector = false;
        private Vector2 dragOffsetActionSelector;
        private GUIStyle overlayDimStyle;
        private GUIStyle actionSelectorBoxStyle;

        private bool isDragging = false;
        private Vector2 dragOffset;
        private float menuX = 100f;
        private float menuY = 100f;
        private float titleBarHeight = 30f;

        private void CheckIfHost()
        {
            isHost = !SemiFunc.IsMultiplayer() || PhotonNetwork.IsMasterClient;
        }

        private void UpdateTeleportOptions()
        {
            List<string> sourceOptions = new List<string>(); // Create source array with "All" option + players
            sourceOptions.Add("All Players"); // Add "All" as the first option
            sourceOptions.AddRange(playerNames); // Then add all individual players
            teleportPlayerSourceOptions = sourceOptions.ToArray();
            List<string> destOptions = new List<string>(); // Create destination array with players + "The Void"
            destOptions.AddRange(playerNames);       // Add all players
            destOptions.Add("The Void");            // Add void as last option
            teleportPlayerDestOptions = destOptions.ToArray();
            teleportPlayerSourceIndex = 0;  // Reset selections to defaults // Default to "All"
            teleportPlayerDestIndex = teleportPlayerDestOptions.Length - 1;  // Default to void
        }
        private void UpdateEnemyTeleportOptions()
        {
            List<string> destOptions = new List<string>();
            destOptions.AddRange(playerNames); // Add all players (including local player)
            enemyTeleportDestOptions = destOptions.ToArray();
            enemyTeleportDestIndex = 0; // Default to first player
            float centerPoint = menuX + 300f; // Center of the menu area
            enemyTeleportTotalWidth = enemyTeleportLabelWidth + 10f + enemyTeleportToWidth + 10f + enemyTeleportDropdownWidth;
            enemyTeleportStartX = centerPoint - (enemyTeleportTotalWidth / 2);
        }
        private void CheckForLevelChange()
        {
            string currentLevelName = RunManager.instance.levelCurrent != null ? RunManager.instance.levelCurrent.name : ""; // Get current level name
            if (currentLevelName != previousLevelName && !string.IsNullOrEmpty(currentLevelName) && !pendingLevelUpdate) // Check if level has just changed
            {
                DLog.Log($"Level change detected from {previousLevelName} to {currentLevelName}");
                previousLevelName = currentLevelName;
                pendingLevelUpdate = true; // Set the flag and timer for delayed update
                levelChangeDetectedTime = Time.time;
                DLog.Log($"Player lists will update in {LEVEL_UPDATE_DELAY} seconds");
                showSourceDropdown = false; // Reset dropdown states immediately to ensure clean UI after level change
                showDestDropdown = false;
                showEnemyTeleportDropdown = false;
            }
            if (pendingLevelUpdate && Time.time >= levelChangeDetectedTime + LEVEL_UPDATE_DELAY) // Check if it's time to perform the delayed update
            {
                pendingLevelUpdate = false;
                PerformDelayedLevelUpdate();
            }
        }

        private void PerformDelayedLevelUpdate()
        {
            // Cache reflection data once on level change.
            PlayerReflectionCache.CachePlayerControllerData();

            UpdatePlayerList();
            UpdateEnemyList();

            if (showTeleportUI) UpdateTeleportOptions();
            if (showEnemyTeleportUI) UpdateEnemyTeleportOptions();
            if (!Hax2.hasInitializedDefaults)
            {
                PlayerController.LoadDefaultStatsIntoHax2();
                Hax2.hasInitializedDefaults = true;
            }

            // Now reapply all custom stat modifications using our optimized functions
            PlayerController.ReapplyAllStats();

            DLog.Log($"Level update -> Player list: {playerNames.Count} players, Enemy list: {enemyNames.Count} enemies");
        }


        public void Start()
        {
            hotkeyManager = HotkeyManager.Instance;

            availableItemsList = ItemSpawner.GetAvailableItems();

            if (unlimitedBatteryComponent == null)
            {
                GameObject batteryObj = new GameObject("BatteryManager");
                unlimitedBatteryComponent = batteryObj.AddComponent<UnlimitedBattery>();
                DontDestroyOnLoad(batteryObj);
            }

            DebugCheats.texture2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            DebugCheats.texture2.SetPixels(new[] { Color.red, Color.red, Color.red, Color.red });
            DebugCheats.texture2.Apply();

            var playerHealthType = Type.GetType("PlayerHealth, Assembly-CSharp");
            if (playerHealthType != null)
            {
                DLog.Log("playerHealthType is not null");
                Players.playerHealthInstance = FindObjectOfType(playerHealthType);
                DLog.Log(Players.playerHealthInstance != null ? "playerHealthInstance is not null" : "playerHealthInstance null");
            }
            else DLog.Log("playerHealthType null");

            var playerMaxHealth = Type.GetType("ItemUpgradePlayerHealth, Assembly-CSharp");
            if (playerMaxHealth != null)
            {
                Players.playerMaxHealthInstance = FindObjectOfType(playerMaxHealth);
                DLog.Log("playerMaxHealth is not null");
            }
            else DLog.Log("playerMaxHealth null");
        }

        public void Update()
        {
            CheckIfHost();
            levelCheckTimer += Time.deltaTime;
            if (levelCheckTimer >= LEVEL_CHECK_INTERVAL)
            {
                levelCheckTimer = 0f;
                CheckForLevelChange();
            }

            if (Input.GetKeyDown(hotkeyManager.MenuToggleKey))
            {
                Hax2.showMenu = !Hax2.showMenu;

                CursorController.cheatMenuOpen = Hax2.showMenu;
                CursorController.UpdateCursorState();

                DLog.Log("MENU " + Hax2.showMenu);

                if (!Hax2.showMenu) TryUnlockCamera();
                UpdateCursorState();
            }

            if (Input.GetKeyDown(hotkeyManager.ReloadKey)) Start();

            if (Input.GetKeyDown(hotkeyManager.UnloadKey))
            {
                Hax2.showMenu = false;

                CursorController.cheatMenuOpen = Hax2.showMenu;
                CursorController.UpdateCursorState();

                TryUnlockCamera();
                UpdateCursorState();
                Loader.UnloadCheat();
            }

            if (hotkeyManager.ConfiguringHotkey)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        hotkeyManager.ProcessHotkeyConfiguration(key);
                        break;
                    }
                }
            }
            else if (hotkeyManager.ConfiguringSystemKey)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        hotkeyManager.ProcessSystemKeyConfiguration(key);
                        break;
                    }
                }
            }

            if (Mathf.Abs(Hax2.sliderValue - Hax2.oldSliderValue) > 0.01f)
            {
                PlayerController.SetSprintSpeed(Hax2.sliderValue);
                Hax2.oldSliderValue = Hax2.sliderValue;
            }

            if (Mathf.Abs(Hax2.sliderValueStrength - Hax2.oldSliderValueStrength) > 0.01f)
            {
                Strength.MaxStrength();
                Hax2.oldSliderValueStrength = Hax2.sliderValueStrength;
            }

            Strength.UpdateStrength();

            if (RunManager.instance.levelCurrent != null && levelsToSearchItems.Contains(RunManager.instance.levelCurrent.name))
            {
                if (Time.time >= nextUpdateTime)
                {
                    UpdateEnemyList();
                    UpdateItemList();
                    itemList = ItemTeleport.GetItemList();
                    nextUpdateTime = Time.time + updateInterval;
                }

                if (playerColor.isRandomizing)
                {
                    playerColor.colorRandomizer();
                }

                hotkeyManager.CheckAndExecuteHotkeys();

                if (Hax2.showMenu) TryLockCamera();
                if (NoclipController.noclipActive)
                {
                    NoclipController.UpdateMovement();
                }
            }

if (RunManager.instance?.levelCurrent?.name != "Level - Main Menu" && spoofNameActive)
            {  
                if (Time.time - lastSpoofTime >= NAME_SPOOF_DELAY)
                {
                    ChatHijack.ToggleNameSpoofing(spoofNameActive, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
                    DLog.Log("Name spoofing method called successfully");
                    lastSpoofTime = Time.time;
                }
            }
        }

        private void TryLockCamera()
        {
            if (InputManager.instance != null)
            {
                Type type = typeof(InputManager);
                FieldInfo field = type.GetField("disableAimingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    float currentValue = (float)field.GetValue(InputManager.instance);
                    if (currentValue < 2f || currentValue > 10f)
                    {
                        float clampedValue = Mathf.Clamp(currentValue, 2f, 10f);
                        field.SetValue(InputManager.instance, clampedValue);
                    }
                }
                else DLog.LogError("Failed to find field disableAimingTimer.");
            }
            else DLog.LogWarning("InputManager.instance not found!");
        }

        private void TryUnlockCamera()
        {
            if (InputManager.instance != null)
            {
                Type type = typeof(InputManager);
                FieldInfo field = type.GetField("disableAimingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    float currentValue = (float)field.GetValue(InputManager.instance);
                    if (currentValue > 0f)
                    {
                        field.SetValue(InputManager.instance, 0f);
                        DLog.Log("disableAimingTimer reset to 0 (menu closed).");
                    }
                }
                else DLog.LogError("Failed to find field disableAimingTimer.");
            }
            else DLog.LogWarning("InputManager.instance not found!");
        }

        private void UpdateCursorState()
        {
            Cursor.visible = Hax2.showMenu;
            CursorController.cheatMenuOpen = Hax2.showMenu;
            CursorController.UpdateCursorState();
        }

        private void UpdateItemList()
        {
            DebugCheats.valuableObjects.Clear();

            var valuableArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableArray != null)
            {
                DebugCheats.valuableObjects.AddRange(valuableArray);
            }

            var playerDeathHeadArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("PlayerDeathHead, Assembly-CSharp"));
            if (playerDeathHeadArray != null)
            {
                DebugCheats.valuableObjects.AddRange(playerDeathHeadArray);
            }

            itemList = ItemTeleport.GetItemList();
            if (itemList.Count != previousItemCount)
            {
                DLog.Log($"Item list updated: {itemList.Count} items found (including ValuableObject and PlayerDeathHead).");
                previousItemCount = itemList.Count;
            }
        }

        private void UpdateEnemyList()
        {
            enemyNames.Clear();
            enemyList.Clear();

            DebugCheats.UpdateEnemyList();
            enemyList = DebugCheats.enemyList;

            foreach (var enemy in enemyList)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    string enemyName = "Enemy";
                    var enemyParent = enemy.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                    if (enemyParent != null)
                    {
                        var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                        enemyName = nameField?.GetValue(enemyParent) as string ?? "Enemy";
                    }
                    int health = Enemies.GetEnemyHealth(enemy);
                    DebugCheats.enemyHealthCache[enemy] = health;
                    int maxHealth = Enemies.GetEnemyMaxHealth(enemy);
                    float healthPercentage = maxHealth > 0 ? (float)health / maxHealth : 0f;
                    string healthColor = healthPercentage > 0.66f ? "<color=green>" : (healthPercentage > 0.33f ? "<color=yellow>" : "<color=red>");
                    string healthText = health >= 0 ? $"{healthColor}HP: {health}/{maxHealth}</color>" : "<color=gray>HP: Unknown</color>";
                    enemyNames.Add($"{enemyName} [{healthText}]");
                }
            }

            if (enemyNames.Count == 0) enemyNames.Add("No enemies found");
        }

        private void ActionSelectorWindow(int windowID)
        {
            if (GUI.Button(new Rect(370, 5, 20, 20), "X"))
            {
                showingActionSelector = false;
            }

            GUI.DragWindow(new Rect(0, 0, 400, 30));

            Rect scrollViewRect = new Rect(10, 35, 380, 355);
            var availableActions = hotkeyManager.GetAvailableActions();
            Rect contentRect = new Rect(0, 0, 360, availableActions.Count * 35);
            actionSelectorScroll = GUI.BeginScrollView(scrollViewRect, actionSelectorScroll, contentRect);

            for (int i = 0; i < availableActions.Count; i++)
            {
                Rect actionRect = new Rect(0, i * 35, 340, 30);
                if (GUI.Button(actionRect, availableActions[i].Name))
                {
                    hotkeyManager.AssignActionToHotkey(i);
                    showingActionSelector = false;
                }

                if (actionRect.Contains(Event.current.mousePosition))
                {
                    Rect tooltipRect = new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y, 200, 30);
                    GUI.Label(tooltipRect, availableActions[i].Description);
                }
            }

            GUI.EndScrollView();
        }

        private void UpdatePlayerList()
        {
            var fakePlayers = playerNames.Where(name => name.Contains("FakePlayer")).ToList();
            var fakePlayerCount = fakePlayers.Count;

            playerNames.Clear();
            playerList.Clear();

            var players = SemiFunc.PlayerGetList();
            foreach (var player in players)
            {
                playerList.Add(player);
                string baseName = SemiFunc.PlayerGetName(player) ?? "Unknown Player";
                bool isAlive = IsPlayerAlive(player, baseName);
                string statusText = isAlive ? "<color=green>[LIVE]</color> " : "<color=red>[DEAD]</color> ";
                playerNames.Add(statusText + baseName);
            }

            for (int i = 0; i < fakePlayerCount; i++)
            {
                playerNames.Add(fakePlayers[i]);
                playerList.Add(null);
            }

            if (playerNames.Count == 0) playerNames.Add("No player Found");
        }

        private bool IsPlayerAlive(object player, string playerName)
        {
            int health = Players.GetPlayerHealth(player);
            if (health < 0)
            {
                DLog.Log($"Could not get health for {playerName}, assuming dead");
                return true; // If we can't get health, assume player is dead
            }
            return health > 0;
        }

        private void InitializeGUIStyles()
        {
            menuStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeSolidBackground(new Color(0.21f, 0.21f, 0.21f), 0.7f) },
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 10, 10),
                border = new RectOffset(5, 5, 5, 5)
            };

            overlayDimStyle = new GUIStyle();
            overlayDimStyle.normal.background = MakeSolidBackground(new Color(0f, 0f, 0f, 0.5f), 0.5f);

            actionSelectorBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeSolidBackground(new Color(0.25f, 0.25f, 0.25f), 0.95f) },
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 10, 10)
            };

            UIHelper.InitSliderStyles();
        }

        public void OnGUI()
        {
            if (!initialized)
            {
                InitializeGUIStyles();
                initialized = true;
            }
            UIHelper.InitSliderStyles();

            if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool || DebugCheats.draw3DPlayerEspBool || DebugCheats.draw3DItemEspBool || DebugCheats.drawChamsBool) DebugCheats.DrawESP();

            GUIStyle style = new GUIStyle(GUI.skin.label) { wordWrap = false };
            if (showWatermark)
            {
                GUIContent content = new GUIContent($"D.A.R.K CHEAT | {hotkeyManager.MenuToggleKey} - MENU");
                Vector2 size = style.CalcSize(content);
                GUI.Label(new Rect(10, 10, size.x, size.y), content, style);
                GUI.Label(new Rect(10 + size.x + 10, 10, 200, size.y), "MADE BY Github/D4rkks", style);
            }

            if (showingActionSelector) // handle modal first
            {
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);

                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400);

                if (Event.current.type == EventType.MouseDown ||  // trying to only block events that are outside the modal window
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    if (!modalRect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use(); // blocking outer event
                    }


                    modalRect = GUI.Window(12345, modalRect, ActionSelectorWindow, "", actionSelectorBoxStyle);
                    actionSelectorX = modalRect.x;
                    actionSelectorY = modalRect.y;
                }

                GUI.depth = 0;

                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                {
                    return;
                }
            }

            if (showMenu)
            {
                GUIStyle overlayStyle = new GUIStyle();
                overlayStyle.normal.background = MakeSolidBackground(Color.clear, 0f);
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, overlayStyle);

                UpdateCursorState();

                Rect menuRect = new Rect(menuX, menuY, 600, 800);
                Rect titleRect = new Rect(menuX, menuY, 600, titleBarHeight);

                GUI.Box(menuRect, "", menuStyle);
                UIHelper.Begin("D.A.R.K. Menu 1.2", menuX, menuY, 600, 800, 30, 30, 10);

                if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;
                    dragOffset = Event.current.mousePosition - new Vector2(menuX, menuY);
                }
                if (Event.current.type == EventType.MouseUp) isDragging = false;
                if (isDragging && Event.current.type == EventType.MouseDrag)
                {
                    Vector2 newPosition = Event.current.mousePosition - dragOffset;
                    menuX = Mathf.Clamp(newPosition.x, 0, Screen.width - 600);
                    menuY = Mathf.Clamp(newPosition.y, 0, Screen.height - 800);
                }

                float tabWidth = 75f;
                float tabHeight = 40f;
                float spacing = 5f;
                float totalWidth = 7 * tabWidth + 6 * spacing;
                float startX = menuX + (600 - totalWidth) / 2f;

                float contentWidth = 450f;
                float centerX = menuX + (600f - contentWidth) / 2;

                GUIStyle tabStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white, background = MakeSolidBackground(Color.gray, 1f) },
                    hover = { textColor = Color.yellow, background = MakeSolidBackground(new Color(0.2f, 0.2f, 0.2f), 1f) },
                    active = { textColor = Color.green, background = MakeSolidBackground(Color.black, 1f) }
                };
                GUIStyle selectedTabStyle = new GUIStyle(tabStyle)
                {
                    normal = { textColor = Color.white, background = MakeSolidBackground(new Color(0.35f, 0.35f, 0.35f), 1f) }
                };

                if (GUI.Button(new Rect(startX, menuY + 30, tabWidth, tabHeight), "Self", currentCategory == MenuCategory.Self ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Self;
                if (GUI.Button(new Rect(startX + (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "ESP", currentCategory == MenuCategory.ESP ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.ESP;
                if (GUI.Button(new Rect(startX + 2 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Combat", currentCategory == MenuCategory.Combat ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Combat;
                if (GUI.Button(new Rect(startX + 3 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Misc", currentCategory == MenuCategory.Misc ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Misc;
                if (GUI.Button(new Rect(startX + 4 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Enemies", currentCategory == MenuCategory.Enemies ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Enemies;
                if (GUI.Button(new Rect(startX + 5 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Items", currentCategory == MenuCategory.Items ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Items;
                if (GUI.Button(new Rect(startX + 6 * (tabWidth + spacing), menuY + 30, tabWidth, tabHeight), "Hotkeys", currentCategory == MenuCategory.Hotkeys ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Hotkeys;

                GUIStyle instructionStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = Color.white } };
                GUI.Label(new Rect(menuX + 23, menuY + 75, 580, 20), $"Open/Close: {hotkeyManager.MenuToggleKey} | Reload: {hotkeyManager.ReloadKey} | Unload: {hotkeyManager.UnloadKey}", instructionStyle);

                float yPos = 10;
                float parentSpacing = 40;    // Space between main parent options when children are hidden
                float childSpacing = 30;     // Space between child options
                float childIndent = 20;      // Indentation for child options

                switch (currentCategory)
                {
                    case MenuCategory.Self:
                        Rect selfViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float selfContentHeight = 1200; // Calculate based on actual content
                        Rect selfContentRect = new Rect(0, 0, 540, selfContentHeight);
                        selfScrollPosition = GUI.BeginScrollView(selfViewRect, selfScrollPosition, selfContentRect, false, selfContentHeight > selfViewRect.height);

                        float selfYPos = yPos;

                        bool newGodModeState = UIHelper.ButtonBool("God Mode", godModeActive, 0, selfYPos);
                        if (newGodModeState != godModeActive) { PlayerController.GodMode(); godModeActive = newGodModeState; DLog.Log("God mode toggled: " + godModeActive); }
                        selfYPos += parentSpacing;

                        bool newNoclipActive = UIHelper.ButtonBool("Noclip", NoclipController.noclipActive, 0, selfYPos);
                        if (newNoclipActive != NoclipController.noclipActive) { NoclipController.ToggleNoclip(); }
                        selfYPos += parentSpacing;

                        bool newHealState = UIHelper.ButtonBool("Infinite Health", infiniteHealthActive, 0, selfYPos);
                        if (newHealState != infiniteHealthActive) { infiniteHealthActive = newHealState; PlayerController.MaxHealth(); }
                        selfYPos += parentSpacing;

                        bool newStaminaState = UIHelper.ButtonBool("Infinite Stamina", stamineState, 0, selfYPos);
                        if (newStaminaState != stamineState) { stamineState = newStaminaState; PlayerController.MaxStamina(); DLog.Log("God mode toggled: " + stamineState); }
                        selfYPos += parentSpacing;

                        bool newUnlimitedBatteryState = UIHelper.ButtonBool("[HOST] Unlimited Battery", unlimitedBatteryActive, 0, selfYPos);
                        if (newUnlimitedBatteryState != unlimitedBatteryActive)
                        {
                            unlimitedBatteryActive = newUnlimitedBatteryState;
                            if (unlimitedBatteryComponent != null)
                                unlimitedBatteryComponent.unlimitedBatteryEnabled = unlimitedBatteryActive;
                        }
                        selfYPos += parentSpacing;

                        bool newTumbleGuardActive = UIHelper.ButtonBool("Grab Guard", Hax2.debounce, 0, selfYPos);
                        if (newTumbleGuardActive != Hax2.debounce) { PlayerTumblePatch.ToggleTumbleGuard(); }
                        selfYPos += parentSpacing;

                        bool newPlayerColorState = UIHelper.ButtonBool("RGB Player", playerColor.isRandomizing, 0, selfYPos);
                        if (newPlayerColorState != playerColor.isRandomizing)
                        {
                            playerColor.isRandomizing = newPlayerColorState;
                            DLog.Log("Randomize toggled: " + playerColor.isRandomizing);
                        }
                        selfYPos += parentSpacing;

                        UIHelper.Label("Strength: " + sliderValueStrength, 0, selfYPos);
                        selfYPos += childIndent;
                        oldSliderValueStrength = sliderValueStrength;
                        sliderValueStrength = UIHelper.Slider(sliderValueStrength, 1f, 100f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("[HOST] Throw Strength: " + Hax2.throwStrength, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.throwStrength = UIHelper.Slider(Hax2.throwStrength, 0f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Grab Range: " + Hax2.grabRange, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.grabRange = UIHelper.Slider(Hax2.grabRange, 0f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Sprint Speed: " + Hax2.sliderValue, 0, selfYPos);
                        selfYPos += childIndent;
                        oldSliderValue = sliderValue;
                        Hax2.sliderValue = UIHelper.Slider(Hax2.sliderValue, 1f, 30f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Stamina Recharge Delay: " + Hax2.staminaRechargeDelay, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.staminaRechargeDelay = UIHelper.Slider(Hax2.staminaRechargeDelay, 0f, 10f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Stamina Recharge Rate: " + Hax2.staminaRechargeRate, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.staminaRechargeRate = UIHelper.Slider(Hax2.staminaRechargeRate, 1f, 20f, 0, selfYPos);
                        if (Hax2.staminaRechargeDelay != oldStaminaRechargeDelay || Hax2.staminaRechargeRate != oldStaminaRechargeRate)
                        {
                            PlayerController.DecreaseStaminaRechargeDelay(Hax2.staminaRechargeDelay, Hax2.staminaRechargeRate);
                            DLog.Log($"Stamina recharge updated: Delay={Hax2.staminaRechargeDelay}x, Rate={Hax2.staminaRechargeRate}x");
                            oldStaminaRechargeDelay = Hax2.staminaRechargeDelay;
                            oldStaminaRechargeRate = Hax2.staminaRechargeRate;
                        }
                        selfYPos += childIndent;

                        UIHelper.Label("Extra Jumps: " + Hax2.extraJumps, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.extraJumps = (int)UIHelper.Slider(Hax2.extraJumps, 1f, 100f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Jump Force: " + Hax2.jumpForce, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.jumpForce = UIHelper.Slider(Hax2.jumpForce, 1f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Gravity: " + Hax2.customGravity, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.customGravity = UIHelper.Slider(Hax2.customGravity, -10f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Crouch Delay: " + Hax2.crouchDelay, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.crouchDelay = UIHelper.Slider(Hax2.crouchDelay, 0f, 5f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Crouch Speed: " + Hax2.crouchSpeed, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.crouchSpeed = UIHelper.Slider(Hax2.crouchSpeed, 1f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Slide Decay: " + Hax2.slideDecay, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.slideDecay = UIHelper.Slider(Hax2.slideDecay, -10f, 50f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Flashlight Intensity: " + Hax2.flashlightIntensity, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.flashlightIntensity = UIHelper.Slider(Hax2.flashlightIntensity, 1f, 100f, 0, selfYPos);
                        selfYPos += childIndent;

                        if (Hax2.crouchDelay != OldcrouchDelay)
                        {
                            PlayerController.SetCrouchDelay(Hax2.crouchDelay);
                            OldcrouchDelay = Hax2.crouchDelay;
                        }
                        if (Hax2.jumpForce != Hax2.OldjumpForce)
                        {
                            PlayerController.SetJumpForce(Hax2.jumpForce);
                            OldjumpForce = Hax2.jumpForce;
                        }
                        if (Hax2.customGravity != Hax2.OldcustomGravity)
                        {
                            PlayerController.SetCustomGravity(Hax2.customGravity);
                            OldcustomGravity = Hax2.customGravity;
                        }
                        if (Hax2.extraJumps != Hax2.OldextraJumps)
                        {
                            PlayerController.SetExtraJumps(Hax2.extraJumps);
                            OldextraJumps = Hax2.extraJumps;
                        }
                        if (Hax2.crouchSpeed != Hax2.OldcrouchSpeed)
                        {
                            PlayerController.SetCrouchSpeed(Hax2.crouchSpeed);
                            OldcrouchSpeed = Hax2.crouchSpeed;
                        }
                        if (Hax2.grabRange != Hax2.OldgrabRange)
                        {
                            PlayerController.SetGrabRange(Hax2.grabRange);
                            OldgrabRange = Hax2.grabRange;
                        }
                        if (Hax2.throwStrength != Hax2.OldthrowStrength)
                        {
                            PlayerController.SetThrowStrength(Hax2.throwStrength);
                            OldthrowStrength = Hax2.throwStrength;
                        }
                        if (Hax2.slideDecay != Hax2.OldslideDecay)
                        {
                            PlayerController.SetSlideDecay(Hax2.slideDecay);
                            OldslideDecay = Hax2.slideDecay;
                        }
                        if (Hax2.flashlightIntensity != OldflashlightIntensity)
                        {
                            PlayerController.SetFlashlightIntensity(Hax2.flashlightIntensity);
                            OldflashlightIntensity = Hax2.flashlightIntensity;
                        }

                        // Ensure FOVEditor exists
                        if (FOVEditor.Instance == null)
                        {
                            GameObject fovObject = new GameObject("FOVEditor");
                            fovObject.AddComponent<FOVEditor>();
                        }

                        // Wait for Instance to initialize before using
                        if (FOVEditor.Instance != null)
                        {
                            float currentFOV = FOVEditor.Instance.GetFOV();
                            UIHelper.Label("Field of View: " + Mathf.RoundToInt(currentFOV), 0, selfYPos);
                            selfYPos += childIndent;

                            float newFOV = UIHelper.Slider(currentFOV, 60f, 120f, 0, selfYPos);
                            if (newFOV != currentFOV)
                            {
                                FOVEditor.Instance.SetFOV(newFOV);
                                Hax2.fieldOfView = newFOV;
                            }
                            selfYPos += childIndent;
                        }
                        else
                        {
                            UIHelper.Label("Loading FOV Editor...", 0, selfYPos);
                            selfYPos += childIndent * 2;
                        }


                        GUI.EndScrollView();
                        break;

                    case MenuCategory.ESP:
                        Rect espViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float espContentHeight = 1000; // Calculate based on actual content
                        Rect espContentRect = new Rect(0, 0, 540, espContentHeight);
                        espScrollPosition = GUI.BeginScrollView(espViewRect, espScrollPosition, espContentRect, false, espContentHeight > espViewRect.height);

                        float espYPos = yPos;

                        // Enemy ESP section
                        DebugCheats.drawEspBool = UIHelper.Checkbox("Enemy ESP", DebugCheats.drawEspBool, 0, espYPos);
                        espYPos += DebugCheats.drawEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawEspBool)
                        {
                            DebugCheats.showEnemyBox = UIHelper.Checkbox("2D Box", DebugCheats.showEnemyBox, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.drawChamsBool = UIHelper.Checkbox("Chams", DebugCheats.drawChamsBool, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showEnemyNames = UIHelper.Checkbox("Names", DebugCheats.showEnemyNames, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showEnemyDistance = UIHelper.Checkbox("Distance", DebugCheats.showEnemyDistance, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showEnemyHP = UIHelper.Checkbox("Health", DebugCheats.showEnemyHP, 20, espYPos);
                            espYPos += childSpacing;

                        }

                        // Item ESP section
                        DebugCheats.drawItemEspBool = UIHelper.Checkbox("Item ESP", DebugCheats.drawItemEspBool, 0, espYPos);
                        espYPos += DebugCheats.drawItemEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawItemEspBool)
                        {
                            DebugCheats.draw3DItemEspBool = UIHelper.Checkbox("3D Box", DebugCheats.draw3DItemEspBool, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.drawItemChamsBool = UIHelper.Checkbox("Chams", DebugCheats.drawItemChamsBool, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showItemNames = UIHelper.Checkbox("Names", DebugCheats.showItemNames, 20, espYPos);
                            espYPos += childSpacing;

                            DebugCheats.showItemDistance = UIHelper.Checkbox("Distance", DebugCheats.showItemDistance, 20, espYPos);
                            espYPos += childSpacing;

                            // Item Distance slider (only shown when Show Item Distance is enabled)
                            if (DebugCheats.showItemDistance)
                            {
                                GUI.Label(new Rect(40, espYPos, 200, 20), $"Max Item Distance: {DebugCheats.maxItemEspDistance:F0}m");
                                espYPos += childIndent;
                                DebugCheats.maxItemEspDistance = GUI.HorizontalSlider(new Rect(40, espYPos, 200, 20), DebugCheats.maxItemEspDistance, 0f, 1000f);
                                espYPos += childIndent;
                            }

                            DebugCheats.showItemValue = UIHelper.Checkbox("Value", DebugCheats.showItemValue, 20, espYPos);
                            espYPos += childSpacing;

                            // Value Range Slider (only shown when Show Item Value is enabled)
                            if (DebugCheats.showItemValue)
                            {
                                GUI.Label(new Rect(40, espYPos, 200, 20), $"Min Item Value: ${DebugCheats.minItemValue}");
                                espYPos += childIndent;

                                // Simple min value slider
                                DebugCheats.minItemValue = Mathf.RoundToInt(GUI.HorizontalSlider(
                                    new Rect(40, espYPos, 200, 20),
                                    DebugCheats.minItemValue, 0, 50000));
                                espYPos += childIndent;
                            }

                            DebugCheats.showPlayerDeathHeads = UIHelper.Checkbox("Dead Player Heads", DebugCheats.showPlayerDeathHeads, 20, espYPos);
                            espYPos += childSpacing;
                        }

                        // Chams Color Picker Section
                        if (DebugCheats.drawChamsBool || DebugCheats.drawItemChamsBool)
                        {
                            if (GUI.Button(new Rect(0, espYPos, 200, 30), "Configure Chams Colors"))
                            {
                                showColorPicker = !showColorPicker;
                            }
                            espYPos += parentSpacing;

                            float currentY = 10;

                            if (showColorPicker)
                            {
                                // Color option selection
                                GUI.Label(new Rect(280, currentY, 200, 20), "         Select color to modify:", GUI.skin.label);
                                currentY += childIndent;

                                string[] colorOptions = new string[] {
                                    "Enemy Visible",
                                    "Enemy Hidden",
                                    "Item Visible",
                                    "Item Hidden"
                                };

                                for (int i = 0; i < colorOptions.Length; i++)
                                {
                                    bool isSelected = selectedColorOption == i;
                                    GUIStyle optionStyle = new GUIStyle(GUI.skin.button);

                                    // Get the current color for preview
                                    Color previewColor;
                                    switch (i)
                                    {
                                        case 0: previewColor = DebugCheats.enemyVisibleColor; break;
                                        case 1: previewColor = DebugCheats.enemyHiddenColor; break;
                                        case 2: previewColor = DebugCheats.itemVisibleColor; break;
                                        case 3: previewColor = DebugCheats.itemHiddenColor; break;
                                        default: previewColor = Color.white; break;
                                    }

                                    // Set button background to the current color
                                    optionStyle.normal.background = MakeSolidBackground(previewColor, 1f);
                                    optionStyle.normal.textColor = GetContrastColor(previewColor);

                                    if (GUI.Button(new Rect(285, currentY, 200, 30), colorOptions[i], optionStyle))
                                    {
                                        selectedColorOption = i;
                                    }
                                    currentY += childSpacing;
                                }
                                currentY += childIndent;

                                // Get the current color based on selection
                                Color currentColor;
                                switch (selectedColorOption)
                                {
                                    case 0: currentColor = DebugCheats.enemyVisibleColor; break;
                                    case 1: currentColor = DebugCheats.enemyHiddenColor; break;
                                    case 2: currentColor = DebugCheats.itemVisibleColor; break;
                                    case 3: currentColor = DebugCheats.itemHiddenColor; break;
                                    default: currentColor = Color.white; break;
                                }

                                // RGB sliders
                                GUI.Label(new Rect(285, currentY, 200, 20), "Red:", GUI.skin.label);
                                currentY += childIndent;
                                float r = GUI.HorizontalSlider(new Rect(285, currentY, 200, 20), currentColor.r, 0f, 1f);
                                currentY += childSpacing;

                                GUI.Label(new Rect(285, currentY, 200, 20), "Green:", GUI.skin.label);
                                currentY += childIndent;
                                float g = GUI.HorizontalSlider(new Rect(285, currentY, 200, 20), currentColor.g, 0f, 1f);
                                currentY += childSpacing;

                                GUI.Label(new Rect(285, currentY, 200, 20), "Blue:", GUI.skin.label);
                                currentY += childIndent;
                                float b = GUI.HorizontalSlider(new Rect(285, currentY, 200, 20), currentColor.b, 0f, 1f);
                                currentY += childSpacing;

                                GUI.Label(new Rect(285, currentY, 200, 20), "Opacity:", GUI.skin.label);
                                currentY += childIndent;
                                float a = GUI.HorizontalSlider(new Rect(285, currentY, 200, 20), currentColor.a, 0f, 1f);
                                currentY += childSpacing;

                                // Update the color if any slider changed
                                Color newColor = new Color(r, g, b, a);
                                if (newColor != currentColor)
                                {
                                    switch (selectedColorOption)
                                    {
                                        case 0: DebugCheats.enemyVisibleColor = newColor; break;
                                        case 1: DebugCheats.enemyHiddenColor = newColor; break;
                                        case 2: DebugCheats.itemVisibleColor = newColor; break;
                                        case 3: DebugCheats.itemHiddenColor = newColor; break;
                                    }
                                }
                            }
                        }

                        // Extraction ESP section
                        DebugCheats.drawExtractionPointEspBool = UIHelper.Checkbox("Extraction ESP", DebugCheats.drawExtractionPointEspBool, 0, espYPos);
                        espYPos += DebugCheats.drawExtractionPointEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawExtractionPointEspBool)
                        {
                            DebugCheats.showExtractionNames = UIHelper.Checkbox("Name/Status", DebugCheats.showExtractionNames, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showExtractionDistance = UIHelper.Checkbox("Distance", DebugCheats.showExtractionDistance, 20, espYPos);
                            espYPos += childSpacing;
                        }

                        // Player ESP section
                        DebugCheats.drawPlayerEspBool = UIHelper.Checkbox("Player ESP", DebugCheats.drawPlayerEspBool, 0, espYPos);
                        espYPos += DebugCheats.drawPlayerEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawPlayerEspBool)
                        {
                            DebugCheats.draw2DPlayerEspBool = UIHelper.Checkbox("2D Box", DebugCheats.draw2DPlayerEspBool, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.draw3DPlayerEspBool = UIHelper.Checkbox("3D Box", DebugCheats.draw3DPlayerEspBool, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showPlayerNames = UIHelper.Checkbox("Names", DebugCheats.showPlayerNames, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showPlayerDistance = UIHelper.Checkbox("Distance", DebugCheats.showPlayerDistance, 20, espYPos);
                            espYPos += childSpacing;
                            DebugCheats.showPlayerHP = UIHelper.Checkbox("Health", DebugCheats.showPlayerHP, 20, espYPos);
                            espYPos += childSpacing;
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Combat:
                        Rect combatViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float combatContentHeight = 1000; // Calculate based on actual content
                        Rect combatContentRect = new Rect(0, 0, 540, combatContentHeight);
                        combatScrollPosition = GUI.BeginScrollView(combatViewRect, combatScrollPosition, combatContentRect, false, combatContentHeight > combatViewRect.height);

                        float combatYPos = yPos;

                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", 0, combatYPos);
                        combatYPos += childIndent;

                        float playerListItemHeight = 35; // Calculate the actual height needed for the player list
                        float playerListContentHeight = playerNames.Count * playerListItemHeight;
                        float playerListViewHeight = Math.Min(200, Math.Max(playerListContentHeight, 35)); // Min height of 35 for at least one row

                        playerScrollPosition = GUI.BeginScrollView(
                            new Rect(0, combatYPos, 540, playerListViewHeight),
                            playerScrollPosition,
                            new Rect(0, 0, 520, playerListContentHeight),
                            false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * playerListItemHeight, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        combatYPos += playerListViewHeight + 15;

                        if (UIHelper.Button("Revive", 0, combatYPos)) { Players.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Player revived: " + playerNames[selectedPlayerIndex]); }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button("Kill", 0, combatYPos)) { Players.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Player killed: " + playerNames[selectedPlayerIndex]); }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button("Max Heal", 0, combatYPos))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Players.HealPlayer(playerList[selectedPlayerIndex], 50, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} healed.");
                            }
                            else
                            {
                                DLog.Log("No valid player selected to heal!");
                            }
                        }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button("-1 Damage", 0, combatYPos))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Players.DamagePlayer(playerList[selectedPlayerIndex], 1, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} damaged.");
                            }
                            else
                            {
                                DLog.Log("No valid player selected to damage!");
                            }
                        }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button("Tumble", 0, combatYPos)) { Players.ForcePlayerTumble(); DLog.Log("Player tumbled: " + playerNames[selectedPlayerIndex]); }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button(showTeleportUI ? "Hide Teleport Options" : "Teleport Options", 0, combatYPos))
                        {
                            showTeleportUI = !showTeleportUI;
                            if (showTeleportUI)
                            {
                                UpdateTeleportOptions();
                            }
                        }
                        combatYPos += parentSpacing;

                        if (showTeleportUI)
                        {
                            float sourceDropdownWidth = 180;
                            float toTextWidth = 15;
                            float destDropdownWidth = 180;
                            float tpCenterX = 270;
                            float tpSpacing = 15;
                            float tpStartX = tpCenterX - ((sourceDropdownWidth + tpSpacing + toTextWidth + tpSpacing + destDropdownWidth) / 2);

                            float sourceYPos = combatYPos;
                            string currentSource = teleportPlayerSourceIndex >= 0 && teleportPlayerSourceIndex < teleportPlayerSourceOptions.Length ?
                                teleportPlayerSourceOptions[teleportPlayerSourceIndex] : "No source available";

                            if (GUI.Button(new Rect(tpStartX, combatYPos, sourceDropdownWidth, 25), currentSource)) showSourceDropdown = !showSourceDropdown;

                            GUI.Label(new Rect(tpStartX + sourceDropdownWidth + tpSpacing, combatYPos, toTextWidth, 25), "to");

                            string currentDestination = teleportPlayerDestIndex >= 0 && teleportPlayerDestIndex < teleportPlayerDestOptions.Length ?
                                teleportPlayerDestOptions[teleportPlayerDestIndex] : "No destination available";

                            if (GUI.Button(new Rect(tpStartX + sourceDropdownWidth + tpSpacing + toTextWidth + tpSpacing, combatYPos, destDropdownWidth, 25), currentDestination)) showDestDropdown = !showDestDropdown;
                            combatYPos += parentSpacing;


                            if (showSourceDropdown)
                            {
                                int itemHeight = 25;
                                int maxVisibleItems = 6;
                                int visibleItems = Math.Min(teleportPlayerSourceOptions.Length, maxVisibleItems);
                                float dropdownHeight = visibleItems * itemHeight;

                                Rect dropdownRect = new Rect(tpStartX, sourceYPos + 25, sourceDropdownWidth, dropdownHeight);

                                float contentHeight = teleportPlayerSourceOptions.Length * itemHeight;

                                // Adjust content height to account for skipping the selected item
                                if (teleportPlayerSourceIndex >= 0 && teleportPlayerSourceIndex < teleportPlayerSourceOptions.Length)
                                    contentHeight -= itemHeight;

                                sourceDropdownScrollPosition = GUI.BeginScrollView(dropdownRect, sourceDropdownScrollPosition, new Rect(0, 0, 180, contentHeight));

                                int displayedIndex = 0;
                                for (int i = 0; i < teleportPlayerSourceOptions.Length; i++)
                                {
                                    if (i != teleportPlayerSourceIndex)
                                    {
                                        if (GUI.Button(new Rect(0, displayedIndex * itemHeight, 180, itemHeight), teleportPlayerSourceOptions[i]))
                                        {
                                            teleportPlayerSourceIndex = i;
                                        }
                                        displayedIndex++;
                                    }
                                }
                                GUI.EndScrollView();
                            }

                            if (showDestDropdown)
                            {
                                int itemHeight = 25;
                                int maxVisibleItems = 6;
                                int visibleItems = Math.Min(teleportPlayerDestOptions.Length, maxVisibleItems);
                                float dropdownHeight = visibleItems * itemHeight;

                                Rect dropdownRect = new Rect(tpStartX + destDropdownWidth + tpSpacing + toTextWidth + tpSpacing, sourceYPos + 25, destDropdownWidth, dropdownHeight);

                                float contentHeight = teleportPlayerDestOptions.Length * itemHeight;

                                // Adjust content height to account for skipping the selected item
                                if (teleportPlayerDestIndex >= 0 && teleportPlayerDestIndex < teleportPlayerDestOptions.Length)
                                    contentHeight -= itemHeight;

                                destDropdownScrollPosition = GUI.BeginScrollView(dropdownRect, destDropdownScrollPosition, new Rect(0, 0, 180, contentHeight));

                                int displayedIndex = 0;
                                for (int i = 0; i < teleportPlayerDestOptions.Length; i++)
                                {
                                    if (i != teleportPlayerDestIndex)
                                    {
                                        if (GUI.Button(new Rect(0, displayedIndex * itemHeight, 180, itemHeight), teleportPlayerDestOptions[i]))
                                        {
                                            teleportPlayerDestIndex = i;
                                        }
                                        displayedIndex++;
                                    }
                                }
                                GUI.EndScrollView();
                            }

                            float executeButtonYPos = combatYPos + 10;

                            float sourceDropdownOffset = 0;
                            float destDropdownOffset = 0;

                            if (showSourceDropdown && teleportPlayerSourceOptions.Length > 0)
                                sourceDropdownOffset = Math.Min(teleportPlayerSourceOptions.Length, 6) * 25;
                            if (showDestDropdown && teleportPlayerDestOptions.Length > 0)
                                destDropdownOffset = Math.Min(teleportPlayerDestOptions.Length, 6) * 25;
                            executeButtonYPos += Math.Max(sourceDropdownOffset, destDropdownOffset);
                            if (GUI.Button(new Rect(tpCenterX - 75, executeButtonYPos, 150, 25), "Execute Teleport"))
                            {
                                Teleport.ExecuteTeleportWithSeparateOptions(
                                    teleportPlayerSourceIndex,
                                    teleportPlayerDestIndex,
                                    teleportPlayerSourceOptions,
                                    teleportPlayerDestOptions,
                                    playerList);
                                showSourceDropdown = false;
                                showDestDropdown = false;

                                DLog.Log("Teleport executed successfully");
                            }
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Misc:
                        Rect miscViewRect = new Rect(menuX + 30, menuY + 95, 560, 700); // visible scroll area
                        float miscContentHeight = 1200f; // adjust this to match your actual layout height
                        Rect miscContentRect = new Rect(0, 0, 540, miscContentHeight); // content bounds

                        miscScrollPosition = GUI.BeginScrollView(miscViewRect, miscScrollPosition, miscContentRect, false, miscContentHeight > miscViewRect.height);

                        float miscYPos = yPos;

                        // Everything inside here gets scrolled:
                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", 0, miscYPos);
                        miscYPos += childIndent;

                        float miscPlayerListItemHeight = 35;
                        float miscPlayerListContentHeight = playerNames.Count * miscPlayerListItemHeight;
                        float miscPlayerListViewHeight = Math.Min(200, Math.Max(miscPlayerListContentHeight, 35));

                        playerScrollPosition = GUI.BeginScrollView(
                            new Rect(0, miscYPos, 540, miscPlayerListViewHeight),
                            playerScrollPosition,
                            new Rect(0, 0, 520, miscPlayerListContentHeight),
                            false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            GUI.color = (i == selectedPlayerIndex) ? Color.white : Color.gray;
                            if (GUI.Button(new Rect(0, i * miscPlayerListItemHeight, 520, 30), playerNames[i]))
                                selectedPlayerIndex = i;
                        }
                        GUI.color = Color.white;
                        GUI.EndScrollView();
                        miscYPos += miscPlayerListViewHeight + 15;

                        if (!playerMuteStates.ContainsKey(selectedPlayerIndex))
                        {
                            playerMuteStates[selectedPlayerIndex] = false;
                        }
                        bool newMuteState = UIHelper.ButtonBool("Force Mute", playerMuteStates[selectedPlayerIndex], 0, miscYPos);
                        if (newMuteState != playerMuteStates[selectedPlayerIndex])
                        {
                            playerMuteStates[selectedPlayerIndex] = newMuteState;
                            if (newMuteState)
                            {
                                MiscFeatures.ForcePlayerMicVolume(-9999999);
                                DLog.Log($"Muted player: {playerNames[selectedPlayerIndex]}");
                            }
                            else
                            {
                                MiscFeatures.ForcePlayerMicVolume(100);
                                DLog.Log($"Unmuted player: {playerNames[selectedPlayerIndex]}");
                            }
                        }
                        miscYPos += parentSpacing;

                        if (UIHelper.Button("Force High Volume", 0, miscYPos)) { MiscFeatures.ForcePlayerMicVolume(9999999); forceUnmute = false; }
                        miscYPos += parentSpacing;

                        if (UIHelper.Button("Kick Player", 0, miscYPos)) { MiscFeatures.CrashSelectedPlayerNew(); DLog.Log("Attempting to crash player: " + playerNames[selectedPlayerIndex]); }
                        miscYPos += parentSpacing;

                        if (UIHelper.Button("Crash Lobby", 0, miscYPos))
                        {
                            DLog.Log("Crashing Lobby!");
                            GameObject localPlayer = DebugCheats.GetLocalPlayer();
                            if (localPlayer == null)
                                return;
                            Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                            transform.position = targetPosition;
                            CrashLobby.Crash(targetPosition);
                        }
                        miscYPos += parentSpacing;

                        // Force Host section
                        float screenWidth = 540f; // Total width of the scroll view
                        float buttonSpacing = 10f; // Small spacing between buttons
                        float buttonHeight = 30f;

                        float hostButtonWidth = (screenWidth / 2) - (buttonSpacing / 2); // Make Force Host button take up left half of screen (minus small spacing)
                        float levelButtonWidth = (screenWidth / 2) - (buttonSpacing / 2); // Level selector takes the right half (minus small spacing)

                        if (GUI.Button(new Rect(0, miscYPos, hostButtonWidth, buttonHeight), "Force Host & Start"))
                        { // Draw "Force Host" button aligned to the left
                            ForceHost.Instance.StartCoroutine(ForceHost.Instance.ForceStart(availableLevels[selectedLevelIndex]));
                        }

                        if (GUI.Button(new Rect(hostButtonWidth + buttonSpacing, miscYPos, levelButtonWidth, buttonHeight), availableLevels[selectedLevelIndex]))
                        { // Draw dropdown toggle button - fills the rest of the space to the right
                            showLevelDropdown = !showLevelDropdown;
                        }
                        miscYPos += buttonHeight + 10f;

                        if (showLevelDropdown) // Dropdown menu
                        {
                            int itemHeight = 25;
                            int maxVisibleItems = 6;
                            int visibleItems = Mathf.Min(availableLevels.Length, maxVisibleItems);
                            float dropdownHeight = visibleItems * itemHeight;
                            Rect dropdownRect = new Rect(hostButtonWidth + buttonSpacing, miscYPos, levelButtonWidth, dropdownHeight);
                            Rect dropdownContentRect = new Rect(0, 0, levelButtonWidth - 20, availableLevels.Length * itemHeight);
                            levelDropdownScroll = GUI.BeginScrollView(dropdownRect, levelDropdownScroll, dropdownContentRect, false, true);
                            for (int i = 0; i < availableLevels.Length; i++)
                            {
                                if (GUI.Button(new Rect(0, i * itemHeight, levelButtonWidth - 20, itemHeight), availableLevels[i]))
                                {
                                    selectedLevelIndex = i;
                                    showLevelDropdown = false;
                                }
                            }
                            GUI.EndScrollView();
                            miscYPos += dropdownHeight + 5f;
                        }

                        // === Spoof Name ===
                        float spoofButtonWidth = 120f;
                        float spoofDropdownWidth = 130f;
                        float spoofSpacing = 10f;
                        float spoofHeight = 30f;
                        float totalSpoofRowWidth = spoofButtonWidth + spoofSpacing + spoofDropdownWidth + spoofSpacing + (540f - spoofButtonWidth - spoofDropdownWidth - (2 * spoofSpacing));
                        float spoofStartX = (540f - totalSpoofRowWidth) / 2f;
                        float spoofTextBoxWidth = 540f - spoofButtonWidth - spoofDropdownWidth - (2 * spoofSpacing);

                        if (GUI.Button(new Rect(spoofStartX, miscYPos, spoofButtonWidth, spoofHeight), "Spoof Name"))
                        {
                            if (!string.IsNullOrEmpty(spoofedNameText))
                            {
                                ChatHijack.ToggleNameSpoofing(true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
                                DLog.Log("Spoofed name to " + spoofedNameText);
                            }
                            else
                            {
                                DLog.Log("Please enter a name to spoof");
                            }
                        }

                        if (GUI.Button(new Rect(spoofStartX + spoofButtonWidth + spoofSpacing, miscYPos, spoofDropdownWidth, spoofHeight), spoofTargetVisibleName))
                        { // [Dropdown: Player List]
                            spoofDropdownVisible = !spoofDropdownVisible;
                        }

                        spoofedNameText = GUI.TextField( // [Text Box: Spoofed Name]
                            new Rect(spoofStartX + spoofButtonWidth + spoofSpacing + spoofDropdownWidth + spoofSpacing, miscYPos, spoofTextBoxWidth, spoofHeight),
                            spoofedNameText);
                        miscYPos += spoofHeight + 5f;

                        if (spoofDropdownVisible) // Dropdown buttons (if expanded)
                        {
                            for (int i = 0; i < playerNames.Count + 1; i++)
                            {
                                string name = (i == 0) ? "All" : playerNames[i - 1];
                                if (GUI.Button(new Rect(spoofStartX + spoofButtonWidth + spoofSpacing, miscYPos, spoofDropdownWidth, 25f), name))
                                {
                                    spoofTargetVisibleName = name;
                                    spoofDropdownVisible = false;

                                    if (!string.IsNullOrEmpty(spoofedNameText))
                                    { // Update spoofing if text is entered when changing target
                                        ChatHijack.ToggleNameSpoofing(true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
                                        DLog.Log("Updated spoofing target to " + spoofTargetVisibleName);
                                    }
                                }
                                miscYPos += 25f;
                            }
                            miscYPos += 5f;
                        }

                        // Add a new button to reset the spoofed name
                        if (GUI.Button(new Rect(spoofStartX, miscYPos, 540, 30), "Reset Spoofed Name"))
                        {
                            ChatHijack.ToggleNameSpoofing(false, "", spoofTargetVisibleName, playerList, playerNames);
                            spoofedNameText = "";
                            DLog.Log("Reset names to original and cleared text.");
                        }
                        miscYPos += 35f;

                        bool newSpoofNameState = UIHelper.ButtonBool("Toggle Spoof Name", spoofNameActive, 0, miscYPos);
                        if (newSpoofNameState != spoofNameActive) { spoofNameActive = newSpoofNameState; Debug.Log("Spoof Name Toggled: " + spoofNameActive); }
                        miscYPos += parentSpacing;


                        // === Color Change ===
                        float colorButtonWidth = 120f;
                        float colorDropdownWidth = 130f;
                        float colorSpacing = 10f;
                        float colorHeight = 30f;
                        float totalColorRowWidth = colorButtonWidth + colorSpacing + colorDropdownWidth + colorSpacing + (540f - colorButtonWidth - colorDropdownWidth - (2 * colorSpacing));
                        float colorStartX = (540f - totalColorRowWidth) / 2f;
                        float colorTextBoxWidth = 540f - colorButtonWidth - colorDropdownWidth - (2 * colorSpacing);

                        if (GUI.Button(new Rect(colorStartX, miscYPos, colorButtonWidth, colorHeight), "Spoof Color"))
                        {
                            string targetName = colorTargetVisibleName;
                            int colorIndex;
                            if (int.TryParse(colorIndexText, out colorIndex))
                            {
                                // Use the full range of colors (0-35)
                                colorIndex = Mathf.Clamp(colorIndex, 0, 35);
                                if (colorNameMapping.ContainsKey(colorIndex))
                                {
                                    ChatHijack.ChangePlayerColor(colorIndex, targetName, playerList, playerNames);
                                    DLog.Log($"Changed color to {colorIndex} ({colorNameMapping[colorIndex]}) for {targetName}");
                                }
                                else
                                {
                                    DLog.Log($"Invalid color index: {colorIndex}");
                                }
                            }
                        }

                        if (GUI.Button(new Rect(colorStartX + colorButtonWidth + colorSpacing, miscYPos, colorDropdownWidth, colorHeight), colorTargetVisibleName))
                        {
                            colorDropdownVisible = !colorDropdownVisible;
                        }

                        if (GUI.Button(new Rect(colorStartX + colorButtonWidth + colorSpacing + colorDropdownWidth + colorSpacing, miscYPos, colorTextBoxWidth, colorHeight),
                            int.TryParse(colorIndexText, out int selectedColorIndex) && colorNameMapping.ContainsKey(selectedColorIndex) ? colorNameMapping[selectedColorIndex] : "Select Color"))
                        {
                            showColorIndexDropdown = !showColorIndexDropdown;
                        }
                        miscYPos += colorHeight + 5f;

                        if (colorDropdownVisible)
                        {
                            for (int i = 0; i < playerNames.Count + 1; i++)
                            {
                                string name = (i == 0) ? "All" : playerNames[i - 1];
                                if (GUI.Button(new Rect(colorStartX + colorButtonWidth + colorSpacing, miscYPos, colorDropdownWidth, 25f), name))
                                {
                                    colorTargetVisibleName = name;
                                    colorDropdownVisible = false;
                                }
                                miscYPos += 25f;
                            }
                            miscYPos += 5f;
                        }

                        if (showColorIndexDropdown)
                        {
                            int itemHeight = 25;
                            int maxVisibleItems = 6;
                            int visibleItems = Math.Min(colorNameMapping.Count, maxVisibleItems);
                            float dropdownHeight = visibleItems * itemHeight;

                            Rect dropdownRect = new Rect(colorStartX + colorButtonWidth + colorSpacing + colorDropdownWidth + colorSpacing, miscYPos, colorTextBoxWidth, dropdownHeight);
                            Rect colorContentRect = new Rect(0, 0, colorTextBoxWidth - 20, colorNameMapping.Count * itemHeight);
                            colorIndexScrollPosition = GUI.BeginScrollView(dropdownRect, colorIndexScrollPosition, colorContentRect, false, true);

                            foreach (var colorEntry in colorNameMapping)
                            {
                                if (GUI.Button(new Rect(0, (colorEntry.Key - 1) * itemHeight, colorTextBoxWidth - 20, itemHeight), colorEntry.Value))
                                {
                                    colorIndexText = colorEntry.Key.ToString();
                                    showColorIndexDropdown = false;
                                }
                            }
                            GUI.EndScrollView();
                            miscYPos += dropdownHeight + 5f;
                        }

                        // Chat Spoof UI Layout
                        float chatButtonWidth = 120f; // Match spoofButtonWidth
                        float chatDropdownWidth = 130f; // Match spoofDropdownWidth
                        float chatSpacing = 10f; // Match spoofSpacing
                        float chatHeight = 30f; // Match spoofHeight
                        float totalChatRowWidth = chatButtonWidth + chatSpacing + chatDropdownWidth + chatSpacing + (540f - chatButtonWidth - chatDropdownWidth - (2 * chatSpacing));
                        float chatStartX = (540f - totalChatRowWidth) / 2f; // Match spoofStartX calculation
                        float chatTextBoxWidth = 540f - chatButtonWidth - chatDropdownWidth - (2 * chatSpacing); // Match spoofTextBoxWidth

                        if (GUI.Button(new Rect(chatStartX, miscYPos, chatButtonWidth, chatHeight), "Send Chat"))
                        { // [Send Chat] Button - aligned with Spoof Name button
                            string targetName = ChatDropdownVisibleName;
                            ChatHijack.MakeChat(chatMessageText, targetName, playerList, playerNames);
                        }

                        if (GUI.Button(new Rect(chatStartX + chatButtonWidth + chatSpacing, miscYPos, chatDropdownWidth, chatHeight), ChatDropdownVisibleName))
                        { // [Dropdown: Player List] - aligned with Spoof Name dropdown
                            ChatDropdownVisible = !ChatDropdownVisible;
                        }

                        chatMessageText = GUI.TextField( // [Text Box: Chat Message] - fills the remaining space to the right
                            new Rect(chatStartX + chatButtonWidth + chatSpacing + chatDropdownWidth + chatSpacing, miscYPos, chatTextBoxWidth, chatHeight),
                            chatMessageText);
                        miscYPos += chatHeight + 10f;

                        if (ChatDropdownVisible) // Dropdown buttons (if expanded)
                        {
                            for (int i = 0; i < playerNames.Count + 1; i++)
                            {
                                string name = (i == 0) ? "All" : playerNames[i - 1];
                                if (GUI.Button(new Rect(chatStartX + chatButtonWidth + chatSpacing, miscYPos, chatDropdownWidth, 25f), name))
                                {
                                    ChatDropdownVisibleName = name;
                                    ChatDropdownVisible = false;
                                }
                                miscYPos += 25f;
                            }
                            miscYPos += 5f;
                        }

                        bool newNoFogState = UIHelper.ButtonBool("No Fog", MiscFeatures.NoFogEnabled, 0, miscYPos);
                        if (newNoFogState != MiscFeatures.NoFogEnabled)
                        {
                            MiscFeatures.ToggleNoFog(newNoFogState);
                        }
                        miscYPos += parentSpacing;

                        bool newWatermarkState = UIHelper.ButtonBool("Disable Watermark", !showWatermark, 0, miscYPos);
                        if (newWatermarkState != !showWatermark)
                        {
                            showWatermark = !newWatermarkState;
                        }
                        miscYPos += parentSpacing;

                        UIHelper.Label("Map Tweaks (can't be undone in level):", 0, miscYPos);
                        miscYPos += childIndent;

                        if (UIHelper.Button("Disable '?' Overlay", 0, miscYPos))
                        {
                            MapTools.changeOverlayStatus(true);
                        }
                        miscYPos += parentSpacing;

                        if (UIHelper.Button("Discover Map Valuables", 0, miscYPos))
                        {
                            MapTools.DiscoveryMapValuables();
                        }
                        miscYPos += parentSpacing;

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Enemies:
                        Rect enemyViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float enemyContentHeight = 600; // Calculate based on actual content
                        Rect enemyContentRect = new Rect(0, 0, 540, enemyContentHeight);
                        enemiesScrollPosition = GUI.BeginScrollView(enemyViewRect, enemiesScrollPosition, enemyContentRect, false, enemyContentHeight > enemyViewRect.height);

                        float enemyYPos = yPos;

                        UpdateEnemyList();
                        UIHelper.Label("Select an enemy:", 0, enemyYPos);
                        enemyYPos += childIndent;

                        // Calculate the actual height needed for the enemy list
                        float enemyListItemHeight = 35;
                        float enemyListContentHeight = enemyNames.Count * enemyListItemHeight;
                        float enemyListViewHeight = Math.Min(200, Math.Max(enemyListContentHeight, 35)); // Min height of 35 for at least one row

                        enemyScrollPosition = GUI.BeginScrollView(
                            new Rect(0, enemyYPos, 540, enemyListViewHeight),
                            enemyScrollPosition,
                            new Rect(0, 0, 520, enemyListContentHeight),
                            false, true);
                        for (int i = 0; i < enemyNames.Count; i++)
                        {
                            if (i == selectedEnemyIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * enemyListItemHeight, 520, 30), enemyNames[i])) selectedEnemyIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        enemyYPos += enemyListViewHeight + 15;

                        // --- SPAWN UI SECTION ---
                        float spawnButtonWidth = 100;
                        float spawnCountTextBoxWidth = 50;
                        float spawnDropdownWidth = 200;
                        float gap = 10;

                        // Build and cache the enemy blueprint lists only once.
                        if (cachedFilteredEnemySetups == null || cachedEnemySetupNames == null)
                        {
                            List<EnemySetup> enemySetups = new List<EnemySetup>();
                            List<EnemySetup> enemies1, enemies2, enemies3;
                            if (EnemySpawner.TryGetEnemyLists(out enemies1, out enemies2, out enemies3))
                            {
                                enemySetups.AddRange(enemies1);
                                enemySetups.AddRange(enemies2);
                                enemySetups.AddRange(enemies3);
                            }
                            // Filter the list: remove any whose name contains "Enemy Group", and remove "Enemy -" prefix.
                            cachedFilteredEnemySetups = new List<EnemySetup>();
                            cachedEnemySetupNames = new List<string>();
                            foreach (var setup in enemySetups)
                            {
                                if (setup.name.Contains("Enemy Group"))
                                    continue;

                                string displayName = setup.name;
                                if (displayName.StartsWith("Enemy -"))
                                {
                                    displayName = displayName.Substring("Enemy -".Length).Trim();
                                }
                                cachedFilteredEnemySetups.Add(setup);
                                cachedEnemySetupNames.Add(displayName);
                            }
                        }

                        // Layout: [Spawn Button] [Integer Text Box] [Dropdown Button]

                        // Spawn Button
                        Rect spawnButtonRect = new Rect(0, enemyYPos, spawnButtonWidth, 25);
                        if (UIHelper.Button("Spawn", spawnButtonRect.x, enemyYPos, spawnButtonWidth, 25))
                        {
                            LevelGenerator levelGenerator = UnityEngine.Object.FindObjectOfType<LevelGenerator>();
                            if (levelGenerator == null)
                            {
                                DLog.Log("LevelGenerator instance not found!");
                            }
                            else
                            {
                                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                                if (localPlayer == null)
                                {
                                    DLog.Log("Local player not found!");
                                }
                                else
                                {
                                    Vector3 spawnPosition = localPlayer.transform.position + Vector3.up * 1.5f;

                                    // Filter input: allow only numbers and limit to 2 characters.
                                    spawnCountText = System.Text.RegularExpressions.Regex.Replace(spawnCountText, "[^0-9]", "");
                                    if (spawnCountText.Length > 2)
                                        spawnCountText = spawnCountText.Substring(0, 2);

                                    // Parse the number; default to 1 if parsing fails.
                                    int spawnCount = 1;
                                    if (!int.TryParse(spawnCountText, out spawnCount))
                                    {
                                        spawnCount = 1;
                                    }
                                    spawnCount = Mathf.Clamp(spawnCount, 1, 10);

                                    if (spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedFilteredEnemySetups.Count)
                                    {
                                        for (int i = 0; i < spawnCount; i++)
                                        {
                                            EnemySpawner.SpawnSpecificEnemy(levelGenerator, cachedFilteredEnemySetups[spawnEnemyIndex], spawnPosition);
                                        }
                                        DLog.Log($"Spawn triggered for {spawnCount} enemy(ies): {cachedEnemySetupNames[spawnEnemyIndex]}");
                                    }
                                    else
                                    {
                                        DLog.Log("Invalid spawn enemy index.");
                                    }
                                }
                            }
                        }

                        // Textbox for number of enemies to spawn.
                        Rect spawnCountTextRect = new Rect(spawnButtonRect.x + spawnButtonWidth + gap, enemyYPos, spawnCountTextBoxWidth, 25);
                        spawnCountText = GUI.TextField(spawnCountTextRect, spawnCountText); // Accepts only numbers due to filtering above.

                        // Dropdown Button for selecting the enemy blueprint.
                        Rect spawnDropdownButtonRect = new Rect(spawnCountTextRect.x + spawnCountTextBoxWidth + gap, enemyYPos, spawnDropdownWidth, 25);
                        string spawnDropdownText = (spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedEnemySetupNames.Count) ?
                                                   cachedEnemySetupNames[spawnEnemyIndex] : "Select enemy";
                        if (GUI.Button(spawnDropdownButtonRect, spawnDropdownText))
                        {
                            showSpawnDropdown = !showSpawnDropdown;
                        }
                        enemyYPos += 35;  // Advance past the top row of controls.

                        // Expanded Dropdown List (if toggled open).
                        if (showSpawnDropdown)
                        {
                            int itemHeight = 25;
                            int maxVisibleItems = 6;
                            int visibleItems = Math.Min(cachedEnemySetupNames.Count, maxVisibleItems);
                            float dropdownHeight = visibleItems * itemHeight;

                            // Determine if a vertical scrollbar is needed.
                            float vScrollbarWidth = (cachedEnemySetupNames.Count * itemHeight > dropdownHeight) ? 16f : 0f;

                            // Draw the dropdown list directly below the dropdown button, aligned with it.
                            Rect spawnDropdownListRect = new Rect(spawnDropdownButtonRect.x, enemyYPos, spawnDropdownWidth, dropdownHeight);
                            Rect spawnViewRect = new Rect(0, 0, spawnDropdownWidth - vScrollbarWidth, cachedEnemySetupNames.Count * itemHeight);
                            spawnDropdownScrollPosition = GUI.BeginScrollView(
                                spawnDropdownListRect,
                                spawnDropdownScrollPosition,
                                spawnViewRect,
                                false, false
                            );

                            // Create a centered GUIStyle for the dropdown buttons.
                            GUIStyle centeredStyle = new GUIStyle(GUI.skin.button)
                            {
                                alignment = TextAnchor.MiddleCenter
                            };

                            for (int i = 0; i < cachedEnemySetupNames.Count; i++)
                            {
                                if (GUI.Button(new Rect(0, i * itemHeight, spawnDropdownWidth - vScrollbarWidth, itemHeight), cachedEnemySetupNames[i], centeredStyle))
                                {
                                    spawnEnemyIndex = i;
                                    showSpawnDropdown = false;
                                }
                            }
                            GUI.EndScrollView();
                            enemyYPos += dropdownHeight + 5;
                        }

                        if (UIHelper.Button("Kill Enemy", 0, enemyYPos))
                        {
                            Enemies.KillSelectedEnemy(selectedEnemyIndex, enemyList, enemyNames);
                            DLog.Log($"Attempt to kill the selected enemy completed: {enemyNames[selectedEnemyIndex]}");
                        }
                        enemyYPos += parentSpacing;

                        if (UIHelper.Button("Kill All Enemies", 0, enemyYPos))
                        {
                            Enemies.KillAllEnemies();
                            DLog.Log("Attempt to kill all enemies completed.");
                        }
                        enemyYPos += parentSpacing;

                        bool newBlindState = UIHelper.ButtonBool("Blind Enemies", blindEnemies, 0, enemyYPos);
                        if (newBlindState != blindEnemies)
                        {
                            blindEnemies = newBlindState;
                            DLog.Log("Blind Enemies toggled: " + blindEnemies);
                        }
                        enemyYPos += parentSpacing;

                        if (UIHelper.Button(showEnemyTeleportUI ? "Hide Teleport Options" : "Teleport Options", 0, enemyYPos))
                        {
                            showEnemyTeleportUI = !showEnemyTeleportUI;
                            if (showEnemyTeleportUI)
                            {
                                UpdateEnemyTeleportOptions();
                            }
                        }
                        enemyYPos += parentSpacing;

                        if (showEnemyTeleportUI)
                        {
                            float labelWidth = 150;
                            float dropdownWidth = 200;
                            float tpCenterX = 270;
                            float tpSpacing = 20;

                            float tpTotalWidth = labelWidth + tpSpacing + dropdownWidth;
                            float tpStartX = tpCenterX - (tpTotalWidth / 2);

                            GUI.Label(new Rect(tpStartX, enemyYPos, labelWidth, 25), "Teleport Enemy To      →");

                            string currentDestination = enemyTeleportDestIndex >= 0 && enemyTeleportDestIndex < enemyTeleportDestOptions.Length ?
                                enemyTeleportDestOptions[enemyTeleportDestIndex] : "No players available";

                            if (GUI.Button(new Rect(tpStartX + labelWidth + tpSpacing, enemyYPos, dropdownWidth, 25), currentDestination))
                            {
                                showEnemyTeleportDropdown = enemyTeleportDestOptions.Length > 0 ? !showEnemyTeleportDropdown : false;
                            }
                            enemyYPos += parentSpacing;

                            if (showEnemyTeleportDropdown)
                            {
                                int itemHeight = enemyTeleportDestOptions.Length > 0 ? 25 : 0;
                                int maxVisibleItems = 6;
                                int visibleItems = Math.Min(enemyTeleportDestOptions.Length, maxVisibleItems);
                                float dropdownHeight = visibleItems * itemHeight;

                                Rect dropdownRect = new Rect(tpStartX + labelWidth + tpSpacing, enemyYPos - 15, dropdownWidth, dropdownHeight);

                                float contentHeight = enemyTeleportDestOptions.Length * itemHeight;

                                // Adjust content height to account for skipping the selected item
                                if (enemyTeleportDestIndex >= 0 && enemyTeleportDestIndex < enemyTeleportDestOptions.Length)
                                    contentHeight -= itemHeight;

                                enemyTeleportDropdownScrollPosition = GUI.BeginScrollView(dropdownRect, enemyTeleportDropdownScrollPosition, new Rect(0, 0, dropdownWidth - 20, contentHeight));

                                int displayedIndex = 0;
                                for (int i = 0; i < enemyTeleportDestOptions.Length; i++)
                                {
                                    if (i != enemyTeleportDestIndex)
                                    {
                                        if (GUI.Button(new Rect(0, displayedIndex * itemHeight, dropdownWidth - 20, itemHeight), enemyTeleportDestOptions[i])) enemyTeleportDestIndex = i;
                                        displayedIndex++;
                                    }
                                }
                                GUI.EndScrollView();
                            }

                            enemyYPos += 10;
                            float dropdownOffset = 0;

                            if (showEnemyTeleportDropdown && enemyTeleportDestOptions.Length > 1)
                            {
                                dropdownOffset = Math.Min(enemyTeleportDestOptions.Length - 1, 5) * 25;
                                enemyYPos += dropdownOffset;
                            }
                            if (GUI.Button(new Rect(tpCenterX - 75f, enemyYPos, 150f, 25f), "Execute Teleport"))
                            {
                                int playerIndex = enemyTeleportDestIndex;
                                if (playerIndex >= 0 && playerIndex < playerList.Count)
                                {
                                    if (DebugCheats.IsLocalPlayer(playerList[playerIndex]))
                                    {
                                        Enemies.TeleportEnemyToMe(selectedEnemyIndex, enemyList, enemyNames);
                                    }
                                    else
                                    {
                                        Enemies.TeleportEnemyToPlayer(selectedEnemyIndex, enemyList, enemyNames, playerIndex, playerList, playerNames);
                                    }
                                    UpdateEnemyList();
                                    DLog.Log($"Teleported {enemyNames[selectedEnemyIndex]} to {playerNames[playerIndex]}.");
                                }
                                else
                                {
                                    DLog.Log("Invalid player index for teleport target");
                                }
                            }
                            enemyYPos += parentSpacing;
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Items:
                        Rect itemsViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float itemsContentHeight = 800; // Calculate based on actual content
                        Rect itemsContentRect = new Rect(0, 0, 540, itemsContentHeight);
                        itemsScrollPosition = GUI.BeginScrollView(itemsViewRect, itemsScrollPosition, itemsContentRect, false, itemsContentHeight > itemsViewRect.height);

                        float itemYPos = yPos;

                        UIHelper.Label("Select an item:", 0, itemYPos);
                        itemYPos += childIndent;

                        // Calculate the actual height needed for the item list
                        float itemListItemHeight = 35;
                        float itemListContentHeight = itemList.Count * itemListItemHeight;
                        float itemListViewHeight = Math.Min(200, Math.Max(itemListContentHeight, 35)); // Min height of 35 for at least one row

                        itemScrollPosition = GUI.BeginScrollView(
                            new Rect(0, itemYPos, 540, itemListViewHeight),
                            itemScrollPosition,
                            new Rect(0, 0, 520, itemListContentHeight),
                            false, true);
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            if (i == selectedItemIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * itemListItemHeight, 520, 30), $"{itemList[i].Name} [Value: ${itemList[i].Value}]"))
                            {
                                selectedItemIndex = i;
                            }
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        itemYPos += itemListViewHeight + 15;

                        if (UIHelper.Button("[HOST] Teleport Item to Me", 0, itemYPos))
                        {
                            if (selectedItemIndex >= 0 && selectedItemIndex < itemList.Count)
                            {
                                ItemTeleport.TeleportItemToMe(itemList[selectedItemIndex]);
                                DLog.Log($"Teleported item: {itemList[selectedItemIndex].Name}");
                            }
                            else
                            {
                                DLog.Log("No valid item selected for teleport!");
                            }
                        }
                        itemYPos += parentSpacing;

                        if (UIHelper.Button("[HOST] Teleport All Items to Me", 0, itemYPos))
                        {
                            ItemTeleport.TeleportAllItemsToMe();
                            DLog.Log("Teleporting all items initiated.");
                        }
                        itemYPos += parentSpacing;

                        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                        labelStyle.normal.textColor = Color.white;

                        GUI.Label(new Rect(0, itemYPos, 540, 20), "Change Item Value:", GUI.skin.label);
                        itemYPos += 25;

                        int displayValue = (int)Mathf.Pow(10, itemValueSliderPos);
                        GUI.Label(new Rect(0, itemYPos, 540, 20), $"${displayValue:N0}", GUI.skin.label);
                        itemYPos += 25;

                        float newSliderPos = GUI.HorizontalSlider(new Rect(0, itemYPos, 540, 20), itemValueSliderPos, 3.0f, 9.0f);
                        itemYPos += 25;

                        if (newSliderPos != itemValueSliderPos)
                        {
                            itemValueSliderPos = newSliderPos;
                        }

                        if (GUI.Button(new Rect(0, itemYPos, 540, 30), "[HOST] Apply Value Change"))
                        {
                            if (selectedItemIndex >= 0 && selectedItemIndex < itemList.Count)
                            {
                                ItemTeleport.SetItemValue(itemList[selectedItemIndex], displayValue);
                                DLog.Log($"Updated value to ${displayValue:N0}: {itemList[selectedItemIndex].Name}");
                            }
                            else
                            {
                                DLog.Log("No valid item selected to change value!");
                            }
                        }
                        itemYPos += parentSpacing;

                        if (UIHelper.Button(showItemSpawner ? "Hide Item Spawner" : "Show Item Spawner", 0, itemYPos))
                        {
                            showItemSpawner = !showItemSpawner;
                            if (showItemSpawner && availableItemsList.Count == 0)
                            {
                                availableItemsList = ItemSpawner.GetAvailableItems();
                            }
                        }
                        itemYPos += parentSpacing;

                        if (showItemSpawner)
                        {
                            GUI.Label(new Rect(0, itemYPos, 540, 20), "Select item to spawn:");
                            itemYPos += childIndent;

                            // Calculate the actual height needed for the item spawner list
                            float spawnerItemHeight = 30;
                            float spawnerContentHeight = availableItemsList.Count * spawnerItemHeight;
                            float spawnerViewHeight = Math.Min(150, Math.Max(spawnerContentHeight, 30)); // Min height of 30 for at least one row

                            itemSpawnerScrollPosition = GUI.BeginScrollView(
                                new Rect(0, itemYPos, 540, spawnerViewHeight),
                                itemSpawnerScrollPosition,
                                new Rect(0, 0, 520, spawnerContentHeight),
                                false, true);

                            for (int i = 0; i < availableItemsList.Count; i++)
                            {
                                if (i == selectedItemToSpawnIndex) GUI.color = Color.white;
                                else GUI.color = Color.gray;

                                if (GUI.Button(new Rect(0, i * spawnerItemHeight, 520, 30), availableItemsList[i]))
                                {
                                    selectedItemToSpawnIndex = i;
                                }

                                GUI.color = Color.white;
                            }
                            GUI.EndScrollView();
                            itemYPos += spawnerViewHeight + 15;

                            bool isValuable = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count && availableItemsList[selectedItemToSpawnIndex].Contains("Valuable");

                            if (isValuable)
                            {
                                string formattedValue = string.Format("{0:n0}", itemSpawnValue);
                                GUI.Label(new Rect(0, itemYPos, 540, 20), $"Item Value: ${formattedValue}");
                                itemYPos += childIndent;

                                float sliderValue = Mathf.Log10((float)itemSpawnValue / 1000f) / 6f; // 6 = log10(1,000,000,000/1,000)
                                float newSliderValue = GUI.HorizontalSlider(new Rect(0, itemYPos, 540, 20), sliderValue, 0f, 1f);

                                if (newSliderValue != sliderValue)
                                {
                                    // keep host check for value adjustment
                                    if (isHost)
                                    {
                                        itemSpawnValue = (int)(Mathf.Pow(10, newSliderValue * 6f) * 1000f);
                                        itemSpawnValue = Mathf.Clamp(itemSpawnValue, 1000, 1000000000);
                                    }
                                }

                                itemYPos += childIndent;
                            }

                            GUI.enabled = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count;

                            if (GUI.Button(new Rect(0, itemYPos, 540, 30), "Spawn Selected Item"))
                            {
                                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                                if (localPlayer != null)
                                {
                                    Vector3 spawnPosition = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up * 1f;
                                    string itemName = availableItemsList[selectedItemToSpawnIndex];

                                    if (isValuable)
                                    {
                                        ItemSpawner.SpawnItem(itemName, spawnPosition, itemSpawnValue);
                                        DLog.Log($"Spawned valuable: {itemName} with value: ${itemSpawnValue}");
                                    }
                                    else
                                    {
                                        ItemSpawner.SpawnItem(itemName, spawnPosition);
                                        DLog.Log($"Spawned item: {itemName}");
                                    }
                                }
                                else
                                {
                                    DLog.Log("Local player not found!");
                                }
                            }

                            GUI.enabled = true;
                            itemYPos += parentSpacing;

                            // Move this button inside the showItemSpawner check
                            if (GUI.Button(new Rect(0, itemYPos, 540, 30), "Spawn 50 of Selected Item"))
                            {
                                ItemSpawner.SpawnSelectedItemMultiple(50, availableItemsList, selectedItemToSpawnIndex, itemSpawnValue);
                            }
                            itemYPos += parentSpacing;
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Hotkeys:
                        Rect viewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        float hotkeyContentHeight = 800; // Calculate based on actual content
                        Rect contentRect = new Rect(0, 0, 540, hotkeyContentHeight);

                        if (!string.IsNullOrEmpty(hotkeyManager.KeyAssignmentError) && Time.time - hotkeyManager.ErrorMessageTime < HotkeyManager.ERROR_MESSAGE_DURATION)
                        {
                            GUIStyle errorStyle = new GUIStyle(GUI.skin.label)
                            {
                                fontSize = 14,
                                fontStyle = FontStyle.Bold,
                                normal = { textColor = Color.red },
                                alignment = TextAnchor.MiddleCenter
                            };

                            GUI.Label(new Rect(menuX + 30, menuY + 95, 560, 25), hotkeyManager.KeyAssignmentError, errorStyle);

                            viewRect.y += 30;
                            viewRect.height -= 30;
                        }

                        hotkeyScrollPosition = GUI.BeginScrollView(viewRect, hotkeyScrollPosition, contentRect, false, hotkeyContentHeight > viewRect.height);

                        float hotkeyYPos = yPos;

                        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 16,
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = Color.white }
                        };

                        GUI.Label(new Rect(20, hotkeyYPos, 540, 25), "Hotkey Configuration", headerStyle);
                        hotkeyYPos += childSpacing;

                        GUI.Label(new Rect(20, hotkeyYPos, 540, 20), "How to set up a hotkey:", instructionStyle);
                        hotkeyYPos += childIndent;
                        GUI.Label(new Rect(40, hotkeyYPos, 540, 20), "1. Click on a key field → press desired key", instructionStyle);
                        hotkeyYPos += childIndent;
                        GUI.Label(new Rect(40, hotkeyYPos, 540, 20), "2. Click on action field → select function", instructionStyle);
                        hotkeyYPos += 25;

                        GUIStyle warningStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 12,
                            normal = { textColor = Color.yellow }
                        };
                        GUI.Label(new Rect(20, hotkeyYPos, 540, 20), "Warning: Ensure each key is only assigned to one action", warningStyle);
                        hotkeyYPos += childSpacing;

                        GUI.Label(new Rect(10, hotkeyYPos, 540, 25), "System Keys", headerStyle);
                        hotkeyYPos += childSpacing;

                        string menuToggleKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 0 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.MenuToggleKey.ToString();
                        GUI.Label(new Rect(10, hotkeyYPos, 150, 30), "Menu Toggle:");
                        if (GUI.Button(new Rect(170, hotkeyYPos, 290, 30), menuToggleKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(0);
                        }
                        hotkeyYPos += parentSpacing;

                        string reloadKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 1 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.ReloadKey.ToString();
                        GUI.Label(new Rect(10, hotkeyYPos, 150, 30), "Reload:");
                        if (GUI.Button(new Rect(170, hotkeyYPos, 290, 30), reloadKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(1);
                        }
                        hotkeyYPos += parentSpacing;

                        string unloadKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 2 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.UnloadKey.ToString();
                        GUI.Label(new Rect(10, hotkeyYPos, 150, 30), "Unload:");
                        if (GUI.Button(new Rect(170, hotkeyYPos, 290, 30), unloadKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(2);
                        }
                        hotkeyYPos += 50;

                        GUI.Label(new Rect(10, hotkeyYPos, 540, 25), "Action Hotkeys", headerStyle);
                        hotkeyYPos += childSpacing;

                        for (int i = 0; i < 12; i++)
                        {
                            KeyCode currentKey = hotkeyManager.GetHotkeyForSlot(i);
                            string keyText = (currentKey == KeyCode.None) ? "Not Set" : currentKey.ToString();
                            string actionName = hotkeyManager.GetActionNameForKey(currentKey);

                            Rect slotRect = new Rect(10, hotkeyYPos, 150, 30);
                            bool isSelected = hotkeyManager.SelectedHotkeySlot == i && hotkeyManager.ConfiguringHotkey;

                            if (GUI.Button(slotRect, isSelected ? "Press any key..." : keyText))
                            {
                                hotkeyManager.StartHotkeyConfiguration(i);
                            }

                            Rect actionRect = new Rect(170, hotkeyYPos, 290, 30);
                            if (GUI.Button(actionRect, actionName))
                            {
                                if (currentKey != KeyCode.None)
                                {
                                    showingActionSelector = true;
                                    hotkeyManager.ShowActionSelector(i, currentKey);
                                }
                                else
                                {
                                    DLog.Log("Please assign a key to this slot first");
                                }
                            }

                            Rect clearRect = new Rect(470, hotkeyYPos, 60, 30);
                            if (GUI.Button(clearRect, "Clear") && currentKey != KeyCode.None)
                            {
                                hotkeyManager.ClearHotkeyBinding(i);
                            }

                            hotkeyYPos += parentSpacing;
                        }

                        if (GUI.Button(new Rect(10, hotkeyYPos, 540, 30), "Save Hotkey Settings"))
                        {
                            hotkeyManager.SaveHotkeySettings();
                            DLog.Log("Hotkey settings saved manually");
                        }

                        GUI.EndScrollView();
                        break;
                }
            }

            if (showingActionSelector)
            {
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);
                if (Event.current.type == EventType.MouseDown || // draw full-screen overlay and consume mouse events so main GUI is blocked
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use(); // consume events
                }

                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400); // draw modal window using GUI.Window for natural dragging
                modalRect = GUI.Window(12345, modalRect, ActionSelectorWindow, "", actionSelectorBoxStyle);
                actionSelectorX = modalRect.x;
                actionSelectorY = modalRect.y;
            }
        }

        private static Texture2D MakeSolidBackground(Color color, float alpha)//fix
        {
            Color key = new Color(color.r, color.g, color.b, alpha);
            if (!solidTextures.ContainsKey(key))
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, key);
                texture.Apply();
                solidTextures[key] = texture;
            }
            return solidTextures[key];
        }

        // Helper method to get a contrasting color for text based on background color
        private Color GetContrastColor(Color backgroundColor)
        {
            // Calculate perceived brightness using the formula: 
            // (0.299*R + 0.587*G + 0.114*B)
            float brightness = (0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b);

            // Return white for dark backgrounds, black for light backgrounds
            return brightness < 0.5f ? Color.white : Color.black;
        }

    }
}
