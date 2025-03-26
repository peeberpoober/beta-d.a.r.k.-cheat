using System;
using System.Reflection;
using UnityEngine;

namespace dark_cheat
{
    public static class PlayerReflectionCache
    {
        // Cached types and instances
        public static Type PlayerControllerType { get; private set; }
        public static object PlayerControllerInstance { get; private set; }
        public static FieldInfo PlayerAvatarScriptField { get; private set; }
        public static object PlayerAvatarScriptInstance { get; private set; }
        public static FieldInfo PlayerHealthField { get; private set; }
        public static object PlayerHealthInstance { get; private set; }
        public static FieldInfo MaxHealthField { get; private set; }
        public static FieldInfo EnergyStartField { get; private set; }
        public static FieldInfo EnergyCurrentField { get; private set; }
        public static FieldInfo FlashlightControllerField { get; private set; }
        public static object FlashlightControllerInstance { get; private set; }
        public static FieldInfo BaseIntensityField { get; private set; }
        public static FieldInfo CrouchTimeMinField { get; set; }
        public static FieldInfo PhotonViewField { get; set; }
        public static FieldInfo SprintRechargeTimeField { get; private set; }
        public static FieldInfo SprintRechargeAmountField { get; private set; }

        public static void CachePlayerControllerData()
        {
            PlayerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (PlayerControllerType == null) // Get the PlayerController type from the game assembly.
            {
                DLog.Log("PlayerReflectionCache: PlayerController type not found.");
                return;
            }

            PlayerControllerInstance = GameHelper.FindObjectOfType(PlayerControllerType);
            if (PlayerControllerInstance == null) // Locate the instance in the scene.
            {
                DLog.Log("PlayerReflectionCache: PlayerController instance not found.");
                return;
            }

            PlayerAvatarScriptField = PlayerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (PlayerAvatarScriptField == null) // Get the playerAvatarScript field (it might be public or non-public).
            {
                DLog.Log("PlayerReflectionCache: playerAvatarScript field not found.");
                return;
            }
            PlayerAvatarScriptInstance = PlayerAvatarScriptField.GetValue(PlayerControllerInstance);
            if (PlayerAvatarScriptInstance == null)
            {
                DLog.Log("PlayerReflectionCache: playerAvatarScript instance is null.");
                return;
            }

            PlayerHealthField = PlayerAvatarScriptInstance.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (PlayerHealthField == null) // Get the playerHealth field from the playerAvatarScript instance.
            {
                DLog.Log("PlayerReflectionCache: playerHealth field not found.");
                return;
            }
            PlayerHealthInstance = PlayerHealthField.GetValue(PlayerAvatarScriptInstance);
            if (PlayerHealthInstance == null)
            {
                DLog.Log("PlayerReflectionCache: playerHealth instance is null.");
                return;
            }

            MaxHealthField = PlayerHealthInstance.GetType().GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (MaxHealthField == null) // Cache the maxHealth field from the playerHealth instance (it might be non-public).
            {
                DLog.Log("PlayerReflectionCache: maxHealth field not found.");
            }

            EnergyStartField = PlayerControllerType.GetField("EnergyStart", BindingFlags.Public | BindingFlags.Instance);
            if (EnergyStartField == null) // Cache EnergyStart and EnergyCurrent from PlayerController.
            {
                DLog.Log("PlayerReflectionCache: EnergyStart field not found.");
            }
            EnergyCurrentField = PlayerControllerType.GetField("EnergyCurrent", BindingFlags.Public | BindingFlags.Instance);
            if (EnergyCurrentField == null)
            {
                DLog.Log("PlayerReflectionCache: EnergyCurrent field not found.");
            }

            DLog.Log("PlayerReflectionCache: Caching complete.");

            CrouchTimeMinField = PlayerControllerType.GetField("CrouchTimeMin", BindingFlags.Public | BindingFlags.Instance);
            if (CrouchTimeMinField == null)
            {
                DLog.Log("PlayerReflectionCache: CrouchTimeMin field not found.");
            }

            PhotonViewField = PlayerControllerType.GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (PhotonViewField == null)
            {
                DLog.Log("PlayerReflectionCache: PhotonView field not found.");
            }

            FlashlightControllerField = PlayerAvatarScriptInstance.GetType().GetField("flashlightController", BindingFlags.Public | BindingFlags.Instance);
            if (FlashlightControllerField != null) // Cache flashlightController from playerAvatarScript
            {
                FlashlightControllerInstance = FlashlightControllerField.GetValue(PlayerAvatarScriptInstance);
                if (FlashlightControllerInstance != null)
                {
                    BaseIntensityField = FlashlightControllerInstance.GetType().GetField("baseIntensity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (BaseIntensityField == null) // Cache the baseIntensity field (usually non-public)
                    {
                        DLog.Log("PlayerReflectionCache: baseIntensity field not found in flashlightController.");
                    }
                }
                else
                {
                    DLog.Log("PlayerReflectionCache: flashlightController instance is null.");
                }
            }
            else
            {
                DLog.Log("PlayerReflectionCache: flashlightController field not found in playerAvatarScript.");
            }

            SprintRechargeTimeField = PlayerControllerType.GetField("sprintRechargeTime", BindingFlags.NonPublic | BindingFlags.Instance);
            SprintRechargeAmountField = PlayerControllerType.GetField("sprintRechargeAmount", BindingFlags.NonPublic | BindingFlags.Instance);

            DLog.Log("PlayerReflectionCache: Caching complete.");
        }
    }
}
