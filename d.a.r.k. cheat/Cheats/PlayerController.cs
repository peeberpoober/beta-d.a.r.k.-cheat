using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

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
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                DLog.Log("PlayerController found.");

                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);

                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);

                            var godModeField = playerHealthInstance.GetType().GetField("godMode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (godModeField != null)
                            {
                                bool currentGodMode = (bool)godModeField.GetValue(playerHealthInstance);

                                bool newGodModeState = !currentGodMode;
                                godModeField.SetValue(playerHealthInstance, newGodModeState);

                                Hax2.godModeActive = !newGodModeState;

                                DLog.Log("God Mode " + (newGodModeState ? "enabled" : "disabled"));
                            }
                            else
                            {
                                DLog.Log("godMode field not found in playerHealth.");
                            }
                        }
                        else
                        {
                            DLog.Log("playerHealth field not found in playerAvatarScript.");
                        }
                    }
                    else
                    {
                        DLog.Log("playerAvatarScript field not found in PlayerController.");
                    }
                }
                else
                {
                    DLog.Log("playerControllerInstance not found.");
                }
            }
            else
            {
                DLog.Log("PlayerController type not found.");
            }
        }

        public static void SetSprintSpeed(float value)
        {
            var playerInSpeedType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerInSpeedType != null)
            {
                DLog.Log("playerInSpeedType not null");
                playerSpeedInstance = GameHelper.FindObjectOfType(playerInSpeedType);
                if (playerSpeedInstance != null)
                {
                    DLog.Log("playerSpeedInstance not null");
                }
                else
                {
                    DLog.Log("playerSpeedInstance is null");
                }
            }
            else
            {
                DLog.Log("playerInSpeedType is null");
            }

            if (playerSpeedInstance != null)
            {
                DLog.Log("playerSpeedInstance not null");

                var playerControllerType = playerSpeedInstance.GetType();

                var sprintSpeedField = playerControllerType.GetField("SprintSpeed", BindingFlags.Public | BindingFlags.Instance);

                if (sprintSpeedField != null)
                {
                    sprintSpeedField.SetValue(playerSpeedInstance, value);
                    DLog.Log("SprintSpeed value set to " + value);
                }
                else
                {
                    DLog.Log("SprintSpeed field not found in PlayerController.");
                }
            }
        }

        public static void MaxHealth()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                DLog.Log("PlayerController found.");
                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var playerAvatarScriptField = playerControllerInstance.GetType().GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        var playerHealthField = playerAvatarScriptInstance.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.Instance);
                        if (playerHealthField != null)
                        {
                            var playerHealthInstance = playerHealthField.GetValue(playerAvatarScriptInstance);
                            var damageMethod = playerHealthInstance.GetType().GetMethod("UpdateHealthRPC");
                            if (damageMethod != null)
                            {
                                if (Hax2.infiniteHealthActive)
                                {
                                    damageMethod.Invoke(playerHealthInstance, new object[] { 999999, 100, true });
                                }
                                else if (!Hax2.infiniteHealthActive)
                                {
                                    damageMethod.Invoke(playerHealthInstance, new object[] { 100, 100, true });
                                }
                                DLog.Log("Maximum health adjusted to 999999.");
                            }
                            else
                            {
                                DLog.Log("'UpdateHealthRPC' method not found in playerHealth.");
                            }
                        }
                        else
                        {
                            DLog.Log("'playerHealth' field not found in playerAvatarScript.");
                        }
                    }
                    else
                    {
                        DLog.Log("'playerAvatarScript' field not found in PlayerController.");
                    }
                }
                else
                {
                    DLog.Log("playerControllerInstance not found.");
                }
            }
            else
            {
                DLog.Log("PlayerController type not found.");
            }
        }

        public static void MaxStamina()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType != null)
            {
                DLog.Log("PlayerController found.");

                var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
                if (playerControllerInstance != null)
                {
                    var energyCurrentField = playerControllerInstance.GetType().GetField("EnergyCurrent", BindingFlags.Public | BindingFlags.Instance);
                    if (energyCurrentField != null)
                    {
                        if (Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 999999);
                        }
                        else if (!Hax2.stamineState)
                        {
                            energyCurrentField.SetValue(playerControllerInstance, 40);
                        }

                        DLog.Log("EnergyCurrent set to " + (Hax2.stamineState ? 999999 : 40));
                    }
                    else
                    {
                        DLog.Log("EnergyCurrent field not found in playerAvatarScript.");
                    }
                }
                else
                {
                    DLog.Log("playerControllerInstance not found.");
                }
            }
            else
            {
                DLog.Log("PlayerController type not found.");
            }
        }

        public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            desiredDelayMultiplier = delayMultiplier;
            desiredRateMultiplier = rateMultiplier;

            DLog.Log("Attempting to decrease stamina recharge delay.");

            var sprintRechargeTimeField = playerControllerType.GetField("sprintRechargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeTimeField != null)
            {
                float defaultRechargeTime = 1f;
                float newRechargeTime = defaultRechargeTime * delayMultiplier;
                sprintRechargeTimeField.SetValue(playerControllerInstance, newRechargeTime);
                DLog.Log($"sprintRechargeTime set to {newRechargeTime} (multiplier: {delayMultiplier})");
            }
            else
            {
                DLog.Log("sprintRechargeTime field not found in PlayerController.");
            }

            var sprintRechargeAmountField = playerControllerType.GetField("sprintRechargeAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sprintRechargeAmountField != null)
            {
                float defaultRechargeAmount = 2f;
                float newRechargeAmount = defaultRechargeAmount * rateMultiplier;
                sprintRechargeAmountField.SetValue(playerControllerInstance, newRechargeAmount);
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
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
            if (playerAvatarScriptField != null)
            {
                var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                if (playerAvatarScriptInstance != null)
                {
                    var flashlightControllerField = playerAvatarScriptInstance.GetType().GetField("flashlightController", BindingFlags.Public | BindingFlags.Instance);
                    if (flashlightControllerField != null)
                    {
                        var flashlightControllerInstance = flashlightControllerField.GetValue(playerAvatarScriptInstance);
                        if (flashlightControllerInstance != null)
                        {
                            var baseIntensityField = flashlightControllerInstance.GetType().GetField("baseIntensity", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (baseIntensityField != null)
                            {
                                baseIntensityField.SetValue(flashlightControllerInstance, value);
                                DLog.Log($"Flashlight BaseIntensity set to {value}");
                            }
                        }
                    }
                }
            }
        }

        public static void SetCrouchDelay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                DLog.Log($"CrouchTimeMin set to {value}");
            }
            else
            {
                DLog.Log("CrouchTimeMin field not found in PlayerController.");
            }
        }
        public static void SetCrouchSpeed(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var crouchTimeMinField = playerControllerType.GetField("CrouchSpeed", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, value);
                DLog.Log($"CrouchSpeed set to {value}");
            }
            else
            {
                DLog.Log("CrouchSpeed field not found in PlayerController.");
            }
        }

        public static void SetJumpForce(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpForceField = playerControllerType.GetField("JumpForce", BindingFlags.Public | BindingFlags.Instance);
            if (jumpForceField != null)
            {
                jumpForceField.SetValue(playerControllerInstance, value);
                DLog.Log($"JumpForce set to {value}");
            }
            else
            {
                DLog.Log("JumpForce field not found in PlayerController.");
            }
        }

        public static void SetExtraJumps(int value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var jumpExtraField = playerControllerType.GetField("JumpExtra", BindingFlags.NonPublic | BindingFlags.Instance);
            if (jumpExtraField != null)
            {
                jumpExtraField.SetValue(playerControllerInstance, value);
                DLog.Log($"JumpExtra set to {value}");
            }
            else
            {
                DLog.Log("JumpExtra field not found in PlayerController.");
            }
        }

        public static void SetCustomGravity(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var customGravityField = playerControllerType.GetField("CustomGravity", BindingFlags.Public | BindingFlags.Instance);
            if (customGravityField != null)
            {
                customGravityField.SetValue(playerControllerInstance, value);
                DLog.Log($"CustomGravity set to {value}");
            }
            else
            {
                DLog.Log("CustomGravity field not found in PlayerController.");
            }
        }

        public static void SetCrawlDelay(float crawlDelay)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            // Aplicar localmente
            var crouchTimeMinField = playerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (crouchTimeMinField != null)
            {
                crouchTimeMinField.SetValue(playerControllerInstance, crawlDelay);
                DLog.Log($"CrouchTimeMin set locally to {crawlDelay}");
            }
            else
            {
                DLog.Log("CrouchTimeMin field not found in PlayerController.");
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                var photonViewField = playerControllerType.GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                PhotonView photonView = photonViewField != null ? (PhotonView)photonViewField.GetValue(playerControllerInstance) : null;

                if (photonView == null)
                {
                    var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
                    if (playerAvatarScriptField != null)
                    {
                        var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
                        photonView = playerAvatarScriptInstance?.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(playerAvatarScriptInstance) as PhotonView;
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
            else
            {
                DLog.Log("Not connected to Photon, crawl delay applied only locally.");
            }
        }

        public static void SetGrabRange(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScript = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerControllerInstance);
            if (playerAvatarScript == null) return;

            var physGrabber = playerAvatarScript.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerAvatarScript);
            if (physGrabber == null) return;

            var grabRangeField = physGrabber.GetType().GetField("grabRange", BindingFlags.Public | BindingFlags.Instance);
            if (grabRangeField != null)
            {
                grabRangeField.SetValue(physGrabber, value);
                DLog.Log($"GrabRange set to {value}");
            }
            else
            {
                DLog.Log("GrabRange field not found in PhysGrabber.");
            }
        }

        public static void SetThrowStrength(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var playerAvatarScript = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerControllerInstance);
            if (playerAvatarScript == null) return;

            var physGrabber = playerAvatarScript.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance)?.GetValue(playerAvatarScript);
            if (physGrabber == null) return;

            var throwStrengthField = physGrabber.GetType().GetField("throwStrength", BindingFlags.Public | BindingFlags.Instance);
            if (throwStrengthField != null)
            {
                throwStrengthField.SetValue(physGrabber, value);
                DLog.Log($"ThrowStrength set to {value}");
            }
            else
            {
                DLog.Log("ThrowStrength field not found in PhysGrabber.");
            }
        }

        public static void SetSlideDecay(float value)
        {
            InitializePlayerController();
            if (playerControllerInstance == null) return;

            var slideDecayField = playerControllerType.GetField("SlideDecay", BindingFlags.Public | BindingFlags.Instance);
            if (slideDecayField != null)
            {
                slideDecayField.SetValue(playerControllerInstance, value);
                DLog.Log($"SlideDecay set to {value}");
            }
            else
            {
                DLog.Log("SlideDecay field not found in PlayerController.");
            }
        }
    }
}
