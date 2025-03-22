using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;
using System.Collections.Generic;

namespace dark_cheat
{
    static class Strength
    {
        private static object physGrabberInstance;
        private static float lastStrengthUpdateTime = 0f;
        private static float strengthUpdateCooldown = 0.1f;
        private static PhotonView physGrabberPhotonView;
        private static PhotonView punManagerPhotonView;
        private static float lastAppliedStrength = -1f;
        private static bool? lastGrabbedState = null;

        private static void InitializePlayerController()
        {
            if (PlayerController.playerControllerType == null)
            {
                DLog.Log("PlayerController type not found.");
                return;
            }
            if (PlayerController.playerControllerInstance == null)
            {
                PlayerController.playerControllerInstance = GameHelper.FindObjectOfType(PlayerController.playerControllerType);
                if (PlayerController.playerControllerInstance == null)
                {
                    DLog.Log("PlayerController instance not found.");
                }
            }
        }

        public static void MaxStrength()
        {
            var playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
            if (playerControllerType == null) { DLog.Log("PlayerController type not found."); return; }
            var playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
            if (playerControllerInstance == null) { DLog.Log("PlayerController instance not found."); return; }
            var playerAvatarScriptField = playerControllerType.GetField("playerAvatarScript", BindingFlags.Public | BindingFlags.Instance);
            if (playerAvatarScriptField == null) { DLog.Log("playerAvatarScript field not found in PlayerController."); return; }
            var playerAvatarScriptInstance = playerAvatarScriptField.GetValue(playerControllerInstance);
            if (playerAvatarScriptInstance == null) { DLog.Log("playerAvatarScript instance is null."); return; }
            var physGrabberField = playerAvatarScriptInstance.GetType().GetField("physGrabber", BindingFlags.Public | BindingFlags.Instance);
            if (physGrabberField == null) { DLog.Log("physGrabber field not found in PlayerAvatarScript."); return; }
            physGrabberInstance = physGrabberField.GetValue(playerAvatarScriptInstance);
            if (physGrabberInstance == null) { DLog.Log("physGrabber instance is null."); return; }
            var physGrabberPhotonViewField = physGrabberInstance.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.Instance);
            if (physGrabberPhotonViewField != null)
            {
                physGrabberPhotonView = (PhotonView)physGrabberPhotonViewField.GetValue(physGrabberInstance);
            }
            if (physGrabberPhotonView == null) { DLog.Log("PhotonView not found in PhysGrabber."); }
            var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
            var punManagerInstance = GameHelper.FindObjectOfType(punManagerType);
            if (punManagerInstance != null)
            {
                punManagerPhotonView = (PhotonView)punManagerType.GetField("photonView", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(punManagerInstance);
                if (punManagerPhotonView == null) { DLog.Log("PhotonView not found in PunManager."); }
            }
            else { DLog.Log("PunManager instance not found."); }

            ApplyGrabStrength();
            SetServerGrabStrength(Hax2.sliderValueStrength);
        }

