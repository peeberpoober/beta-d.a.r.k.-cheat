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
            private static LayerMask originalMask;
            private static bool hasStoredOriginalMask = false;

            static GrabThroughWallsPatch()
            {
                ReflectionCache.Initialize();
            }

            [HarmonyPrefix]
            public static bool Prefix(PhysGrabber __instance, bool _grab, ref bool __result)
            {
                if (!enableGrabThroughWalls) return true;
                if (!__instance.isLocal) return true;
                try
                {
                    if (!hasStoredOriginalMask)
                    {
                        originalMask = (LayerMask)ReflectionCache.maskField.GetValue(__instance);
                        hasStoredOriginalMask = true;
                    }
                    if (_grab)
                    {
                        Camera playerCamera = (Camera)ReflectionCache.playerCameraField.GetValue(__instance);
                        Transform physGrabPoint = (Transform)ReflectionCache.physGrabPointField.GetValue(__instance);
                        Transform physGrabPointPuller = (Transform)ReflectionCache.physGrabPointPullerField.GetValue(__instance);
                        Transform physGrabPointPlane = (Transform)ReflectionCache.physGrabPointPlaneField.GetValue(__instance);
                        if (playerCamera == null) return true;
                        float maxDistance = 10f;
                        Vector3 rayOrigin = playerCamera.transform.position;
                        Vector3 rayDirection = playerCamera.transform.forward;
                        LayerMask physGrabObjectMask = LayerMask.GetMask(new string[]
                        {
                    "PhysGrabObject",
                    "PhysGrabObjectCart",
                    "PhysGrabObjectHinge",
                    "StaticGrabObject"
                        });
                        RaycastHit[] hits = Physics.SphereCastAll(rayOrigin, 0.5f, rayDirection, maxDistance, physGrabObjectMask);
                        if (hits.Length > 0)
                        {
                            float closestDistance = float.MaxValue;
                            RaycastHit closestHit = new RaycastHit();
                            bool foundValidHit = false;
                            foreach (RaycastHit hit in hits)
                            {
                                if (hit.collider.CompareTag("Phys Grab Object") && hit.distance < closestDistance)
                                {
                                    PhysGrabObject phgo = hit.transform.GetComponent<PhysGrabObject>();
                                    if (phgo != null)
                                    {
                                        closestHit = hit;
                                        closestDistance = hit.distance;
                                        foundValidHit = true;
                                    }
                                }
                            }
                            if (foundValidHit)
                            {
                                Transform hitTransform = closestHit.transform;
                                PhysGrabObject physGrabObject = hitTransform.GetComponent<PhysGrabObject>();
                                if (physGrabObject == null) return true;
                                FieldInfo grabDisableTimerField = AccessTools.Field(typeof(PhysGrabObject), "grabDisableTimer");
                                if (grabDisableTimerField != null)
                                {
                                    float grabDisableTimer = (float)grabDisableTimerField.GetValue(physGrabObject);
                                    if (grabDisableTimer > 0f)
                                    {
                                        return false;
                                    }
                                }
                                if (physGrabObject.rb.IsSleeping())
                                {
                                    physGrabObject.OverrideIndestructible(0.5f);
                                    physGrabObject.OverrideBreakEffects(0.5f);
                                }
                                ReflectionCache.grabbedObjectTransformField.SetValue(__instance, hitTransform);
                                PhysGrabObjectCollider colliderComponent = closestHit.collider.GetComponent<PhysGrabObjectCollider>();
                                if (colliderComponent == null)
                                {
                                    Debug.LogWarning("[GrabThroughWallsPatch] Could not find PhysGrabObjectCollider component");
                                    return true;
                                }
                                var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
                                if (playerControllerType == null)
                                {
                                    Debug.LogError("[GrabThroughWallsPatch] Could not find PlayerController type");
                                    return true;
                                }
                                var instanceField = AccessTools.Field(playerControllerType, "instance");
                                if (instanceField == null)
                                {
                                    Debug.LogError("[GrabThroughWallsPatch] Could not find PlayerController.instance field");
                                    return true;
                                }
                                var playerControllerInstance = instanceField.GetValue(null);
                                if (playerControllerInstance == null)
                                {
                                    Debug.LogError("[GrabThroughWallsPatch] PlayerController.instance is null");
                                    return true;
                                }
                                ReflectionCache.physGrabPointActivateMethod.Invoke(__instance, null);
                                physGrabPointPuller.gameObject.SetActive(true);
                                Rigidbody hitRigidbody = closestHit.rigidbody;
                                ReflectionCache.grabbedObjectField.SetValue(__instance, hitRigidbody);
                                Vector3 hitPoint = closestHit.point;
                                var roomVolumeCheckField = AccessTools.Field(typeof(PhysGrabObject), "roomVolumeCheck");
                                var roomVolumeCheck = roomVolumeCheckField.GetValue(physGrabObject);
                                var currentSizeField = AccessTools.Field(roomVolumeCheck.GetType(), "currentSize");
                                Vector3 currentSize = (Vector3)currentSizeField.GetValue(roomVolumeCheck);
                                if (currentSize.magnitude < 0.5f)
                                {
                                    hitPoint = closestHit.collider.bounds.center;
                                }
                                float distanceToHit = Vector3.Distance(playerCamera.transform.position, hitPoint);
                                Vector3 position = playerCamera.transform.position + playerCamera.transform.forward * distanceToHit;
                                physGrabPointPlane.position = position;
                                physGrabPointPuller.position = position;
                                float physRotatingTimer = (float)ReflectionCache.physRotatingTimerField.GetValue(__instance);
                                if (physRotatingTimer <= 0f)
                                {
                                    Vector3 forward = Camera.main.transform.InverseTransformDirection(hitTransform.forward);
                                    Vector3 up = Camera.main.transform.InverseTransformDirection(hitTransform.up);
                                    Vector3 right = Camera.main.transform.InverseTransformDirection(hitTransform.right);
                                    ReflectionCache.cameraRelativeGrabbedForwardField.SetValue(__instance, forward);
                                    ReflectionCache.cameraRelativeGrabbedUpField.SetValue(__instance, up);
                                    ReflectionCache.cameraRelativeGrabbedRightField.SetValue(__instance, right);
                                }
                                physGrabPoint.position = hitPoint;
                                var forceGrabPointField = AccessTools.Field(typeof(PhysGrabObject), "forceGrabPoint");
                                Transform forceGrabPoint = (Transform)forceGrabPointField.GetValue(physGrabObject);
                                if (forceGrabPoint == null)
                                {
                                    Vector3 localPos = hitTransform.InverseTransformPoint(hitPoint);
                                    ReflectionCache.localGrabPositionField.SetValue(__instance, localPos);
                                }
                                else
                                {
                                    Vector3 forcePoint = forceGrabPoint.position;
                                    float forceDist = 1f;
                                    Vector3 forcePos = playerCamera.transform.position + playerCamera.transform.forward * forceDist - playerCamera.transform.up * 0.3f;
                                    physGrabPoint.position = forcePoint;
                                    physGrabPointPlane.position = forcePos;
                                    physGrabPointPuller.position = forcePos;
                                    Vector3 localGrabPos = hitTransform.InverseTransformPoint(forcePoint);
                                    ReflectionCache.localGrabPositionField.SetValue(__instance, localGrabPos);
                                }
                                if (__instance.isLocal)
                                {
                                    var physGrabObjectField = AccessTools.Field(playerControllerType, "physGrabObject");
                                    var physGrabActiveField = AccessTools.Field(playerControllerType, "physGrabActive");
                                    physGrabObjectField.SetValue(playerControllerInstance, hitTransform.gameObject);
                                    physGrabActiveField.SetValue(playerControllerInstance, true);
                                }
                                ReflectionCache.initialPressTimerField.SetValue(__instance, 0.1f);
                                ReflectionCache.prevGrabbedField.SetValue(__instance, __instance.grabbed);
                                ReflectionCache.grabbedField.SetValue(__instance, true);
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GrabThroughWallsPatch] Error in Prefix: {ex.Message}\n{ex.StackTrace}");
                }
                return true;
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