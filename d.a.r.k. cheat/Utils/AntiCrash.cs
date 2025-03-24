using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace dark_cheat
{
    public static class AntiCrashProtection
    {
        private static Dictionary<string, List<float>> rpcTimestamps = new Dictionary<string, List<float>>();
        private static Dictionary<string, bool> blockedRpcs = new Dictionary<string, bool>();
        private static float blockDuration = 30f; // Seconds to block after detecting exploit
        private static int rpcThreshold = 15; // Number of RPCs to trigger blocking
        private static float timeWindow = 3f; // Time window in seconds

        public static bool ShouldBlockRpc(string rpcName)
        {
            if (blockedRpcs.TryGetValue(rpcName, out bool isBlocked) && isBlocked) // Check if currently blocked
            {
                Debug.Log($"Blocked malicious RPC: {rpcName}");
                return true;
            }

            float currentTime = Time.time; // Track RPC call
            if (!rpcTimestamps.ContainsKey(rpcName))
            {
                rpcTimestamps[rpcName] = new List<float>();
            }

            rpcTimestamps[rpcName].Add(currentTime); // Add current timestamp

            rpcTimestamps[rpcName].RemoveAll(timestamp => currentTime - timestamp > timeWindow); // Remove timestamps outside the window

            if (rpcTimestamps[rpcName].Count >= rpcThreshold)
            { // Check if threshold exceeded
                Debug.LogWarning($"RPC spam detected for {rpcName}! Blocking for {blockDuration} seconds.");
                blockedRpcs[rpcName] = true;

                MonoBehaviour mb = global::UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
                if (mb != null) // Schedule unblock
                {
                    mb.StartCoroutine(UnblockRpcAfterDelay(rpcName, blockDuration));
                }

                return true;
            }

            return false;
        }

        private static IEnumerator UnblockRpcAfterDelay(string rpcName, float delay)
        {
            yield return new WaitForSeconds(delay);
            blockedRpcs[rpcName] = false;
            Debug.Log($"Unblocked RPC: {rpcName}");
        }
    }

    [HarmonyPatch(typeof(LevelGenerator))] // Harmony patches for the RPC methods
    public static class LevelGeneratorPatches
    {
        [HarmonyPatch("ItemSetup")] // The method name to patch
        [HarmonyPrefix] // Run our code before the original method
        public static bool ItemSetupPrefix()
        {
            return !AntiCrashProtection.ShouldBlockRpc("ItemSetup"); // Return false to prevent original method from running if we should block
        }

        [HarmonyPatch("NavMeshSetupRPC")] // The method name to patch
        [HarmonyPrefix] // Run our code before the original method
        public static bool NavMeshSetupRPCPrefix()
        {
            return !AntiCrashProtection.ShouldBlockRpc("NavMeshSetupRPC"); // Return false to prevent original method from running if we should block
        }
    }
}
