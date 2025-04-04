using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace dark_cheat
{
    class PlayerController
    {

        public static object playerSpeedInstance;
        public static object reviveInstance;
        public static object enemyDirectorInstance;
        public static object playerControllerInstance;
        public static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

        private static float desiredDelayMultiplier = 1f;
        private static float desiredRateMultiplier = 1f;

        private static void InitializePlayerController()
        {
            if (playerControllerType == null)
            {
                DLog.Log("PlayerController type not found.");
                return;
            }

            playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
            if (playerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not found in current scene.");
            }
            else
            {
                DLog.Log("PlayerController instance updated successfully.");
            }
        }

        public static void GodMode()
        {
            if (PlayerReflectionCache.PlayerHealthInstance == null) // Ensure our cache is up-to-date.
            {
                DLog.Log("PlayerHealth instance not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }
            if (PlayerReflectionCache.PlayerHealthInstance != null)
            {
                var godModeField = PlayerReflectionCache.PlayerHealthInstance.GetType()
                    .GetField("godMode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (godModeField != null)
                {
                    bool currentGodMode = (bool)godModeField.GetValue(PlayerReflectionCache.PlayerHealthInstance);
                    bool newGodModeState = !currentGodMode;
                    godModeField.SetValue(PlayerReflectionCache.PlayerHealthInstance, newGodModeState);
                    Hax2.godModeActive = newGodModeState;
                    DLog.Log("God Mode " + (newGodModeState ? "enabled" : "disabled"));
                }
                else
                {
                    DLog.Log("godMode field not found.");
                }
            }
        }

        public static void SetSprintSpeed(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }
            if (PlayerReflectionCache.PlayerControllerInstance != null)
            {
                var sprintSpeedField = PlayerReflectionCache.PlayerControllerType.GetField("SprintSpeed", BindingFlags.Public | BindingFlags.Instance);
                if (sprintSpeedField != null)
                {
                    sprintSpeedField.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                    DLog.Log($"SprintSpeed set to {value}");
                }
                else
                {
                    DLog.Log("SprintSpeed field not found.");
                }
            }
        }

        public static void MaxHealth()
        {
            if (PlayerReflectionCache.PlayerHealthInstance == null) // Ensure the cache is populated.
            {
                DLog.Log("PlayerHealth instance not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }

            if (PlayerReflectionCache.PlayerHealthInstance == null)
            {
                DLog.Log("Unable to cache PlayerHealth instance.");
                return;
            }

            var updateHealthMethod = PlayerReflectionCache.PlayerHealthInstance.GetType() // Get the UpdateHealthRPC method from the cached playerHealth instance.
                                        .GetMethod("UpdateHealthRPC", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateHealthMethod != null)
            {
                if (Hax2.infiniteHealthActive)
                {
                    updateHealthMethod.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[] { 999999, 100, true });
                }
                else
                {
                    updateHealthMethod.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[] { 100, 100, true });
                }
                DLog.Log("Current health set to " + (Hax2.infiniteHealthActive ? "999999" : "100") + " using cached reflection data.");
            }
            else
            {
                DLog.Log("UpdateHealthRPC method not found in cached PlayerHealth instance.");
            }
        }

        public static void MaxStamina()
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null) // Ensure the cache is up-to-date.
            {
                DLog.Log("PlayerController instance not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("Unable to cache PlayerController instance.");
                return;
            }

            if (PlayerReflectionCache.EnergyCurrentField != null) // Check for the cached EnergyCurrent field.
            {
                int newStamina = Hax2.stamineState ? 999999 : 40;
                PlayerReflectionCache.EnergyCurrentField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newStamina);
                DLog.Log("EnergyCurrent set to " + newStamina);
            }
            else
            {
                DLog.Log("EnergyCurrent field not found in cache.");
            }
        }

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null) // Ensure the PlayerController instance is cached.
            {
                DLog.Log("PlayerController instance not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("Unable to cache PlayerController instance.");
                return;
            }

            desiredDelayMultiplier = delayMultiplier;
            desiredRateMultiplier = rateMultiplier;
            DLog.Log("Attempting to decrease stamina recharge delay.");

            FieldInfo sprintRechargeTimeField = PlayerReflectionCache.SprintRechargeTimeField;
            if (sprintRechargeTimeField != null) // Retrieve the cached sprintRechargeTime field.
            {
                float defaultRechargeTime = 1f;
                float newRechargeTime = defaultRechargeTime * delayMultiplier;
                sprintRechargeTimeField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newRechargeTime);
                DLog.Log($"sprintRechargeTime set to {newRechargeTime} (multiplier: {delayMultiplier})");
            }
            else
            {
                DLog.Log("sprintRechargeTime field not found in PlayerController.");
            }

            FieldInfo sprintRechargeAmountField = PlayerReflectionCache.SprintRechargeAmountField;
            if (sprintRechargeAmountField != null) // Retrieve the cached sprintRechargeAmount field.
            {
                float defaultRechargeAmount = 2f;
                float newRechargeAmount = defaultRechargeAmount * rateMultiplier;
                sprintRechargeAmountField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newRechargeAmount);
                DLog.Log($"sprintRechargeAmount set to {newRechargeAmount} (multiplier: {rateMultiplier})");
            }
            else
            {
                DLog.Log("sprintRechargeAmount field not found in PlayerController.");
            }
        }

        public static void ReapplyStaminaSettings()
        {
            InitializePlayerController();
            if (playerControllerInstance != null)
            {
                DecreaseStaminaRechargeDelay(desiredDelayMultiplier, desiredRateMultiplier);
                DLog.Log("Reapplied stamina settings after scene change.");
            }
        }

        public static void SetFlashlightIntensity(float value)
        {
            if (PlayerReflectionCache.FlashlightControllerInstance == null || PlayerReflectionCache.BaseIntensityField == null)
            {  // Ensure our cache is populated.
                DLog.Log("Flashlight controller data not cached. Caching now.");
                PlayerReflectionCache.CachePlayerControllerData();
            }

            if (PlayerReflectionCache.FlashlightControllerInstance != null && PlayerReflectionCache.BaseIntensityField != null)
            {
                PlayerReflectionCache.BaseIntensityField.SetValue(PlayerReflectionCache.FlashlightControllerInstance, value);
                DLog.Log($"Flashlight BaseIntensity set to {value}");
            }
            else
            {
                DLog.Log("Unable to set flashlight intensity. Cached data missing.");
            }
        }

        public static void SetCrouchDelay(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"CrouchTimeMin set to {value}");
            }
            else
            {
                DLog.Log("CrouchTimeMin field not found in PlayerController.");
            }
        }
        public static void SetCrouchSpeed(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CrouchSpeed", BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"CrouchSpeed set to {value}");
            }
            else
            {
                DLog.Log("CrouchSpeed field not found in PlayerController.");
            }
        }

        public static void SetJumpForce(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("JumpForce", BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"JumpForce set to {value}");
            }
            else
            {
                DLog.Log("JumpForce field not found in PlayerController.");
            }
        }

        public static void SetExtraJumps(int value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("JumpExtra", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"JumpExtra set to {value}");
            }
            else
            {
                DLog.Log("JumpExtra field not found in PlayerController.");
            }
        }

        public static void SetCustomGravity(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CustomGravity", BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"CustomGravity set to {value}");
            }
            else
            {
                DLog.Log("CustomGravity field not found in PlayerController.");
            }
        }

        public static void SetCrawlDelay(float crawlDelay)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null) // Ensure cache is updated.
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }

            FieldInfo crouchField = PlayerReflectionCache.CrouchTimeMinField;
            if (crouchField == null) // Use cached CrouchTimeMin field (if not cached, retrieve and cache it)
            {
                crouchField = PlayerReflectionCache.PlayerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
                PlayerReflectionCache.CrouchTimeMinField = crouchField;
            }
            if (crouchField != null)
            {
                crouchField.SetValue(PlayerReflectionCache.PlayerControllerInstance, crawlDelay);
                DLog.Log($"CrouchTimeMin set locally to {crawlDelay}");
            }
            else
            {
                DLog.Log("CrouchTimeMin field not found in PlayerController.");
                return;
            }

            PhotonView photonView = null;
            if (PlayerReflectionCache.PhotonViewField != null)
            {
                photonView = (PhotonView)PlayerReflectionCache.PhotonViewField.GetValue(PlayerReflectionCache.PlayerControllerInstance);
            }
            if (photonView == null)
            {
                var avatarPhotonField = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (avatarPhotonField != null) // If not found on the controller, try retrieving it from the cached playerAvatarScript.
                {
                    photonView = (PhotonView)avatarPhotonField.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
                }
            }

            if (photonView != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("SetCrawlDelayRPC", RpcTarget.AllBuffered, crawlDelay);
                    DLog.Log($"Master Client set crawl delay to {crawlDelay} and synced via RPC.");
                }
                else
                {
                    photonView.RPC("SetCrawlDelayRPC", RpcTarget.MasterClient, crawlDelay);
                    DLog.Log($"Requested Master Client to set crawl delay to {crawlDelay} via RPC.");
                }
            }
            else
            {
                DLog.Log("PhotonView not found for crawl delay synchronization.");
            }
        }

        public static void SetGrabRange(float value)
        {
            if (PlayerReflectionCache.PlayerAvatarScriptInstance == null)
            {
                DLog.Log("PlayerAvatarScript instance not cached.");
                return;
            }

            FieldInfo physGrabberField = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance);
            if (physGrabberField == null) // Retrieve and (optionally) cache the physGrabber field from playerAvatarScript.
            {
                DLog.Log("physGrabber field not found in playerAvatarScript.");
                return;
            }
            var physGrabber = physGrabberField.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
            if (physGrabber == null)
            {
                DLog.Log("physGrabber instance is null.");
                return;
            }
            FieldInfo grabRangeField = physGrabber.GetType().GetField("grabRange", BindingFlags.Public | BindingFlags.Instance);
            if (grabRangeField != null)
            {
                grabRangeField.SetValue(physGrabber, value);
                DLog.Log($"GrabRange set to {value}");
            }
            else
            {
                DLog.Log("GrabRange field not found in physGrabber.");
            }
        }

        public static void SetThrowStrength(float value)
        {
            if (PlayerReflectionCache.PlayerAvatarScriptInstance == null)
            {
                DLog.Log("PlayerAvatarScript instance not cached.");
                return;
            }
            FieldInfo physGrabberField = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance);
            if (physGrabberField == null)
            {
                DLog.Log("physGrabber field not found in playerAvatarScript.");
                return;
            }
            var physGrabber = physGrabberField.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
            if (physGrabber == null)
            {
                DLog.Log("physGrabber instance is null.");
                return;
            }
            FieldInfo throwStrengthField = physGrabber.GetType().GetField("throwStrength", BindingFlags.Public | BindingFlags.Instance);
            if (throwStrengthField != null)
            {
                throwStrengthField.SetValue(physGrabber, value);
                DLog.Log($"ThrowStrength set to {value}");
            }
            else
            {
                DLog.Log("ThrowStrength field not found in physGrabber.");
            }
        }

        public static void SetSlideDecay(float value)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("Optimized SetSlideDecay: PlayerController instance not cached.");
                return;
            }

            FieldInfo slideDecayField = PlayerReflectionCache.PlayerControllerType.GetField("SlideDecay", BindingFlags.Public | BindingFlags.Instance);
            if (slideDecayField != null) // Retrieve the SlideDecay field from the cached PlayerControllerType.
            {
                slideDecayField.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
                DLog.Log($"SlideDecay set to {value}");
            }
            else
            {
                DLog.Log("SlideDecay field not found in PlayerController.");
            }
        }

        public static int GetCurrentMaxHealth()
        {
            if (PlayerReflectionCache.MaxHealthField == null)
            {
                DLog.Log("maxHealth field not cached.");
                return 0;
            }
            try
            {
                int maxHealth = (int)PlayerReflectionCache.MaxHealthField.GetValue(PlayerReflectionCache.PlayerHealthInstance);
                DLog.Log("Current maxHealth is " + maxHealth);
                return maxHealth;
            }
            catch (Exception ex)
            {
                DLog.Log("Exception retrieving maxHealth: " + ex.Message);
                return 0;
            }
        }

        public static void SetMaxHealth(int newMaxHealth)
        {
            if (PlayerReflectionCache.PlayerHealthInstance == null)
            {
                DLog.Log("PlayerHealth instance not cached.");
                return;
            }
            var updateHealthMethod = PlayerReflectionCache.PlayerHealthInstance.GetType().GetMethod("UpdateHealthRPC", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateHealthMethod != null)
            {
                // Set both current and max health to newMaxHealth.
                updateHealthMethod.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[] { newMaxHealth, newMaxHealth, true });
                DLog.Log($"Maximum health updated to {newMaxHealth} (current health set to {newMaxHealth}).");
            }
            else
            {
                DLog.Log("'UpdateHealthRPC' method not found in playerHealth.");
            }
        }

        public static float GetCurrentMaxStamina()
        {
            if (PlayerReflectionCache.EnergyStartField == null)
            {
                DLog.Log("EnergyStart field not cached.");
                return 0f;
            }
            try
            {
                float maxStamina = (float)PlayerReflectionCache.EnergyStartField.GetValue(PlayerReflectionCache.PlayerControllerInstance);
                DLog.Log("Current EnergyStart is " + maxStamina);
                return maxStamina;
            }
            catch (Exception ex)
            {
                DLog.Log("Exception retrieving EnergyStart: " + ex.Message);
                return 0f;
            }
        }

        public static void SetMaxStamina(float newMaxStamina)
        {
            if (PlayerReflectionCache.PlayerControllerInstance == null)
            {
                DLog.Log("PlayerController instance not cached.");
                return;
            }
            if (PlayerReflectionCache.EnergyStartField != null)
            {
                PlayerReflectionCache.EnergyStartField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newMaxStamina);
                DLog.Log($"EnergyStart updated to {newMaxStamina}.");
            }
            else
            {
                DLog.Log("EnergyStart field not found.");
            }
            if (PlayerReflectionCache.EnergyCurrentField != null)
            {
                PlayerReflectionCache.EnergyCurrentField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newMaxStamina);
                DLog.Log($"EnergyCurrent updated to {newMaxStamina}.");
            }
            else
            {
                DLog.Log("EnergyCurrent field not found.");
            }
        }


        public static void ReapplyAllStats()
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            DLog.Log("Reapplying all custom stats after level load...");

            string steamID = PhotonNetwork.LocalPlayer.UserId;

            int cheatMaxHealth = 100;

            int currentMaxHealth = GetCurrentMaxHealth(); // Get the player's current in-game maxHealth from their health component.

            // Log the determined values for debugging.
            DLog.Log($"Final Max Health determined: {Mathf.Max(cheatMaxHealth, currentMaxHealth)} (cheat: {cheatMaxHealth}, current: {currentMaxHealth})");

            int finalMaxHealth = Mathf.Max(cheatMaxHealth, currentMaxHealth); // Use the higher of the cheat value and the current in-game maxHealth.

            SetMaxHealth(finalMaxHealth); // Apply the new max health.

            float cheatMaxStamina = 40f;
            float currentMaxStamina = GetCurrentMaxStamina(); // Get the current in-game max stamina.
            DLog.Log($"Final Max Stamina determined: {Mathf.Max(cheatMaxStamina, currentMaxStamina)} (cheat: {cheatMaxStamina}, current: {currentMaxStamina})");
            float finalMaxStamina = Mathf.Max(cheatMaxStamina, currentMaxStamina); // Use the higher value.
            SetMaxStamina(finalMaxStamina); // Apply the new max stamina.

            SetSprintSpeed(5);   
            Strength.MaxStrength();      
            MaxStamina();        
            ReapplyStaminaSettings();
            SetThrowStrength(Hax2.throwStrength);
            SetGrabRange(5);
            SetCrouchDelay(Hax2.crouchDelay);
            SetCrouchSpeed(1);
            SetJumpForce(17);
            SetExtraJumps(0);
            SetCustomGravity(30);
            SetSlideDecay(Hax2.slideDecay);
            SetFlashlightIntensity(Hax2.flashlightIntensity);
            if (FOVEditor.Instance != null)
            {
                FOVEditor.Instance.SetFOV(Hax2.fieldOfView);
            }

            DLog.Log("Finished reapplying all custom stat modifications.");
        }

        public static void LoadDefaultStatsIntoHax2()
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            DLog.Log("Loading default stats from PlayerController into Hax2");

            var type = playerControllerType;

            float GetFloat(string fieldName, float fallback)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object value = field.GetValue(playerControllerInstance);
                    if (value is float f) return f;
                }
                return fallback;
            }

            int GetInt(string fieldName, int fallback)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object value = field.GetValue(playerControllerInstance);
                    if (value is int i) return i;
                }
                return fallback;
            }

            //Hax2.sliderValue = GetFloat("SprintSpeed", 0f);
            //Hax2.sliderValueStrength = GetFloat("grabStrength", 1f);
            //Hax2.throwStrength = GetFloat("throwStrength", 1f);
            //Hax2.grabRange = GetFloat("grabRange", 5f);
            //Hax2.jumpForce = GetFloat("JumpForce", 0f);
            Hax2.crouchSpeed = GetFloat("CrouchSpeed", 1f);
            Hax2.crouchDelay = GetFloat("CrouchTimeMin", 0f);
            //Hax2.customGravity = GetFloat("CustomGravity", 0f);
            Hax2.extraJumps = GetInt("JumpExtra", 0); // oops - we cant set this 0 as int
            Hax2.flashlightIntensity = 1f;

            Hax2.fieldOfView = 70f;

            Hax2.oldSliderValue = Hax2.sliderValue;
            Hax2.oldSliderValueStrength = Hax2.sliderValueStrength;

            DLog.Log("Default stat values loaded into Hax2.");
        }

        public static void SetFieldOfView(float fov)
        {
            FOVEditor editor = GameObject.FindObjectOfType<FOVEditor>();
            if (editor == null)
            {
                GameObject obj = new GameObject("FOVEditor");
                editor = obj.AddComponent<FOVEditor>();
                GameObject.DontDestroyOnLoad(obj);
            }
            editor.SetFOV(fov);
        }

        public static string GetLocalPlayerSteamID()
        {
            int localActorID = PhotonNetwork.LocalPlayer.ActorNumber;

            var playerObjs = SemiFunc.PlayerGetList();
            if (playerObjs == null)
            {
                Debug.LogWarning("Player list is null!");
                return "";
            }

            foreach (var playerObj in playerObjs)
            {
                if (playerObj == null)
                    continue;

                try
                {
                    var photonViewField = playerObj.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    PhotonView view = photonViewField != null ? photonViewField.GetValue(playerObj) as PhotonView : null;
                    if (view == null)
                        continue;
                    if (view.OwnerActorNr == localActorID)
                    {
                        var steamIdField = playerObj.GetType().GetField("steamID", BindingFlags.NonPublic | BindingFlags.Instance);
                        string steamID = steamIdField != null ? steamIdField.GetValue(playerObj) as string : "";
                        return steamID;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error retrieving SteamID from player object: {e.Message}");
                }
            }
            return "";
        }
    }
}
