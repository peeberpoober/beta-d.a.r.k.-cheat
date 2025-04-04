using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using dark_cheat;
using SingularityGroup.HotReload;
using System.Linq;
using System.IO;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine.SceneManagement;

namespace dark_cheat
{
    static class Troll
    {

        public static void InfiniteLoadingSelectedPlayer()
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

                Debug.Log($"Attempting to cause {Hax2.playerNames[Hax2.selectedPlayerIndex]} to go to an infinite loading screen...");

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

                int masterActorID = PhotonNetwork.MasterClient.ActorNumber;
                FieldInfo actorNumberField = typeof(Player).GetField("actorNumber", BindingFlags.NonPublic | BindingFlags.Instance);
                if (actorNumberField != null)
                {
                    actorNumberField.SetValue(PhotonNetwork.LocalPlayer, masterActorID);

                    photonView.RPC("OutroStartRPC", RpcTarget.All);

                    actorNumberField.SetValue(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error causing infinite loading screen {Hax2.playerNames[Hax2.selectedPlayerIndex]}: {e.Message}");
            }
        }

        public static void SceneRecovery()
        {
            Debug.Log("[Recovery] === Begin Scene Recovery ===");

            SceneManager.LoadScene("LobbyJoin", LoadSceneMode.Single);
            Debug.Log("[Recovery] Loaded LobbyJoin scene.");

            Hax2.CoroutineHost.StartCoroutine(LoadReloadSceneAfterDelay());
        }

        private static IEnumerator LoadReloadSceneAfterDelay()
        {
            yield return new WaitForSeconds(3.0f);

            SceneManager.LoadScene("Reload", LoadSceneMode.Single);
            Debug.Log("[Recovery] Loaded Reload scene.");

            yield return new WaitForSeconds(0.5f);
            PhotonNetwork.Disconnect();
            Debug.Log("[Recovery] === Scene Recovery Complete ===");
        }

        public static void ForcePlayerGlitch()
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

            try
            {
                Debug.Log($"Forcing {Hax2.playerNames[Hax2.selectedPlayerIndex]} to glitch.");
                var photonViewField = selectedPlayer.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (photonViewField == null) { Debug.Log("PhotonView field not found!"); return; }
                var photonView = photonViewField.GetValue(selectedPlayer) as PhotonView;
                if (photonView == null) { Debug.Log("PhotonView is not valid!"); return; }
                photonView.RPC("PlayerGlitchShortRPC", RpcTarget.All);
                Debug.Log($"Forced {Hax2.playerNames[Hax2.selectedPlayerIndex]} to glitch.");
            }
            catch (Exception e)
            {
                Debug.Log($"Error forcing {Hax2.playerNames[Hax2.selectedPlayerIndex]} to glitch: {e.Message}");
            }
        }
    }
}
