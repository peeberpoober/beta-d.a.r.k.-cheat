using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    // Define your fake values here.
                    string fakeName = Hax2.persistentNameText;
                    string fakeSteamId = "765611472644157498";

                    // Branch based on multiplayer mode.
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
            static bool Prefix(int playerID, PlayerAvatar player, bool culled, bool playerNear)
            {
                if (!Hax2.blindEnemies)
                {
                    return true;
                }

                if (player != null && player.photonView != null)
                {
                    // if player being "seen" is the local player, block
                    if (player.photonView.IsMine)
                    {
                        return false;
                    }
                }

                return true;
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
                    { // in case DebugCheats isn't available/initialized yet
                        localPlayerGO = DebugCheats.GetLocalPlayer();
                        if (localPlayerGO != null)
                        {
                            localPlayerAvatar = localPlayerGO.GetComponent<PlayerAvatar>();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        DLog.LogWarning($"Error accessing DebugCheats.GetLocalPlayer(): {ex.Message}");
                    }


                    // fallback
                    if (localPlayerAvatar == null && GameDirector.instance != null && GameDirector.instance.PlayerList != null)
                    {
                        try
                        {
                            localPlayerAvatar = GameDirector.instance.PlayerList.FirstOrDefault(p => p != null && p.photonView != null && p.photonView.IsMine);
                        }
                        catch (System.Exception ex)
                        {
                            DLog.LogWarning($"Error accessing GameDirector.instance.PlayerList: {ex.Message}");
                        }
                    }

                    if (localPlayerAvatar == null)
                    {
                        DLog.LogWarning("EnemyStateInvestigate_BlindAudio_Patch: Could not find local player avatar via any method.");
                        return true;
                    }


                    Vector3 localPlayerPos = localPlayerAvatar.transform.position;

                    // if sound close enough to be our player, block
                    if (Vector3.Distance(position, localPlayerPos) < LOCAL_PLAYER_SOUND_THRESHOLD)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        // Remove the annoying truck engine at main menu
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
}