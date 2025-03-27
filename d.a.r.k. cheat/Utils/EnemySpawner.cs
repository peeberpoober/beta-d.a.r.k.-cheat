using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
// NOTE: This will cause errors for trying to access "1007" player index, while it doesnt exist.
// The base issue is that it "sees" the player, but immediately is out of range when the vision method is called.
// I believe the reason this happens is because it gets 'fake' teleported to the player on spawn, and immediately gets a new position.
// If this causes problems in the future, a potential fix would be to pre-assign an initial spawn point that isnt the local player. Preferably one away from any players.
// For the timebeing, this does not appear to have any real impact, outside of sending scary red text in console when spawning the enemy.
// tdlr: '1007' errors have no impact on performance or functionality, and have been intentionally left in.
namespace dark_cheat.Utils
{
    public static class EnemySpawner
    {
        // Cache fields for the EnemyDirector.
        private static Type enemyDirectorTypeCache = null;
        private static object enemyDirectorInstanceCache = null;
        private static FieldInfo enemiesDifficulty1FieldCache = null;
        private static FieldInfo enemiesDifficulty2FieldCache = null;
        private static FieldInfo enemiesDifficulty3FieldCache = null;

        public static bool TryGetEnemyLists(out List<EnemySetup> enemiesDifficulty1,
                                              out List<EnemySetup> enemiesDifficulty2,
                                              out List<EnemySetup> enemiesDifficulty3)
        {
            enemiesDifficulty1 = null;
            enemiesDifficulty2 = null;
            enemiesDifficulty3 = null;

            // Cache the type.
            if (enemyDirectorTypeCache == null)
            {
                enemyDirectorTypeCache = Type.GetType("EnemyDirector, Assembly-CSharp");
                if (enemyDirectorTypeCache == null)
                {
                    Debug.Log("EnemyDirector type not found!");
                    return false;
                }
            }

            // Cache the instance.
            if (enemyDirectorInstanceCache == null)
            {
                enemyDirectorInstanceCache = UnityEngine.Object.FindObjectOfType(enemyDirectorTypeCache);
                if (enemyDirectorInstanceCache == null)
                {
                    Debug.Log("EnemyDirector instance not found!");
                    return false;
                }
            }

            // Cache the fields.
            if (enemiesDifficulty1FieldCache == null)
            {
                enemiesDifficulty1FieldCache = enemyDirectorTypeCache.GetField("enemiesDifficulty1", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (enemiesDifficulty1FieldCache == null)
                {
                    Debug.Log("enemiesDifficulty1 field not found!");
                    return false;
                }
            }
            if (enemiesDifficulty2FieldCache == null)
            {
                enemiesDifficulty2FieldCache = enemyDirectorTypeCache.GetField("enemiesDifficulty2", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (enemiesDifficulty2FieldCache == null)
                {
                    Debug.Log("enemiesDifficulty2 field not found!");
                    return false;
                }
            }
            if (enemiesDifficulty3FieldCache == null)
            {
                enemiesDifficulty3FieldCache = enemyDirectorTypeCache.GetField("enemiesDifficulty3", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (enemiesDifficulty3FieldCache == null)
                {
                    Debug.Log("enemiesDifficulty3 field not found!");
                    return false;
                }
            }

            enemiesDifficulty1 = enemiesDifficulty1FieldCache.GetValue(enemyDirectorInstanceCache) as List<EnemySetup>;
            enemiesDifficulty2 = enemiesDifficulty2FieldCache.GetValue(enemyDirectorInstanceCache) as List<EnemySetup>;
            enemiesDifficulty3 = enemiesDifficulty3FieldCache.GetValue(enemyDirectorInstanceCache) as List<EnemySetup>;

            if (enemiesDifficulty1 == null || enemiesDifficulty1.Count == 0)
            {
                Debug.Log("enemiesDifficulty1 is empty or null!");
                return false;
            }
            if (enemiesDifficulty2 == null || enemiesDifficulty2.Count == 0)
            {
                Debug.Log("enemiesDifficulty2 is empty or null!");
                return false;
            }
            if (enemiesDifficulty3 == null || enemiesDifficulty3.Count == 0)
            {
                Debug.Log("enemiesDifficulty3 is empty or null!");
                return false;
            }

            return true;
        }

        public static void SpawnSpecificEnemy(object levelGenerator, object enemySetup, Vector3 position)
        {
            try
            {
                var levelGeneratorType = levelGenerator.GetType();
                var enemySpawnMethod = levelGeneratorType.GetMethod("EnemySpawn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (enemySpawnMethod == null)
                {
                    Debug.Log("EnemySpawn method not found!");
                    return;
                }

                // Spawn the enemy
                var spawnedEnemy = enemySpawnMethod.Invoke(levelGenerator, new object[] { enemySetup, position });
                if (spawnedEnemy == null)
                {
                    Debug.Log("Failed to spawn enemy!");
                    return;
                }

                // Get the Enemy component
                var enemyType = Type.GetType("Enemy, Assembly-CSharp");
                var enemyComponent = (spawnedEnemy as GameObject)?.GetComponent(enemyType);
                if (enemyComponent == null)
                {
                    Debug.Log("Enemy component not found on spawned object!");
                    return;
                }

                // Initialize vision system
                var visionType = Type.GetType("EnemyVision, Assembly-CSharp");
                var visionComponent = (spawnedEnemy as GameObject)?.GetComponent(visionType);
                if (visionComponent != null)
                {
                    // Get the vision trigger method
                    var visionTriggerMethod = visionType.GetMethod("VisionTrigger", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (visionTriggerMethod != null)
                    {
                        // Get local player
                        var localPlayer = PhotonNetwork.LocalPlayer;
                        if (localPlayer != null)
                        {
                            // Get PlayerAvatar from cache
                            var playerAvatar = PlayerReflectionCache.PlayerAvatarScriptInstance;
                            if (playerAvatar != null)
                            {
                                // Get the player dictionary field from EnemyVision
                                var playerDictField = visionType.GetField("playerDict", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (playerDictField != null)
                                {
                                    var playerDict = playerDictField.GetValue(visionComponent) as Dictionary<int, object>;
                                    if (playerDict != null)
                                    {
                                        // Add the player to the dictionary if not already present
                                        if (!playerDict.ContainsKey(localPlayer.ActorNumber))
                                        {
                                            playerDict.Add(localPlayer.ActorNumber, playerAvatar);
                                        }

                                        // Now call VisionTrigger with proper parameters
                                        visionTriggerMethod.Invoke(visionComponent, new object[] {
                                            localPlayer.ActorNumber,
                                            playerAvatar,
                                            false, // culled
                                            true   // playerNear
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                // Initialize enemy state
                var enemyStateField = enemyType.GetField("state", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (enemyStateField != null)
                {
                    enemyStateField.SetValue(enemyComponent, 0); // Set to idle state
                }

                Debug.Log("Successfully spawned and initialized enemy!");
            }
            catch (Exception e)
            {
                Debug.Log($"Error in SpawnSpecificEnemy: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
