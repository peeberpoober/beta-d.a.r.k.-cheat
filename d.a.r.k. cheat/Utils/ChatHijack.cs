using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace dark_cheat
{
    public static class ChatHijack
    {
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
                    DLog.Log($"[ChatHijack] PhotonView field not found for player {playerName}.");
                    continue;
                }

                PhotonView photonView = photonViewField.GetValue(player) as PhotonView;
                if (photonView == null)
                {
                    DLog.Log($"[ChatHijack] Invalid PhotonView for player {playerName}.");
                    continue;
                }

                photonView.RPC("ChatMessageSendRPC", RpcTarget.All, message, false);
                DLog.Log($"[ChatHijack] Sent message as {playerName}: {message}");
            }
        }
    }
}
