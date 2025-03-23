using Mono.Security.Authenticode;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using dark_cheat;

namespace dark_cheat
{
    public class ForceHost : MonoBehaviourPunCallbacks
    {

        public NetworkManager manager;
        public MenuManager menuManager;
        public PhotonView pview;
        public IEnumerator ForceStart(string levelName)
        {
            pview = GameObject.Find("Run Manager PUN").GetComponent<PhotonView>();
            PhotonNetwork.CurrentRoom.IsOpen = false;
            SteamManager.instance.LockLobby();
            DataDirector.instance.RunsPlayedAdd();
            pview.RPC("UpdateLevelRPC", RpcTarget.All, levelName, 0, false);
            RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
            manager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();
            manager.enabled = false;
            yield return new WaitForSeconds(2f);
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            pview.RPC("UpdateLevelRPC", RpcTarget.All, levelName, 0, false);
            yield return new WaitForSeconds(1f);
            manager.enabled = true;
        }
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            return;
            base.OnMasterClientSwitched(newMasterClient);
        }

        private static ForceHost _instance;
        public static ForceHost Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("ForceHost").AddComponent<ForceHost>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
    }
}
