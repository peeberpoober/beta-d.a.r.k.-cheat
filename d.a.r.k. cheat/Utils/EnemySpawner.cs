/* // COMMENTED OUT --- TOO UNSTABLE. NEEDS WORK.
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
*/
