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
using Sirenix.OdinInspector;
using static Photon.Pun.UtilityScripts.TabViewManager;
using System.IO;
using Steamworks;
using Steamworks.Data;

namespace dark_cheat
{

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
        public static float offsetESp = 0.0f;
        public static bool showMenu = true;
        public static bool godModeActive = false;
        public static bool debounce = false;
        public static bool infiniteHealthActive = false;
        public static bool stamineState = false;
        public static bool unlimitedBatteryActive = false;
        public static UnlimitedBattery unlimitedBatteryComponent;
        public static bool blindEnemies = false;
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

        public static bool showTotalValue = true;
        public static bool showPlayerStatus = true;
        private int totalValuableValue = 0;

        public static string[] levelsToSearchItems = { "Level - Manor", "Level - Wizard", "Level - Arctic", "Level - Shop", "Level - Lobby", "Level - Recording" };

        private GUIStyle menuStyle;
        private bool initialized = false;
        private static Dictionary<UnityEngine.Color, Texture2D> solidTextures = new Dictionary<UnityEngine.Color, Texture2D>();

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
        public static string spoofedNameText = "Text";
        public static string persistentNameText = "Text";
        private string originalSteamName = Steamworks.SteamClient.Name; // Store real name at startup
        public static bool spoofNameActive = false;
        private float lastSpoofTime = 0f;
        private const float NAME_SPOOF_DELAY = 4f;
        static public bool hasAlreadySpoofed = false;

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

        //SLIDER VALUES
        public static float sliderValue = 1f;
        public static float sliderValueStrength = 1f;
        public static float jumpForce = 1f;
        public static float customGravity = 1f;
        public static int extraJumps = 0;
        public static float flashlightIntensity = 0f;
        public static float crouchDelay = 0f;
        public static float crouchSpeed = 0f;
        public static float grabRange = 1f;
        public static float tumbleLaunch = 0f;
        public static float throwStrength = 0f;
        public static float slideDecay = 0f;

        public static float oldSliderValue = 1f;
        public static float oldSliderValueStrength = 1f;
        public static float OldjumpForce = 1f;
        public static float OldcustomGravity = 1f;
        public static float OldextraJumps = 0f;
        public static float OldflashlightIntensity = 0f;
        public static float OldcrouchDelay = 0f;
        public static float OldcrouchSpeed = 0f;
        public static float OldgrabRange = 1f;
        public static float OldtumbleLaunch = 0f;
        public static float OldthrowStrength = 0f;
        public static float OldslideDecay = 0f;

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

        private HotkeyManager hotkeyManager; // Reference to the HotkeyManager
        public static bool showWatermark = true;

        private float menuX = 100f;

        // === NEW GUI ===

        private Rect menuRect = new Rect(100, 100, 640, 500);
        private Vector2 scrollPos;
        private string[] tabs = new string[] { "SELF", "VISUALS", "COMBAT", "MISC", "ENEMIES", "ITEMS", "HOTKEYS", "TROLLING", "CONFIG", "SERVERS" };
        private int currentTab = 0;
        public static Texture2D toggleBackground;
        public string configstatus = "Waiting For Action...";

        //=== Animation ===
        private float toggleAnimationSpeed = 8f;     // Higher = faster
        private Dictionary<string, float> toggleAnimations = new Dictionary<string, float>();

        // === Resizing ===
        private bool isResizing = false;
        private Vector2 resizeStartMousePos;
        private Vector2 resizeStartSize;
        private GUIStyle titleStyle, tabStyle, horizontalSliderStyle, horizontalThumbStyle, scrollbarStyle, scrollbarThumbStyle, tabSelectedStyle, sectionHeaderStyle, buttonStyle, labelStyle, backgroundStyle, warningStyle, boxStyle, textFieldStyle, verticalSliderStyle, verticalThumbStyle;

        // === Items Tab ===
        private Vector2 itemScroll;
        private Vector2 itemSpawnerScroll;
        private string itemSpawnSearch = "";

        //Chams Window
        private static bool showChamsWindow = false;
        private static Rect chamsWindowRect = new Rect(100, 100, 220, 400);

        public Texture2D toggleBgTexture;
        public Texture2D toggleKnobOffTexture;
        public Texture2D toggleKnobOnTexture;

        public static bool useModernESP = false;

        //Hotkey Window
        private static bool showingActionSelector = false;
        private static Rect featureSelectorRect = new Rect(200, 200, 400, 400);
        private static Vector2 actionScroll;

        //Server Browser
        private bool hideFullLobbies = false;
        private Vector2 memberWindowScroll;
        private bool showMemberWindow = false;
        private static Rect lobbyMemberWindowRect = new Rect(100, 100, 320, 240);
        private static SteamId selectedLobbyId = 0;
        public static Dictionary<SteamId, string> LobbyHostCache = new Dictionary<SteamId, string>();
        public static Dictionary<SteamId, List<string>> LobbyMemberCache = new Dictionary<SteamId, List<string>>();
        private string lobbySearchTerm = "";
        private enum SortMode { None, RegionAZ, RegionZA, MostPlayers, LeastPlayers }
        private SortMode sortMode = SortMode.None;
        private static Vector2 serverListScroll;
        private enum SortOption { None, RegionAsc, RegionDesc, MostPlayers, LeastPlayers }
        public class LobbyCoroutineHost : MonoBehaviour { }
        public static LobbyCoroutineHost CoroutineHost;
        private Rect previousWindowRect;
        private bool hasStoredPreviousSize = false;
        private bool wasInServerTab = false;
        private static Texture2D rowBgNormal;
        private static Texture2D rowBgHover;
        private static Texture2D rowBgSelected;

        //TextEditor
        private bool showTextEditorPopup = false;
        private string largeTextBoxContent = "";
        private Rect editorPopupRect = new Rect(200, 200, 400, 300);
        private string activeTextFieldId = null;
        private static Vector2 textboxscroll;

        // === END OF NEW GUI ===

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
            float now = Time.time;
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
            DebugCheats.texture2.SetPixels(new[] { UnityEngine.Color.red, UnityEngine.Color.red, UnityEngine.Color.red, UnityEngine.Color.red });
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

            toggleBgTexture = TextureLoader.LoadEmbeddedTexture("dark_cheat.images.toggle_bg.png");
            toggleKnobOffTexture = TextureLoader.LoadEmbeddedTexture("dark_cheat.images.toggle_knobOff.png");
            toggleKnobOnTexture = TextureLoader.LoadEmbeddedTexture("dark_cheat.images.toggle_knobOn.png");

            ConfigManager.LoadAllToggles();

            GameObject LCH = new GameObject("LobbyCoroutineHost");
            UnityEngine.Object.DontDestroyOnLoad(LCH);
            Hax2.CoroutineHost = LCH.AddComponent<LobbyCoroutineHost>();
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
            totalValuableValue = 0;

