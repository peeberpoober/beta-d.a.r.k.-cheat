using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // Required for ReceiverGroup and RaiseEventOptions
using ExitGames.Client.Photon; // Required for SendOptions
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
namespace dark_cheat
{
    static class DebugCheats
    {
        private static bool _drawItemChamsBool = false;
        public static bool drawItemChamsBool
        {
            get => _drawItemChamsBool;
            set
            {
                if (value != _drawItemChamsBool)
                {
                    _drawItemChamsBool = value;

                    if (!value)
                    {
                        foreach (var kvp in itemOriginalMaterials)
                        {
                            if (kvp.Key != null)
                                kvp.Key.materials = kvp.Value;
                        }
                        itemOriginalMaterials.Clear();
                    }
                }
            }
        }
        public static int minItemValue = 0;
        public static int maxItemValue = 50000;
        public static float maxItemEspDistance = 1000f;
        public static bool showEnemyBox = true; // Default to true since it was previously always on
        private static int frameCounter = 0;
        public static List<Enemy> enemyList = new List<Enemy>();
        public static List<object> valuableObjects = new List<object>();
        private static List<object> playerList = new List<object>();
        private static float scaleX, scaleY;
        public static Texture2D texture2;
        private static float lastUpdateTime = 0f;
        private const float updateInterval = 5f;
        private static GameObject localPlayer;
        
        private static int lastPlayerDataCount = 0;
        private static int lastEnemyCount = 0;
        private static int lastItemCount = 0;
        private static int lastPlayerCount = 0;
        private static int lastExtractionPointCount = 0;
        private static string lastLocalPlayerName = "";
        private static Vector3 lastExtractionPosition = Vector3.zero;
        private static List<ExtractionPointData> extractionPointList = new List<ExtractionPointData>();

        public static bool drawEspBool = false;
        public static bool drawItemEspBool = false;
        public static bool draw3DItemEspBool = false;
        public static bool drawPlayerEspBool = false;
        public static bool draw2DPlayerEspBool = false;
        public static bool draw3DPlayerEspBool = true;
        public static bool drawExtractionPointEspBool = false;

        public static GUIStyle nameStyle;
        public static GUIStyle valueStyle;
        public static GUIStyle enemyStyle;
        public static GUIStyle healthStyle;
        public static GUIStyle distanceStyle;

        public static bool showEnemyNames = true;
        public static bool showEnemyDistance = true;
        public static bool showEnemyHP = true;
        public static bool showItemNames = true;
        public static bool showItemValue = true;
        public static bool showItemDistance = false;
        public static bool showPlayerDeathHeads = true;
        public static bool showExtractionNames = true;
        public static bool showExtractionDistance = true;
        public static bool showPlayerNames = true;
        public static bool showPlayerDistance = true;
        public static bool showPlayerHP = true;
        private static Camera cachedCamera;
        private static Material visibleMaterial;
        private static Material hiddenMaterial;
        private static Material itemVisibleMaterial;
        private static Material itemHiddenMaterial;
        private static Dictionary<Renderer, Material[]> enemyOriginalMaterials = new Dictionary<Renderer, Material[]>();

        public static Color enemyVisibleColor = new Color(0f, 0.5f, 0.1f, 1f); // Default: Green
        public static Color enemyHiddenColor = new Color(0.4f, 0.04f, 0.2f, 0.5f); // Default: Pink
        public static Color itemVisibleColor = new Color(0.6f, 0.6f, 0f, 0.85f); // Default: Yellow
        public static Color itemHiddenColor = new Color(0.6f, 0.3f, 0f, 0.4f); // Default: Orange
        private static Dictionary<Renderer, Material[]> itemOriginalMaterials = new Dictionary<Renderer, Material[]>();
        private static bool cachedOriginalCamera = false;
        private static float originalFarClipPlane = 0f;
        private static DepthTextureMode originalDepthTextureMode = DepthTextureMode.None;
        private static bool originalOcclusionCulling = false;

        private static List<PlayerData> playerDataList = new List<PlayerData>();
        private static float playerUpdateInterval = 1f;
        private static Dictionary<int, int> playerHealthCache = new Dictionary<int, int>();
        private static float lastPlayerUpdateTime = 0f;
        public static Dictionary<Enemy, int> enemyHealthCache = new Dictionary<Enemy, int>();
        private const float maxEspDistance = 100f;

