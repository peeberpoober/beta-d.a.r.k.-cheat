using HarmonyLib;
using Photon.Pun;

namespace dark_cheat
{
    [HarmonyPatch(typeof(PlayerTumble))]
    internal class PlayerTumblePatch
    {
        public static bool Debounce;
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void PreFixUpdate(PlayerTumble __instance, PhysGrabObject ___physGrabObject)
        {
            bool flag = __instance == null || ___physGrabObject == null || !___physGrabObject.grabbed || !__instance.playerAvatar.photonView.IsMine || !Hax2.debounce;
            if (!flag)
            {
                ___physGrabObject.playerGrabbing.ForEach(delegate (PhysGrabber physGrabber)
                {
                    physGrabber.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, new object[]
                    {
                        true,
                        0.01f
                    });
                });
            }
        }
    }
}