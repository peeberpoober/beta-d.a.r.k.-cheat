using Photon.Pun;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace dark_cheat
{
    class MiscFeatures
    {
        private static float previousFarClip = 0f;
        public static bool NoFogEnabled = false;

        public static void ToggleNoFog(bool enable)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                DLog.LogError("Camera.main not found!");
                return;
            }

            if (enable)
            {
                if (previousFarClip == 0f)
                    previousFarClip = cam.farClipPlane;

                cam.farClipPlane = 500f;
                RenderSettings.fog = false;
                NoFogEnabled = true;
                DLog.Log("NoFog enabled");
            }
            else
            {
                if (previousFarClip > 0f)
                    cam.farClipPlane = previousFarClip;
                RenderSettings.fog = true;
                NoFogEnabled = false;
                DLog.Log("NoFog disabled");
            }
        }

        /* private void AddFakePlayer()
        {
            int fakePlayerId = playerNames.Count(name => name.Contains("FakePlayer")) + 1;
            string fakeName = $"<color=green>[LIVE]</color> FakePlayer{fakePlayerId}";
            playerNames.Add(fakeName);
            playerList.Add(null);
            DLog.Log($"Added fake player: {fakeName}");
        } */

        private static string StripRichTextTags(string name)
        {
            return Regex.Replace(name, "<color=.*?>(.*?)<\\/color> ", ""); // Removes "[LIVE] " or "[DEAD] " at the start
        }

        public static void ForcePlayerMicVolume(int volume)
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
            string selectedPlayerName = StripRichTextTags(Hax2.playerNames[Hax2.selectedPlayerIndex]);
            Debug.Log($"Searching for PlayerVoiceChat belonging to: '{selectedPlayerName}'");
            foreach (var playerVoiceChat in Object.FindObjectsOfType<PlayerVoiceChat>())
            {
                string voiceChatOwnerName = StripRichTextTags(playerVoiceChat.GetComponent<PhotonView>().Owner.NickName);
                if (voiceChatOwnerName == selectedPlayerName)
                {
                    Debug.Log($"Found PlayerVoiceChat for {selectedPlayerName} on {playerVoiceChat.gameObject.name}!");
                    var photonView = playerVoiceChat.GetComponent<PhotonView>();
                    if (photonView == null)
                    {
                        Debug.LogError("PhotonView not found on PlayerVoiceChat GameObject!");
                        return;
                    }
                    Debug.Log($"Attempting to set microphone volume for {selectedPlayerName} to {volume}");
                    photonView.RPC("MicrophoneVolumeSettingRPC", RpcTarget.All, volume);
                    Debug.Log($"Successfully set {selectedPlayerName}'s mic volume to {volume}!");
                    return;
                }
            }
            Debug.LogError($"No matching PlayerVoiceChat found for '{selectedPlayerName}'!");
        }
    }
}