            var valuableArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableArray != null)
            {
                foreach (var val in valuableArray)
                {
                    DebugCheats.valuableObjects.Add(val);

                    var valueField = val.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (valueField != null)
                    {
                        object valueObj = valueField.GetValue(val);
                        if (valueObj is int intValue)
                        {
                            totalValuableValue += intValue;
                        }
                        else if (valueObj is float floatValue)
                        {
                            totalValuableValue += Mathf.RoundToInt(floatValue);
                        }
                        else
                        {
                            Debug.LogWarning($"[Valuable] Unknown value type: {valueObj?.GetType()?.Name}");
                        }
                    }
                }
            }

            var playerDeathHeadArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("PlayerDeathHead, Assembly-CSharp"));
            if (playerDeathHeadArray != null)
            {
                DebugCheats.valuableObjects.AddRange(playerDeathHeadArray);
            }

            itemList = ItemTeleport.GetItemList();
            if (itemList.Count != previousItemCount)
            {
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

        void OnGUI()
        {

            InitStyles();
            if (showMenu)
            {
                menuRect = GUI.Window(0, menuRect, DrawMenuWindow, "", backgroundStyle);
                HandleResize();
                if (showChamsWindow)
                {
                    chamsWindowRect = GUI.Window(10001, chamsWindowRect, DrawChamsColorWindow, "", backgroundStyle);
                }
                if (showingActionSelector)
                {
                    GUI.Box(featureSelectorRect, "", boxStyle);
                    featureSelectorRect = GUI.Window(9999, featureSelectorRect, DrawFeatureSelectionWindow, "", boxStyle);
                }
                if (showMemberWindow && selectedLobbyId != 0)
                {
                    lobbyMemberWindowRect = GUI.Window(1299, lobbyMemberWindowRect, DrawLobbyMemberWindow, GUIContent.none, backgroundStyle);
                }
                if (showTextEditorPopup)
                {
                    editorPopupRect = GUI.Window(9989, editorPopupRect, DrawTextEditorPopup, "", backgroundStyle);
                }
            }

            if (useModernESP)
            {
                if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
                {
                    ModernESP.Render();
                }
            }
            else
            {
                if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
                {
                    DebugCheats.DrawESP();
                    ModernESP.ClearItemLabels();
                    ModernESP.ClearEnemyLabels();
                }
            }

            GUIStyle style = new GUIStyle(GUI.skin.label) { wordWrap = false };
            if (showWatermark)
            {
                GUIContent content = new GUIContent($"DARK CHEAT | {hotkeyManager.MenuToggleKey} - MENU");
                Vector2 size = style.CalcSize(content);
                GUI.Label(new Rect(10, 10, size.x, size.y), content, style);
                GUI.Label(new Rect(10 + size.x + 10, 10, 200, size.y), "@Github/D4rkks (+collabs)", style);
            }

            GUIStyle boldStyle = new GUIStyle(GUI.skin.label);
            boldStyle.fontSize = 16;
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.normal.textColor = UnityEngine.Color.white;
            float startX2 = 20f;
            float startYX2 = 250f;
            float lineHeightX2 = 24f;
            int lineX2 = 0;

            if (DebugCheats.drawPlayerEspBool && showPlayerStatus)
            {
                var allPlayers = SemiFunc.PlayerGetList();
                if (allPlayers == null) return;

                List<string> alivePlayers = new List<string>();
                List<string> deadPlayers = new List<string>();

                foreach (var player in allPlayers)
                {
                    string name = SemiFunc.PlayerGetName(player) ?? "Unknown";

                    var isDisabledField = player.GetType().GetField("isDisabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (isDisabledField != null)
                    {
                        bool isDisabled = (bool)isDisabledField.GetValue(player);
                        if (isDisabled)
                            deadPlayers.Add(name);
                        else
                            alivePlayers.Add(name);
                    }
                    else
                    {
                        Debug.LogWarning($"[DeathCheck] 'isDisabled' field not found on player: {name}");
                    }
                }



                GUI.color = UnityEngine.Color.green;
                GUI.Label(new Rect(startX2, startYX2 + (lineX2++ * lineHeightX2), 400f, lineHeightX2), "Alive Players:", boldStyle);
                foreach (var name in alivePlayers)
                {
                    string displayName = $"- {name}";
                    GUI.Label(new Rect(startX2, startYX2 + (lineX2++ * lineHeightX2), 400f, lineHeightX2), displayName);
                }

                lineX2++; // Adds space

                GUI.color = UnityEngine.Color.red;
                GUI.Label(new Rect(startX2, startYX2 + (lineX2++ * lineHeightX2), 400f, lineHeightX2), "Dead Players:", boldStyle);
                foreach (var name in deadPlayers)
                {
                    string displayName = $"- {name}";
                    GUI.Label(new Rect(startX2, startYX2 + (lineX2++ * lineHeightX2), 400f, lineHeightX2), displayName);
                }

                GUI.color = UnityEngine.Color.white;
            }

            lineX2++;
            lineX2++;
            lineX2++;

            if (DebugCheats.drawItemEspBool && showTotalValue)
            {
                GUI.color = UnityEngine.Color.yellow;
                GUI.Label(new Rect(startX2, startYX2 + (lineX2++ * lineHeightX2), 400f, lineHeightX2), $"Total Value on Map: ${totalValuableValue}", boldStyle);
                GUI.color = UnityEngine.Color.white;
                lineX2++;
            }

        }

        bool DrawCustomToggle(string id, bool state)
        {
            float toggleWidth = 50f;
            float toggleHeight = 24f;
            float knobSize = 40f;
            float padding = (toggleHeight - knobSize) / 2f;

            GUILayout.BeginVertical(GUILayout.Width(toggleWidth), GUILayout.Height(toggleHeight));
            Rect toggleRect = GUILayoutUtility.GetRect(toggleWidth, toggleHeight);
            GUILayout.EndVertical();

            if (!toggleAnimations.TryGetValue(id, out float progress))
            {
                progress = state ? 1f : 0f;
                toggleAnimations[id] = progress;
            }

            float target = state ? 1f : 0f;

            if (Event.current.type == EventType.Repaint && !Mathf.Approximately(progress, target))
            {
                progress = Mathf.MoveTowards(progress, target, Time.deltaTime * toggleAnimationSpeed);
                toggleAnimations[id] = progress;
            }

            GUI.DrawTexture(new Rect(toggleRect.x, toggleRect.y, toggleWidth, toggleHeight), toggleBgTexture);

            float travelDistance = toggleWidth - knobSize - 2 * padding;
            float knobX = toggleRect.x + padding + travelDistance * progress;
            Rect knobRect = new Rect(knobX, toggleRect.y + padding, knobSize, knobSize);

            GUI.DrawTexture(knobRect, state ? toggleKnobOnTexture : toggleKnobOffTexture);

            if (Event.current.type == EventType.MouseDown && toggleRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return !state;
            }

            return state;
        }

        bool ToggleLogic(string id, string label, ref bool value, Action onToggle = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(200));

            bool newValue = DrawCustomToggle(id, value);
            if (newValue != value)
            {
                value = newValue;
                onToggle?.Invoke();
            }

            GUILayout.EndHorizontal();
            return value;
        }

        void DrawMenuWindow(int windowID)
        {
            // === Server Browser Tab Resize Logic ===
            if (currentTab == 9 && !wasInServerTab)
            {
                if (!hasStoredPreviousSize)
                {
                    previousWindowRect = menuRect;
                    hasStoredPreviousSize = true;
                }

                menuRect.width = 1200f;
                menuRect.height = 765f;
                wasInServerTab = true;
            }
            else if (currentTab != 9 && wasInServerTab)
            {
                // Revert to previous layout
                if (hasStoredPreviousSize)
                    menuRect = previousWindowRect;

                wasInServerTab = false;
            }
            GUI.DragWindow(new Rect(0, 0, menuRect.width, 25));

            float tabPanelWidth = 120f;
            float padding = 20f;
            float contentPanelWidth = Mathf.Max(200f, menuRect.width - tabPanelWidth - padding * 2);

            GUILayout.BeginVertical();
            GUILayout.Space(5);

            GUILayout.Label("DARK MENU 1.3", titleStyle);

            GUILayout.BeginHorizontal();

            // === Left Tabs ===
            GUILayout.BeginVertical(GUILayout.Width(tabPanelWidth));
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUILayout.Button(tabs[i], i == currentTab ? tabSelectedStyle : tabStyle))
                {
                    currentTab = i;
                }
            }
            GUILayout.EndVertical();

            // === Right Tab Contents ===
            GUILayout.BeginVertical(boxStyle, GUILayout.Width(contentPanelWidth));
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            switch (currentTab)
            {
                case 0: DrawSelfTab(); break;
                case 1: DrawVisualsTab(); break;
                case 2: DrawCombatTab(); break;
                case 3: DrawMiscTab(); break;
                case 4: DrawEnemiesTab(); break;
                case 5: DrawItemsTab(); break;
                case 6: DrawHotkeysTab(); break;
                case 7: DrawTrollingTab(); break;
                case 8: DrawConfigTab(); break;
                case 9: DrawServersTab(); break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void DrawSelfTab()
        {
            GUILayout.Label("HEALTH", sectionHeaderStyle);

            ToggleLogic("god_mode", " GOD MODE", ref godModeActive, PlayerController.GodMode);
            GUILayout.Space(5);

            ToggleLogic("inf_health", " Infinite Health", ref infiniteHealthActive, PlayerController.MaxHealth);
            GUILayout.Space(10);

            GUILayout.Label("MOVEMENT", sectionHeaderStyle);

            ToggleLogic("No_Clip", " NoClip", ref NoclipController.noclipActive, NoclipController.ToggleNoclip);
            GUILayout.Space(5);

            ToggleLogic("inf_stam", " Infinite Stamina", ref stamineState, PlayerController.MaxStamina);
            GUILayout.Space(10);

            GUILayout.Label("MISCELLANEOUS", sectionHeaderStyle);

            ToggleLogic("rgb_player", " RGB Player", ref playerColor.isRandomizing, null);
            GUILayout.Space(5);

            ToggleLogic("No_Fog", " No Fog", ref MiscFeatures.NoFogEnabled, MiscFeatures.ToggleNoFog);
            GUILayout.Space(5);

            ToggleLogic("WaterMark_Toggle", " Show Watermark", ref showWatermark, null);
            GUILayout.Space(5);

            ToggleLogic("Grab_Guard", " Grab Guard", ref debounce, null);
            GUILayout.Space(10);

            GUILayout.Label("Strength: " + Mathf.RoundToInt(sliderValueStrength), labelStyle);
            sliderValueStrength = GUILayout.HorizontalSlider(sliderValueStrength, 1f, 30f, GUILayout.Width(200));
            if (sliderValueStrength != oldSliderValueStrength)
            {
                int newStrength = Mathf.RoundToInt(sliderValueStrength);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerGrabStrengthRPC", RpcTarget.AllBuffered, steamID, newStrength);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                oldSliderValueStrength = sliderValueStrength;
            }

            // Display label and slider
            GUILayout.Label("Throw Strength: " + Mathf.RoundToInt(throwStrength), labelStyle);
            throwStrength = GUILayout.HorizontalSlider(throwStrength, 0f, 30f, GUILayout.Width(200));
            if (throwStrength != OldthrowStrength)
            {
                int newThrowStrength = Mathf.RoundToInt(throwStrength);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerThrowStrengthRPC", RpcTarget.AllBuffered, steamID, newThrowStrength);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                OldthrowStrength = throwStrength;
            }

            GUILayout.Label("Speed: " + Mathf.RoundToInt(sliderValue), labelStyle);
            sliderValue = GUILayout.HorizontalSlider(sliderValue, 1f, 30f, GUILayout.Width(200));
            if (sliderValue != oldSliderValue)
            {
                int newSpeed = Mathf.RoundToInt(sliderValue);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerSprintSpeedRPC", RpcTarget.AllBuffered, steamID, newSpeed);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                oldSliderValue = sliderValue;
            }

            GUILayout.Label("Grab Range: " + Mathf.RoundToInt(grabRange), labelStyle);
            grabRange = GUILayout.HorizontalSlider(grabRange, 1f, 30f, GUILayout.Width(200));
            if (grabRange != OldgrabRange)
            {
                int newGrabRange = Mathf.RoundToInt(grabRange);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerGrabRangeRPC", RpcTarget.AllBuffered, steamID, newGrabRange);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                OldgrabRange = grabRange;
            }

            GUILayout.Label("Stamina Recharge Delay: " + Mathf.RoundToInt(staminaRechargeDelay), labelStyle);
            staminaRechargeDelay = GUILayout.HorizontalSlider(staminaRechargeDelay, 1f, 30f, GUILayout.Width(200));
            if (staminaRechargeDelay != oldStaminaRechargeDelay)
            {
                oldStaminaRechargeDelay = staminaRechargeDelay;
                Debug.Log("Stamina Recharge Delay to: " + staminaRechargeDelay);
            }

            GUILayout.Label("Stamina Recharge Rate: " + Mathf.RoundToInt(staminaRechargeRate), labelStyle);
            staminaRechargeRate = GUILayout.HorizontalSlider(staminaRechargeRate, 1f, 30f, GUILayout.Width(200));
            if (staminaRechargeDelay != oldStaminaRechargeDelay || staminaRechargeRate != oldStaminaRechargeRate)
            {
                PlayerController.DecreaseStaminaRechargeDelay(staminaRechargeDelay, staminaRechargeRate);
                Debug.Log($"Stamina recharge updated: Delay={staminaRechargeDelay}x, Rate={staminaRechargeRate}x");
                oldStaminaRechargeDelay = staminaRechargeDelay;
                oldStaminaRechargeRate = staminaRechargeRate;
            }

            GUILayout.Label("Extra Jumps: " + Mathf.RoundToInt(extraJumps), labelStyle);
            extraJumps = (int)GUILayout.HorizontalSlider(extraJumps, 0f, 30f, GUILayout.Width(200));
            if (extraJumps != OldextraJumps)
            {
                int newExtraJumps = Mathf.RoundToInt(extraJumps);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerExtraJumpRPC", RpcTarget.AllBuffered, steamID, newExtraJumps);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                OldextraJumps = extraJumps;
            }

            GUILayout.Label("Tumble Launch: " + Mathf.RoundToInt(tumbleLaunch), labelStyle);
            tumbleLaunch = (int)GUILayout.HorizontalSlider(tumbleLaunch, 0f, 20f, GUILayout.Width(200));
            if (tumbleLaunch != OldtumbleLaunch)
            {
                int newtumbleLaunch = Mathf.RoundToInt(tumbleLaunch);
                string steamID = PlayerController.GetLocalPlayerSteamID();

                PunManager punManager = GameObject.FindObjectOfType<PunManager>();
                PhotonView punManagerView = punManager.GetComponent<PhotonView>();

                if (punManagerView != null)
                {
                    punManagerView.RPC("UpgradePlayerTumbleLaunchRPC", RpcTarget.AllBuffered, steamID, newtumbleLaunch);
                }
                else
                {
                    Debug.LogError("PhotonView not found on PunManager GameObject!");
                }

                OldtumbleLaunch = tumbleLaunch;
            }

            GUILayout.Label("Jump Force: " + Mathf.RoundToInt(jumpForce), labelStyle);
            jumpForce = GUILayout.HorizontalSlider(jumpForce, 1f, 30f, GUILayout.Width(200));
            if (jumpForce != OldjumpForce)
            {
                PlayerController.SetJumpForce(17+jumpForce);
                OldjumpForce = jumpForce;
            }

            GUILayout.Label("Gravity: " + Mathf.RoundToInt(customGravity), labelStyle);
            customGravity = GUILayout.HorizontalSlider(customGravity, 1f, 30f, GUILayout.Width(200));
            if (customGravity != OldcustomGravity)
            {
                PlayerController.SetCustomGravity(30+customGravity);
                OldcustomGravity = customGravity;
            }

            GUILayout.Label("Crouch Delay: " + Mathf.RoundToInt(crouchDelay), labelStyle);
            crouchDelay = GUILayout.HorizontalSlider(crouchDelay, 0f, 30f, GUILayout.Width(200));
            if (crouchDelay != OldcrouchDelay)
            {
                PlayerController.SetCrouchDelay(crouchDelay);
                OldcrouchDelay = crouchDelay;
            }

            GUILayout.Label("Crouch Speed: " + Mathf.RoundToInt(crouchSpeed), labelStyle);
            crouchSpeed = GUILayout.HorizontalSlider(crouchSpeed, 1f, 30f, GUILayout.Width(200));
            if (crouchSpeed != OldcrouchSpeed)
            {
                PlayerController.SetCrouchSpeed(crouchSpeed);
                OldcrouchSpeed = crouchSpeed;
            }

            GUILayout.Label("Slide Decay: " + Mathf.RoundToInt(slideDecay), labelStyle);
            slideDecay = GUILayout.HorizontalSlider(slideDecay, 0f, 20f, GUILayout.Width(200));
            if (slideDecay != OldslideDecay)
            {
                PlayerController.SetSlideDecay(slideDecay);
                OldslideDecay = slideDecay;
            }

            GUILayout.Label("Flashlight Intensity: " + Mathf.RoundToInt(flashlightIntensity), labelStyle);
            flashlightIntensity = GUILayout.HorizontalSlider(flashlightIntensity, 1f, 20f, GUILayout.Width(200));
            if (flashlightIntensity != OldflashlightIntensity)
            {
                PlayerController.SetFlashlightIntensity(flashlightIntensity);
                OldflashlightIntensity = flashlightIntensity;
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
                GUILayout.Label("Field of View: " + Mathf.RoundToInt(currentFOV), labelStyle);
                float newFOV = GUILayout.HorizontalSlider(currentFOV, 60f, 120f, GUILayout.Width(200));
                if (newFOV != currentFOV)
                {
                    FOVEditor.Instance.SetFOV(newFOV);
                    fieldOfView = newFOV;
                }
            }
            else
            {
                GUILayout.Label("Loading FOV Editor...", labelStyle);
            }
        }

        void DrawVisualsTab()
        {
            GUILayout.Space(5);
            ToggleLogic("modern_esp", " Use Modern ESP", ref useModernESP, null);

            // === Enemy ESP ===
            GUILayout.Label("Enemy ESP", sectionHeaderStyle);
            ToggleLogic("enable_en_esp", " Enable Enemy ESP", ref DebugCheats.drawEspBool);
            // Only show options when enabled (no animation)
            if (DebugCheats.drawEspBool)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                ToggleLogic("show_2d_box_enemy", " Show 2D Box", ref DebugCheats.showEnemyBox);
                GUILayout.Space(5);
                bool enemyChams = DebugCheats.drawChamsBool;
                ToggleLogic("show_chams_enemy", " Show Chams", ref enemyChams);
                DebugCheats.drawChamsBool = enemyChams;
                GUILayout.Space(5);
                ToggleLogic("show_names_enemy", " Show Names", ref DebugCheats.showEnemyNames);
                GUILayout.Space(5);
                ToggleLogic("show_distance_enemy", " Show Distance", ref DebugCheats.showEnemyDistance);
                GUILayout.Space(5);
                ToggleLogic("show_health_enemy", " Show Health", ref DebugCheats.showEnemyHP);
                GUILayout.EndVertical();
            }

            // === Item ESP ===
            GUILayout.Space(10);
            GUILayout.Label("Item ESP", sectionHeaderStyle);
            ToggleLogic("enable_item_esp", " Enable Item ESP", ref DebugCheats.drawItemEspBool);

            if (DebugCheats.drawItemEspBool)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                ToggleLogic("show_3d_box_item", " Show 3D Box", ref DebugCheats.draw3DItemEspBool);
                GUILayout.Space(5);
                bool itemChams = DebugCheats.drawItemChamsBool;
                ToggleLogic("show_chams_item", " Show Chams", ref itemChams);
                DebugCheats.drawItemChamsBool = itemChams;
                // === Chams Color Picker ===
                if (DebugCheats.drawChamsBool || DebugCheats.drawItemChamsBool)
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("Configure Chams Colors", buttonStyle, GUILayout.Height(30)))
                    {
                        showChamsWindow = !showChamsWindow;
                    }
                }
                GUILayout.Space(5);
                ToggleLogic("show_names_item", " Show Names", ref DebugCheats.showItemNames);
                GUILayout.Space(5);
                ToggleLogic("show_distance_item", " Show Distance", ref DebugCheats.showItemDistance);
                if (DebugCheats.showItemDistance)
                {
                    GUILayout.Label($"Max Distance: {DebugCheats.maxItemEspDistance:F0}m", labelStyle);
                    DebugCheats.maxItemEspDistance = GUILayout.HorizontalSlider(DebugCheats.maxItemEspDistance, 0f, 1000f);
                }
                GUILayout.Space(5);
                ToggleLogic("show_value_item", " Show Value", ref DebugCheats.showItemValue);
                if (DebugCheats.showItemValue)
                {
                    GUILayout.Label($"Min Value: ${DebugCheats.minItemValue}", labelStyle);
                    DebugCheats.minItemValue = Mathf.RoundToInt(GUILayout.HorizontalSlider(DebugCheats.minItemValue, 0, 50000));
                }
                GUILayout.Space(5);
                ToggleLogic("show_dead_heads", " Show Dead Player Heads", ref DebugCheats.showPlayerDeathHeads);
                GUILayout.EndVertical();
            }

            // === Extraction ESP ===
            GUILayout.Space(10);
            GUILayout.Label("Extraction ESP", sectionHeaderStyle);
            ToggleLogic("enable_extract_esp", " Enable Extraction ESP", ref DebugCheats.drawExtractionPointEspBool);

            if (DebugCheats.drawExtractionPointEspBool)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                ToggleLogic("show_extract_names", " Show Name/Status", ref DebugCheats.showExtractionNames);
                GUILayout.Space(5);
                ToggleLogic("show_extract_distance", " Show Distance", ref DebugCheats.showExtractionDistance);
                GUILayout.EndVertical();
            }

            // === Player ESP ===
            GUILayout.Space(10);
            GUILayout.Label("Player ESP", sectionHeaderStyle);
            ToggleLogic("enable_player_esp", " Enable Player ESP", ref DebugCheats.drawPlayerEspBool);

            if (DebugCheats.drawPlayerEspBool)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(5);
                ToggleLogic("show_2d_box_player", " Show 2D Box", ref DebugCheats.draw2DPlayerEspBool);
                GUILayout.Space(5);
                ToggleLogic("show_3d_box_player", " Show 3D Box", ref DebugCheats.draw3DPlayerEspBool);
                GUILayout.Space(5);
                ToggleLogic("show_names_player", " Show Names", ref DebugCheats.showPlayerNames);
                GUILayout.Space(5);
                ToggleLogic("show_distance_player", " Show Distance", ref DebugCheats.showPlayerDistance);
                GUILayout.Space(5);
                ToggleLogic("show_health_player", " Show Health", ref DebugCheats.showPlayerHP);
                GUILayout.Space(5);
                ToggleLogic("show_alive_dead_list", " Show Alive/Dead List", ref showPlayerStatus);
                GUILayout.EndVertical();
            }
        }

        void DrawCombatTab()
        {
            UpdatePlayerList();
            GUILayout.Label("Select a player:", sectionHeaderStyle);

            for (int i = 0; i < playerNames.Count; i++)
            {
                GUIStyle playerButtonStyle = new GUIStyle(GUI.skin.button);
                playerButtonStyle.alignment = TextAnchor.MiddleCenter;
                playerButtonStyle.fontSize = 14;
                playerButtonStyle.fontStyle = FontStyle.Bold;
                playerButtonStyle.fixedHeight = 28;
                playerButtonStyle.margin = new RectOffset(2, 2, 2, 2);
                playerButtonStyle.padding = new RectOffset(8, 8, 4, 4);
                playerButtonStyle.border = new RectOffset(4, 4, 4, 4);

                // Set background & text color based on selection
                if (i == selectedPlayerIndex)
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(50, 50, 70, 255)); // Highlight
                    playerButtonStyle.normal.textColor = new Color32(255, 165, 0, 255);
                }
                else
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(30, 30, 30, 255)); // Default
                    playerButtonStyle.normal.textColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                    playerButtonStyle.hover.background = MakeSolidBackground(new UnityEngine.Color32(40, 40, 50, 255)); // On hover
                    playerButtonStyle.hover.textColor = UnityEngine.Color.white;
                }

                if (GUILayout.Button(playerNames[i], playerButtonStyle))
                {
                    selectedPlayerIndex = i;
                }
            }

            GUILayout.Space(40);

            if (GUILayout.Button("-1 Damage", buttonStyle))
            {
                if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                {
                    Players.DamagePlayer(playerList[selectedPlayerIndex], 1, playerNames[selectedPlayerIndex]);
                    Debug.Log($"Player {playerNames[selectedPlayerIndex]} damaged.");
                }
                else
                {
                    Debug.Log("No valid player selected to damage!");
                }
            }

            if (GUILayout.Button("Max Heal", buttonStyle))
            {
                if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                {
                    Players.HealPlayer(playerList[selectedPlayerIndex], 50, playerNames[selectedPlayerIndex]);
                    Debug.Log($"Player {playerNames[selectedPlayerIndex]} healed.");
                }
                else
                {
                    Debug.Log("No valid player selected to heal!");
                }
            }

            if (GUILayout.Button("Kill", buttonStyle))
            {
                Players.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
                Debug.Log("Player killed: " + playerNames[selectedPlayerIndex]);
            }

            if (GUILayout.Button("Revive", buttonStyle))
            {
                Players.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
                Debug.Log("Player revived: " + playerNames[selectedPlayerIndex]);
            }

            if (GUILayout.Button("Tumble (10s)", buttonStyle))
            {
                Players.ForcePlayerTumble();
                Debug.Log("Player tumbled: " + playerNames[selectedPlayerIndex]);
            }

            if (GUILayout.Button(showTeleportUI ? "Hide Teleport Options" : "Teleport Options", buttonStyle))
            {
                showTeleportUI = !showTeleportUI;
                if (showTeleportUI)
                {
                    UpdateTeleportOptions();
                }
            }

            if (showTeleportUI)
            {
                DrawTeleportOptions();
            }
        }

        void DrawMiscTab()
        {
            UpdatePlayerList();
            GUILayout.Label("Select a player:", sectionHeaderStyle);

            for (int i = 0; i < playerNames.Count; i++)
            {
                GUIStyle playerButtonStyle = new GUIStyle(GUI.skin.button);
                playerButtonStyle.alignment = TextAnchor.MiddleCenter;
                playerButtonStyle.fontSize = 14;
                playerButtonStyle.fontStyle = FontStyle.Bold;
                playerButtonStyle.fixedHeight = 28;
                playerButtonStyle.margin = new RectOffset(2, 2, 2, 2);
                playerButtonStyle.padding = new RectOffset(8, 8, 4, 4);
                playerButtonStyle.border = new RectOffset(4, 4, 4, 4);

                // Set background & text color based on selection
                if (i == selectedPlayerIndex)
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(50, 50, 70, 255)); // Highlight
                    playerButtonStyle.normal.textColor = new Color32(255, 165, 0, 255);
                }
                else
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(30, 30, 30, 255)); // Default
                    playerButtonStyle.normal.textColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                    playerButtonStyle.hover.background = MakeSolidBackground(new UnityEngine.Color32(40, 40, 50, 255)); // On hover
                    playerButtonStyle.hover.textColor = UnityEngine.Color.white;
                }

                if (GUILayout.Button(playerNames[i], playerButtonStyle))
                {
                    selectedPlayerIndex = i;
                }
            }

            GUILayout.Space(40);

            if (GUILayout.Button("[HOST] Spawn Money", buttonStyle))
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer != null)
                {
                    Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                    ItemSpawner.SpawnMoney(targetPosition);
                    Debug.Log("Money spawned.");
                }
            }

            // Force Host + Level Dropdown
            GUILayout.BeginHorizontal();
            //if (GUILayout.Button("Force Host [Broken]", buttonStyle)) ForceHost.Instance.StartCoroutine(ForceHost.Instance.ForceStart(availableLevels[selectedLevelIndex]));
            //if (GUILayout.Button(availableLevels[selectedLevelIndex], buttonStyle)) showLevelDropdown = !showLevelDropdown;
            GUILayout.EndHorizontal();

            if (showLevelDropdown)
            {
                levelDropdownScroll = GUILayout.BeginScrollView(levelDropdownScroll, GUILayout.Height(150));
                for (int i = 0; i < availableLevels.Length; i++)
                {
                    if (GUILayout.Button(availableLevels[i], buttonStyle))
                    {
                        selectedLevelIndex = i;
                        showLevelDropdown = false;
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(10);

            // === Spoof Name ===
            GUILayout.Label("Name Spoofing", sectionHeaderStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spoof Name", buttonStyle))
            {
                if (!string.IsNullOrEmpty(spoofedNameText))
                {
                    ChatHijack.ToggleNameSpoofing(true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
                }
            }

            if (GUILayout.Button(spoofTargetVisibleName, buttonStyle)) spoofDropdownVisible = !spoofDropdownVisible;

            string fieldId = "SpoofNameField";

            GUI.SetNextControlName(fieldId);
            spoofedNameText = GUILayout.TextField(spoofedNameText, textFieldStyle, GUILayout.Width(110));

            if (GUI.GetNameOfFocusedControl() == fieldId && !showTextEditorPopup)
            {
                activeTextFieldId = fieldId;
                largeTextBoxContent = spoofedNameText;
                showTextEditorPopup = true;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            if (spoofDropdownVisible)
            {
                for (int i = 0; i < playerNames.Count + 1; i++)
                {
                    string name = (i == 0) ? "All" : playerNames[i - 1];
                    if (GUILayout.Button(name, buttonStyle))
                    {
                        spoofTargetVisibleName = name;
                        spoofDropdownVisible = false;
                        if (!string.IsNullOrEmpty(spoofedNameText))
                        {
                            ChatHijack.ToggleNameSpoofing(true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
                        }
                    }
                }
            }

            if (GUILayout.Button("Reset Spoofed Name", buttonStyle))
            {
                ChatHijack.ToggleNameSpoofing(false, "", spoofTargetVisibleName, playerList, playerNames);
                spoofedNameText = "Text";
            }
            GUILayout.Space(10);

            ToggleLogic("persistent_spoof_name", " Persistent Spoof Name", ref spoofNameActive, null);
            if (spoofNameActive)
            {
                fieldId = "PersistentSpoofNameField";
                GUI.SetNextControlName(fieldId);
                persistentNameText = GUILayout.TextField(persistentNameText, textFieldStyle, GUILayout.Width(210));
                if (GUI.GetNameOfFocusedControl() == fieldId && !showTextEditorPopup)
                {
                    activeTextFieldId = fieldId;
                    largeTextBoxContent = persistentNameText;
                    showTextEditorPopup = true;
                    GUI.FocusControl(null); // prevent retrigger
                }
            }

            GUILayout.Space(10);

            // === Color Spoof ===
            GUILayout.Label("Color Spoofing", sectionHeaderStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spoof Color", buttonStyle))
            {
                if (int.TryParse(colorIndexText, out int index) && colorNameMapping.ContainsKey(index))
                {
                    ChatHijack.ChangePlayerColor(index, colorTargetVisibleName, playerList, playerNames);
                }
            }

            if (GUILayout.Button(colorTargetVisibleName, buttonStyle)) colorDropdownVisible = !colorDropdownVisible;

            if (GUILayout.Button(colorNameMapping.TryGetValue(int.Parse(colorIndexText), out var cname) ? cname : "Select Color", buttonStyle))
            {
                showColorIndexDropdown = !showColorIndexDropdown;
            }
            GUILayout.EndHorizontal();

            if (colorDropdownVisible)
            {
                for (int i = 0; i < playerNames.Count + 1; i++)
                {
                    string name = (i == 0) ? "All" : playerNames[i - 1];
                    if (GUILayout.Button(name, buttonStyle))
                    {
                        colorTargetVisibleName = name;
                        colorDropdownVisible = false;
                    }
                }
            }

            if (showColorIndexDropdown)
            {
                colorIndexScrollPosition = GUILayout.BeginScrollView(colorIndexScrollPosition, GUILayout.Height(150));
                foreach (var kvp in colorNameMapping)
                {
                    if (GUILayout.Button(kvp.Value, buttonStyle))
                    {
                        colorIndexText = kvp.Key.ToString();
                        showColorIndexDropdown = false;
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(10);

            // === Chat Spoof ===
            GUILayout.Label("Chat Spammer", sectionHeaderStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Send Chat", buttonStyle))
            {
                ChatHijack.MakeChat(chatMessageText, ChatDropdownVisibleName, playerList, playerNames);
            }

            if (GUILayout.Button(ChatDropdownVisibleName, buttonStyle)) ChatDropdownVisible = !ChatDropdownVisible;

            fieldId = "chatmessageField";
            GUI.SetNextControlName(fieldId);
            chatMessageText = GUILayout.TextField(chatMessageText, textFieldStyle, GUILayout.Width(110));
            if (GUI.GetNameOfFocusedControl() == fieldId && !showTextEditorPopup)
            {
                activeTextFieldId = fieldId;
                largeTextBoxContent = chatMessageText;
                showTextEditorPopup = true;
                GUI.FocusControl(null); // prevent retrigger
            }
            GUILayout.EndHorizontal();

            if (ChatDropdownVisible)
            {
                for (int i = 0; i < playerNames.Count + 1; i++)
                {
                    string name = (i == 0) ? "All" : playerNames[i - 1];
                    if (GUILayout.Button(name, buttonStyle))
                    {
                        ChatDropdownVisibleName = name;
                        ChatDropdownVisible = false;
                    }
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Map Tweaks (can't be undone in level):", sectionHeaderStyle);

            if (GUILayout.Button("Disable '?' Overlay", buttonStyle)) MapTools.changeOverlayStatus(true);
            if (GUILayout.Button("Discover Map Valuables", buttonStyle)) MapTools.DiscoveryMapValuables();

        }

        void DrawEnemiesTab()
        {
            try
            {
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

                    cachedFilteredEnemySetups = new List<EnemySetup>();
                    cachedEnemySetupNames = new List<string>();
                    if (enemySetups != null)
                    {
                        foreach (var setup in enemySetups)
                        {
                            if (setup != null && !setup.name.Contains("Enemy Group"))
                            {
                                string displayName = setup.name.StartsWith("Enemy -") ?
                                    setup.name.Substring("Enemy -".Length).Trim() :
                                    setup.name;

                                cachedFilteredEnemySetups.Add(setup);
                                cachedEnemySetupNames.Add(displayName);
                            }
                        }
                    }
                }
                UpdateEnemyList();
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label("Select an enemy:", sectionHeaderStyle);

                enemyScrollPosition = GUILayout.BeginScrollView(enemyScrollPosition, GUILayout.Height(Mathf.Min(200, enemyNames.Count * 35)));

                for (int i = 0; i < enemyNames.Count; i++)
                {
                    GUIStyle enemyButtonStyle = new GUIStyle(GUI.skin.button);
                    enemyButtonStyle.fontSize = 13;
                    enemyButtonStyle.fontStyle = FontStyle.Bold;
                    enemyButtonStyle.alignment = TextAnchor.MiddleCenter;
                    enemyButtonStyle.margin = new RectOffset(4, 4, 2, 2);
                    enemyButtonStyle.padding = new RectOffset(6, 6, 4, 4);
                    enemyButtonStyle.border = new RectOffset(4, 4, 4, 4);

                    // Backgrounds
                    if (i == selectedEnemyIndex)
                    {
                        enemyButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(60, 30, 10, 200)); // Deep orange highlight
                        enemyButtonStyle.normal.textColor = new UnityEngine.Color(1f, 0.55f, 0.1f); // Orange text
                    }
                    else
                    {
                        enemyButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(30, 30, 35, 180));
                        enemyButtonStyle.normal.textColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                    }

                    if (GUILayout.Button(enemyNames[i], enemyButtonStyle, GUILayout.Height(30)))
                        selectedEnemyIndex = i;
                }

                GUILayout.EndScrollView();
                GUI.color = UnityEngine.Color.white;

                GUILayout.Space(40);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("[HOST] Spawn", buttonStyle))
                {
                    TrySpawnEnemy();
                }
                spawnCountText = GUILayout.TextField(spawnCountText, textFieldStyle);
                string spawnDropdownText = (spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedEnemySetupNames.Count) ?
                                           cachedEnemySetupNames[spawnEnemyIndex] : "Select enemy";
                if (GUILayout.Button(spawnDropdownText, buttonStyle))
                {
                    showSpawnDropdown = !showSpawnDropdown;
                }
                GUILayout.EndHorizontal();

                if (showSpawnDropdown)
                {
                    spawnDropdownScrollPosition = GUILayout.BeginScrollView(spawnDropdownScrollPosition, GUILayout.Height(150));
                    for (int i = 0; i < cachedEnemySetupNames.Count; i++)
                    {
                        if (GUILayout.Button(cachedEnemySetupNames[i], buttonStyle))
                        {
                            spawnEnemyIndex = i;
                            showSpawnDropdown = false;
                        }
                    }
                    GUILayout.EndScrollView();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Kill Enemy", buttonStyle)) Enemies.KillSelectedEnemy(selectedEnemyIndex, enemyList, enemyNames);
                if (GUILayout.Button("Kill All Enemies", buttonStyle)) Enemies.KillAllEnemies();
                ToggleLogic("blind_enemies", " Blind Enemies", ref blindEnemies, null);

                if (GUILayout.Button(showEnemyTeleportUI ? "Hide Teleport Options" : "Teleport Options", buttonStyle))
                {
                    showEnemyTeleportUI = !showEnemyTeleportUI;
                    if (showEnemyTeleportUI) UpdateEnemyTeleportOptions();
                }

                if (showEnemyTeleportUI)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Teleport Enemy To →", labelStyle);
                    string currentDest = (enemyTeleportDestIndex >= 0 && enemyTeleportDestIndex < enemyTeleportDestOptions.Length) ?
                                         enemyTeleportDestOptions[enemyTeleportDestIndex] : "No players available";
                    if (GUILayout.Button(currentDest, buttonStyle)) showEnemyTeleportDropdown = !showEnemyTeleportDropdown;
                    GUILayout.EndHorizontal();

                    if (showEnemyTeleportDropdown)
                    {
                        enemyTeleportDropdownScrollPosition = GUILayout.BeginScrollView(enemyTeleportDropdownScrollPosition, GUILayout.Height(150));
                        for (int i = 0; i < enemyTeleportDestOptions.Length; i++)
                        {
                            if (i != enemyTeleportDestIndex)
                            {
                                if (GUILayout.Button(enemyTeleportDestOptions[i])) enemyTeleportDestIndex = i;
                            }
                        }
                        GUILayout.EndScrollView();
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("Execute Teleport", buttonStyle))
                    {
                        int playerIndex = enemyTeleportDestIndex;
                        if (playerIndex >= 0 && playerIndex < playerList.Count)
                        {
                            if (DebugCheats.IsLocalPlayer(playerList[playerIndex]))
                                Enemies.TeleportEnemyToMe(selectedEnemyIndex, enemyList, enemyNames);
                            else
                                Enemies.TeleportEnemyToPlayer(selectedEnemyIndex, enemyList, enemyNames, playerIndex, playerList, playerNames);
                            UpdateEnemyList();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnemiesTab] GUI Exception: {e}");
            }
        }

        void DrawItemsTab()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Select an item:", sectionHeaderStyle);
            var sortedItems = itemList.OrderByDescending(item => item.Value).ToList();

            itemScroll = GUILayout.BeginScrollView(itemScroll, GUILayout.Height(200));
            for (int i = 0; i < sortedItems.Count; i++)
            {
                GUIStyle itemButtonStyle = new GUIStyle(GUI.skin.button);
                itemButtonStyle.fontSize = 13;
                itemButtonStyle.fontStyle = FontStyle.Bold;
                itemButtonStyle.alignment = TextAnchor.MiddleCenter;
                itemButtonStyle.margin = new RectOffset(4, 4, 2, 2);
                itemButtonStyle.padding = new RectOffset(6, 6, 4, 4);
                itemButtonStyle.border = new RectOffset(4, 4, 4, 4);

                if (i == selectedItemIndex)
                {
                    itemButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(45, 25, 5, 220));
                    itemButtonStyle.normal.textColor = new UnityEngine.Color(1f, 0.55f, 0.1f);
                }
                else
                {
                    itemButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(28, 28, 30, 180));
                    itemButtonStyle.normal.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f);
                }

                string itemLabel = $"{sortedItems[i].Name}   [Value: ${sortedItems[i].Value}]";
                if (GUILayout.Button(itemLabel, itemButtonStyle, GUILayout.Height(30)))
                    selectedItemIndex = i;
            }
            GUILayout.EndScrollView();
            GUI.color = UnityEngine.Color.white;


            if (GUILayout.Button("Teleport Item to Me", buttonStyle))
            {
                if (selectedItemIndex >= 0 && selectedItemIndex < sortedItems.Count)
                    ItemTeleport.TeleportItemToMe(sortedItems[selectedItemIndex]);
            }

            if (GUILayout.Button("Teleport All Items to Me", buttonStyle))
                ItemTeleport.TeleportAllItemsToMe();

            GUILayout.Space(10);
            GUILayout.Label("Change Item Value:", sectionHeaderStyle);
            int displayValue = (int)Mathf.Pow(10, itemValueSliderPos);
            GUILayout.Label($"${displayValue:N0}", labelStyle);
            itemValueSliderPos = GUILayout.HorizontalSlider(itemValueSliderPos, 3f, 9f);
            if (GUILayout.Button("Apply Value Change", buttonStyle))
            {
                if (selectedItemIndex >= 0 && selectedItemIndex < sortedItems.Count)
                {
                    var selectedItem = sortedItems[selectedItemIndex];
                    ItemTeleport.SetItemValue(selectedItem, displayValue);
                }
            }

            if (GUILayout.Button(showItemSpawner ? "Hide Item Spawner" : "Show Item Spawner", buttonStyle))
            {
                showItemSpawner = !showItemSpawner;
                if (showItemSpawner && availableItemsList.Count == 0)
                    availableItemsList = ItemSpawner.GetAvailableItems();
            }

            if (showItemSpawner)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Select item to spawn:", labelStyle, GUILayout.Width(160));
                itemSpawnSearch = GUILayout.TextField(itemSpawnSearch, textFieldStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
                var filteredItems = string.IsNullOrWhiteSpace(itemSpawnSearch)
                    ? availableItemsList
                    : availableItemsList.Where(item => item.ToLower().Contains(itemSpawnSearch.ToLower())).ToList();


                itemSpawnerScroll = GUILayout.BeginScrollView(itemSpawnerScroll, GUILayout.Height(150));
                for (int i = 0; i < filteredItems.Count; i++)
                {
                    GUIStyle itemSpawnButtonStyle = new GUIStyle(GUI.skin.button);
                    itemSpawnButtonStyle.fontSize = 13;
                    itemSpawnButtonStyle.fontStyle = FontStyle.Bold;
                    itemSpawnButtonStyle.alignment = TextAnchor.MiddleCenter;
                    itemSpawnButtonStyle.margin = new RectOffset(4, 4, 2, 2);
                    itemSpawnButtonStyle.padding = new RectOffset(6, 6, 4, 4);
                    itemSpawnButtonStyle.border = new RectOffset(4, 4, 4, 4);

                    if (i == selectedItemToSpawnIndex)
                    {
                        itemSpawnButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(45, 25, 5, 220));
                        itemSpawnButtonStyle.normal.textColor = new UnityEngine.Color(1f, 0.55f, 0.1f);
                    }
                    else
                    {
                        itemSpawnButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(28, 28, 30, 180));
                        itemSpawnButtonStyle.normal.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f);
                    }

                    if (GUILayout.Button(filteredItems[i], itemSpawnButtonStyle, GUILayout.Height(30)))
                        selectedItemToSpawnIndex = availableItemsList.IndexOf(filteredItems[i]);
                }
                GUILayout.EndScrollView();

                bool isValuable = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count &&
                                  availableItemsList[selectedItemToSpawnIndex].Contains("Valuable");

                if (isValuable)
                {
                    GUILayout.Label($"Item Value: ${itemSpawnValue:n0}", labelStyle);
                    float itemsliderValue = Mathf.Log10((float)itemSpawnValue / 1000f) / 6f;
                    float newitemSliderValue = GUILayout.HorizontalSlider(itemsliderValue, 0f, 1f);
                    if (newitemSliderValue != itemsliderValue && isHost)
                        itemSpawnValue = Mathf.Clamp((int)(Mathf.Pow(10, newitemSliderValue * 6f) * 1000f), 1000, 1000000000);
                }

                GUI.enabled = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count;
                if (GUILayout.Button("Spawn Selected Item", buttonStyle))
                {
                    GameObject localPlayer = DebugCheats.GetLocalPlayer();
                    if (localPlayer != null)
                    {
                        Vector3 spawnPos = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up;
                        string itemName = availableItemsList[selectedItemToSpawnIndex];
                        if (isValuable)
                            ItemSpawner.SpawnItem(itemName, spawnPos, itemSpawnValue);
                        else
                            ItemSpawner.SpawnItem(itemName, spawnPos);
                    }
                }

                if (GUILayout.Button("Spawn 50 of Selected Item", buttonStyle))
                    ItemSpawner.SpawnSelectedItemMultiple(50, availableItemsList, selectedItemToSpawnIndex, itemSpawnValue);
                GUI.enabled = true;
            }

            GUILayout.EndVertical();
        }

        void DrawHotkeysTab()
        {
            GUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(hotkeyManager.KeyAssignmentError) && Time.time - hotkeyManager.ErrorMessageTime < HotkeyManager.ERROR_MESSAGE_DURATION)
            {
                GUIStyle errorStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = UnityEngine.Color.red },
                    alignment = TextAnchor.MiddleCenter
                };
                GUILayout.Label(hotkeyManager.KeyAssignmentError, errorStyle, GUILayout.Height(25));
            }

            GUILayout.Label("Hotkey Configuration", sectionHeaderStyle);
            GUILayout.Space(5);

            GUILayout.Label("How to set up a hotkey:", labelStyle);
            GUILayout.Label("1. Click on a key field → press desired key", labelStyle);
            GUILayout.Label("2. Click on action field → select function", labelStyle);

            GUILayout.Space(10);
            GUILayout.Label("Warning: Ensure each key is only assigned to one action", warningStyle);

            GUILayout.Space(10);
            GUILayout.Label("System Keys", sectionHeaderStyle);

            DrawHotkeyField("Menu Toggle:", hotkeyManager.MenuToggleKey, () => hotkeyManager.StartConfigureSystemKey(0), 0);
            DrawHotkeyField("Reload:", hotkeyManager.ReloadKey, () => hotkeyManager.StartConfigureSystemKey(1), 1);
            DrawHotkeyField("Unload:", hotkeyManager.UnloadKey, () => hotkeyManager.StartConfigureSystemKey(2), 2);

            GUILayout.Space(20);
            GUILayout.Label("Action Hotkeys", sectionHeaderStyle);

            for (int i = 0; i < 12; i++)
            {
                KeyCode currentKey = hotkeyManager.GetHotkeyForSlot(i);
                string keyText = (hotkeyManager.SelectedHotkeySlot == i && hotkeyManager.ConfiguringHotkey) ? "Press any key..." : (currentKey == KeyCode.None ? "Not Set" : currentKey.ToString());
                string actionName = hotkeyManager.GetActionNameForKey(currentKey);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(keyText, buttonStyle))
                {
                    hotkeyManager.StartHotkeyConfiguration(i);
                }

                if (GUILayout.Button(actionName, buttonStyle))
                {
                    if (currentKey != KeyCode.None)
                    {
                        showingActionSelector = true;
                        hotkeyManager.ShowActionSelector(i, currentKey);
                    }
                    else
                    {
                        Debug.Log("Please assign a key to this slot first");
                    }
                }

                if (GUILayout.Button("Clear", buttonStyle))
                {
                    hotkeyManager.ClearHotkeyBinding(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Save Hotkey Settings", buttonStyle))
            {
                hotkeyManager.SaveHotkeySettings();
                Debug.Log("Hotkey settings saved manually");
            }

            GUILayout.EndVertical();
        }

        void DrawTrollingTab()
        {
            UpdatePlayerList();
            GUILayout.Label("Select a player:", sectionHeaderStyle);

            for (int i = 0; i < playerNames.Count; i++)
            {
                GUIStyle playerButtonStyle = new GUIStyle(GUI.skin.button);
                playerButtonStyle.alignment = TextAnchor.MiddleCenter;
                playerButtonStyle.fontSize = 14;
                playerButtonStyle.fontStyle = FontStyle.Bold;
                playerButtonStyle.fixedHeight = 28;
                playerButtonStyle.margin = new RectOffset(2, 2, 2, 2);
                playerButtonStyle.padding = new RectOffset(8, 8, 4, 4);
                playerButtonStyle.border = new RectOffset(4, 4, 4, 4);

                // Set background & text color based on selection
                if (i == selectedPlayerIndex)
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(50, 50, 70, 255)); // Highlight
                    playerButtonStyle.normal.textColor = new Color32(255, 165, 0, 255);
                }
                else
                {
                    playerButtonStyle.normal.background = MakeSolidBackground(new UnityEngine.Color32(30, 30, 30, 255)); // Default
                    playerButtonStyle.normal.textColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                    playerButtonStyle.hover.background = MakeSolidBackground(new UnityEngine.Color32(40, 40, 50, 255)); // On hover
                    playerButtonStyle.hover.textColor = UnityEngine.Color.white;
                }

                if (GUILayout.Button(playerNames[i], playerButtonStyle))
                {
                    selectedPlayerIndex = i;
                }
            }
            GUILayout.Space(40);

            if (GUILayout.Button("Force Glitch", buttonStyle)) { Troll.ForcePlayerGlitch(); }
            GUILayout.Space(5);

            if (GUILayout.Button("Force Unmute", buttonStyle)) { MiscFeatures.ForcePlayerMicVolume(100); }
            GUILayout.Space(5);

            if (GUILayout.Button("Force Mute", buttonStyle)) { MiscFeatures.ForcePlayerMicVolume(-1); }
            GUILayout.Space(5);

            if (GUILayout.Button("Force Infinite Tumble", buttonStyle)) { Players.ForcePlayerTumble(9999999f); }
            GUILayout.Space(5);

            if (GUILayout.Button("Infinite Loading Screen", buttonStyle)) { Troll.InfiniteLoadingSelectedPlayer(); }
            GUILayout.Space(5);

            if (GUILayout.Button("Remove Infinite Loading Screen", buttonStyle)) { Troll.SceneRecovery(); }
            GUILayout.Space(5);

            if (GUILayout.Button("Crash Selected Player", buttonStyle)) { MiscFeatures.CrashSelectedPlayerNew(); }
            GUILayout.Space(5);

            if (GUILayout.Button("Crash Lobby", buttonStyle)) 
            {
                DLog.Log("Crashing Lobby!");
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                    return;
                Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                transform.position = targetPosition;
                CrashLobby.Crash(targetPosition);
            }
            GUILayout.Space(5);
        }

        void DrawConfigTab()
        {
            GUILayout.Label("Config", sectionHeaderStyle);
            GUILayout.Label(configstatus, titleStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAVE CONFIG", buttonStyle))
            {
                ConfigManager.SaveAllToggles();
                configstatus = "Config Saved";
            }
            if (GUILayout.Button("LOAD CONFIG", buttonStyle))
            {
                ConfigManager.LoadAllToggles();
                configstatus = "Config Loaded";
            }
            GUILayout.EndHorizontal();
        }

        void DrawFeatureSelectionWindow(int id)
        {
            GUILayout.Label("Select a feature to bind", sectionHeaderStyle);

            actionScroll = GUILayout.BeginScrollView(actionScroll, false, true, GUILayout.Width(380), GUILayout.Height(320));

            List<HotkeyManager.HotkeyAction> actions = HotkeyManager.Instance.GetAvailableActions();
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (GUILayout.Button(action.Name, buttonStyle))
                {
                    HotkeyManager.Instance.AssignActionToHotkey(i);
                    showingActionSelector = false;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);

            if (GUILayout.Button("Cancel", buttonStyle))
            {
                showingActionSelector = false;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 30));
        }

        void DrawHotkeyField(string label, KeyCode key, Action configureCallback, int index)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, buttonStyle);

            string keyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == index && hotkeyManager.WaitingForAnyKey)
                ? "Press any key..." : key.ToString();

            if (GUILayout.Button(keyText, buttonStyle))
            {
                configureCallback();
            }
            GUILayout.EndHorizontal();
        }

        void DrawChamsColorWindow(int windowID)
        {
            // Allow the window to be dragged from the top bar.
            GUI.DragWindow(new Rect(0, 0, chamsWindowRect.width, 25));

            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.Label("Chams Color Picker", titleStyle);

            string[] colorOptions = { "Enemy Visible", "Enemy Hidden", "Item Visible", "Item Hidden" };

            for (int i = 0; i < colorOptions.Length; i++)
            {
                UnityEngine.Color previewColor = UnityEngine.Color.white;
                if (i == 0) previewColor = DebugCheats.enemyVisibleColor;
                else if (i == 1) previewColor = DebugCheats.enemyHiddenColor;
                else if (i == 2) previewColor = DebugCheats.itemVisibleColor;
                else if (i == 3) previewColor = DebugCheats.itemHiddenColor;

                GUIStyle previewStyle = new GUIStyle(GUI.skin.button);
                previewStyle.normal.background = colorpicker(previewColor, 1f);
                previewStyle.normal.textColor = GetContrastColor(previewColor);
                previewStyle.fontStyle = FontStyle.Bold;
                previewStyle.alignment = TextAnchor.MiddleCenter;

                if (GUILayout.Button(colorOptions[i], previewStyle, GUILayout.Height(25)))
                {
                    selectedColorOption = i;
                }
            }

            GUILayout.Space(10);

            // Determine the current color based on the selected option
            UnityEngine.Color currentColor = UnityEngine.Color.white;
            if (selectedColorOption == 0) currentColor = DebugCheats.enemyVisibleColor;
            else if (selectedColorOption == 1) currentColor = DebugCheats.enemyHiddenColor;
            else if (selectedColorOption == 2) currentColor = DebugCheats.itemVisibleColor;
            else if (selectedColorOption == 3) currentColor = DebugCheats.itemHiddenColor;

            float sliderHeight = 200f;

            // Horizontal layout for the sliders with flexible spaces for centering
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // R
            GUILayout.BeginVertical();
            GUILayout.Label("R", labelStyle, GUILayout.Width(20));
            GUILayout.Space(5); // Space between the label and slider
            float r = GUILayout.VerticalSlider(currentColor.r, 1f, 0f, GUILayout.Height(sliderHeight), GUILayout.Width(20));
            GUILayout.EndVertical();

            GUILayout.Space(20); // Space between columns

            // G
            GUILayout.BeginVertical();
            GUILayout.Label("G", labelStyle, GUILayout.Width(20));
            GUILayout.Space(5);
            float g = GUILayout.VerticalSlider(currentColor.g, 1f, 0f, GUILayout.Height(sliderHeight), GUILayout.Width(20));
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // B
            GUILayout.BeginVertical();
            GUILayout.Label("B", labelStyle, GUILayout.Width(20));
            GUILayout.Space(5);
            float b = GUILayout.VerticalSlider(currentColor.b, 1f, 0f, GUILayout.Height(sliderHeight), GUILayout.Width(20));
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // A
            GUILayout.BeginVertical();
            GUILayout.Label("A", labelStyle, GUILayout.Width(20));
            GUILayout.Space(5);
            float a = GUILayout.VerticalSlider(currentColor.a, 1f, 0f, GUILayout.Height(sliderHeight), GUILayout.Width(20));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            UnityEngine.Color newColor = new UnityEngine.Color(r, g, b, a);
            if (newColor != currentColor)
            {
                if (selectedColorOption == 0) DebugCheats.enemyVisibleColor = newColor;
                else if (selectedColorOption == 1) DebugCheats.enemyHiddenColor = newColor;
                else if (selectedColorOption == 2) DebugCheats.itemVisibleColor = newColor;
                else if (selectedColorOption == 3) DebugCheats.itemHiddenColor = newColor;
            }

            GUILayout.EndVertical();
        }

        void DrawServersTab()
        {
            if (rowBgNormal == null)
            {
                rowBgNormal = MakeSolidBackground(new UnityEngine.Color32(30, 30, 30, 255));
                rowBgHover = MakeSolidBackground(new UnityEngine.Color32(40, 40, 50, 255));
                rowBgSelected = MakeSolidBackground(new UnityEngine.Color32(50, 50, 70, 255));
            }

            GUILayout.Label("Server Browser", titleStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Lobbies", buttonStyle, GUILayout.Width(160)))
            {
                LobbyHostCache.Clear();
                LobbyMemberCache.Clear();
                LobbyFinder.AlreadyTriedLobbies.Clear();
                LobbyFinder.RefreshLobbies();
            }
            ToggleLogic("hide_full_lobbies", " Hide Full Lobbies", ref hideFullLobbies, null);
            ToggleLogic("show_lobby_members", " Show Lobby Members", ref showMemberWindow, null);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", labelStyle, GUILayout.Width(60));
            lobbySearchTerm = GUILayout.TextField(lobbySearchTerm, textFieldStyle, GUILayout.Width(260));

            if (GUILayout.Button("Region A-Z", buttonStyle, GUILayout.Width(100))) sortMode = SortMode.RegionAZ;
            if (GUILayout.Button("Region Z-A", buttonStyle, GUILayout.Width(100))) sortMode = SortMode.RegionZA;
            if (GUILayout.Button("Most Players", buttonStyle, GUILayout.Width(110))) sortMode = SortMode.MostPlayers;
            if (GUILayout.Button("Least Players", buttonStyle, GUILayout.Width(120))) sortMode = SortMode.LeastPlayers;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lobby Name", labelStyle, GUILayout.Width(280));
            GUILayout.Space(10);
            GUILayout.Label("Players", labelStyle, GUILayout.Width(80));
            GUILayout.Space(10);
            GUILayout.Label("Region", labelStyle, GUILayout.Width(80));
            GUILayout.Space(50);
            GUILayout.Label("Host", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            serverListScroll = GUILayout.BeginScrollView(serverListScroll, boxStyle, GUILayout.Height(500));
            var lobbies = new List<Lobby>(LobbyFinder.FoundLobbies);

            switch (sortMode)
            {
                case SortMode.RegionAZ: lobbies.Sort((a, b) => string.Compare(a.GetData("Region"), b.GetData("Region"))); break;
                case SortMode.RegionZA: lobbies.Sort((a, b) => string.Compare(b.GetData("Region"), a.GetData("Region"))); break;
                case SortMode.MostPlayers: lobbies.Sort((a, b) => b.MemberCount.CompareTo(a.MemberCount)); break;
                case SortMode.LeastPlayers: lobbies.Sort((a, b) => a.MemberCount.CompareTo(b.MemberCount)); break;
            }

            foreach (var lobby in lobbies)
            {
                if (hideFullLobbies && lobby.MemberCount >= lobby.MaxMembers)
                    continue;

                if (!LobbyHostCache.ContainsKey(lobby.Id))
                    continue;

                if (lobby.MemberCount < 3)
                    continue;

                string hostDisplay = LobbyHostCache.TryGetValue(lobby.Id, out var hostStr) ? hostStr : "Fetching...";

                if (hostDisplay.Contains("Failed (0)"))
                    continue;

                bool matchesSearch = string.IsNullOrWhiteSpace(lobbySearchTerm) ||
                    lobby.Id.ToString().IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (hostDisplay.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (LobbyMemberCache.TryGetValue(lobby.Id, out var members) && members.Exists(m => m.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0));


                if (!matchesSearch) continue;

                GUIStyle rowStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 32,
                    margin = new RectOffset(2, 2, 2, 2),
                    padding = new RectOffset(8, 8, 4, 4),
                    border = new RectOffset(4, 4, 4, 4)
                };

                if (lobby.Id == selectedLobbyId)
                {
                    rowStyle.normal.background = rowBgSelected;
                    rowStyle.normal.textColor = new Color32(255, 165, 0, 255);
                }
                else
                {
                    rowStyle.normal.background = rowBgNormal;
                    rowStyle.normal.textColor = new UnityEngine.Color(0.8f, 0.8f, 0.8f);
                    rowStyle.hover.background = rowBgHover;
                    rowStyle.hover.textColor = UnityEngine.Color.white;
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                string hostName = hostDisplay.Contains("(") ? hostDisplay.Substring(0, hostDisplay.IndexOf("(")).Trim() : hostDisplay;
                string lobbyName = "Lobby of " + (string.IsNullOrWhiteSpace(hostName) ? "Unknown" : hostName);
                if (lobby.MaxMembers > 6)
                    lobbyName += " <color=red>(Modded)</color>";

                string region = lobby.GetData("Region");
                int current = Mathf.Max(0, lobby.MemberCount - 1);
                int max = lobby.MaxMembers;

                if (GUILayout.Button(lobbyName, rowStyle, GUILayout.Width(280)))
                    selectedLobbyId = lobby.Id;

                GUILayout.Space(10);
                GUILayout.Label(current + "/" + max, labelStyle, GUILayout.Width(80));
                GUILayout.Space(10);
                GUILayout.Label(region, labelStyle, GUILayout.Width(80));
                GUILayout.Space(10);
                GUILayout.Label(hostDisplay, labelStyle, GUILayout.Width(260));

                GUILayout.EndHorizontal();
                GUILayout.Space(6);
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Join Lobby", buttonStyle, GUILayout.Width(120)))
            {
                var lobby = LobbyFinder.FoundLobbies.Find(l => l.Id == selectedLobbyId);
                if (lobby.Id != 0)
                {
                    LobbyFinder.JoinLobbyAndPlay(lobby);
                }
            }

            GUILayout.Space(12);

            if (selectedLobbyId != 0 && LobbyFinder.FoundLobbies.Find(l => l.Id == selectedLobbyId) is Lobby selectedLobby)
            {
                string region = selectedLobby.GetData("Region");
                string host = LobbyHostCache.TryGetValue(selectedLobbyId, out var hostStr) ? hostStr : "Unknown";
                GUILayout.Label($"Selected: {selectedLobbyId} | Host: {host} | Region: {region}", labelStyle);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Get Invite Link", buttonStyle, GUILayout.Width(150)))
            {
                if (LobbyFinder.FoundLobbies.Find(l => l.Id == selectedLobbyId) is Lobby inviteLobby)
                {
                    string hostSteamId = inviteLobby.Owner.Id.ToString();
                    string invite = $"steam://joinlobby/3241660/{inviteLobby.Id}/{hostSteamId}";
                    GUIUtility.systemCopyBuffer = invite;
                    Debug.Log("[InviteLink] Copied: " + invite);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTextEditorPopup(int id)
        {
            GUILayout.BeginVertical(boxStyle);
            textboxscroll = GUILayout.BeginScrollView(textboxscroll, GUILayout.Height(235));
            largeTextBoxContent = GUILayout.TextArea(largeTextBoxContent, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            if (activeTextFieldId == "SpoofNameField")
                spoofedNameText = largeTextBoxContent;
            else if (activeTextFieldId == "PersistentSpoofNameField")
                persistentNameText = largeTextBoxContent;
            else if (activeTextFieldId == "chatmessageField")
                chatMessageText = largeTextBoxContent;

            GUILayout.Space(10);
            if (GUILayout.Button("Close", buttonStyle, GUILayout.Height(25)))
            {
                showTextEditorPopup = false;
                activeTextFieldId = null;
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawLobbyMemberWindow(int id)
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("Lobby Members", sectionHeaderStyle);
            GUILayout.Space(4);
            memberWindowScroll = GUILayout.BeginScrollView(memberWindowScroll, GUILayout.Height(150));
            if (LobbyMemberCache.TryGetValue(selectedLobbyId, out var members))
            {
                foreach (var line in members)
                {
                    if (!line.Contains(SteamClient.Name))
                    {
                        GUILayout.Label("• " + line, labelStyle);
                        GUILayout.Space(5);
                    }
                }
            }
            else
            {
                GUILayout.Label("Fetching players...", warningStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(8);
            if (GUILayout.Button("Close", buttonStyle))
                showMemberWindow = false;
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        void InitStyles()
        {
            if (titleStyle == null)
            {
                // === TITLE ===
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color32(255, 165, 0, 255) }
                };

                // === TABS ===
                tabStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 27,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(8, 8, 4, 4),
                    margin = new RectOffset(4, 4, 2, 2)
                };
                tabStyle.normal.textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f);
                tabStyle.normal.background = MakeSolidBackground(new Color32(40, 40, 40, 255));
                tabStyle.hover.textColor = new Color32(255, 165, 0, 255);
                tabStyle.hover.background = MakeSolidBackground(new Color32(50, 50, 50, 255));
                tabStyle.active.textColor = UnityEngine.Color.white;
                tabStyle.active.background = MakeSolidBackground(new Color32(20, 20, 20, 255));

                tabSelectedStyle = new GUIStyle(tabStyle);
                tabSelectedStyle.normal.background = MakeSolidBackground(new Color32(30, 30, 30, 255));
                tabSelectedStyle.normal.textColor = new Color32(255, 165, 0, 255);

                // === SECTION HEADERS ===
                sectionHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new UnityEngine.Color(1f, 0.5f, 0f) }
                };

                // === BOX BACKGROUND ===
                boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = MakeSolidBackground(new Color32(25, 28, 35, 220)) },
                    padding = new RectOffset(10, 10, 10, 10)
                };

                // === SCROLLBAR ===
                scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
                {
                    fixedWidth = 12, // Slightly wider to allow visual spacing
                    border = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(2, 2, 2, 2),
                    padding = new RectOffset(0, 0, 0, 0)
                };
                scrollbarStyle.normal.background = MakeSolidBackground(new Color32(25, 25, 25, 255));
                scrollbarStyle.hover.background = MakeSolidBackground(new Color32(35, 35, 35, 255));
                scrollbarStyle.active.background = MakeSolidBackground(new Color32(45, 45, 45, 255));
                GUI.skin.verticalScrollbar = scrollbarStyle;

                // === SCROLLBAR THUMB ===
                scrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb)
                {
                    fixedWidth = 8, // Slightly smaller to center inside the track
                    margin = new RectOffset(2, 2, 2, 2),
                    border = new RectOffset(4, 4, 4, 4)
                };
                scrollbarThumbStyle.normal.background = MakeSolidBackground(new Color32(90, 90, 90, 255));
                scrollbarThumbStyle.hover.background = MakeSolidBackground(new Color32(110, 110, 110, 255));
                scrollbarThumbStyle.active.background = MakeSolidBackground(new Color32(130, 130, 130, 255));
                GUI.skin.verticalScrollbarThumb = scrollbarThumbStyle;


                // === SLIDER ===
                horizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                horizontalSliderStyle.fixedHeight = 12;
                horizontalSliderStyle.margin = new RectOffset(4, 4, 6, 6);
                horizontalSliderStyle.normal.background = MakeSolidBackground(new Color32(40, 40, 40, 255));
                horizontalSliderStyle.hover.background = MakeSolidBackground(new Color32(50, 50, 50, 255));
                horizontalSliderStyle.active.background = MakeSolidBackground(new Color32(60, 60, 60, 255));

                GUI.skin.horizontalSlider = horizontalSliderStyle;

                // === SLIDER ===
                verticalSliderStyle = new GUIStyle(GUI.skin.verticalSlider);
                verticalSliderStyle.fixedHeight = 190;
                verticalSliderStyle.margin = new RectOffset(4, 4, 6, 6);
                verticalSliderStyle.normal.background = MakeSolidBackground(new Color32(40, 40, 40, 255));
                verticalSliderStyle.hover.background = MakeSolidBackground(new Color32(50, 50, 50, 255));
                verticalSliderStyle.active.background = MakeSolidBackground(new Color32(60, 60, 60, 255));

                GUI.skin.verticalSlider = verticalSliderStyle;

                // === SLIDER THUMB ===
                horizontalThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                horizontalThumbStyle.fixedWidth = 16;
                horizontalThumbStyle.fixedHeight = 16;
                horizontalThumbStyle.border = new RectOffset(4, 4, 4, 4);
                horizontalThumbStyle.margin = new RectOffset(0, 0, 0, 0);
                horizontalThumbStyle.padding = new RectOffset(0, 0, 0, 0);
                horizontalThumbStyle.normal.background = MakeSolidBackground(new Color32(90, 90, 120, 255));
                horizontalThumbStyle.hover.background = MakeSolidBackground(new Color32(110, 110, 140, 255));
                horizontalThumbStyle.active.background = MakeSolidBackground(new Color32(130, 130, 160, 255));

                GUI.skin.horizontalSliderThumb = horizontalThumbStyle;

                verticalThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
                verticalThumbStyle.fixedWidth = 16;
                verticalThumbStyle.fixedHeight = 16;
                verticalThumbStyle.border = new RectOffset(4, 4, 4, 4);
                verticalThumbStyle.margin = new RectOffset(-2, -2, -2, -2);
                verticalThumbStyle.padding = new RectOffset(0, 0, 0, 0);
                verticalThumbStyle.normal.background = MakeSolidBackground(new Color32(90, 90, 120, 255));
                verticalThumbStyle.hover.background = MakeSolidBackground(new Color32(110, 110, 140, 255));
                verticalThumbStyle.active.background = MakeSolidBackground(new Color32(130, 130, 160, 255));

                GUI.skin.verticalSliderThumb = verticalThumbStyle;

                // === BUTTONS ===
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(12, 12, 6, 6),
                    margin = new RectOffset(4, 4, 4, 4),
                    border = new RectOffset(6, 6, 6, 6)
                };

                Color32 baseColor = new Color32(45, 45, 55, 255);
                Color32 hoverColor = new Color32(60, 50, 35, 255);
                Color32 activeColor = new Color32(90, 60, 30, 255);
                Color32 textColor = new Color32(240, 240, 240, 255);
                Color32 hoverText = new Color32(255, 165, 0, 255);

                buttonStyle.normal.background = MakeSolidBackground(baseColor);
                buttonStyle.normal.textColor = textColor;

                buttonStyle.hover.background = MakeSolidBackground(hoverColor);
                buttonStyle.hover.textColor = hoverText;

                buttonStyle.active.background = MakeSolidBackground(activeColor);
                buttonStyle.active.textColor = hoverText;

                // === LABELS ===
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = UnityEngine.Color.white }
                };
                labelStyle.richText = true;

                warningStyle = new GUIStyle(labelStyle);
                warningStyle.normal.textColor = UnityEngine.Color.yellow;

                // === TEXTFIELD ===
                textFieldStyle = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 28,
                    padding = new RectOffset(10, 10, 6, 6),
                    margin = new RectOffset(4, 4, 4, 4),
                    border = new RectOffset(4, 4, 4, 4),

                    // 👇 This makes the typing follow the cursor
                    wordWrap = false,
                    clipping = TextClipping.Clip,

                    normal =
                    {
                        background = MakeSolidBackground(new Color32(50, 50, 60, 255)),
                        textColor = new Color32(230, 230, 230, 255)
                    },
                    focused =
                    {
                        background = MakeSolidBackground(new Color32(60, 60, 80, 255)),
                        textColor = UnityEngine.Color.white
                    },
                    hover =
                    {
                        background = MakeSolidBackground(new Color32(55, 55, 70, 255)),
                        textColor = UnityEngine.Color.white
                    },
                    active =
                    {
                        background = MakeSolidBackground(new Color32(70, 70, 90, 255)),
                        textColor = UnityEngine.Color.white
                    }
                };

                // === WINDOW BACKGROUND ===
                backgroundStyle = new GUIStyle(GUI.skin.window);
                Texture2D bg = MakeSolidBackground(new Color32(15, 18, 25, 230));
                backgroundStyle.normal.background = bg;
                backgroundStyle.onNormal.background = bg;
                backgroundStyle.focused.background = bg;
                backgroundStyle.onFocused.background = bg;
                backgroundStyle.border = new RectOffset(0, 0, 0, 0);
                backgroundStyle.margin = new RectOffset(0, 0, 0, 0);
                backgroundStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            // === Fallback repairs ===
            if (backgroundStyle == null || backgroundStyle.normal.background == null)
            {
                backgroundStyle = new GUIStyle(GUI.skin.window);
                backgroundStyle.normal.background = MakeSolidBackground(new Color32(15, 18, 25, 230));
            }

            if (boxStyle == null || boxStyle.normal.background == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = MakeSolidBackground(new Color32(25, 28, 35, 220));
                boxStyle.padding = new RectOffset(10, 10, 10, 10);
            }
        }

        void HandleResize()
        {
            float handleSize = 16f;
            Rect resizeHandleRect = new Rect(menuRect.xMax - handleSize, menuRect.yMax - handleSize, handleSize, handleSize);

            GUI.Box(resizeHandleRect, "", GUI.skin.box);

            if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
            {
                isResizing = true;
                resizeStartMousePos = Event.current.mousePosition;
                resizeStartSize = new Vector2(menuRect.width, menuRect.height);
                Event.current.Use();
            }

            if (isResizing)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 offset = Event.current.mousePosition - resizeStartMousePos;
                    menuRect.width = Mathf.Clamp(resizeStartSize.x + offset.x, 400, 1200);
                    menuRect.height = Mathf.Clamp(resizeStartSize.y + offset.y, 300, 1000);
                    Event.current.Use();
                }
                if (Event.current.type == EventType.MouseUp)
                {
                    isResizing = false;
                    Event.current.Use();
                }
            }
        }

        private void TrySpawnEnemy()
        {
            LevelGenerator levelGenerator = UnityEngine.Object.FindObjectOfType<LevelGenerator>();
            if (levelGenerator == null)
            {
                Debug.Log("LevelGenerator instance not found!");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null)
            {
                Debug.Log("Local player not found!");
                return;
            }

            Vector3 spawnPosition = localPlayer.transform.position + Vector3.up * 1.5f;

            spawnCountText = System.Text.RegularExpressions.Regex.Replace(spawnCountText, "[^0-9]", "");
            if (spawnCountText.Length > 2)
                spawnCountText = spawnCountText.Substring(0, 2);

            int spawnCount = 1;
            if (!int.TryParse(spawnCountText, out spawnCount))
                spawnCount = 1;

            spawnCount = Mathf.Clamp(spawnCount, 1, 10);

            if (spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedFilteredEnemySetups.Count)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    EnemySpawner.SpawnSpecificEnemy(levelGenerator, cachedFilteredEnemySetups[spawnEnemyIndex], spawnPosition);
                }

                Debug.Log($"Spawned {spawnCount}x {cachedEnemySetupNames[spawnEnemyIndex]}");
            }
            else
            {
                Debug.Log("Invalid enemy selection.");
            }
        }

        private void DrawTeleportOptions()
        {
            int itemHeight = 25;
            int maxVisibleItems = 6;

            GUILayout.Space(10f);
            GUILayout.Label("Teleport Options", sectionHeaderStyle);

            GUILayout.BeginHorizontal();

            string currentSource = (teleportPlayerSourceIndex >= 0 && teleportPlayerSourceIndex < teleportPlayerSourceOptions.Length)
                ? teleportPlayerSourceOptions[teleportPlayerSourceIndex]
                : "No source available";

            if (GUILayout.Button(currentSource, buttonStyle))
            {
                showSourceDropdown = !showSourceDropdown;
            }

            GUILayout.Label("to", labelStyle);

            string currentDest = (teleportPlayerDestIndex >= 0 && teleportPlayerDestIndex < teleportPlayerDestOptions.Length)
                ? teleportPlayerDestOptions[teleportPlayerDestIndex]
                : "No destination available";

            if (GUILayout.Button(currentDest, buttonStyle))
            {
                showDestDropdown = !showDestDropdown;
            }

            GUILayout.EndHorizontal();

            if (showSourceDropdown)
            {
                float dropdownHeight = Mathf.Min(teleportPlayerSourceOptions.Length, maxVisibleItems) * itemHeight;
                sourceDropdownScrollPosition = GUILayout.BeginScrollView(sourceDropdownScrollPosition, GUILayout.Height(dropdownHeight));
                for (int i = 0; i < teleportPlayerSourceOptions.Length; i++)
                {
                    if (i != teleportPlayerSourceIndex && GUILayout.Button(teleportPlayerSourceOptions[i], buttonStyle))
                    {
                        teleportPlayerSourceIndex = i;
                        showSourceDropdown = false;
                    }
                }
                GUILayout.EndScrollView();
            }

            if (showDestDropdown)
            {
                float dropdownHeight = Mathf.Min(teleportPlayerDestOptions.Length, maxVisibleItems) * itemHeight;
                destDropdownScrollPosition = GUILayout.BeginScrollView(destDropdownScrollPosition, GUILayout.Height(dropdownHeight));
                for (int i = 0; i < teleportPlayerDestOptions.Length; i++)
                {
                    if (i != teleportPlayerDestIndex && GUILayout.Button(teleportPlayerDestOptions[i], buttonStyle))
                    {
                        teleportPlayerDestIndex = i;
                        showDestDropdown = false;
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Execute Teleport", buttonStyle))
            {
                Teleport.ExecuteTeleportWithSeparateOptions(
                    teleportPlayerSourceIndex,
                    teleportPlayerDestIndex,
                    teleportPlayerSourceOptions,
                    teleportPlayerDestOptions,
                    playerList
                );

                Debug.Log("Teleport executed successfully");

                showSourceDropdown = false;
                showDestDropdown = false;
            }
        }

        Texture2D MakeSolidBackground(UnityEngine.Color color)
        {
            Texture2D texture = new Texture2D(4, 4);
            UnityEngine.Color[] pixels = new UnityEngine.Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        public static Texture2D colorpicker(UnityEngine.Color color, float alpha = 1f)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new UnityEngine.Color(color.r, color.g, color.b, alpha));
            tex.Apply();
            return tex;
        }

        private UnityEngine.Color GetContrastColor(UnityEngine.Color backgroundColor)
        {
            float brightness = (0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b);
            return brightness < 0.5f ? UnityEngine.Color.white : UnityEngine.Color.black;
        }

    }
}
