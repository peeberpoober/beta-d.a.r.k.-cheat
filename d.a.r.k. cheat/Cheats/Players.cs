using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace dark_cheat
{
    static class Players
    {
        static public object playerHealthInstance;
        static public object playerMaxHealthInstance;

        private static Type postProcessingType = null;
        private static Type cameraZoomType = null;

        public static void HealPlayer(object targetPlayer, int healAmount, string playerName)
        {
            if (targetPlayer == null)
            {
                DLog.Log("Target player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to heal: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }

                var healthType = playerHealthInstance.GetType();
                var healMethod = healthType.GetMethod("Heal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healMethod != null)
                {
                    healMethod.Invoke(playerHealthInstance, new object[] { healAmount, true });
                    DLog.Log($"'Heal' method called locally with {healAmount} HP.");
                }
                else
                {
                    DLog.Log("'Heal' method not found!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    var currentHealthField = healthType.GetField("currentHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    int currentHealth = currentHealthField != null ? (int)currentHealthField.GetValue(playerHealthInstance) : 0;
                    int maxHealth = GetPlayerMaxHealth(playerHealthInstance);

                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] { maxHealth, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' sent to all with health={currentHealth + maxHealth}, maxHealth={maxHealth}, effect=true.");

                    try
                    {
                        photonView.RPC("HealRPC", RpcTarget.AllBuffered, new object[] { healAmount, true });
                        DLog.Log($"RPC 'HealRPC' sent with {healAmount} HP.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HealRPC' not registered, relying on UpdateHealthRPC.");
                    }
                }
                else
                {
                    DLog.Log("Not connected to Photon, local healing only.");
                }
                DLog.Log($"Healing attempt completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error trying to heal: {e.Message}");
            }
        }

        public static void DamagePlayer(object targetPlayer, int damageAmount, string playerName)
        {
            if (targetPlayer == null)
            {
                DLog.Log("Target player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to damage: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = targetPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(targetPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }

                var playerHealthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(targetPlayer);
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }

                var healthType = playerHealthInstance.GetType();
                var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (hurtMethod != null)
                {
                    hurtMethod.Invoke(playerHealthInstance, new object[] { damageAmount, true, -1 });
                    DLog.Log($"'Hurt' method called locally with {damageAmount} damage.");
                }
                else
                {
                    DLog.Log("'Hurt' method not found!");
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    int maxHealth = GetPlayerMaxHealth(playerHealthInstance);

                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { damageAmount, Vector3.zero, false, -1 });
                    DLog.Log($"RPC 'HurtOtherRPC' sent with {damageAmount} damage.");

                    try
                    {
                        photonView.RPC("HurtRPC", RpcTarget.AllBuffered, new object[] { damageAmount, true, -1 });
                        DLog.Log($"RPC 'HurtRPC' sent with {damageAmount} damage.");
                    }
                    catch
                    {
                        DLog.Log("RPC 'HurtRPC' not registered, relying on HurtOtherRPC.");
                    }
                }
                else
                {
                    DLog.Log("Not connected to Photon, local damage only.");
                }
                DLog.Log($"Damage attempt completed.");
            }
            catch (Exception e)
            {
                DLog.Log($"Error trying to damage: {e.Message}");
            }
        }
        internal static void ReviveSelectedPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count || selectedPlayerIndex >= playerNames.Count)
            {
                DLog.Log("Invalid player index for revival!");
                return;
            }

            object selectedPlayer = playerList[selectedPlayerIndex];
            string playerName = playerNames[selectedPlayerIndex];

            if (selectedPlayer == null)
            {
                DLog.Log($"Selected player at index {selectedPlayerIndex} is null!");
                return;
            }

            ReviveSelectedPlayer(selectedPlayer, playerList, playerName);
        }

        public static void ReviveSelectedPlayer(object selectedPlayer, System.Collections.Generic.List<object> playerList, string playerName)
        {
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            object playerDeathHeadInstance = null;
            GameObject playerDeathHeadGameObject = null;
            try
            {
                var playerDeathHeadField = selectedPlayer.GetType().GetField("playerDeathHead", BindingFlags.Public | BindingFlags.Instance);
                if (playerDeathHeadField != null)
                {
                    playerDeathHeadInstance = playerDeathHeadField.GetValue(selectedPlayer);
                    if (playerDeathHeadInstance != null)
                    {
                        if (playerDeathHeadInstance is Component deathHeadComponent)
                        {
                            playerDeathHeadGameObject = deathHeadComponent.gameObject;
                            DLog.Log("Got PlayerDeathHead GameObject.");
                        }
                        else
                        {
                            DLog.Log("'playerDeathHead' instance is not a Component, cannot get GameObject.");
                        }

                        var inExtractionPointField = playerDeathHeadInstance.GetType().GetField("inExtractionPoint", BindingFlags.NonPublic | BindingFlags.Instance);
                        var reviveMethod = playerDeathHeadInstance.GetType().GetMethod("Revive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (inExtractionPointField != null)
                        {
                            inExtractionPointField.SetValue(playerDeathHeadInstance, true);
                            DLog.Log("'inExtractionPoint' field set to true.");
                        }
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(playerDeathHeadInstance, null);
                            DLog.Log("'Revive' method successfully called for: " + playerName);
                        }
                        else DLog.Log("'Revive' method not found!");
                    }
                    else DLog.Log("'playerDeathHead' instance not found.");
                }
                else DLog.Log("'playerDeathHead' field not found.");

                var playerHealthField = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField != null)
                {
                    var playerHealthInstance = playerHealthField.GetValue(selectedPlayer);
                    if (playerHealthInstance != null)
                    {
                        var healthType = playerHealthInstance.GetType();
                        var healthField = healthType.GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        int maxHealth = GetPlayerMaxHealth(playerHealthInstance);
                        if (healthField != null)
                        {
                            healthField.SetValue(playerHealthInstance, maxHealth);
                            DLog.Log($"Health set directly to {maxHealth} via 'health' field.");
                        }
                        else
                        {
                            DLog.Log("'health' field not found, attempting HealPlayer as fallback.");
                            Players.HealPlayer(selectedPlayer, maxHealth, playerName);
                        }

                        int currentHealth = GetPlayerHealth(selectedPlayer);
                        DLog.Log($"Current health after revive: {(currentHealth >= 0 ? currentHealth.ToString() : "Unknown")}");
                    }
                    else DLog.Log("PlayerHealth instance is null, health restoration failed.");
                }
                else DLog.Log("'playerHealth' field not found, healing not performed.");

                // --- Camera Darkening Fix ---
                if (playerDeathHeadGameObject != null)
                {
                    ResetPostProcessingOverrides(playerDeathHeadGameObject);
                    ResetCameraZoomOverride(playerDeathHeadGameObject);
                }
                else
                {
                    DLog.Log("Cannot reset effects because playerDeathHead GameObject is null.");
                }

            }
            catch (Exception e)
            {
                DLog.Log($"Error reviving and healing {playerName}: {e.Message}");
            }
        }

        internal static void KillSelectedPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count || selectedPlayerIndex >= playerNames.Count)
            {
                DLog.Log("Invalid player index for kill operation!");
                return;
            }

            object selectedPlayer = playerList[selectedPlayerIndex];
            string playerName = playerNames[selectedPlayerIndex];

            if (selectedPlayer == null)
            {
                DLog.Log($"Selected player at index {selectedPlayerIndex} is null!");
                return;
            }

            KillSelectedPlayer(selectedPlayer, playerList, playerName);
        }

        public static void KillSelectedPlayer(object selectedPlayer, System.Collections.Generic.List<object> playerList, string playerName)
        {
            if (selectedPlayer == null)
            {
                DLog.Log("Selected player is null!");
                return;
            }

            try
            {
                DLog.Log($"Attempting to kill: {playerName} | MasterClient: {PhotonNetwork.IsMasterClient}");
                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { DLog.Log("PhotonViewField not found!"); return; }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null) { DLog.Log("PhotonView is not valid!"); return; }
                var playerHealthField = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) { DLog.Log("'playerHealth' field not found!"); return; }
                var playerHealthInstance = playerHealthField.GetValue(selectedPlayer); // Keep original variable name
                if (playerHealthInstance == null) { DLog.Log("playerHealth instance is null!"); return; }
                var healthType = playerHealthInstance.GetType();
                var deathMethod = healthType.GetMethod("Death", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (deathMethod == null) { DLog.Log("'Death' method not found!"); return; }
                deathMethod.Invoke(playerHealthInstance, null);
                DLog.Log($"'Death' method called locally for {playerName}.");

                var playerAvatarField = healthType.GetField("playerAvatar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerAvatarField != null)
                {
                    var playerAvatarInstance = playerAvatarField.GetValue(playerHealthInstance);
                    if (playerAvatarInstance != null)
                    {
                        var playerAvatarType = playerAvatarInstance.GetType();
                        var playerDeathMethod = playerAvatarType.GetMethod("PlayerDeath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (playerDeathMethod != null) { playerDeathMethod.Invoke(playerAvatarInstance, new object[] { -1 }); DLog.Log($"'PlayerDeath' method called locally for {playerName}."); }
                        else DLog.Log("'PlayerDeath' method not found in PlayerAvatar!");
                    }
                    else DLog.Log("PlayerAvatar instance is null!");
                }
                else DLog.Log("'playerAvatar' field not found in PlayerHealth!");

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    int maxHealth = GetPlayerMaxHealth(playerHealthInstance);
                    photonView.RPC("UpdateHealthRPC", RpcTarget.AllBuffered, new object[] { 0, maxHealth, true });
                    DLog.Log($"RPC 'UpdateHealthRPC' sent to all with health=0, maxHealth={maxHealth}, effect=true.");
                    try { photonView.RPC("PlayerDeathRPC", RpcTarget.AllBuffered, new object[] { -1 }); DLog.Log("Trying RPC 'PlayerDeathRPC' to force death..."); }
                    catch { DLog.Log("RPC 'PlayerDeathRPC' not registered, trying alternative..."); }
                    photonView.RPC("HurtOtherRPC", RpcTarget.AllBuffered, new object[] { 9999, Vector3.zero, false, -1 });
                    DLog.Log("RPC 'HurtOtherRPC' sent with 9999 damage to ensure death.");
                }
                else DLog.Log("Not connected to Photon, death is only local.");
                DLog.Log($"Attempt to kill {playerName} completed.");
            }
            catch (Exception e) { DLog.Log($"Error trying to kill {playerName}: {e.Message}"); }
        }

        public static void ForcePlayerTumble(float duration = 10f)  // Default duration is 10 seconds
        {
            if (Hax2.selectedPlayerIndex < 0 || Hax2.selectedPlayerIndex >= Hax2.playerList.Count)
            {
                Debug.Log("Invalid player index!");
                return;
            }
            var selectedPlayer = Hax2.playerList[Hax2.selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                Debug.Log("Selected player is null!");
                return;
            }
            try
            {
                Debug.Log($"Forcing {Hax2.playerNames[Hax2.selectedPlayerIndex]} to tumble for {duration} seconds.");
                var playerTumbleField = selectedPlayer.GetType().GetField("tumble", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerTumbleField == null) { Debug.Log("PlayerTumble field not found!"); return; }
                var playerTumble = playerTumbleField.GetValue(selectedPlayer) as PlayerTumble;
                if (playerTumble == null) { Debug.Log("PlayerTumble instance is null!"); return; }
                var photonViewField = playerTumble.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { Debug.Log("PhotonView field not found on PlayerTumble!"); return; }
                var photonView = photonViewField.GetValue(playerTumble) as PhotonView;
                if (photonView == null) { Debug.Log("PhotonView is not valid!"); return; }
                photonView.RPC("TumbleSetRPC", RpcTarget.All, true, false);
                photonView.RPC("TumbleOverrideTimeRPC", RpcTarget.All, duration);
                photonView.RPC("TumbleForceRPC", RpcTarget.All, new Vector3(10f, 50f, 0f));  // Throw forward
                photonView.RPC("TumbleTorqueRPC", RpcTarget.All, new Vector3(0f, 0f, 2000f));
                Debug.Log($"Forced {Hax2.playerNames[Hax2.selectedPlayerIndex]} to tumble for {duration} seconds.");
            }
            catch (Exception e)
            {
                Debug.Log($"Error forcing {Hax2.playerNames[Hax2.selectedPlayerIndex]} to tumble: {e.Message}");
            }
        }

        public static int GetPlayerHealth(object player)
        {
            try
            {
                var playerHealthField = player.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) return 100;

                var playerHealthInstance = playerHealthField.GetValue(player);
                if (playerHealthInstance == null) return 100;

                var healthField = playerHealthInstance.GetType().GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) return 100;

                return (int)healthField.GetValue(playerHealthInstance);
            }
            catch (Exception e)
            {
                DLog.Log($"Error getting player health: {e.Message}");
                return 100;
            }
        }

        public static int GetPlayerMaxHealth(object playerHealthInstance)
        {
            if (playerHealthInstance == null)
            {
                DLog.Log("playerHealthInstance is null when trying to get maxHealth!");
                return 100;
            }

            var healthType = playerHealthInstance.GetType();
            var maxHealthField = healthType.GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;
        }

        // --- Helper Methods for Camera Darkening Fix ---
        private static void ResetPostProcessingOverrides(GameObject sourceObject)
        {
            try
            {
                if (postProcessingType == null)
                {
                    postProcessingType = Type.GetType("PostProcessing, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                }
                if (postProcessingType == null) { DLog.Log("Could not find PostProcessing Type."); return; }
                var ppInstanceProperty = postProcessingType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (ppInstanceProperty == null) { DLog.Log("Could not find PostProcessing.Instance property."); return; }
                object ppInstance = ppInstanceProperty.GetValue(null);
                if (ppInstance != null)
                {
                    DLog.Log("Found PostProcessing instance. Attempting to cancel overrides...");
                    TryCallMethod(ppInstance, "VignetteOverride", Color.clear, 0f, 0f, 0f, 0f, 0f, sourceObject);
                    TryCallMethod(ppInstance, "SaturationOverride", 0f, 0f, 0f, 0f, sourceObject);
                    TryCallMethod(ppInstance, "ContrastOverride", 0f, 0f, 0f, 0f, sourceObject);
                    DLog.Log("Called PostProcessing override methods with default values.");
                }
                else { DLog.Log("PostProcessing instance is null."); }
            }
            catch (Exception e) { DLog.Log($"Error resetting PostProcessing: {e.Message}"); }
        }

        private static void ResetCameraZoomOverride(GameObject sourceObject)
        {
            try
            {
                if (cameraZoomType == null)
                {
                    cameraZoomType = Type.GetType("CameraZoom, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                }
                if (cameraZoomType == null) { DLog.Log("Could not find CameraZoom Type."); return; }
                var czInstanceProperty = cameraZoomType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (czInstanceProperty == null) { DLog.Log("Could not find CameraZoom.Instance property."); return; }
                object czInstance = czInstanceProperty.GetValue(null);
                if (czInstance != null)
                {
                    DLog.Log("Found CameraZoom instance. Attempting to cancel override...");
                    float defaultZoom = 60f;
                    var defaultZoomField = cameraZoomType.GetField("playerZoomDefault", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (defaultZoomField != null)
                    {
                        try { defaultZoom = (float)defaultZoomField.GetValue(czInstance); DLog.Log($"Using default zoom value: {defaultZoom}"); } catch { DLog.Log("Could not get default zoom value, using fallback 60f."); }
                    }
                    else { DLog.Log("Could not find 'playerZoomDefault' field, using fallback 60f."); }
                    TryCallMethod(czInstance, "OverrideZoomSet", defaultZoom, 0f, 0f, 0f, sourceObject, 200);
                    DLog.Log("Called CameraZoom override method with default values.");
                }
                else { DLog.Log("CameraZoom instance is null."); }
            }
            catch (Exception e) { DLog.Log($"Error resetting CameraZoom: {e.Message}"); }
        }

        private static bool TryCallMethod(object instance, string methodName, params object[] parameters)
        {
            if (instance == null) { DLog.Log($"Cannot call '{methodName}', instance is null."); return false; }
            try
            {
                var paramTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) { paramTypes[i] = parameters[i]?.GetType() ?? typeof(object); if (parameters[i] is GameObject) paramTypes[i] = typeof(GameObject); }
                var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);
                if (method == null) { DLog.Log($"Method '{methodName}' with specific args not found, trying by name only..."); method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); }
                if (method != null) { method.Invoke(instance, parameters); return true; }
                else { DLog.Log($"Method '{methodName}' not found on {instance.GetType().Name}."); return false; }
            }
            catch (Exception e)
            {
                DLog.Log($"Error calling method '{methodName}' on {instance.GetType().Name}: {e.GetType().Name} - {e.Message}");
                if (e is TargetParameterCountException) { DLog.Log("Parameter count mismatch. Check method signature."); }
                else if (e is ArgumentException) { DLog.Log("Argument type mismatch. Check parameter types."); }
                return false;
            }
        }
    }
}