        private static void ApplyGrabStrength()
        {
            if (physGrabberInstance == null) { DLog.Log("physGrabberInstance is null in ApplyGrabStrength"); return; }

            var grabStrengthField = physGrabberInstance.GetType().GetField("grabStrength", BindingFlags.Public | BindingFlags.Instance);
            if (grabStrengthField != null)
            {
                float currentStrength = (float)grabStrengthField.GetValue(physGrabberInstance);
                if (currentStrength != Hax2.sliderValueStrength)
                {
                    grabStrengthField.SetValue(physGrabberInstance, Hax2.sliderValueStrength);
                    DLog.Log($"grabStrength forced locally to {Hax2.sliderValueStrength} (was {currentStrength})");

                    if (Hax2.sliderValueStrength <= 1f)
                    {
                        ResetGrabbedObject();
                    }
                }
            }
            else { DLog.Log("grabStrength field not found"); }

           var grabbedField = physGrabberInstance.GetType().GetField("grabbed", BindingFlags.Public | BindingFlags.Instance);
           bool isGrabbed = grabbedField != null && (bool)grabbedField.GetValue(physGrabberInstance);

           // Only log if the grabbed state changed
           if (lastGrabbedState == null || isGrabbed != lastGrabbedState)
           {
               DLog.Log($"isGrabbed: {isGrabbed}");
               lastGrabbedState = isGrabbed;
           }

            if (isGrabbed)
            {
                var grabbedObjectTransformField = physGrabberInstance.GetType().GetField("grabbedObjectTransform", BindingFlags.Public | BindingFlags.Instance);
                Transform grabbedObjectTransform = grabbedObjectTransformField != null ? (Transform)grabbedObjectTransformField.GetValue(physGrabberInstance) : null;
                if (grabbedObjectTransform == null) { DLog.Log("grabbedObjectTransform is null"); return; }

                DLog.Log($"Attempting to grab object: {grabbedObjectTransform.name}");

                PhysGrabObject physGrabObject = grabbedObjectTransform.GetComponent<PhysGrabObject>();
                if (physGrabObject == null)
                {
                    DLog.Log($"PhysGrabObject component not found on {grabbedObjectTransform.name}");
                    return;
                }

                Rigidbody rb = physGrabObject.rb;
                if (rb == null) { DLog.Log($"Rigidbody not found on {grabbedObjectTransform.name}"); return; }

                var physGrabPointField = physGrabberInstance.GetType().GetField("physGrabPoint", BindingFlags.Public | BindingFlags.Instance);
                Transform physGrabPoint = physGrabPointField != null ? (Transform)physGrabPointField.GetValue(physGrabberInstance) : null;
                if (physGrabPoint == null) { DLog.Log("physGrabPoint is null"); return; }

                var pullerPositionField = physGrabberInstance.GetType().GetField("physGrabPointPullerPosition", BindingFlags.Public | BindingFlags.Instance);
                Vector3 pullerPosition = pullerPositionField != null ? (Vector3)pullerPositionField.GetValue(physGrabberInstance) : Vector3.zero;
                if (pullerPosition == Vector3.zero && pullerPositionField == null) { DLog.Log("pullerPositionField is null"); return; }

                Vector3 direction = (pullerPosition - physGrabPoint.position).normalized;
                float forceMagnitude = Hax2.sliderValueStrength * 50000f;
                DLog.Log($"Calculated forceMagnitude: {forceMagnitude} for {grabbedObjectTransform.name}");

                var physGrabObjectPhotonViewField = physGrabObject.GetType().GetField("photonView", BindingFlags.NonPublic | BindingFlags.Instance);
                PhotonView physGrabObjectPhotonView = physGrabObjectPhotonViewField != null ? (PhotonView)physGrabObjectPhotonViewField.GetValue(physGrabObject) : null;

                if (physGrabObjectPhotonView != null)
                {
                    if (!physGrabObjectPhotonView.IsMine)
                    {
                        physGrabObjectPhotonView.RequestOwnership();
                        DLog.Log($"Requested ownership of {grabbedObjectTransform.name}");
                    }

                    if (physGrabObjectPhotonView.IsMine)
                    {
                        rb.AddForceAtPosition(direction * forceMagnitude, physGrabPoint.position, ForceMode.Force);
                        DLog.Log($"Applied extra force {forceMagnitude} to {grabbedObjectTransform.name} as owner");
                    }
                    else if (PhotonNetwork.IsMasterClient)
                    {
                        rb.AddForceAtPosition(direction * forceMagnitude, physGrabPoint.position, ForceMode.Force);
                        DLog.Log($"Applied extra force {forceMagnitude} to {grabbedObjectTransform.name} as Master Client");
                    }
                    else
                    {
                        physGrabObjectPhotonView.RPC("ApplyExtraForceRPC", RpcTarget.MasterClient, direction, forceMagnitude, physGrabPoint.position);
                        DLog.Log($"Requested extra force {forceMagnitude} to {grabbedObjectTransform.name} via RPC");
                        if (Hax2.sliderValueStrength == lastAppliedStrength)
                        {
                            rb.AddForceAtPosition(direction * forceMagnitude, physGrabPoint.position, ForceMode.Force);
                        }
                    }
                }
                else { DLog.Log($"physGrabObjectPhotonView is null on {grabbedObjectTransform.name}"); }
            }
        }

        private static void ResetGrabbedObject()
        {
            var grabbedObjectTransformField = physGrabberInstance.GetType().GetField("grabbedObjectTransform", BindingFlags.Public | BindingFlags.Instance);
            Transform grabbedObjectTransform = grabbedObjectTransformField != null ? (Transform)grabbedObjectTransformField.GetValue(physGrabberInstance) : null;
            if (grabbedObjectTransform != null)
            {
                DLog.Log($"Resetting object: {grabbedObjectTransform.name}");
                PhysGrabObject physGrabObject = grabbedObjectTransform.GetComponent<PhysGrabObject>();
                if (physGrabObject != null && physGrabObject.rb != null)
                {
                    physGrabObject.rb.velocity = Vector3.zero;
                    physGrabObject.rb.angularVelocity = Vector3.zero;
                    DLog.Log($"Reset velocity of {grabbedObjectTransform.name} due to strength reset ({Hax2.sliderValueStrength})");

                    var photonView = physGrabObject.GetComponent<PhotonView>();
                    if (photonView != null && !photonView.IsMine && PhotonNetwork.IsConnected)
                    {
                        photonView.RPC("ResetVelocityRPC", RpcTarget.MasterClient);
                        DLog.Log($"Requested velocity reset for {grabbedObjectTransform.name} via RPC");
                    }
                }
                else
                {
                    DLog.Log($"PhysGrabObject or Rigidbody not found on {grabbedObjectTransform.name} during reset");
                }
            }
            else
            {
                DLog.Log("No grabbed object to reset");
            }
        }

