using HarmonyLib;
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
    }
}