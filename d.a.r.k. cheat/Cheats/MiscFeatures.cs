using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace dark_cheat
{
    class MiscFeatures
    {
        private static float previousFarClip = 0f;
        public static bool NoFogEnabled = false;

        public static void ToggleNoFog()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                DLog.LogError("Camera.main not found!");
                return;
            }

            if (NoFogEnabled)
            {
                if (previousFarClip == 0f)
                    previousFarClip = cam.farClipPlane;

                cam.farClipPlane = 500f;
                RenderSettings.fog = false;
                DLog.Log("NoFog enabled");
            }
            else
            {
                if (previousFarClip > 0f)
                    cam.farClipPlane = previousFarClip;
                RenderSettings.fog = true;
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
            foreach (var playerVoiceChat in global::UnityEngine.Object.FindObjectsOfType<PlayerVoiceChat>())
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
        public static void CrashSelectedPlayerNew()
        {
            try
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
                Debug.Log($"Attempting to crash {Hax2.playerNames[Hax2.selectedPlayerIndex]}...");
                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    Debug.Log("PhotonView field not found!");
                    return;
                }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null)
                {
                    Debug.Log("PhotonView is null!");
                    return;
                }
                var targetPlayer = photonView.Owner;
                if (targetPlayer == null)
                {
                    Debug.Log("Could not retrieve Photon player from PhotonView!");
                    return;
                }
                LevelGenerator levelGenerator = global::UnityEngine.Object.FindObjectOfType<LevelGenerator>();
                if (levelGenerator == null)
                {
                    Debug.LogError("[KickExploit] Could not find LevelGenerator PhotonView.");
                    return;
                }
                for (int i = 0; i < 5000; i++)
                {
                    global::UnityEngine.Random.Range(0, 9999);
                    levelGenerator.PhotonView.RPC("ItemSetup", targetPlayer, Array.Empty<object>());
                    levelGenerator.PhotonView.RPC("NavMeshSetupRPC", targetPlayer, Array.Empty<object>());
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error crashing {Hax2.playerNames[Hax2.selectedPlayerIndex]}: {e.Message}");
            }
        }

        public static void ForceActivateAllExtractionPoints()
        {
            try
            {
                var roundDirector = RoundDirector.instance;
                if (roundDirector == null)
                {
                    Debug.LogError("[ForceActivate] RoundDirector instance not found.");
                    return;
                }

                var photonViewField = typeof(RoundDirector).GetField("photonView", BindingFlags.NonPublic | BindingFlags.Instance);
                var photonView = photonViewField?.GetValue(roundDirector) as PhotonView;

                if (photonView == null)
                {
                    Debug.LogError("[ForceActivate] photonView not found on RoundDirector.");
                    return;
                }

                var extractionPointsField = typeof(RoundDirector).GetField("extractionPointList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (extractionPointsField == null)
                {
                    Debug.LogError("[ForceActivate] extractionPointList field not found.");
                    return;
                }

                var extractionList = extractionPointsField.GetValue(roundDirector) as List<GameObject>;
                if (extractionList == null)
                {
                    Debug.LogError("[ForceActivate] extractionPointList is null or invalid.");
                    return;
                }

                foreach (var point in extractionList)
                {
                    if (point == null || !point.activeInHierarchy) continue;

                    var view = point.GetComponent<PhotonView>();
                    if (view != null)
                    {
                        photonView.RPC("ExtractionPointActivateRPC", RpcTarget.All, view.ViewID);
                        Debug.Log($"[ForceActivate] Activated extraction point: {point.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ForceActivate] Exception occurred: {ex.Message}");
            }
        }
    }
}
