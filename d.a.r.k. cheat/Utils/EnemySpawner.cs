using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace dark_cheat.Utils
{
    public static class EnemySpawner
    {
        public static bool TryGetEnemyLists(out List<EnemySetup> enemiesDifficulty1,
                                              out List<EnemySetup> enemiesDifficulty2,
                                              out List<EnemySetup> enemiesDifficulty3)
        {
            enemiesDifficulty1 = null;
            enemiesDifficulty2 = null;
            enemiesDifficulty3 = null;

            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            var enemyDirectorInstance = UnityEngine.Object.FindObjectOfType(enemyDirectorType);
            if (enemyDirectorInstance == null)
            {
                Debug.Log("EnemyDirector instance not found!");
                return false;
            }

            var enemiesDifficulty1Field = enemyDirectorType.GetField("enemiesDifficulty1", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var enemiesDifficulty2Field = enemyDirectorType.GetField("enemiesDifficulty2", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var enemiesDifficulty3Field = enemyDirectorType.GetField("enemiesDifficulty3", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (enemiesDifficulty1Field == null)
            {
                Debug.Log("enemiesDifficulty1 field not found!");
                return false;
            }
            if (enemiesDifficulty2Field == null)
            {
                Debug.Log("enemiesDifficulty2 field not found!");
                return false;
            }
            if (enemiesDifficulty3Field == null)
            {
                Debug.Log("enemiesDifficulty3 field not found!");
                return false;
            }

            enemiesDifficulty1 = enemiesDifficulty1Field.GetValue(enemyDirectorInstance) as List<EnemySetup>;
            enemiesDifficulty2 = enemiesDifficulty2Field.GetValue(enemyDirectorInstance) as List<EnemySetup>;
            enemiesDifficulty3 = enemiesDifficulty3Field.GetValue(enemyDirectorInstance) as List<EnemySetup>;

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

        public static void Spawn2()
        {
            List<EnemySetup> enemiesDifficulty1;
            List<EnemySetup> enemiesDifficulty2;
            List<EnemySetup> enemiesDifficulty3;


            if (!TryGetEnemyLists(out enemiesDifficulty1, out enemiesDifficulty2, out enemiesDifficulty3))
            { // Retrieve enemy lists via reflection.
                return;
            }

            EnemySetup specificEnemyType = null;

            for (int i = 0; i < enemiesDifficulty1.Count; i++) // Process enemies from difficulty 1.
            {
                if (enemiesDifficulty1[i].name.Contains("Hidden"))
                {
                    Debug.Log($"Found {enemiesDifficulty1[i].name} in level 1");
                }
            }

            for (int i = 0; i < enemiesDifficulty2.Count; i++) // Process enemies from difficulty 2.
            {
                if (enemiesDifficulty2[i].name.Contains("Hidden"))
                {
                    Debug.Log($"Found {enemiesDifficulty2[i].name} in level 2");
                    specificEnemyType = enemiesDifficulty2[i];
                    break;
                }
            }

            for (int i = 0; i < enemiesDifficulty3.Count; i++) // Process enemies from difficulty 3.
            {
                if (enemiesDifficulty3[i].name.Contains("Hidden"))
                {
                    Debug.Log($"Found {enemiesDifficulty3[i].name} in level 3");
                }
            }

            if (specificEnemyType == null)
            {
                Debug.Log("EnemySetup is null!");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer(); // Use DebugCheats.GetLocalPlayer() to retrieve the local player's position.
            if (localPlayer == null)
            {
                Debug.Log("Local player not found!");
                return;
            }
            Vector3 spawnPosition = localPlayer.transform.position + Vector3.up * 1.5f;

            LevelGenerator levelGenerator = UnityEngine.Object.FindObjectOfType<LevelGenerator>();
            if (levelGenerator == null) // Retrieve LevelGenerator instance.
            {
                Debug.Log("LevelGenerator instance not found!");
                return;
            }

            SpawnSpecificEnemy(levelGenerator, specificEnemyType, spawnPosition);
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

                enemySpawnMethod.Invoke(levelGenerator, new object[] { enemySetup, position });
                Debug.Log("Successfully spawned specific enemy type!");
            }
            catch (Exception e)
            {
                Debug.Log($"Error in SpawnSpecificEnemy: {e.Message}");
            }
        }
    }
}