        private static FieldInfo _levelAnimationStartedField =
            typeof(LoadingUI).GetField("levelAnimationStarted", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool _drawChamsBool = false;
        public static bool drawChamsBool
        {
            get => _drawChamsBool;
            set
            {
                if (value != _drawChamsBool)
                {
                    _drawChamsBool = value;

                    if (!value)
                    {
                        foreach (var kvp in enemyOriginalMaterials)
                        {
                            if (kvp.Key != null)
                                kvp.Key.materials = kvp.Value;
                        }
                        enemyOriginalMaterials.Clear();

                        if (cachedOriginalCamera)
                        {
                            cachedCamera.farClipPlane = originalFarClipPlane;
                            cachedCamera.depthTextureMode = originalDepthTextureMode;
                            cachedCamera.useOcclusionCulling = originalOcclusionCulling;
                        }
                    }
                }
            }
        }

        public class PlayerData
        {
            public object PlayerObject { get; }
            public PhotonView PhotonView { get; }
            public Transform Transform { get; }
            public bool IsAlive { get; set; }
            public string Name { get; set; }

            public PlayerData(object player)
            {
                PlayerObject = player;
                PhotonView = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(player) as PhotonView;
                Transform = player.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(player) as Transform;
                Name = (player as PlayerAvatar) != null ? (SemiFunc.PlayerGetName(player as PlayerAvatar) ?? "Unknown Player") : "Unknown Player";
                IsAlive = true;
            }
        }
        static DebugCheats()
        {
            cachedCamera = Camera.main;
            if (cachedCamera != null)
            {
                scaleX = (float)Screen.width / cachedCamera.pixelWidth;
                scaleY = (float)Screen.height / cachedCamera.pixelHeight;
            }
            UpdateLists();
            UpdateLocalPlayer();
            UpdateExtractionPointList();
            UpdatePlayerDataList();
        }
        private static void UpdatePlayerDataList()
        {
            playerDataList.Clear();
            playerHealthCache.Clear();
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        var data = new PlayerData(player);
                        if (data.PhotonView != null && data.Transform != null)
                        {
                            playerDataList.Add(data);
                            int health = Players.GetPlayerHealth(player);
                            playerHealthCache[data.PhotonView.ViewID] = health;
                        }
                    }
                }
            }
            
