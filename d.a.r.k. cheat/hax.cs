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
        private float nextUpdateTime = 0f;
        private const float updateInterval = 10f;

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
        private float oldSliderValue = 0.5f;
        private float oldSliderValueStrength = 0.5f;
        private float sliderValue = 0.5f;
        public static float sliderValueStrength = 0.5f;
        public static float offsetESp = 0.5f;
        public static bool showMenu = true;
        public static bool godModeActive = false;
        public static bool debounce = false;
        public static bool infiniteHealthActive = false;
        public static bool stamineState = false;
        public static bool unlimitedBatteryActive = false;
        public static UnlimitedBattery unlimitedBatteryComponent;
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

        public static string[] levelsToSearchItems = { "Level - Manor", "Level - Wizard", "Level - Arctic" };

        private GUIStyle menuStyle;
        private bool initialized = false;
        private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();

        private enum MenuCategory { Self, ESP, Combat, Misc, Enemies, Items, Hotkeys }
        private MenuCategory currentCategory = MenuCategory.Self;

        public static float staminaRechargeDelay = 1f;
        public static float staminaRechargeRate = 1f;
        public static float oldStaminaRechargeDelay = 1f;
        public static float oldStaminaRechargeRate = 1f;

        public static float jumpForce = 1f;
        public static float customGravity = 1f;
        public static int extraJumps = 1;
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
        private int previousItemCount = 0;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private float menuX = 50f;
        private float menuY = 50f;
        private const float titleBarHeight = 30f;

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
            UpdatePlayerList(); // Update all player and enemy lists
            UpdateEnemyList();
            if (showTeleportUI) // Update teleport options if UIs are open
            {
                UpdateTeleportOptions();
            }
            if (showEnemyTeleportUI)
            {
                UpdateEnemyTeleportOptions();
            }
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

                if (oldSliderValue != sliderValue)
                {
                    PlayerController.SetSprintSpeed(sliderValue);
                    oldSliderValue = sliderValue;
                }
                if (oldSliderValueStrength != sliderValueStrength)
                {
                    Strength.MaxStrength();
                    oldSliderValueStrength = sliderValueStrength;
                }
                if (playerColor.isRandomizing)
                {
                    playerColor.colorRandomizer();
                }

                hotkeyManager.CheckAndExecuteHotkeys(); // Execute hotkeys only when in game

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
                GUI.Label(new Rect(menuX + 25, menuY + 75, 580, 20), $"Open/Close: {hotkeyManager.MenuToggleKey} | Reload: {hotkeyManager.ReloadKey} | Unload: {hotkeyManager.UnloadKey}", instructionStyle);

                float yPos = 10;
                float parentSpacing = 40;    // Space between main parent options when children are hidden
                float childSpacing = 30;     // Space between child options
                float childIndent = 20;      // Indentation for child options
                float scrollListSpacing = 215; // Space between list and next parent option

                switch (currentCategory)
                {
                    case MenuCategory.Self:
                        Rect selfViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        Rect selfContentRect = new Rect(0, 0, 540, 1200);
                        selfScrollPosition = GUI.BeginScrollView(selfViewRect, selfScrollPosition, selfContentRect);

                        float selfYPos = yPos;

                        bool newGodModeState = UIHelper.ButtonBool("God Mode", godModeActive, 0, selfYPos);
                        if (newGodModeState != godModeActive) { PlayerController.GodMode(); godModeActive = newGodModeState; DLog.Log("God mode toggled: " + godModeActive); }
                        selfYPos += parentSpacing;

                        bool newTumbleGuardActive = UIHelper.ButtonBool("Tumble Guard", Hax2.debounce, 0, selfYPos);
                        if (newTumbleGuardActive != Hax2.debounce) { PlayerTumblePatch.ToggleTumbleGuard(); }
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

                        UIHelper.Label("Speed: " + sliderValue, 0, selfYPos);
                        selfYPos += childIndent;
                        oldSliderValue = sliderValue;
                        sliderValue = UIHelper.Slider(sliderValue, 1f, 30f, 0, selfYPos);
                        selfYPos += childIndent;

                        UIHelper.Label("Range: " + Hax2.grabRange, 0, selfYPos);
                        selfYPos += childIndent;
                        Hax2.grabRange = UIHelper.Slider(Hax2.grabRange, 0f, 50f, 0, selfYPos);
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

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.ESP:
                        Rect espViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        Rect espContentRect = new Rect(0, 0, 540, 1200);
                        espScrollPosition = GUI.BeginScrollView(espViewRect, espScrollPosition, espContentRect);

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

                            DebugCheats.drawItemChamsBool = UIHelper.Checkbox("Item Chams", DebugCheats.drawItemChamsBool, 20, espYPos);
                            espYPos += childSpacing;
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
                        Rect combatContentRect = new Rect(0, 0, 540, 1200);
                        combatScrollPosition = GUI.BeginScrollView(combatViewRect, combatScrollPosition, combatContentRect);

                        float combatYPos = yPos;

                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", 0, combatYPos);
                        combatYPos += childIndent;

                        playerScrollPosition = GUI.BeginScrollView(new Rect(0, combatYPos, 540, 200), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        combatYPos += scrollListSpacing;

                        if (UIHelper.Button("-2 Damage", 0, combatYPos))
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

                        if (UIHelper.Button("Kill", 0, combatYPos)) { Players.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Player killed: " + playerNames[selectedPlayerIndex]); }
                        combatYPos += parentSpacing;

                        if (UIHelper.Button("Revive", 0, combatYPos)) { Players.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Player revived: " + playerNames[selectedPlayerIndex]); }
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
                        Rect miscViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        Rect miscContentRect = new Rect(0, 0, 540, 1200);
                        miscScrollPosition = GUI.BeginScrollView(miscViewRect, miscScrollPosition, miscContentRect);

                        float miscYPos = yPos;

                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", 0, miscYPos);
                        miscYPos += childIndent;

                        playerScrollPosition = GUI.BeginScrollView(new Rect(0, miscYPos, 540, 200), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        miscYPos += scrollListSpacing;

                        if (UIHelper.Button("Force Mute", 0, miscYPos))
                        {
                            MiscFeatures.ForceMutePlayer();
                        }
                        miscYPos += parentSpacing;

                        if (UIHelper.Button("Force High Volume", 0, miscYPos))
                        {
                            MiscFeatures.ForcePlayerMicVolumeHigh(9999);
                        }
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

                        if (UIHelper.Button("[HOST] Spawn Money", 0, miscYPos))
                        {
                            DLog.Log("'Spawn Money' button clicked!");
                            GameObject localPlayer = DebugCheats.GetLocalPlayer();
                            if (localPlayer == null)
                            {
                                DLog.Log("Local player not found!");
                                return;
                            }
                            Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
                            transform.position = targetPosition;
                            ItemSpawner.SpawnMoney(targetPosition);
                            DLog.Log("Money spawned.");
                        }
                        miscYPos += parentSpacing;

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

                        MapTools.showMapTweaks = UIHelper.Checkbox("Map Tweaks", MapTools.showMapTweaks, 0, miscYPos);
                        miscYPos += parentSpacing;

                        if (MapTools.showMapTweaks) // Only show the buttons if the checkbox is selected
                        {
                            if (UIHelper.Button("Disable '?' overlay(could not be undone on level)", 0, miscYPos))
                            {
                                MapTools.changeOverlayStatus(true);
                            }
                            miscYPos += parentSpacing;

                            if (UIHelper.Button("Discover Map Valuables", 0, miscYPos))
                            {
                                MapTools.DiscoveryMapValuables();
                            }
                            miscYPos += parentSpacing;
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Enemies:
                        Rect enemyViewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        Rect enemyContentRect = new Rect(0, 0, 540, 1200);
                        enemiesScrollPosition = GUI.BeginScrollView(enemyViewRect, enemiesScrollPosition, enemyContentRect);

                        float enemyYPos = yPos;

                        UpdateEnemyList();
                        UIHelper.Label("Select an enemy:", 0, enemyYPos);
                        enemyYPos += childIndent;

                        enemyScrollPosition = GUI.BeginScrollView(new Rect(0, enemyYPos, 540, 200), enemyScrollPosition, new Rect(0, 0, 520, enemyNames.Count * 35), false, true);
                        for (int i = 0; i < enemyNames.Count; i++)
                        {
                            if (i == selectedEnemyIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), enemyNames[i])) selectedEnemyIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        enemyYPos += scrollListSpacing;

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
                                        if (GUI.Button(new Rect(0, displayedIndex * itemHeight, dropdownWidth - 20, itemHeight), enemyTeleportDestOptions[i]))
                                        {
                                            enemyTeleportDestIndex = i;
                                            showEnemyTeleportDropdown = false;
                                        }
                                        displayedIndex++;
                                    }
                                }
                                GUI.EndScrollView();
                            }

                            float executeButtonYPos = enemyYPos + 10;
                            float dropdownOffset = 0;

                            if (showEnemyTeleportDropdown && enemyTeleportDestOptions.Length > 0) dropdownOffset = Math.Min(enemyTeleportDestOptions.Length, 6) * 25;
                            if (GUI.Button(new Rect(tpCenterX - 75f, executeButtonYPos, 150f, 25f), "Execute Teleport"))
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
                                showEnemyTeleportDropdown = false;
                            }
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Items:
                        Rect itemsViewRect = new Rect(menuX + 20, menuY + 95, 560, 700);
                        Rect itemsContentRect = new Rect(0, 0, 540, 1200);
                        itemsScrollPosition = GUI.BeginScrollView(itemsViewRect, itemsScrollPosition, itemsContentRect);

                        float itemYPos = yPos;

                        UIHelper.Label("Select an item:", 0, itemYPos);
                        itemYPos += childIndent;
                        itemScrollPosition = GUI.BeginScrollView(new Rect(0, itemYPos, 540, 200), itemScrollPosition, new Rect(0, 0, 520, itemList.Count * 35), false, true);
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            if (i == selectedItemIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), $"{itemList[i].Name} [Value: {itemList[i].Value}$]"))
                            {
                                selectedItemIndex = i;
                            }
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        itemYPos += 215;

                        if (UIHelper.Button("Teleport Item to Me [HOST]", 0, itemYPos))
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

                        if (UIHelper.Button("Teleport All Items to Me [HOST]", 0, itemYPos))
                        {
                            ItemTeleport.TeleportAllItemsToMe();
                            DLog.Log("Teleporting all items initiated.");
                        }
                        itemYPos += parentSpacing;

                        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                        labelStyle.normal.textColor = Color.white;

                        GUI.Label(new Rect(0, itemYPos, 540, 20), "Change Item Value [HOST]:", GUI.skin.label);
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

                        if (GUI.Button(new Rect(0, itemYPos, 540, 30), "Apply Value Change"))
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

                            itemSpawnerScrollPosition = GUI.BeginScrollView(
                                new Rect(0, itemYPos, 540, 150),
                                itemSpawnerScrollPosition,
                                new Rect(0, 0, 520, availableItemsList.Count * 30),
                                false, true);

                            for (int i = 0; i < availableItemsList.Count; i++)
                            {
                                if (i == selectedItemToSpawnIndex) GUI.color = Color.white;
                                else GUI.color = Color.gray;

                                if (GUI.Button(new Rect(0, i * 30, 520, 30), availableItemsList[i]))
                                {
                                    selectedItemToSpawnIndex = i;
                                }

                                GUI.color = Color.white;
                            }
                            GUI.EndScrollView();
                            itemYPos += 160;

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
                        }

                        GUI.EndScrollView();
                        break;

                    case MenuCategory.Hotkeys:
                        Rect viewRect = new Rect(menuX + 30, menuY + 95, 560, 700);
                        Rect contentRect = new Rect(0, 0, 540, 1200);

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

                        hotkeyScrollPosition = GUI.BeginScrollView(viewRect, hotkeyScrollPosition, contentRect);

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
                solidTextures[color] = texture;
            }
            return solidTextures[color];
        }

    }
}
