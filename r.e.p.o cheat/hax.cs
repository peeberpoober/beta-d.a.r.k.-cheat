using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using HarmonyLib;
using SingularityGroup.HotReload;


namespace r.e.p.o_cheat
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
            // Custom style for the slider
            if (sliderStyle == null)
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
            Rect rect = NextControlRect(customX, customY);
            rect.height = 12f;
            return Mathf.Round(GUI.HorizontalSlider(rect, val, min, max, sliderStyle, thumbStyle));
        }
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

        private int selectedPlayerIndex = 0;
        private List<string> playerNames = new List<string>();
        private List<object> playerList = new List<object>();
        private int selectedEnemyIndex = 0;
        private List<string> enemyNames = new List<string>();
        private List<Enemy> enemyList = new List<Enemy>();
        private float oldSliderValue = 0.5f;
        private float oldSliderValueStrength = 0.5f;
        private float sliderValue = 0.5f;
        public static float sliderValueStrength = 0.5f;
        public static float offsetESp = 0.5f;
        private bool showMenu = true;
        public static bool godModeActive = false;
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

        private enum MenuCategory { Player, ESP, Combat, Misc, Enemies, Items, Hotkeys }
        private MenuCategory currentCategory = MenuCategory.Player;

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
        private float lastItemListUpdateTime = 0f;
        private const float itemListUpdateInterval = 2f;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private float menuX = 50f;
        private float menuY = 50f;
        private const float titleBarHeight = 30f;


        private bool showingActionSelector = false;
        private Vector2 actionSelectorScroll = Vector2.zero;
        private Vector2 hotkeyScrollPosition = Vector2.zero;

        private HotkeyManager hotkeyManager; // Reference to the HotkeyManager

        public bool showWatermark = true;

        private float actionSelectorX = 300f;
        private float actionSelectorY = 200f;
        private bool isDraggingActionSelector = false;
        private Vector2 dragOffsetActionSelector;
        private GUIStyle overlayDimStyle;
        private GUIStyle actionSelectorBoxStyle;
        private static bool cursorStateInitialized = false;
        private void UpdateTeleportOptions()
        {
            teleportPlayerSourceOptions = playerNames.ToArray(); // Create source array with players only

            List<string> destOptions = new List<string>(); // Create destination array with players + "Void"
            destOptions.AddRange(playerNames);       // Add all players (including local player)
            destOptions.Add("The Void");            // Add void as last option only for destinations
            teleportPlayerDestOptions = destOptions.ToArray();
            teleportPlayerSourceIndex = 0;  // Default to first player
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
        public void Start()
        {
            CursorController.Init();
            
            UpdateCursorState();
            hotkeyManager = HotkeyManager.Instance;
            hotkeyManager.Initialize();

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
                Health_Player.playerHealthInstance = FindObjectOfType(playerHealthType);
                DLog.Log(Health_Player.playerHealthInstance != null ? "playerHealthInstance is not null" : "playerHealthInstance null");
            }
            else DLog.Log("playerHealthType null");

            var playerMaxHealth = Type.GetType("ItemUpgradePlayerHealth, Assembly-CSharp");
            if (playerMaxHealth != null)
            {
                Health_Player.playerMaxHealthInstance = FindObjectOfType(playerMaxHealth);
                DLog.Log("playerMaxHealth is not null");
            }
            else DLog.Log("playerMaxHealth null");
        }

        public void Update()
        {
            Strength.UpdateStrength();

            if (RunManager.instance.levelCurrent != null && levelsToSearchItems.Contains(RunManager.instance.levelCurrent.name))
            {
                if (Time.time >= nextUpdateTime)
                {
                    DebugCheats.UpdateEnemyList();
                    nextUpdateTime = Time.time + updateInterval;
                }

                // Reduce item list updates from every frame to every 5 seconds
                if (Time.time - lastItemListUpdateTime > 5f)
                {
                    UpdateItemList();
                    itemList = ItemTeleport.GetItemList();
                    lastItemListUpdateTime = Time.time;
                }
            }

            if (oldSliderValue != sliderValue)
            {
                PlayerController.RemoveSpeed(sliderValue);
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

            // Prevent excessive logging by adding a cooldown
            if (Time.time - lastItemListUpdateTime > 10f)
            {
                DLog.Log($"Item list contains {itemList.Count} items.");
                lastItemListUpdateTime = Time.time;
            }

            if (Input.GetKeyDown(hotkeyManager.MenuToggleKey))
            {
                showMenu = !showMenu;
                CursorController.cheatMenuOpen = showMenu;
                CursorController.UpdateCursorState();
                DLog.Log("MENU " + showMenu);
                if (!showMenu) TryUnlockCamera();
                UpdateCursorState();
            }

            if (Input.GetKeyDown(hotkeyManager.ReloadKey)) Start();

            if (Input.GetKeyDown(hotkeyManager.UnloadKey))
            {
                showMenu = false;
                CursorController.cheatMenuOpen = showMenu;
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
            else
            {
                hotkeyManager.CheckAndExecuteHotkeys();
            }

            if (showMenu) TryLockCamera();

            if (NoclipController.noclipActive)
            {
                NoclipController.UpdateMovement();
            }

            if (MapTools.showMapTweaks)
            {
                if (MapTools.mapDisableHiddenOverlayCheckboxActive && !MapTools.mapDisableHiddenOverlayActive)
                {
                    MapTools.changeOverlayStatus(true);
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
                    field.SetValue(InputManager.instance, 0f);
                    DLog.Log("disableAimingTimer reset to 0 (menu closed).");
                }
                else DLog.LogError("Failed to find field disableAimingTimer.");
            }
            else DLog.LogWarning("InputManager.instance not found!");
        }

        private void UpdateCursorState()
        {
            Cursor.visible = showMenu;
            CursorController.cheatMenuOpen = showMenu;
            CursorController.UpdateCursorState();
            Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
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
            DLog.Log($"Item list updated: {itemList.Count} items found (including ValuableObject and PlayerDeathHead).");
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
            int health = Health_Player.GetPlayerHealth(player);
            if (health < 0) {
                DLog.Log($"Could not get health for {playerName}, assuming dead");
                return true; // If we can't get health, assume player is dead
            }

            return health > 0;

            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            Health_Player.KillSelectedPlayer(selectedPlayer, playerNames[selectedPlayerIndex]);
            DLog.Log("Attempt to kill the selected player completed.");

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

            // handle modal first
            if (showingActionSelector)
            {
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);

                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400);

                // trying to only block events that are outside the modal window
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    if (!modalRect.Contains(Event.current.mousePosition))
                    {
                        // blocking outer event
                        Event.current.Use();
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

                Rect menuRect = new Rect(menuX, menuY, 600, 730);
                Rect titleRect = new Rect(menuX, menuY, 600, titleBarHeight);

                GUI.Box(menuRect, "", menuStyle);
                UIHelper.Begin("D.A.R.K. Menu 1.1.2.2", menuX, menuY, 600, 800, 30, 30, 10);

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
                    menuY = Mathf.Clamp(newPosition.y, 0, Screen.height - 730);
                }

                float tabWidth = 75f;
                float tabHeight = 40f;
                float spacing = 5f;
                float totalWidth = 7 * tabWidth + 6 * spacing;
                float startX = menuX + (600 - totalWidth) / 2f;

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

                if (GUI.Button(new Rect(startX, menuY + 30, tabWidth, tabHeight), "Player", currentCategory == MenuCategory.Player ? selectedTabStyle : tabStyle))
                    currentCategory = MenuCategory.Player;
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
                GUI.Label(new Rect(menuX + 10, menuY + 75, 580, 20), $"Press {hotkeyManager.ReloadKey} to reload! Press {hotkeyManager.MenuToggleKey} to close! Press {hotkeyManager.UnloadKey} to unload!", instructionStyle);

                float currentY = menuY + 105; // Starting Y position
                float parentSpacing = 30f;    // Space between main parent options when children are hidden
                float childIndent = 20f;      // Indentation for child options
                float childSpacing = 30f;     // Space between child options

                switch (currentCategory)
                {
                    case MenuCategory.Player:
                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", menuX + 30, menuY + 95);
                        playerScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 150), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Heal Player", menuX + 30, menuY + 275))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Health_Player.HealPlayer(playerList[selectedPlayerIndex], 50, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} healed.");
                            }
                            else
                            {
                                DLog.Log("No valid player selected to heal!");
                            }
                        }
                        if (UIHelper.Button("Damage Player", menuX + 30, menuY + 315))
                        {
                            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
                            {
                                Health_Player.DamagePlayer(playerList[selectedPlayerIndex], 1, playerNames[selectedPlayerIndex]);
                                DLog.Log($"Player {playerNames[selectedPlayerIndex]} damaged.");
                            }
                            else
                            {
                                DLog.Log("No valid player selected to damage!");
                            }
                        }
                        bool newHealState = UIHelper.ButtonBool("Toggle Infinite Health", infiniteHealthActive, menuX + 30, menuY + 355);
                        if (newHealState != infiniteHealthActive) { infiniteHealthActive = newHealState; PlayerController.MaxHealth(); }
                        bool newStaminaState = UIHelper.ButtonBool("Toggle Infinite Stamina", stamineState, menuX + 30, menuY + 395);
                        if (newStaminaState != stamineState) { stamineState = newStaminaState; PlayerController.MaxStamina(); DLog.Log("God mode toggled: " + stamineState); }
                        bool newGodModeState = UIHelper.ButtonBool("Toggle God Mode", godModeActive, menuX + 30, menuY + 435);
                        if (newGodModeState != godModeActive) { PlayerController.GodMode(); godModeActive = newGodModeState; DLog.Log("God mode toggled: " + godModeActive); }

                        bool newNoclipActive = UIHelper.ButtonBool("Toggle Noclip", NoclipController.noclipActive, menuX + 30, menuY + 475);
                        if (newNoclipActive != NoclipController.noclipActive) { NoclipController.ToggleNoclip(); }

                        UIHelper.Label("Speed Value " + sliderValue, menuX + 30, menuY + 515);

                        oldSliderValue = sliderValue;
                        sliderValue = UIHelper.Slider(sliderValue, 1f, 30f, menuX + 30, menuY + 535);

                        UIHelper.Label("Strength Value: " + sliderValueStrength, menuX + 30, menuY + 555);
                        oldSliderValueStrength = sliderValueStrength;
                        sliderValueStrength = UIHelper.Slider(sliderValueStrength, 1f, 100f, menuX + 30, menuY + 575);

                        UIHelper.Label("Stamina Recharge Delay: " + Hax2.staminaRechargeDelay, menuX + 30, menuY + 605);
                        Hax2.staminaRechargeDelay = UIHelper.Slider(Hax2.staminaRechargeDelay, 0f, 10f, menuX + 30, menuY + 626);

                        UIHelper.Label("Stamina Recharge Rate: " + Hax2.staminaRechargeRate, menuX + 30, menuY + 645);
                        Hax2.staminaRechargeRate = UIHelper.Slider(Hax2.staminaRechargeRate, 1f, 20f, menuX + 30, menuY + 665);

                        if (Hax2.staminaRechargeDelay != oldStaminaRechargeDelay || Hax2.staminaRechargeRate != oldStaminaRechargeRate)
                        {
                            PlayerController.DecreaseStaminaRechargeDelay(Hax2.staminaRechargeDelay, Hax2.staminaRechargeRate);
                            DLog.Log($"Stamina recharge updated: Delay={Hax2.staminaRechargeDelay}x, Rate={Hax2.staminaRechargeRate}x");
                            oldStaminaRechargeDelay = Hax2.staminaRechargeDelay;
                            oldStaminaRechargeRate = Hax2.staminaRechargeRate;
                        }
                        break;

                    case MenuCategory.ESP:

                        float currentY = menuY + 105; // Starting Y position
                        float parentSpacing = 30f;    // Space between main parent options when children are hidden
                        float childIndent = 20f;      // Indentation for child options
                        float childSpacing = 30f;     // Space between child options

                        // Enemy ESP section
                        DebugCheats.drawEspBool = UIHelper.Checkbox("Enemy ESP", DebugCheats.drawEspBool, menuX + 30, currentY);
                        currentY += DebugCheats.drawEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawEspBool)
                        {
                            DebugCheats.showEnemyBox = UIHelper.Checkbox("Toggle Box", DebugCheats.showEnemyBox, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showEnemyNames = UIHelper.Checkbox("Toggle Names", DebugCheats.showEnemyNames, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showEnemyDistance = UIHelper.Checkbox("Toggle Distance", DebugCheats.showEnemyDistance, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showEnemyHP = UIHelper.Checkbox("Show Enemy HP", DebugCheats.showEnemyHP, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.drawChamsBool = UIHelper.Checkbox("Toggle Chams", DebugCheats.drawChamsBool, menuX + 50, currentY);
                            currentY += parentSpacing;
                        }
                        // Item ESP section
                        DebugCheats.drawItemEspBool = UIHelper.Checkbox("Item ESP", DebugCheats.drawItemEspBool, menuX + 30, currentY);
                        currentY += DebugCheats.drawItemEspBool ? childIndent : parentSpacing;
                        if (DebugCheats.drawItemEspBool)
                        {
                            DebugCheats.showItemNames = UIHelper.Checkbox("Show Item Names", DebugCheats.showItemNames, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showItemDistance = UIHelper.Checkbox("Show Item Distance", DebugCheats.showItemDistance, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showItemValue = UIHelper.Checkbox("Show Item Value", DebugCheats.showItemValue, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.draw3DItemEspBool = UIHelper.Checkbox("3D Item ESP", DebugCheats.draw3DItemEspBool, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showPlayerDeathHeads = UIHelper.Checkbox("Show Dead Player Heads", DebugCheats.showPlayerDeathHeads, menuX + 50, currentY);
                            currentY += childSpacing;

                            // Max Distance Slider
                            GUI.Label(new Rect(menuX + 50, currentY, 200, 20), $"Max Item Distance: {DebugCheats.maxItemEspDistance:F0}m");
                            currentY += 20;
                            DebugCheats.maxItemEspDistance = GUI.HorizontalSlider(new Rect(menuX + 50, currentY, 200, 20), DebugCheats.maxItemEspDistance, 0f, 1000f);
                            currentY += parentSpacing;
                        }
                        // Extraction ESP section
                        DebugCheats.drawExtractionPointEspBool = UIHelper.Checkbox("Extraction ESP", DebugCheats.drawExtractionPointEspBool, menuX + 30, currentY);
                        currentY += DebugCheats.drawExtractionPointEspBool ? childIndent : parentSpacing;

                        if (DebugCheats.drawExtractionPointEspBool)
                        {
                            DebugCheats.showExtractionNames = UIHelper.Checkbox("Show Extraction Names", DebugCheats.showExtractionNames, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showExtractionDistance = UIHelper.Checkbox("Show Extraction Distance", DebugCheats.showExtractionDistance, menuX + 50, currentY);
                            currentY += parentSpacing;
                        }
                        // Player ESP section
                        DebugCheats.drawPlayerEspBool = UIHelper.Checkbox("2D Player ESP", DebugCheats.drawPlayerEspBool, menuX + 30, currentY);
                        currentY += parentSpacing;

                        DebugCheats.draw3DPlayerEspBool = UIHelper.Checkbox("3D Player ESP", DebugCheats.draw3DPlayerEspBool, menuX + 30, currentY);
                        currentY += (DebugCheats.drawPlayerEspBool || DebugCheats.draw3DPlayerEspBool) ? childIndent : parentSpacing;

                        if (DebugCheats.drawPlayerEspBool || DebugCheats.draw3DPlayerEspBool)
                        {
                            DebugCheats.showPlayerNames = UIHelper.Checkbox("Show Player Names", DebugCheats.showPlayerNames, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showPlayerDistance = UIHelper.Checkbox("Show Player Distance", DebugCheats.showPlayerDistance, menuX + 50, currentY);
                            currentY += childSpacing;
                            DebugCheats.showPlayerHP = UIHelper.Checkbox("Show Player HP", DebugCheats.showPlayerHP, menuX + 50, currentY);
                            currentY += parentSpacing;
                        }
                        break;

                    case MenuCategory.Combat:
                        UpdatePlayerList();
                        UIHelper.Label("Select a player:", menuX + 30, menuY + 95);
                        playerScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), playerScrollPosition, new Rect(0, 0, 520, playerNames.Count * 35), false, true);
                        for (int i = 0; i < playerNames.Count; i++)
                        {
                            if (i == selectedPlayerIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), playerNames[i])) selectedPlayerIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();

                        if (UIHelper.Button("Revive", menuX + 30, menuY + 330)) { Health_Player.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Player revived: " + playerNames[selectedPlayerIndex]); }
                        if (UIHelper.Button("Kill Selected Player", menuX + 30, menuY + 370)) { Health_Player.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames); DLog.Log("Attempt to kill the selected player completed."); }
                        if (UIHelper.Button(showTeleportUI ? "Hide Teleport Options" : "Teleport Options", menuX + 30, menuY + 410)) // Teleport UI with dropdown system
                        {
                            showTeleportUI = !showTeleportUI; // Initialize teleport options when opening
                            if (showTeleportUI)
                            {
                                UpdateTeleportOptions();
                            }
                        }
                        float contentWidth = 450f; // Total width of dropdowns (200 + 50 + 200)
                        float centerX = menuX + (600f - contentWidth) / 2; // Center in the menu area

                        if (showTeleportUI)
                        {
                            UIHelper.Label("Teleport", centerX + contentWidth / 2 - 30, menuY + 450); // Center the "Teleport" label
                            float sourceDropdownHeight = teleportPlayerSourceOptions.Length * 25f; // Calculate dropdown heights and button positions
                            float destDropdownHeight = teleportPlayerDestOptions.Length * 25f;
                            float maxDropdownHeight = Math.Max(sourceDropdownHeight, destDropdownHeight);
                            float executeButtonY = menuY + 520f;

                            if (showSourceDropdown || showDestDropdown) // If either dropdown is open, move the execute button lower
                            {
                                executeButtonY = menuY + 520f + maxDropdownHeight;
                            }
                            if (GUI.Button(new Rect(centerX, menuY + 480, 200, 25), teleportPlayerSourceOptions[teleportPlayerSourceIndex]))
                            {
                                showSourceDropdown = !showSourceDropdown; // Toggle the source dropdown visibility
                                showDestDropdown = false; // Close other dropdown if open
                            }
                            UIHelper.Label("to", centerX + 210, menuY + 480); // "to" label in the middle
                            if (GUI.Button(new Rect(centerX + 250, menuY + 480, 200, 25), teleportPlayerDestOptions[teleportPlayerDestIndex])) // Display destination selection (right side)
                            {
                                showDestDropdown = !showDestDropdown; // Toggle the destination dropdown visibility
                                showSourceDropdown = false; // Close other dropdown if open
                            }
                            if (showSourceDropdown) // Source dropdown options (if open) - PLAYERS ONLY
                            {
                                for (int i = 0; i < teleportPlayerSourceOptions.Length; i++)
                                {
                                    if (GUI.Button(new Rect(centerX, menuY + 510 + (i * 25), 200, 25), teleportPlayerSourceOptions[i]))
                                    {
                                        teleportPlayerSourceIndex = i;
                                        showSourceDropdown = false;
                                    }
                                }
                            }
                            if (showDestDropdown) // Destination dropdown options (if open) - PLAYERS + VOID
                            {
                                for (int i = 0; i < teleportPlayerDestOptions.Length; i++)
                                {
                                    if (GUI.Button(new Rect(centerX + 250, menuY + 510 + (i * 25), 200, 25), teleportPlayerDestOptions[i]))
                                    {
                                        teleportPlayerDestIndex = i;
                                        showDestDropdown = false;
                                    }
                                }
                            }
                            if (UIHelper.Button("Execute Teleport", menuX + 30, executeButtonY)) // Keep the execute button at its original position
                            {
                                Teleport.ExecuteTeleportWithSeparateOptions( // Call the teleport functionality from the Teleport class with separate source and destination options
                                    teleportPlayerSourceIndex,
                                    teleportPlayerDestIndex,
                                    teleportPlayerSourceOptions,
                                    teleportPlayerDestOptions,
                                    playerList);

                                showSourceDropdown = false; // Hide the dropdowns after executing
                                showDestDropdown = false;
                            }
                        }
                        break;

                    case MenuCategory.Misc:
                        parentSpacing = 40f;
                        childIndent = 20f;

                        if (UIHelper.Button("Spawn Money", menuX + 30, currentY))
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
                            ItemSpawner.SpawnItem(targetPosition);
                            DLog.Log("Money spawned.");
                        }
                        currentY += parentSpacing;

                        bool newPlayerColorState = UIHelper.ButtonBool("RGB Player", playerColor.isRandomizing, menuX + 30, currentY);
                        if (newPlayerColorState != playerColor.isRandomizing)
                        {
                            playerColor.isRandomizing = newPlayerColorState;
                            DLog.Log("Randomize toggled: " + playerColor.isRandomizing);
                        }
                        currentY += parentSpacing;

                        UIHelper.Label("Flashlight Intensity: " + Hax2.flashlightIntensity, menuX + 30, menuY + 185);
                        Hax2.flashlightIntensity = UIHelper.Slider(Hax2.flashlightIntensity, 1f, 100f, menuX + 30, menuY + 205);

                        UIHelper.Label("Crouch Delay: " + Hax2.crouchDelay, menuX + 30, menuY + 225);
                        Hax2.crouchDelay = UIHelper.Slider(Hax2.crouchDelay, 0f, 5f, menuX + 30, menuY + 245);

                        UIHelper.Label("Set Crouch Speed: " + Hax2.crouchSpeed, menuX + 30, menuY + 265);
                        Hax2.crouchSpeed = UIHelper.Slider(Hax2.crouchSpeed, 1f, 50f, menuX + 30, menuY + 285);

                        UIHelper.Label("Set Jump Force: " + Hax2.jumpForce, menuX + 30, menuY + 305);
                        Hax2.jumpForce = UIHelper.Slider(Hax2.jumpForce, 1f, 50f, menuX + 30, menuY + 326);

                        UIHelper.Label("Set Extra Jumps: " + Hax2.extraJumps, menuX + 30, menuY + 345);
                        Hax2.extraJumps = (int)UIHelper.Slider(Hax2.extraJumps, 1f, 100f, menuX + 30, menuY + 365);

                        UIHelper.Label("Set Custom Gravity: " + Hax2.customGravity, menuX + 30, menuY + 385);
                        Hax2.customGravity = UIHelper.Slider(Hax2.customGravity, -10f, 50f, menuX + 30, menuY + 405);

                        bool newNoFogState = UIHelper.ButtonBool("Toggle No Fog", MiscFeatures.NoFogEnabled, menuX + 30, currentY);
                        if (newNoFogState != MiscFeatures.NoFogEnabled)
                        {
                            MiscFeatures.ToggleNoFog(newNoFogState);
                        }
                        currentY += parentSpacing;

                        MapTools.showMapTweaks = UIHelper.Checkbox("Map tweaks", MapTools.showMapTweaks, menuX + 30, currentY);
                        currentY += MapTools.showMapTweaks ? childIndent : parentSpacing;

                        if (MapTools.showMapTweaks)
                        {
                            MapTools.mapDisableHiddenOverlayCheckboxActive = UIHelper.Checkbox("Disable '?' overlay(could not be undone)", MapTools.mapDisableHiddenOverlayCheckboxActive, menuX + 50, currentY);
                            currentY += childSpacing;
                        }

                        UIHelper.Label("Flashlight Intensity: " + Hax2.flashlightIntensity, menuX + 31, currentY);
                        currentY += childIndent;
                        Hax2.flashlightIntensity = UIHelper.Slider(Hax2.flashlightIntensity, 1f, 100f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Crouch Delay: " + Hax2.crouchDelay, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.crouchDelay = UIHelper.Slider(Hax2.crouchDelay, 0f, 5f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Crouch Speed: " + Hax2.crouchSpeed, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.crouchSpeed = UIHelper.Slider(Hax2.crouchSpeed, 1f, 50f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Jump Force: " + Hax2.jumpForce, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.jumpForce = UIHelper.Slider(Hax2.jumpForce, 1f, 50f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Extra Jumps: " + Hax2.extraJumps, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.extraJumps = (int)UIHelper.Slider(Hax2.extraJumps, 1f, 100f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Custom Gravity: " + Hax2.customGravity, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.customGravity = UIHelper.Slider(Hax2.customGravity, -10f, 50f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Grab Range: " + Hax2.grabRange, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.grabRange = UIHelper.Slider(Hax2.grabRange, 0f, 50f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Throw Strength: " + Hax2.throwStrength, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.throwStrength = UIHelper.Slider(Hax2.throwStrength, 0f, 50f, menuX + 30, currentY);
                        currentY += childIndent;

                        UIHelper.Label("Set Slide Decay: " + Hax2.slideDecay, menuX + 30, currentY);
                        currentY += childIndent;
                        Hax2.slideDecay = UIHelper.Slider(Hax2.slideDecay, -10f, 50f, menuX + 30, currentY);
                        currentY += parentSpacing;

                        if (Hax2.flashlightIntensity != OldflashlightIntensity)
                        {
                            PlayerController.SetFlashlightIntensity(Hax2.flashlightIntensity);
                            OldflashlightIntensity = Hax2.flashlightIntensity;
                        }
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


                        bool newNoFogState = UIHelper.ButtonBool("Toggle No Fog", MiscFeatures.NoFogEnabled, menuX + 30, menuY + 565);
                        if (newNoFogState != MiscFeatures.NoFogEnabled)
                        {
                            MiscFeatures.ToggleNoFog(newNoFogState);
                        }

                        bool newUnlimitedBatteryState = UIHelper.ButtonBool("Toggle Unlimited Battery", unlimitedBatteryActive, menuX + 30, menuY + 605);
                        if (newUnlimitedBatteryState != unlimitedBatteryActive)
                        {
                            unlimitedBatteryActive = newUnlimitedBatteryState;
                            if (unlimitedBatteryComponent != null)
                                unlimitedBatteryComponent.unlimitedBatteryEnabled = unlimitedBatteryActive;
                        }

                        bool newWatermarkState = UIHelper.ButtonBool("Disable Watermark", !showWatermark, menuX + 30, menuY + 645);
                        if (newWatermarkState != !showWatermark)
                        {
                            showWatermark = !newWatermarkState;
                        }

                        break;

                    case MenuCategory.Enemies:
                        UpdateEnemyList();
                        UIHelper.Label("Select an enemy:", menuX + 30, menuY + 95);
                        enemyScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), enemyScrollPosition, new Rect(0, 0, 520, enemyNames.Count * 35), false, true);
                        for (int i = 0; i < enemyNames.Count; i++)
                        {
                            if (i == selectedEnemyIndex) GUI.color = Color.white;
                            else GUI.color = Color.gray;
                            if (GUI.Button(new Rect(0, i * 35, 520, 30), enemyNames[i])) selectedEnemyIndex = i;
                            GUI.color = Color.white;
                        }
                        GUI.EndScrollView();
                        if (UIHelper.Button("Kill Selected Enemy", menuX + 30, menuY + 330))
                        {
                            Enemies.KillSelectedEnemy(selectedEnemyIndex, enemyList, enemyNames);
                            DLog.Log($"Attempt to kill the selected enemy completed: {enemyNames[selectedEnemyIndex]}");
                        }
                        if (UIHelper.Button("Kill All Enemies", menuX + 30, menuY + 370))
                        {
                            Enemies.KillAllEnemies();
                            DLog.Log("Attempt to kill all enemies completed.");
                        }
                        if (UIHelper.Button(showEnemyTeleportUI ? "Hide Teleport Options" : "Teleport Options", menuX + 30, menuY + 410))
                        {
                            showEnemyTeleportUI = !showEnemyTeleportUI; // Toggle the teleport UI
                            if (showEnemyTeleportUI)
                            {
                                UpdateEnemyTeleportOptions(); // Initialize teleport options when opening
                            }
                        }
                        if (showEnemyTeleportUI)
                        {
                            float dropdownHeight = enemyTeleportDestOptions.Length * 25f;
                            float executeButtonY = menuY + 480f;
                            if (showEnemyTeleportDropdown)
                            {
                                executeButtonY = menuY + 480f + dropdownHeight; // If dropdown is open, move the execute button lower
                            }
                            UIHelper.Label("Teleport", enemyTeleportStartX, menuY + 450); // "Teleport" label
                            UIHelper.Label("to", enemyTeleportStartX + enemyTeleportLabelWidth + 10f, menuY + 450); // "to" label
                            if (GUI.Button(new Rect(enemyTeleportStartX + enemyTeleportLabelWidth + 10f + enemyTeleportToWidth + 10f,
                                                  menuY + 450, enemyTeleportDropdownWidth, 25),
                                         enemyTeleportDestOptions[enemyTeleportDestIndex]))
                            {
                                showEnemyTeleportDropdown = !showEnemyTeleportDropdown; // Toggle the destination dropdown visibility
                            }
                            if (showEnemyTeleportDropdown) // Destination dropdown options (if open)
                            {
                                for (int i = 0; i < enemyTeleportDestOptions.Length; i++)
                                {
                                    if (GUI.Button(new Rect(enemyTeleportStartX + enemyTeleportLabelWidth + 10f + enemyTeleportToWidth + 10f,
                                                          menuY + 480 + (i * 25), enemyTeleportDropdownWidth, 25),
                                                 enemyTeleportDestOptions[i]))
                                    {
                                        enemyTeleportDestIndex = i;
                                        showEnemyTeleportDropdown = false;
                                    }
                                }
                            }
                            if (UIHelper.Button("Execute Teleport", menuX + 30, executeButtonY)) // Execute teleport button (at original position)
                            {
                                int playerIndex = enemyTeleportDestIndex;
                                if (DebugCheats.IsLocalPlayer(playerList[playerIndex])) // Check if selected player is local player
                                {
                                    Enemies.TeleportEnemyToMe(selectedEnemyIndex, enemyList, enemyNames); // Use existing method for local player
                                }
                                else
                                {
                                    Enemies.TeleportEnemyToPlayer(selectedEnemyIndex, enemyList, enemyNames, playerIndex, playerList, playerNames); // Teleport to another player
                                }
                                UpdateEnemyList();
                                showEnemyTeleportDropdown = false;
                                DLog.Log($"Teleported {enemyNames[selectedEnemyIndex]} to {enemyTeleportDestOptions[enemyTeleportDestIndex]}.");
                            }
                        }
                        break;

                    case MenuCategory.Items:
                        UIHelper.Label("Select an item:", menuX + 30, menuY + 95);

                        itemScrollPosition = GUI.BeginScrollView(new Rect(menuX + 30, menuY + 115, 540, 200), itemScrollPosition, new Rect(0, 0, 520, itemList.Count * 35), false, true);
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

                        if (UIHelper.Button("Teleport Item to Me", menuX + 30, menuY + 330))
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
                        if (UIHelper.Button("Teleport All Items to Me", menuX + 30, menuY + 370))
                        {
                            ItemTeleport.TeleportAllItemsToMe();
                            DLog.Log("Teleporting all items initiated.");
                        }
                        if (UIHelper.Button("Change Item Value to 10K", menuX + 30, menuY + 410))
                        {
                            if (selectedItemIndex >= 0 && selectedItemIndex < itemList.Count)
                            {
                                ItemTeleport.SetItemValue(itemList[selectedItemIndex], 10000);
                                DLog.Log($"Updated value: {itemList[selectedItemIndex].Value}");
                            }
                            else
                            {
                                DLog.Log("No valid item selected to change value!");
                            }
                        }
                        break;

                    case MenuCategory.Hotkeys:
                        Rect viewRect = new Rect(menuX + 20, menuY + 95, 560, 620);
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

                            GUI.Label(new Rect(menuX + 20, menuY + 95, 560, 25), hotkeyManager.KeyAssignmentError, errorStyle);

                            viewRect.y += 30;
                            viewRect.height -= 30;
                        }

                        hotkeyScrollPosition = GUI.BeginScrollView(viewRect, hotkeyScrollPosition, contentRect);

                        float yPos = 10;

                        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 16,
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = Color.white }
                        };

                        GUI.Label(new Rect(50, yPos, 540, 25), "Hotkey Configuration", headerStyle);
                        yPos += 30;

                        GUI.Label(new Rect(50, yPos, 540, 20), "How to set up a hotkey:", instructionStyle);
                        yPos += 20;
                        GUI.Label(new Rect(70, yPos, 540, 20), "1. Click on a key field → press desired key", instructionStyle);
                        yPos += 20;
                        GUI.Label(new Rect(70, yPos, 540, 20), "2. Click on action field → select function", instructionStyle);
                        yPos += 25;

                        GUIStyle warningStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontSize = 12,
                            normal = { textColor = Color.yellow }
                        };
                        GUI.Label(new Rect(50, yPos, 540, 20), "Warning: Ensure each key is only assigned to one action", warningStyle);
                        yPos += 30;

                        GUI.Label(new Rect(10, yPos, 540, 25), "System Keys", headerStyle);
                        yPos += 30;

                        string menuToggleKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 0 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.MenuToggleKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Menu Toggle:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), menuToggleKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(0);
                        }
                        yPos += 40;

                        string reloadKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 1 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.ReloadKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Reload:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), reloadKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(1);
                        }
                        yPos += 40;

                        string unloadKeyText = (hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == 2 && hotkeyManager.WaitingForAnyKey)
                            ? "Press any key..." : hotkeyManager.UnloadKey.ToString();
                        GUI.Label(new Rect(10, yPos, 150, 30), "Unload:");
                        if (GUI.Button(new Rect(170, yPos, 290, 30), unloadKeyText))
                        {
                            hotkeyManager.StartConfigureSystemKey(2);
                        }
                        yPos += 50;

                        GUI.Label(new Rect(10, yPos, 540, 25), "Action Hotkeys", headerStyle);
                        yPos += 30;

                        for (int i = 0; i < 12; i++)
                        {
                            KeyCode currentKey = hotkeyManager.GetHotkeyForSlot(i);
                            string keyText = (currentKey == KeyCode.None) ? "Not Set" : currentKey.ToString();
                            string actionName = hotkeyManager.GetActionNameForKey(currentKey);

                            Rect slotRect = new Rect(10, yPos, 150, 30);
                            bool isSelected = hotkeyManager.SelectedHotkeySlot == i && hotkeyManager.ConfiguringHotkey;

                            if (GUI.Button(slotRect, isSelected ? "Press any key..." : keyText))
                            {
                                hotkeyManager.StartHotkeyConfiguration(i);
                            }

                            Rect actionRect = new Rect(170, yPos, 290, 30);
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

                            Rect clearRect = new Rect(470, yPos, 60, 30);
                            if (GUI.Button(clearRect, "Clear") && currentKey != KeyCode.None)
                            {
                                hotkeyManager.ClearHotkeyBinding(i);
                            }

                            yPos += 40;
                        }

                        if (GUI.Button(new Rect(10, yPos, 540, 30), "Save Hotkey Settings"))
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
                // draw full-screen overlay and consume mouse events so main GUI is blocked
                Rect fullOverlay = new Rect(0, 0, Screen.width, Screen.height);
                GUI.Box(fullOverlay, "", overlayDimStyle);
                if (Event.current.type == EventType.MouseDown ||
                    Event.current.type == EventType.MouseUp ||
                    Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use(); // consume events
                }

                // draw modal window using GUI.Window for natural dragging
                Rect modalRect = new Rect(actionSelectorX, actionSelectorY, 400, 400);
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