        public static void UpdateStrength()
        {
            if (physGrabberInstance != null)
            {
                ApplyGrabStrength();
                if (Hax2.sliderValueStrength != lastAppliedStrength)
                {
                    SetServerGrabStrength(Hax2.sliderValueStrength);
                    lastAppliedStrength = Hax2.sliderValueStrength;
                    lastStrengthUpdateTime = Time.time;
                    if (Hax2.sliderValueStrength <= 1f)
                    {
                        ResetGrabbedObject();
                    }
                }
            }
        }

        public static void SetServerGrabStrength(float strength)
        {
            if (physGrabberInstance == null)
            {
                MaxStrength();
                if (physGrabberInstance == null) return;
            }

            if (punManagerPhotonView == null)
            {
                DLog.Log("PunManager PhotonView not initialized.");
                return;
            }

            string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            if (string.IsNullOrEmpty(steamID))
            {
                DLog.Log("Could not retrieve local SteamID.");
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                var playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamID);
                if (playerAvatar != null)
                {
                    playerAvatar.physGrabber.grabStrength = strength;
                    DLog.Log($"Set grabStrength to {strength} directly on Master Client for SteamID: {steamID}");

                    var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
                    var punManagerInstance = GameHelper.FindObjectOfType(punManagerType);
                    if (punManagerInstance != null)
                    {
                        var statsManagerField = punManagerType.GetField("statsManager", BindingFlags.NonPublic | BindingFlags.Instance);
                        var statsManager = statsManagerField?.GetValue(punManagerInstance);
                        if (statsManager != null)
                        {
                            var playerUpgradeStrengthField = statsManager.GetType().GetField("playerUpgradeStrength", BindingFlags.Public | BindingFlags.Instance);
                            var playerUpgradeStrength = (Dictionary<string, int>)playerUpgradeStrengthField?.GetValue(statsManager);
                            if (playerUpgradeStrength != null)
                            {
                                int targetUpgrades = Mathf.RoundToInt(strength);
                                playerUpgradeStrength[steamID] = targetUpgrades;
                                DLog.Log($"Updated playerUpgradeStrength to {targetUpgrades} upgrades for SteamID: {steamID}");
                            }
                        }
                    }
                }
            }
            else
            {
                int targetUpgrades = Mathf.RoundToInt(strength);
                int currentUpgrades = GetCurrentUpgradeCount(steamID);
                if (targetUpgrades != currentUpgrades)
                {
                    punManagerPhotonView.RPC("UpgradePlayerGrabStrengthRPC", RpcTarget.MasterClient, steamID, targetUpgrades);
                    DLog.Log($"Requested server-side grabStrength set to {strength} (upgrades: {targetUpgrades}) for SteamID: {steamID}");
                }
            }
        }

        private static int GetCurrentUpgradeCount(string steamID)
        {
            var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
            var punManagerInstance = GameHelper.FindObjectOfType(punManagerType);
            if (punManagerInstance != null)
            {
                var statsManagerField = punManagerType.GetField("statsManager", BindingFlags.NonPublic | BindingFlags.Instance);
                var statsManager = statsManagerField?.GetValue(punManagerInstance);
                if (statsManager != null)
                {
                    var playerUpgradeStrengthField = statsManager.GetType().GetField("playerUpgradeStrength", BindingFlags.Public | BindingFlags.Instance);
                    var playerUpgradeStrength = (Dictionary<string, int>)playerUpgradeStrengthField?.GetValue(statsManager);
                    if (playerUpgradeStrength != null && playerUpgradeStrength.ContainsKey(steamID))
                    {
                        return playerUpgradeStrength[steamID];
                    }
                }
            }
            return 0;
        }

        public partial class PhysGrabObject : MonoBehaviour, IPunOwnershipCallbacks
        {
            public Rigidbody rb;
            private PhotonView photonView;

            private void Awake()
            {
                photonView = GetComponent<PhotonView>();
                PhotonNetwork.AddCallbackTarget(this);
            }

            private void OnDestroy()
            {
                PhotonNetwork.RemoveCallbackTarget(this);
            }

            [PunRPC]
            private void ApplyExtraForceRPC(Vector3 direction, float forceMagnitude, Vector3 position)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    rb.AddForceAtPosition(direction * forceMagnitude, position, ForceMode.Force);
                    DLog.Log($"Applied extra force {forceMagnitude} to {gameObject.name} via RPC from client");
                }
            }

            [PunRPC]
            private void ResetVelocityRPC()
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    DLog.Log($"Reset velocity of {gameObject.name} via RPC from client");
                }
            }

            public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer) { /* Implementação existente */ }
            public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner) { /* Implementação existente */ }
            public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest) { /* Implementação existente */ }
        }
    }
}