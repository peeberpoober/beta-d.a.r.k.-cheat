using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace dark_cheat
{
    public static class Patches
    {
        [HarmonyPatch(typeof(SpectateCamera), "PlayerSwitch")]
        public static class SpectateCamera_PlayerSwitch_Patch
        {
            static bool Prefix(bool _next)
            {
                return !Hax2.showMenu;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButtonUp", new[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonUp
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu) { __result = false; return false; }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButtonDown", new[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonDown
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu) { __result = false; return false; }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButton", new[] { typeof(int) })]
        public class Patch_Input_GetMouseButton
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu) { __result = false; return false; }
                return true;
            }
        }

        //Patch interaction with items, doors, players when menu is open
        [HarmonyPatch(typeof(PhysGrabber), "RayCheck")]
        public class Patch_PhysGrabber_RayCheck
        {
            static bool Prefix(bool _grab)
            {
                if (Hax2.showMenu)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerAvatar), "AddToStatsManager")]
        public class Patch_AddToStatsManager
        {
            static bool Prefix(PlayerAvatar __instance)
            {
                if (Hax2.spoofNameActive)
                {
                    string fakeName = Hax2.persistentNameText;
                    string fakeSteamId = "765611472644157498";

                    if (GameManager.Multiplayer())
                    {
                        if (__instance.photonView.IsMine)
                        {
                            __instance.photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, new object[] { fakeName, fakeSteamId });
                            return false;
                        }
                    }
                    else
                    {
                        __instance.AddToStatsManagerRPC(fakeName, fakeSteamId);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InputManager), nameof(InputManager.KeyDown))]
        public class BlockChatKey
        {
            static bool Prefix(InputKey key, ref bool __result)
            {
                if (key == InputKey.Chat && Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(EnemyVision), nameof(EnemyVision.VisionTrigger))]
        public static class EnemyVision_BlindEnemies_Patch
        {
            static bool Prefix(PlayerAvatar player)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    return true;
                }

                if (player != null && player.photonView != null && player.photonView.Owner != null)
                {
                    Photon.Realtime.Player ownerPlayer = player.photonView.Owner;

                    if (ownerPlayer.CustomProperties.TryGetValue("isBlindEnabled", out object isBlindEnabledObj) && isBlindEnabledObj is bool && (bool)isBlindEnabledObj)
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPatch(typeof(EnemyThinMan), "SetTarget", new Type[] { typeof(PlayerAvatar) })]
            public static class EnemyThinMan_SetTarget_Patch
            {
                static bool Prefix(EnemyThinMan __instance, PlayerAvatar _player)
                {
                    PhotonView enemyView = __instance?.GetComponent<PhotonView>();

                    if (!PhotonNetwork.IsMasterClient && (enemyView == null || !enemyView.IsMine))
                    {
                        return true;
                    }

                    if (_player != null && _player.photonView != null && _player.photonView.Owner != null)
                    {
                        Photon.Realtime.Player ownerPlayer = _player.photonView.Owner;

                        if (ownerPlayer.CustomProperties.TryGetValue("isBlindEnabled", out object isBlindEnabledObj) && isBlindEnabledObj is bool && (bool)isBlindEnabledObj)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(EnemyStateInvestigate), nameof(EnemyStateInvestigate.SetRPC))]
            public static class EnemyStateInvestigate_BlindAudio_Patch
            {
                private const float LOCAL_PLAYER_SOUND_THRESHOLD = 1.5f;

                static bool Prefix(Vector3 position)
                {
                    if (!Hax2.blindEnemies)
                    {
                        return true;
                    }

                    PlayerAvatar localPlayerAvatar = null;
                    GameObject localPlayerGO = null;
                    try
                    {
                        localPlayerGO = DebugCheats.GetLocalPlayer();
                        if (localPlayerGO != null)
                        {
                            localPlayerAvatar = localPlayerGO.GetComponent<PlayerAvatar>();
                        }
                    }
                    catch (System.Exception)
                    {

                    }

                    if (localPlayerAvatar == null && GameDirector.instance != null && GameDirector.instance.PlayerList != null)
                    {
                        try
                        {
                            localPlayerAvatar = GameDirector.instance.PlayerList.FirstOrDefault(p => p != null && p.photonView != null && p.photonView.IsMine);
                        }
                        catch (System.Exception)
                        {

                        }
                    }

                    if (localPlayerAvatar == null)
                    {
                        return true;
                    }


                    Vector3 localPlayerPos = localPlayerAvatar.transform.position;

                    if (Vector3.Distance(position, localPlayerPos) < LOCAL_PLAYER_SOUND_THRESHOLD)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(ItemGun), "ShootRPC")]
        public class NoWeaponRecoil
        {
            public static bool _isEnabledForConfig = false;
            private static float local_originalGrabStrengthMultiplier = -1f;
            private static float local_originalTorqueMultiplier = -1f;
            private static FieldInfo _photonViewField = AccessTools.Field(typeof(ItemGun), "photonView");
            private static FieldInfo _physGrabObjectField = AccessTools.Field(typeof(ItemGun), "physGrabObject");
            private static FieldInfo _grabStrengthMultiplierField = AccessTools.Field(typeof(ItemGun), "grabStrengthMultiplier");
            private static FieldInfo _torqueMultiplierField = AccessTools.Field(typeof(ItemGun), "torqueMultiplier");

            [HarmonyPrefix]
            public static bool Prefix(ItemGun __instance)
            {
                local_originalGrabStrengthMultiplier = -1f;
                local_originalTorqueMultiplier = -1f;

                bool isMine = false;
                try
                {
                    if (__instance != null && _photonViewField != null)
                    {
                        PhotonView pv = _photonViewField.GetValue(__instance) as PhotonView;
                        if (pv != null)
                            isMine = pv.IsMine;
                    }
                }
                catch (System.Exception ex)
                {
                }

                bool isCurrentlyEnabled = _isEnabledForConfig;
                if (!isCurrentlyEnabled || !isMine)
                    return true;

                try
                {
                    if (_grabStrengthMultiplierField != null)
                        local_originalGrabStrengthMultiplier = (float)_grabStrengthMultiplierField.GetValue(__instance);
                    if (_torqueMultiplierField != null)
                        local_originalTorqueMultiplier = (float)_torqueMultiplierField.GetValue(__instance);

                    float weakMultiplier = 0.01f;
                    if (_grabStrengthMultiplierField != null)
                        _grabStrengthMultiplierField.SetValue(__instance, weakMultiplier);
                    if (_torqueMultiplierField != null)
                        _torqueMultiplierField.SetValue(__instance, weakMultiplier);

                    __instance.gunRecoilForce = 0f;
                    __instance.cameraShakeMultiplier = 0f;

                    if (_physGrabObjectField != null)
                    {
                        PhysGrabObject gunPhysGrabObject = _physGrabObjectField.GetValue(__instance) as PhysGrabObject;
                        if (gunPhysGrabObject != null)
                        {
                            float overrideDuration = 0.05f;
                            float weakStrength = 0.1f;
                            gunPhysGrabObject.OverrideGrabStrength(weakStrength, overrideDuration);
                            gunPhysGrabObject.OverrideTorqueStrength(weakStrength, overrideDuration);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                }

                return true;
            }

            [HarmonyPostfix]
            public static void Postfix(ItemGun __instance)
            {
                bool isMine = false;
                try
                {
                    if (__instance != null && _photonViewField != null)
                    {
                        PhotonView pv = _photonViewField.GetValue(__instance) as PhotonView;
                        if (pv != null)
                            isMine = pv.IsMine;
                    }
                }
                catch (System.Exception ex)
                {
                }

                if (!isMine)
                    return;

                try
                {
                    if (local_originalGrabStrengthMultiplier != -1f && _grabStrengthMultiplierField != null)
                        _grabStrengthMultiplierField.SetValue(__instance, local_originalGrabStrengthMultiplier);
                    if (local_originalTorqueMultiplier != -1f && _torqueMultiplierField != null)
                        _torqueMultiplierField.SetValue(__instance, local_originalTorqueMultiplier);
                }
                catch (System.Exception ex)
                {
                }

                local_originalGrabStrengthMultiplier = -1f;
                local_originalTorqueMultiplier = -1f;
            }
        }

        private static class ReflectionCache
        {
            public static FieldInfo maskField;
            public static FieldInfo playerCameraField;
            public static FieldInfo grabbedObjectField;
            public static FieldInfo grabbedObjectTransformField;
            public static FieldInfo localGrabPositionField;
            public static FieldInfo physGrabPointField;
            public static FieldInfo physGrabPointPullerField;
            public static FieldInfo physGrabPointPlaneField;
            public static FieldInfo grabDisableTimerField;
            public static FieldInfo cameraRelativeGrabbedForwardField;
            public static FieldInfo cameraRelativeGrabbedUpField;
            public static FieldInfo cameraRelativeGrabbedRightField;
            public static FieldInfo initialPressTimerField;
            public static FieldInfo physRotatingTimerField;
            public static FieldInfo prevGrabbedField;
            public static FieldInfo grabbedField;

            public static MethodInfo physGrabPointActivateMethod;

            public static bool initialized = false;

            public static void Initialize()
            {
                if (initialized) return;
                Type physGrabberType = typeof(PhysGrabber);
                maskField = AccessTools.Field(physGrabberType, "mask");
                playerCameraField = AccessTools.Field(physGrabberType, "playerCamera");
                grabbedObjectField = AccessTools.Field(physGrabberType, "grabbedObject");
                grabbedObjectTransformField = AccessTools.Field(physGrabberType, "grabbedObjectTransform");
                localGrabPositionField = AccessTools.Field(physGrabberType, "localGrabPosition");
                physGrabPointField = AccessTools.Field(physGrabberType, "physGrabPoint");
                physGrabPointPullerField = AccessTools.Field(physGrabberType, "physGrabPointPuller");
                physGrabPointPlaneField = AccessTools.Field(physGrabberType, "physGrabPointPlane");
                grabDisableTimerField = AccessTools.Field(physGrabberType, "grabDisableTimer");
                cameraRelativeGrabbedForwardField = AccessTools.Field(physGrabberType, "cameraRelativeGrabbedForward");
                cameraRelativeGrabbedUpField = AccessTools.Field(physGrabberType, "cameraRelativeGrabbedUp");
                cameraRelativeGrabbedRightField = AccessTools.Field(physGrabberType, "cameraRelativeGrabbedRight");
                initialPressTimerField = AccessTools.Field(physGrabberType, "initialPressTimer");
                physRotatingTimerField = AccessTools.Field(physGrabberType, "physRotatingTimer");
                prevGrabbedField = AccessTools.Field(physGrabberType, "prevGrabbed");
                grabbedField = AccessTools.Field(physGrabberType, "grabbed");
                physGrabPointActivateMethod = AccessTools.Method(physGrabberType, "PhysGrabPointActivate");
                initialized = true;
                Debug.Log("[GrabThroughWallsPatch] Reflection cache initialized successfully");
            }
        }

        [HarmonyPatch(typeof(PhysGrabber), "RayCheck")]
        public class GrabThroughWallsPatch
        {
            public static bool enableGrabThroughWalls = false;
            private static FieldInfo maskField;
            private static LayerMask originalMask;
            private static bool hasStoredOriginalMask = false;

            static GrabThroughWallsPatch()
            {
                maskField = AccessTools.Field(typeof(PhysGrabber), "mask");
            }

            [HarmonyPrefix]
            public static void Prefix(PhysGrabber __instance, bool _grab)
            {
                if (!enableGrabThroughWalls || !__instance.isLocal)
                    return;

                try
                {
                    LayerMask currentMask = (LayerMask)maskField.GetValue(__instance);

                    if (!hasStoredOriginalMask)
                    {
                        originalMask = currentMask;
                        hasStoredOriginalMask = true;
                    }

                    if (_grab)
                    {
                        LayerMask wallsMask = LayerMask.GetMask(new string[] { "Default" });
                        LayerMask maskWithoutWalls = currentMask & ~wallsMask;

                        LayerMask grabbableMask = LayerMask.GetMask(new string[]
                        {
                        "PhysGrabObject",
                        "PhysGrabObjectCart",
                        "PhysGrabObjectHinge",
                        "StaticGrabObject"
                        });

                        maskWithoutWalls |= grabbableMask;

                        maskField.SetValue(__instance, maskWithoutWalls);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GrabThroughWallsPatch] Error in Prefix: {ex.Message}");
                }
            }

            [HarmonyPostfix]
            public static void Postfix(PhysGrabber __instance)
            {
                if (!enableGrabThroughWalls || !__instance.isLocal)
                    return;

                try
                {
                    if (hasStoredOriginalMask)
                    {
                        maskField.SetValue(__instance, originalMask);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GrabThroughWallsPatch] Error in Postfix: {ex.Message}");
                }
            }
        }
        public static void ToggleGrabThroughWalls(bool enabled)
        {
            GrabThroughWallsPatch.enableGrabThroughWalls = enabled;
        }

        public static bool IsGrabThroughWallsEnabled()
        {
            return GrabThroughWallsPatch.enableGrabThroughWalls;
        }
    }


        [HarmonyPatch(typeof(ItemGun), "ShootRPC")]
        public class NoWeaponSpread
        {
            private static float local_originalGunRandomSpread = -1f;
            private static FieldInfo _photonViewField = AccessTools.Field(typeof(ItemGun), "photonView");

            [HarmonyPrefix]
            public static void Prefix(ItemGun __instance)
            {
                local_originalGunRandomSpread = -1f;

                bool isMine = false;
                try
                {
                    if (__instance != null && _photonViewField != null)
                    {
                        PhotonView pv = _photonViewField.GetValue(__instance) as PhotonView;
                        if (pv != null)
                            isMine = pv.IsMine;
                    }
                }
                catch (System.Exception ex)
                {
                }

                if (!isMine)
                    return;

                float spreadMultiplier = ConfigManager.CurrentSpreadMultiplier;
                if (Mathf.Approximately(spreadMultiplier, 1.0f))
                    return;

                try
                {
                    local_originalGunRandomSpread = __instance.gunRandomSpread;
                    float newSpread = local_originalGunRandomSpread * spreadMultiplier;
                    __instance.gunRandomSpread = newSpread;
                }
                catch (System.Exception ex)
                {
                }
            }

            [HarmonyPostfix]
            public static void Postfix(ItemGun __instance)
            {
                bool isMine = false;
                try
                {
                    if (__instance != null && _photonViewField != null)
                    {
                        PhotonView pv = _photonViewField.GetValue(__instance) as PhotonView;
                        if (pv != null)
                            isMine = pv.IsMine;
                    }
                }
                catch (System.Exception ex)
                {
                }

                if (!isMine || local_originalGunRandomSpread < 0f)
                    return;

                try
                {
                    __instance.gunRandomSpread = local_originalGunRandomSpread;
                }
                catch (System.Exception ex)
                {
                }

                local_originalGunRandomSpread = -1f;
            }
        }

        [HarmonyPatch(typeof(ItemGun), "UpdateMaster")]
        public class NoWeaponCooldown
        {
            private static FieldInfo _photonViewField = AccessTools.Field(typeof(ItemGun), "photonView");
            private static FieldInfo _shootCooldownTimerField = AccessTools.Field(typeof(ItemGun), "shootCooldownTimer");

            [HarmonyPrefix]
            public static bool Prefix(ItemGun __instance)
            {
                bool noCooldownEnabled = ConfigManager.NoWeaponCooldownEnabled;
                if (!noCooldownEnabled)
                    return true;

                bool isMine = false;
                try
                {
                    if (__instance != null && _photonViewField != null)
                    {
                        PhotonView pv = _photonViewField.GetValue(__instance) as PhotonView;
                        if (pv != null)
                            isMine = pv.IsMine;
                    }
                }
                catch (System.Exception ex)
                {
                }

                if (!isMine)
                    return true;

                try
                {
                    if (_shootCooldownTimerField != null)
                        _shootCooldownTimerField.SetValue(__instance, 0f);
                }
                catch (System.Exception ex)
                {
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Sound), "PlayLoop")]
        class BlockMenuTruckLoop_PlayLoop
        {
            static bool Prefix(object __instance, ref bool playing)
            {
                var sourceField = AccessTools.Field(__instance.GetType(), "Source");
                AudioSource source = sourceField.GetValue(__instance) as AudioSource;

                if (source?.clip != null && source.clip.name == "menu truck engine loop")
                {
                    source.Stop();
                    source.clip = null;
                    playing = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AudioSource), "Play", new Type[0])]
        class BlockMultipleMenuSounds_Play
        {
            static readonly HashSet<string> BlockedClipNames = new HashSet<string>
            {
                "menu truck engine loop",
                "Ambience Loop Truck Driving",
                "msc main menu",
                "menu truck fire pass",
                "menu truck fire pass swerve01",
                "menu truck body rustle long01",
                "menu truck fire pass swerve02",
                "menu truck body rustle long02",
                "menu truck skeleton hit",
                "menu truck body rustle long03",
                "menu truck skeleton hit skull",
                "menu truck body rustle short02",
                "menu truck swerve fast01",
                "menu truck swerve fast02",
                "menu truck swerve",
                "menu truck speed up",
                "menu truck slow down"
            };

            static bool Prefix(AudioSource __instance)
            {
                if (__instance?.clip != null)
                {
                    string name = __instance.clip.name;

                    if (BlockedClipNames.Contains(name))
                    {
                        return false; // Block
                    }
                }

                return true; // Allow
            }
        }
    }