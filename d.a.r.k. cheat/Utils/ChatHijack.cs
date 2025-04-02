using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;
using dark_cheat;

namespace dark_cheat
{
    public static class ChatHijack
    {
        private static Dictionary<object, string> originalPlayerNames = new Dictionary<object, string>();
        private static bool isSpoofingActive = false; // Dictionary to store original player names

        public static void MakeChat(string message, string targetName, List<object> playerList, List<string> playerNames)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                object player = playerList[i];
                string playerName = playerNames[i];
                if (targetName != "All" && playerName != targetName)
                    continue;
                var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log("[ChatHijack] PhotonView field not found for player " + playerName);
                    continue;
                }
                PhotonView photonView = photonViewField.GetValue(player) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log("[ChatHijack] Invalid PhotonView for player " + playerName);
                    continue;
                }
                photonView.RPC("ChatMessageSendRPC", RpcTarget.All, message, false);
                DLog.Log("[ChatHijack] Sent message as " + playerName + ": " + message);
            }
        }
        private static string StripStatusTags(string name) // Helper method to strip [LIVE] and [DEAD] tags from names
        {
            return Regex.Replace(name, @"\[(LIVE|DEAD)\]\s*", ""); // Use regex to remove [LIVE] or [DEAD] tags
        }

        public static void ToggleNameSpoofing(bool enable, string spoofName, string targetName, List<object> playerList, List<string> playerNames)
        {
            if (enable)
            {
                if (!isSpoofingActive)
                {
                    StoreOriginalNames(playerList, playerNames); // Store original names before applying spoof
                    isSpoofingActive = true;
                }

                SendCustomNameRPC(spoofName, targetName, playerList, playerNames); // Apply the spoof name
                DLog.Log($"Name spoofing enabled for {targetName}");
            }
            else
            {
                RestoreOriginalNames(targetName, playerList, playerNames); // Restore original names
                isSpoofingActive = false;
                DLog.Log("Name spoofing disabled, original names restored");
            }
        }
        private static void StoreOriginalNames(List<object> playerList, List<string> playerNames)
        {
            for (int i = 0; i < playerList.Count; i++) // Store original player names before spoofing
            {
                if (!originalPlayerNames.ContainsKey(playerList[i])) // Only store if not already stored (to avoid overwriting original name)
                {
                    string cleanName = StripStatusTags(playerNames[i]); // Store the name without status tags
                    originalPlayerNames[playerList[i]] = cleanName;
                    DLog.Log($"Stored original name for player: {cleanName} (from {playerNames[i]})");
                }
            }
        }
        private static void RestoreOriginalNames(string targetName, List<object> playerList, List<string> playerNames)
        {
            for (int i = 0; i < playerList.Count; i++) // Restore original names
            {
                object player = playerList[i];
                string playerName = playerNames[i];

                string cleanTargetName = StripStatusTags(targetName); // Strip the target name if it contains tags for comparison
                string cleanPlayerName = StripStatusTags(playerName);

                if (cleanTargetName != "All" && cleanPlayerName != cleanTargetName)
                    continue;

                if (originalPlayerNames.ContainsKey(player))
                {
                    string originalName = originalPlayerNames[player];
                    var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (photonViewField == null)
                    {
                        DLog.Log($"PhotonView field not found for {playerName}.");
                        continue;
                    }

                    PhotonView photonView = photonViewField.GetValue(player) as PhotonView;
                    if (photonView == null)
                    {
                        DLog.Log($"PhotonView is null for {playerName}.");
                        continue;
                    }

                    photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, originalName, "472644");
                    DLog.Log($"Restored original name '{originalName}' for {playerName}");
                }
            }
        }

        public static void SendCustomNameRPC(string spoofName, string targetName, List<object> playerList, List<string> playerNames)
        {
            if (playerList == null || playerNames == null || playerList.Count != playerNames.Count)
            {
                DLog.Log("Invalid player list or mismatched lengths.");
                return;
            }

            string cleanTargetName = StripStatusTags(targetName); // Strip status tags from target name for comparison

            for (int i = 0; i < playerList.Count; i++)
            {
                string playerName = playerNames[i];
                object player = playerList[i];


                string cleanPlayerName = StripStatusTags(playerName); // Strip status tags from player name for comparison

                if (cleanTargetName != "All" && cleanPlayerName != cleanTargetName)
                    continue;

                var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log($"PhotonView field not found for {playerName}.");
                    continue;
                }

                PhotonView photonView = photonViewField.GetValue(player) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log($"PhotonView is null for {playerName}.");
                    continue;
                }

                photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, spoofName, "472644");
                DLog.Log($"Sent spoof name '{spoofName}' to {playerName}.");
            }
        }
        public static void ChangePlayerColor(int colorIndex, string targetName, List<object> playerList, List<string> playerNames)
        {
            if (playerList == null || playerNames == null || playerList.Count != playerNames.Count)
            {
                DLog.Log("Invalid player list or mismatched lengths.");
                return;
            }

            string cleanTargetName = StripStatusTags(targetName);

            for (int i = 0; i < playerList.Count; i++)
            {
                string playerName = playerNames[i];
                object player = playerList[i];

                string cleanPlayerName = StripStatusTags(playerName);

                if (cleanTargetName != "All" && cleanPlayerName != cleanTargetName)
                    continue;

                var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null)
                {
                    DLog.Log($"PhotonView field not found for {playerName}.");
                    continue;
                }

                PhotonView photonView = photonViewField.GetValue(player) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log($"PhotonView is null for {playerName}.");
                    continue;
                }

                photonView.RPC("SetColorRPC", RpcTarget.AllBuffered, colorIndex);
            }
        }
        public static void ClearStoredNames() // Clear stored names
        {
            originalPlayerNames.Clear();
            isSpoofingActive = false;
            DLog.Log("Cleared all stored original names");
        }
    }
}