            // Only log if the count has changed
            if (playerDataList.Count != lastPlayerDataCount) {
                DLog.Log($"Player data list updated: {playerDataList.Count} players.");
                lastPlayerDataCount = playerDataList.Count;
            }
        }
        private static void UpdateExtractionPointList()
        {
            extractionPointList.Clear();

            var extractionPoints = UnityEngine.Object.FindObjectsOfType(Type.GetType("ExtractionPoint, Assembly-CSharp"));
            if (extractionPoints != null)
            {
                foreach (var ep in extractionPoints)
                {
                    var extractionPoint = ep as ExtractionPoint;
                    if (extractionPoint != null && extractionPoint.gameObject.activeInHierarchy)
                    {
                        var currentStateField = extractionPoint.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
                        string cachedState = "Unknown";
                        if (currentStateField != null)
                        {
                            var stateValue = currentStateField.GetValue(extractionPoint);
                            cachedState = stateValue?.ToString() ?? "Unknown";
                        }
                        Vector3 cachedPosition = extractionPoint.transform.position;
                        extractionPointList.Add(new ExtractionPointData(extractionPoint, cachedState, cachedPosition));

                        if (Vector3.Distance(cachedPosition, lastExtractionPosition) > 0.1f)
                        {
                            DLog.Log($"Extraction Point cached at position: {cachedPosition}");
                            lastExtractionPosition = cachedPosition;
                        }
                    }
                }
            }
        }

        public class ExtractionPointData
        {
            public ExtractionPoint ExtractionPoint { get; }
            public string CachedState { get; }
            public Vector3 CachedPosition { get; }

            public ExtractionPointData(ExtractionPoint ep, string state, Vector3 position)
            {
                ExtractionPoint = ep;
                CachedState = state;
                CachedPosition = position;
            }
        }
        private static void UpdateLists()
        {
            UpdateExtractionPointList();
            
            if (extractionPointList.Count != lastExtractionPointCount)
            {
                DLog.Log($"Extraction Points list updated: {extractionPointList.Count} points found.");
                lastExtractionPointCount = extractionPointList.Count;
            }
            
            enemyList.Clear();
            enemyHealthCache.Clear();
            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                var enemyDirectorInstance = enemyDirectorType.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (enemyDirectorInstance != null)
                {
                    var enemiesSpawnedField = enemyDirectorType.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
                    if (enemiesSpawnedField != null)
                    {
                        var enemies = enemiesSpawnedField.GetValue(enemyDirectorInstance) as IEnumerable<object>;
                        if (enemies != null)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy != null)
                                {
                                    var enemyInstanceField = enemy.GetType().GetField("enemyInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("childEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (enemyInstanceField != null)
                                    {
                                        var enemyInstance = enemyInstanceField.GetValue(enemy) as Enemy;
                                        if (enemyInstance != null && enemyInstance.gameObject != null && enemyInstance.gameObject.activeInHierarchy)
                                        {
                                            int health = Enemies.GetEnemyHealth(enemyInstance);
                                            enemyHealthCache[enemyInstance] = health;
                                            enemyList.Add(enemyInstance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            playerList.Clear();
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        playerList.Add(player);
                    }
                }
            }

            lastUpdateTime = Time.time;
            
            // Only log if any of the counts have changed
            if (enemyList.Count != lastEnemyCount || 
                valuableObjects.Count != lastItemCount || 
                playerList.Count != lastPlayerCount)
            {
                DLog.Log($"Lists updated: {enemyList.Count} enemies, {valuableObjects.Count} items, {playerList.Count} players.");
                lastEnemyCount = enemyList.Count;
                lastItemCount = valuableObjects.Count;
                lastPlayerCount = playerList.Count;
            }
        }

        private static void UpdateLocalPlayer()
        {
            GameObject newLocalPlayer = GetLocalPlayer();
            string newPlayerName = newLocalPlayer != null ? newLocalPlayer.name : "null";
            
            // Only log if the local player has changed
            if (newLocalPlayer != localPlayer || newPlayerName != lastLocalPlayerName)
            {
                if (newLocalPlayer != null)
                {
                    DLog.Log("Local player successfully updated: " + newPlayerName);
                }
                else
                {
                    DLog.Log("Failed to update local player!");
                }
                lastLocalPlayerName = newPlayerName;
            }
            
            localPlayer = newLocalPlayer;
        }

        public static bool IsLocalPlayer(object player)
        {
            try
            {
                if (localPlayer == null) // If localPlayer is null, try to update it
                {
                    UpdateLocalPlayer();
                    if (localPlayer == null)
                    {
                        return false;
                    }
                }

                if (player is GameObject playerObj) // If player is a GameObject, compare directly
                {
                    return playerObj == localPlayer;
                }

                if (player is MonoBehaviour playerMono) // If player is a MonoBehaviour, compare its gameObject
                {
                    return playerMono.gameObject == localPlayer;
                }

                var gameObjectProperty = player.GetType().GetProperty("gameObject");
                if (gameObjectProperty != null)
                {
                    GameObject playerGameObject = gameObjectProperty.GetValue(player) as GameObject;
                    return playerGameObject == localPlayer;
                }

                return false;
            }
            catch (System.Exception e)
            {
                DLog.Log($"Error in IsLocalPlayer: {e.Message}");
                return false;
            }
        }

        public static GameObject GetLocalPlayer()
        {
            if (PhotonNetwork.IsConnected)
            {
                var players = SemiFunc.PlayerGetList();
                if (players != null)
                {
                    foreach (var player in players)
                    {
                        var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (photonViewField != null)
                        {
                            var photonView = photonViewField.GetValue(player) as PhotonView;
                            if (photonView != null && photonView.IsMine)
                            {
                                var gameObjectProperty = player.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                                if (gameObjectProperty != null)
                                {
                                    GameObject foundPlayer = gameObjectProperty.GetValue(player) as GameObject;
                                    
                                    // Only log if the player name has changed
                                    string playerName = foundPlayer.name;
                                    if (playerName != lastLocalPlayerName) {
                                        DLog.Log("Local player found via Photon: " + playerName);
                                        lastLocalPlayerName = playerName;
                                    }
                                    return foundPlayer;
                                }
                                
                                // Only log if the player name has changed
                                string playerViewName = photonView.gameObject.name;
                                if (playerViewName != lastLocalPlayerName) {
                                    DLog.Log("Local player found via PhotonView: " + playerViewName);
                                    lastLocalPlayerName = playerViewName;
                                }
                                return photonView.gameObject;
                            }
                        }
                    }
                }

                if (PhotonNetwork.LocalPlayer != null)
                {
                    foreach (var photonView in UnityEngine.Object.FindObjectsOfType<PhotonView>())
                    {
                        if (photonView.Owner == PhotonNetwork.LocalPlayer && photonView.IsMine)
                        {
                            // Only log if the player name has changed
                            string playerName = photonView.gameObject.name;
                            if (playerName != lastLocalPlayerName)
                            {
                                DLog.Log("Local player found via Photon fallback: " + playerName);
                                lastLocalPlayerName = playerName;
                            }
                            return photonView.gameObject;
                        }
                    }
                }
            }
            else
            {
                var players = SemiFunc.PlayerGetList();
                if (players != null && players.Count > 0)
                {
                    var player = players[0];
                    var gameObjectProperty = player.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                    if (gameObjectProperty != null)
                    {
                        GameObject foundPlayer = gameObjectProperty.GetValue(player) as GameObject;
                        // Only log if the player name has changed
                        string playerName = foundPlayer.name;
                        if (playerName != lastLocalPlayerName)
                        {
                            DLog.Log("Local player found in singleplayer via PlayerGetList: " + playerName);
                            lastLocalPlayerName = playerName;
                        }
                        return foundPlayer;
                    }
                }

                var playerAvatarType = Type.GetType("PlayerAvatar, Assembly-CSharp");
                if (playerAvatarType != null)
                {
                    var playerAvatar = UnityEngine.Object.FindObjectOfType(playerAvatarType) as MonoBehaviour;
                    if (playerAvatar != null)
                    {
                        // Only log if the player name has changed
                        string playerName = playerAvatar.gameObject.name;
                        if (playerName != lastLocalPlayerName)
                        {
                            DLog.Log("Local player found in singleplayer via PlayerAvatar: " + playerName);
                            lastLocalPlayerName = playerName;
                        }
                        return playerAvatar.gameObject;
                    }
                }

                var playerByTag = GameObject.FindWithTag("Player");
                if (playerByTag != null)
                {
                    // Only log if the player name has changed
                    string playerName = playerByTag.name;
                    if (playerName != lastLocalPlayerName)
                    {
                        DLog.Log("Local player found in singleplayer via tag 'Player': " + playerName);
                        lastLocalPlayerName = playerName;
                    }
                    return playerByTag;
                }

                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Player") && obj.activeInHierarchy)
                    {
                        // Only log if the player name has changed
                        string playerName = obj.name;
                        if (playerName != lastLocalPlayerName)
                        {
                            DLog.Log("Local player found in singleplayer via generic name: " + playerName);
                            lastLocalPlayerName = playerName;
                        }
                        return obj;
                    }
                }

                DLog.Log("No local player found in singleplayer after all attempts!");
                return null;
            }

            DLog.Log("No local player found!");
            return null;
        }

        public static void UpdateEnemyList()
        {
            enemyList.Clear();
            enemyHealthCache.Clear();

            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                var enemyDirectorInstance = enemyDirectorType.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (enemyDirectorInstance != null)
                {
                    var enemiesSpawnedField = enemyDirectorType.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
                    if (enemiesSpawnedField != null)
                    {
                        var enemies = enemiesSpawnedField.GetValue(enemyDirectorInstance) as IEnumerable<object>;
                        if (enemies != null)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy != null)
                                {
                                    var enemyInstanceField = enemy.GetType().GetField("enemyInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("childEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (enemyInstanceField != null)
                                    {
                                        var enemyInstance = enemyInstanceField.GetValue(enemy) as Enemy;
                                        if (enemyInstance != null && enemyInstance.gameObject != null && enemyInstance.gameObject.activeInHierarchy)
                                        {
                                            int health = Enemies.GetEnemyHealth(enemyInstance);
                                            enemyHealthCache[enemyInstance] = health;
                                            enemyList.Add(enemyInstance);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            DLog.Log("No enemies found in enemiesSpawned");
                        }
                    }
                    else
                    {
                        DLog.Log("Field 'enemiesSpawned' not found");
                    }
                }
                else
                {
                    DLog.Log("EnemyDirector instance is null");
                }
            }
            else
            {
                DLog.Log("EnemyDirector not found");
            }
        }

        public static void RectFilled(float x, float y, float width, float height, Texture2D text)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), text);
        }

        public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        public static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 2f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }

        public static void InitializeStyles()
        {
            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    border = new RectOffset(1, 1, 1, 1)
                };
            }

            if (valueStyle == null)
            {
                valueStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }

            if (enemyStyle == null)
            {
                enemyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }

            if (healthStyle == null)
            {
                healthStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }

            if (distanceStyle == null)
            {
                distanceStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private static void CreateBoundsEdges(Bounds bounds, Color color)
        {
            Vector3[] vertices = new Vector3[8];
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            vertices[0] = new Vector3(min.x, min.y, min.z);
            vertices[1] = new Vector3(max.x, min.y, min.z);
            vertices[2] = new Vector3(max.x, min.y, max.z);
            vertices[3] = new Vector3(min.x, min.y, max.z);
            vertices[4] = new Vector3(min.x, max.y, min.z);
            vertices[5] = new Vector3(max.x, max.y, min.z);
            vertices[6] = new Vector3(max.x, max.y, max.z);
            vertices[7] = new Vector3(min.x, max.y, max.z);

            Vector2[] screenVertices = new Vector2[8];
            bool isVisible = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3 screenPos = cachedCamera.WorldToScreenPoint(vertices[i]);
                if (screenPos.z > 0) isVisible = true;
                screenVertices[i] = new Vector2(screenPos.x * scaleX, Screen.height - (screenPos.y * scaleY));
            }

            if (!isVisible) return;

            DrawLine(screenVertices[0], screenVertices[1], color);
            DrawLine(screenVertices[1], screenVertices[2], color);
            DrawLine(screenVertices[2], screenVertices[3], color);
            DrawLine(screenVertices[3], screenVertices[0], color);

            DrawLine(screenVertices[4], screenVertices[5], color);
            DrawLine(screenVertices[5], screenVertices[6], color);
            DrawLine(screenVertices[6], screenVertices[7], color);
            DrawLine(screenVertices[7], screenVertices[4], color);

            DrawLine(screenVertices[0], screenVertices[4], color);
            DrawLine(screenVertices[1], screenVertices[5], color);
            DrawLine(screenVertices[2], screenVertices[6], color);
            DrawLine(screenVertices[3], screenVertices[7], color);
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            if (texture2 == null) return;

            float distance = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            GUI.color = color;
            Matrix4x4 originalMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, distance, 1f), texture2);
            GUI.matrix = originalMatrix;
            GUI.color = Color.white;
        }

        private static Bounds GetActiveColliderBounds(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            List<Collider> activeColliders = new List<Collider>();

            foreach (Collider col in colliders)
            {
                if (col.enabled && col.gameObject.activeInHierarchy)
                    activeColliders.Add(col);
            }

            if (activeColliders.Count == 0)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        if (renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
                            bounds.Encapsulate(renderers[i].bounds);
                    }
                    return bounds;
                }
                return new Bounds(obj.transform.position, Vector3.one * 0.5f);
            }

            Bounds resultBounds = activeColliders[0].bounds;
            for (int i = 1; i < activeColliders.Count; i++)
            {
                resultBounds.Encapsulate(activeColliders[i].bounds);
            }

            resultBounds.Expand(0.1f);
            return resultBounds;
        }
        public static void DrawESP()
        {
            bool isLevelAnimationStarted = _levelAnimationStartedField != null && (bool)_levelAnimationStartedField.GetValue(LoadingUI.instance);
            if (RunManager.instance.levelCurrent != null && !Hax2.levelsToSearchItems.Contains(RunManager.instance.levelCurrent.name)
                || !Hax2.levelsToSearchItems.Contains(RunManager.instance.levelCurrent.name)
                && isLevelAnimationStarted)
            {
                return;
            }
            InitializeStyles();
            if (!drawEspBool && !drawItemEspBool && !drawExtractionPointEspBool && !drawPlayerEspBool && !draw2DPlayerEspBool && !draw3DPlayerEspBool && !draw3DItemEspBool && !drawChamsBool) return;
            if (localPlayer == null)
            {
                UpdateLocalPlayer();
            }

            // Split updates to avoid doing everything at once
            float currentTime = Time.time;

            // Update player data more frequently but not every frame
            if (currentTime - lastPlayerUpdateTime > playerUpdateInterval)
            {
                UpdatePlayerDataList();
                lastPlayerUpdateTime = currentTime;
            }

            if (currentTime - lastUpdateTime > updateInterval)
            {
                if (drawEspBool || drawItemEspBool || drawExtractionPointEspBool || drawPlayerEspBool || draw2DPlayerEspBool || draw3DPlayerEspBool || draw3DItemEspBool)
                {
                    UpdateLists();
                }
                lastUpdateTime = currentTime;
            }

            if (cachedCamera == null || cachedCamera != Camera.main)
            {
                cachedCamera = Camera.main;
                if (cachedCamera == null)
                {
                    DLog.Log("Camera.main not found!");
                    return;
                }
            }

            scaleX = (float)Screen.width / cachedCamera.pixelWidth;
            scaleY = (float)Screen.height / cachedCamera.pixelHeight;

            if (!visibleMaterial || !hiddenMaterial)
            {
                Shader chamsShader = Shader.Find("Hidden/Internal-Colored");
                if (chamsShader != null)
                {
                    hiddenMaterial = new Material(chamsShader);
                    hiddenMaterial.SetInt("_SrcBlend", 5);
                    hiddenMaterial.SetInt("_DstBlend", 10);
                    hiddenMaterial.SetInt("_Cull", 0);
                    hiddenMaterial.SetInt("_ZTest", 8);
                    hiddenMaterial.SetInt("_ZWrite", 0);
                    hiddenMaterial.SetColor("_Color", enemyHiddenColor);
                    hiddenMaterial.renderQueue = 4000;
                    hiddenMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    hiddenMaterial.EnableKeyword("_EMISSION");

                    visibleMaterial = new Material(chamsShader);
                    visibleMaterial.SetInt("_SrcBlend", 5);
                    visibleMaterial.SetInt("_DstBlend", 10);
                    visibleMaterial.SetInt("_Cull", 0);
                    visibleMaterial.SetInt("_ZTest", 4);
                    visibleMaterial.SetInt("_ZWrite", 0);
                    visibleMaterial.SetColor("_Color", enemyVisibleColor);
                    visibleMaterial.renderQueue = 4001;
                    visibleMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    visibleMaterial.EnableKeyword("_EMISSION");
                }
            }
            else
            {
                // Update colors if materials already exist
                if (hiddenMaterial) hiddenMaterial.SetColor("_Color", enemyHiddenColor);
                if (visibleMaterial) visibleMaterial.SetColor("_Color", enemyVisibleColor);
            }

            if (!itemVisibleMaterial || !itemHiddenMaterial)
            {
                Shader chamsShader = Shader.Find("Hidden/Internal-Colored");
                if (chamsShader != null)
                {
                    itemHiddenMaterial = new Material(chamsShader);
                    itemHiddenMaterial.SetInt("_SrcBlend", 5);
                    itemHiddenMaterial.SetInt("_DstBlend", 10);
                    itemHiddenMaterial.SetInt("_Cull", 0);
                    itemHiddenMaterial.SetInt("_ZTest", 8);
                    itemHiddenMaterial.SetInt("_ZWrite", 0);
                    itemHiddenMaterial.SetColor("_Color", itemHiddenColor);
                    itemHiddenMaterial.renderQueue = 4000;

                    itemVisibleMaterial = new Material(chamsShader);
                    itemVisibleMaterial.SetInt("_SrcBlend", 5);
                    itemVisibleMaterial.SetInt("_DstBlend", 10);
                    itemVisibleMaterial.SetInt("_Cull", 0);
                    itemVisibleMaterial.SetInt("_ZTest", 4);
                    itemVisibleMaterial.SetInt("_ZWrite", 0);
                    itemVisibleMaterial.SetColor("_Color", itemVisibleColor);
                    itemVisibleMaterial.renderQueue = 4001;
                }
            }
            else
            {
                // Update colors if materials already exist
                if (itemHiddenMaterial) itemHiddenMaterial.SetColor("_Color", itemHiddenColor);
                if (itemVisibleMaterial) itemVisibleMaterial.SetColor("_Color", itemVisibleColor);
            }

            if (drawChamsBool)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    if (!cachedOriginalCamera)
                    {
                        originalFarClipPlane = mainCamera.farClipPlane;
                        originalDepthTextureMode = mainCamera.depthTextureMode;
                        originalOcclusionCulling = mainCamera.useOcclusionCulling;
                        cachedOriginalCamera = true;
                    }

                    mainCamera.farClipPlane = 500f;
                    mainCamera.depthTextureMode = DepthTextureMode.None;
                    mainCamera.useOcclusionCulling = false;
                }

                foreach (var enemyInstance in enemyList)
                {
                    if (enemyInstance == null || !enemyInstance.gameObject.activeInHierarchy || enemyInstance.CenterTransform == null) continue;

                    var enemyParent = enemyInstance.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                    var renderers = enemyParent.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;

                        if (!enemyOriginalMaterials.ContainsKey(renderer))
                        {
                            enemyOriginalMaterials[renderer] = renderer.materials;
                        }

                        renderer.materials = new Material[] { hiddenMaterial, visibleMaterial };
                    }
                }
            }

            // === ITEM CHAMS ===
            if (drawItemChamsBool)
            {
                foreach (var valuableObject in valuableObjects)
                {
                    if (valuableObject == null) continue;

                    var transform = valuableObject.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as Transform;
                    if (transform == null || !transform.gameObject.activeInHierarchy) continue;

                    var renderers = transform.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;

                        if (!itemOriginalMaterials.ContainsKey(renderer))
                        {
                            itemOriginalMaterials[renderer] = renderer.materials;
                        }

                        renderer.materials = new Material[] { itemHiddenMaterial, itemVisibleMaterial };
                    }
                }


                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    if (!cachedOriginalCamera)
                    {
                        originalFarClipPlane = mainCamera.farClipPlane;
                        originalDepthTextureMode = mainCamera.depthTextureMode;
                        originalOcclusionCulling = mainCamera.useOcclusionCulling;
                        cachedOriginalCamera = true;
                    }

                    mainCamera.farClipPlane = 500f;

                    mainCamera.depthTextureMode = DepthTextureMode.None;

                    mainCamera.useOcclusionCulling = false;
                }

            }

            if (drawEspBool)
            {
                // Clean up broken/destroyed enemy instances
                enemyList.RemoveAll(enemyInstance =>
                {
                    try
                    {
                        // Check for null or destroyed objects.
                        return enemyInstance == null ||
                               enemyInstance.gameObject == null ||
                               enemyInstance.CenterTransform == null;
                    }
                    catch
                    {
                        return true;
                    }
                });

                foreach (var enemyInstance in enemyList)
                {
                    if (enemyInstance == null || !enemyInstance.gameObject.activeInHierarchy || enemyInstance.CenterTransform == null)
                        continue;

                    Vector3 footPosition = enemyInstance.transform.position;
                    float enemyHeightEstimate = 2f;
                    Vector3 headPosition = enemyInstance.transform.position + Vector3.up * enemyHeightEstimate;

                    Vector3 screenFootPos = cachedCamera.WorldToScreenPoint(footPosition);
                    Vector3 screenHeadPos = cachedCamera.WorldToScreenPoint(headPosition);

                    if (screenFootPos.z > 0 && screenHeadPos.z > 0)
                    {
                        float footX = screenFootPos.x * scaleX;
                        float footY = Screen.height - (screenFootPos.y * scaleY);
                        float headY = Screen.height - (screenHeadPos.y * scaleY);

                        float height = Mathf.Abs(footY - headY);
                        float enemyScale = enemyInstance.transform.localScale.y;
                        float baseWidth = enemyScale * 200f;
                        float distance = screenFootPos.z;
                        float width = (baseWidth / distance) * scaleX;

                        width = Mathf.Clamp(width, 30f, height * 1.2f);
                        height = Mathf.Clamp(height, 40f, 400f);

                        float x = footX;
                        float y = footY;

                        if (showEnemyBox) // Only draw the box if showEnemyBox is true
                        {
                            Box(x, y, width, height, texture2, 1f);
                        }

                        float labelWidth = 200f;
                        float labelX = x - labelWidth / 2f;

                        var enemyParent = enemyInstance.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                        string enemyName = "Enemy";
                        if (enemyParent != null)
                        {
                            var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                            enemyName = nameField?.GetValue(enemyParent) as string ?? "Enemy";
                        }

                        string healthText = "";
                        if (showEnemyHP && enemyHealthCache.ContainsKey(enemyInstance))
                        {
                            int health = Enemies.GetEnemyHealth(enemyInstance);
                            enemyHealthCache[enemyInstance] = health;
                            int maxHealth = Enemies.GetEnemyMaxHealth(enemyInstance);
                            float healthPercentage = maxHealth > 0 ? (float)health / maxHealth : 0f;
                            healthText = health >= 0 ? $" HP: {health}/{maxHealth}" : "";
                            healthStyle.normal.textColor = healthPercentage > 0.66f ? Color.green : (healthPercentage > 0.33f ? Color.yellow : Color.red);
                        }

                        string distanceText = "";
                        if (showEnemyDistance && localPlayer != null)
                        {
                            float distance2 = Vector3.Distance(localPlayer.transform.position, enemyInstance.transform.position);
                            distanceText = $" [{distance2:F1}m]";
                        }

                        string fullText = "";
                        if (showEnemyNames) fullText = enemyName += " " + distanceText;

                        float labelHeight = enemyStyle.CalcHeight(new GUIContent(fullText), labelWidth);
                        float labelY = y - height - labelHeight;

                        // Calculate height for health text if shown
                        float healthLabelHeight = 0f;
                        if (showEnemyHP && !string.IsNullOrEmpty(healthText))
                        {
                            healthLabelHeight = healthStyle.CalcHeight(new GUIContent(healthText), labelWidth);
                        }

                        GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), fullText, enemyStyle);

                        // Display health text if enabled
                        if (showEnemyHP && !string.IsNullOrEmpty(healthText))
                        {
                            GUI.Label(new Rect(labelX, labelY - healthLabelHeight, labelWidth, healthLabelHeight), healthText, healthStyle);
                        }
                    }
                }
            }

            if (drawItemEspBool)
            {
                // Clean up broken/destroyed items
                valuableObjects.RemoveAll(item =>
                {
                    try
                    {
                        return item == null || (item is UnityEngine.Object unityObj && unityObj == null);
                    }
                    catch { return true; }
                });

                foreach (var valuableObject in valuableObjects)
                {
                    if (valuableObject == null) continue;

                    bool isPlayerDeathHead = valuableObject.GetType().Name == "PlayerDeathHead";
                    if (!DebugCheats.showPlayerDeathHeads && isPlayerDeathHead) continue;

                    // Safe transform access
                    Transform transform = null;
                    try
                    {
                        transform = valuableObject?.GetType()?.GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as Transform;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[ESP] Failed to get transform from valuableObject: {e.Message}");
                        continue;
                    }

                    if (transform == null || !transform.gameObject.activeInHierarchy) continue;

                    Vector3 itemPosition = transform.position;
                    float itemDistance = 0f;

                    if (localPlayer != null)
                    {
                        itemDistance = Vector3.Distance(localPlayer.transform.position, itemPosition);
                        if (itemDistance > DebugCheats.maxItemEspDistance) continue;
                    }

                    if (!isPlayerDeathHead && DebugCheats.showItemValue)
                    {
                        int itemValue = 0;
                        var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                        if (valueField != null)
                        {
                            try
                            {
                                itemValue = Convert.ToInt32(valueField.GetValue(valuableObject));
                                if (itemValue < DebugCheats.minItemValue)
                                    continue;
                            }
                            catch (Exception e)
                            {
                                Debug.Log($"Error reading 'dollarValueCurrent': {e.Message}. Defaulting to 0.");
                            }
                        }
                    }

                    Vector3 screenPos = cachedCamera.WorldToScreenPoint(itemPosition);

                    if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                    {
                        float x = screenPos.x * scaleX;
                        float y = Screen.height - (screenPos.y * scaleY);

                        string itemName;
                        Color originalColor = nameStyle.normal.textColor;

                        if (isPlayerDeathHead)
                        {
                            itemName = "Dead Player Head";
                            nameStyle.normal.textColor = Color.red;
                        }
                        else
                        {
                            nameStyle.normal.textColor = Color.yellow;

                            try
                            {
                                itemName = valuableObject.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as string;
                                if (string.IsNullOrEmpty(itemName))
                                {
                                    itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                                }
                            }
                            catch (Exception e)
                            {
                                itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                                Debug.Log($"Error accessing item 'name': {e.Message}. Using GameObject name: {itemName}");
                            }

                            if (itemName.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
                                itemName = itemName.Substring("Valuable".Length).Trim();
                            if (itemName.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
                                itemName = itemName.Substring(0, itemName.Length - "(Clone)".Length).Trim();
                        }

                        int itemValue = 0;
                        if (!isPlayerDeathHead)
                        {
                            var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                            if (valueField != null)
                            {
                                try
                                {
                                    itemValue = Convert.ToInt32(valueField.GetValue(valuableObject));
                                }
                                catch (Exception e)
                                {
                                    Debug.Log($"Error reading 'dollarValueCurrent' for '{itemName}': {e.Message}. Defaulting to 0.");
                                }
                            }
                        }

                        Color distanceColor = isPlayerDeathHead ? Color.red : Color.yellow;
                        nameStyle.normal.textColor = distanceColor;

                        string distanceText = "";
                        if (showItemDistance && localPlayer != null)
                        {
                            float distance = Vector3.Distance(localPlayer.transform.position, itemPosition);
                            distanceText = $" [{distance:F1}m]";
                        }

                        string nameText = showItemNames ? itemName : "";
                        if (showItemDistance) nameText += distanceText;

                        float labelWidth = 200f;
                        float valueLabelHeight = valueStyle.CalcHeight(new GUIContent(itemValue.ToString() + "$"), labelWidth);
                        float nameLabelHeight = nameStyle.CalcHeight(new GUIContent(nameText), labelWidth);
                        float totalHeight = nameLabelHeight + valueLabelHeight + 5f;
                        float labelX = x - labelWidth / 2f;
                        float labelY = y - totalHeight - 5f;

                        if (!string.IsNullOrEmpty(nameText))
                            GUI.Label(new Rect(labelX, labelY, labelWidth, nameLabelHeight), nameText, nameStyle);

                        if (showItemValue && !isPlayerDeathHead)
                            GUI.Label(new Rect(labelX, labelY + nameLabelHeight + 2f, labelWidth, valueLabelHeight), itemValue.ToString() + "$", valueStyle);

                        if (draw3DItemEspBool)
                        {
                            Bounds bounds = GetActiveColliderBounds(transform.gameObject);
                            CreateBoundsEdges(bounds, Color.yellow);
                        }

                        nameStyle.normal.textColor = originalColor;
                    }
                }
            }

            if (drawExtractionPointEspBool)
            {
                foreach (var epData in extractionPointList)
                {
                    if (epData.ExtractionPoint == null || !epData.ExtractionPoint.gameObject.activeInHierarchy) continue;

                    Vector3 screenPos = cachedCamera.WorldToScreenPoint(epData.CachedPosition);

                    if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                    {
                        float x = screenPos.x * scaleX;
                        float y = Screen.height - (screenPos.y * scaleY);

                        string pointName = "Extraction Point";
                        string stateText = $" ({epData.CachedState})";
                        string distanceText = showExtractionDistance && localPlayer != null ? $"[{Vector3.Distance(localPlayer.transform.position, epData.CachedPosition):F1}m]" : "";

                        Color originalColor = nameStyle.normal.textColor;

                        nameStyle.normal.textColor = epData.CachedState == "Active" ? Color.green : (epData.CachedState == "Idle" ? Color.red : Color.cyan);

                        string nameFullText = showExtractionNames ? pointName + stateText : "";
                        if (showExtractionDistance) nameFullText += " " + distanceText;

                        float labelWidth = 200f;
                        float nameLabelHeight = nameStyle.CalcHeight(new GUIContent(nameFullText), labelWidth);
                        float totalHeight = nameLabelHeight;
                        float labelX = x - labelWidth / 2f;
                        float labelY = y - totalHeight - 5f;

                        if (!string.IsNullOrEmpty(nameFullText))
                        {
                            GUI.Label(new Rect(labelX, labelY, labelWidth, nameLabelHeight), nameFullText, nameStyle);
                        }

                        nameStyle.normal.textColor = originalColor;

                    }
                }
            }

            if (drawPlayerEspBool)
            {

                foreach (var playerData in playerDataList)
                {
                    bool isLocalPlayer = false;
                    if (!PhotonNetwork.IsConnected && localPlayer != null && playerData.Transform.gameObject == localPlayer)
                    {
                        isLocalPlayer = true;
                    }
                    if (playerData.PhotonView == null || (playerData.PhotonView.IsMine && PhotonNetwork.IsConnected) || isLocalPlayer || !playerData.Transform.gameObject.activeInHierarchy) continue;

                    Vector3 playerPos = playerData.Transform.position;
                    float distanceToPlayer = localPlayer != null ? Vector3.Distance(localPlayer.transform.position, playerPos) : float.MaxValue;
                    if (distanceToPlayer > maxEspDistance) continue;

                    Vector3 footPosition = playerPos;
                    float playerHeightEstimate = 2f;
                    Vector3 headPosition = playerPos + Vector3.up * playerHeightEstimate;

                    Vector3 screenFootPos = cachedCamera.WorldToScreenPoint(footPosition);
                    Vector3 screenHeadPos = cachedCamera.WorldToScreenPoint(headPosition);
                    bool isInFront = screenFootPos.z > 0 && screenHeadPos.z > 0;

                    if (!isInFront) continue;

                    float footX = screenFootPos.x * scaleX;
                    float footY = Screen.height - (screenFootPos.y * scaleY);
                    float headY = Screen.height - (screenHeadPos.y * scaleY);

                    float height = Mathf.Abs(footY - headY);
                    float playerScale = playerData.Transform.localScale.y;
                    float baseWidth = playerScale * 200f;
                    float width = (baseWidth / (distanceToPlayer + 1f)) * scaleX;
                    width = Mathf.Clamp(width, 30f, height * 1.2f);
                    height = Mathf.Clamp(height, 40f, 400f);

                    float x = footX;
                    float y = footY;

                    if (draw3DPlayerEspBool)
                    {
                        Bounds bounds = GetActiveColliderBounds(playerData.Transform.gameObject);
                        CreateBoundsEdges(bounds, Color.red);
                    }
                    if (draw2DPlayerEspBool)
                    {
                        Box(x, y, width, height, texture2, 2f);
                    }

                    Color originalNameColor = nameStyle.normal.textColor;
                    nameStyle.normal.textColor = Color.white;

                    int health = playerHealthCache.ContainsKey(playerData.PhotonView.ViewID) ? playerHealthCache[playerData.PhotonView.ViewID] : 100;
                    string healthText = "";
                    if (showPlayerHP)
                    {
                        int maxHealth = Players.GetPlayerMaxHealth(playerData.PlayerObject);
                        float healthPercentage = maxHealth > 0 ? (float)health / maxHealth : 0f;
                        healthText = health >= 0 ? $"HP: {health}/{maxHealth}" : "";
                        healthStyle.normal.textColor = healthPercentage > 0.66f ? Color.green : (healthPercentage > 0.33f ? Color.yellow : Color.red);
                    }
                    
                    string distanceText = "";
                    if (showPlayerDistance && localPlayer != null)
                        distanceText = $" [{distanceToPlayer:F1}m]";

                    string nameFullText = showPlayerNames ? playerData.Name : "";
                    if (showPlayerDistance) nameFullText += distanceText;

                    float labelWidth = 200f;
                    float nameHeight = nameStyle.CalcHeight(new GUIContent(nameFullText), labelWidth);
                    float healthHeight = healthStyle.CalcHeight(new GUIContent(healthText), labelWidth);
                    float totalHeight = nameHeight;
                    float labelX = footX - labelWidth / 2f;
                    float labelY = footY - height - totalHeight - 10f;
                    
                    // Calculate height for health text if shown
                    float healthLabelY = labelY;
                    if (showPlayerHP && !string.IsNullOrEmpty(healthText)) {
                        healthLabelY = labelY - healthHeight;
                    }

                    if (!string.IsNullOrEmpty(nameFullText))
                    {
                        GUI.Label(new Rect(labelX, labelY, labelWidth, nameHeight), nameFullText, nameStyle);
                    }
                    
                    // Display health text if enabled
                    if (showPlayerHP && !string.IsNullOrEmpty(healthText))
                    {
                        GUI.Label(new Rect(labelX, healthLabelY, labelWidth, healthHeight), healthText, healthStyle);
                    }

                    nameStyle.normal.textColor = originalNameColor;

                }
            }
        }
    }
}