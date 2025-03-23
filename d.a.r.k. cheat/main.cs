using HarmonyLib;
using System;
using UnityEngine;

namespace dark_cheat
{
    public class Loader
    {
        private static Harmony harmonyInstance;

        [HarmonyPatch(typeof(SpectateCamera), "PlayerSwitch")]
        public static class SpectateCamera_PlayerSwitch_Patch
        {
            static bool Prefix(bool _next)
            {
                if (Hax2.showMenu)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButtonUp", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonUp
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButtonDown", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButtonDown
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Input), "GetMouseButton", new Type[] { typeof(int) })]
        public class Patch_Input_GetMouseButton
        {
            static bool Prefix(int button, ref bool __result)
            {
                if (Hax2.showMenu)
                {
                    __result = false;
                    return false;
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

        private static GameObject Load;

        public static void Init()
        {
            Load = new GameObject();
            Load.AddComponent<Hax2>();
            UnityEngine.Object.DontDestroyOnLoad(Load);

            harmonyInstance = new Harmony("dark_cheat");
            harmonyInstance.PatchAll();

            DLog.Log("Cheat loader initialized successfully!");
        }

        public static void UnloadCheat()
        {
            UnityEngine.Object.Destroy(Load);
            if (harmonyInstance != null)
            {
                // Do not use UnpatchAll, cause it can break mods
                harmonyInstance.UnpatchSelf();
            }

            System.GC.Collect();
        }
    }
}