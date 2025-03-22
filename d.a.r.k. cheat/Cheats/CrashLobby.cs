using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;

namespace dark_cheat
{
    public class CrashLobby : MonoBehaviourPunCallbacks
    {
        private static readonly MethodInfo instantiateMethod = typeof(PhotonNetwork)
            .GetMethod("NetworkInstantiate", BindingFlags.NonPublic | BindingFlags.Static, null,
                        new Type[] { typeof(InstantiateParameters), typeof(bool), typeof(bool) }, null);
        private static readonly FieldInfo currentLevelPrefixField = typeof(PhotonNetwork)
            .GetField("currentLevelPrefix", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Crash(Vector3 position)
        {
            Instance.StartCoroutine(CrashCoroutine(position));
            Instance.StartCoroutine(CrashCoroutine(position));
            Instance.StartCoroutine(CrashCoroutine(position));

        }
        private static IEnumerator CrashCoroutine(Vector3 position)
        {
            while (PhotonNetwork.InRoom)
            {
                SpawnItem(position);
                yield return new WaitForSeconds(0.01f);
            }
            Debug.Log("Not In A Room or Exited");
        }

        private static void EnsureItemVisibility(GameObject item)
        {
            item.SetActive(true);
            foreach (var renderer in item.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
            }
            item.layer = LayerMask.NameToLayer("Default");
        }

        public static void SpawnItem(Vector3 position)
        {
            if (!SemiFunc.IsMultiplayer())
                return;

            GameObject itemPrefab = AssetManager.instance.surplusValuableSmall;
            object[] itemData = new object[] { 4500 };

            object currentLevelPrefix = currentLevelPrefixField.GetValue(null);

            var parameters = new InstantiateParameters(
                "Valuables/" + itemPrefab.name,
                position,
                Quaternion.identity,
                0,
                itemData,
                (byte)currentLevelPrefix,
                null,
                PhotonNetwork.LocalPlayer,
                PhotonNetwork.ServerTimestamp);

            GameObject spawnedItem = (GameObject)instantiateMethod.Invoke(null, new object[] { parameters, true, false });
            EnsureItemVisibility(spawnedItem);
            Destroy(spawnedItem);
        }

        private static CrashLobby _instance;
        public static CrashLobby Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("CrashLobby").AddComponent<CrashLobby>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
    }
}