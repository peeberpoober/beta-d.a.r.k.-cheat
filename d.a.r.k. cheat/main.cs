using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections;

namespace dark_cheat
{
    public class Loader
    {
        private static object harmonyInstance;
        private static GameObject Load;

        public static bool hasTriggeredRecovery = false;
        private static void HandleUnityLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Warning && condition.Contains("Unicode value") && condition.Contains("font asset"))
                return;

            if (type == LogType.Error && condition.Contains("DefaultPool failed to load"))
            {
                if (!hasTriggeredRecovery)
                {
                    hasTriggeredRecovery = true;
                    Debug.Log("[AutoRecovery] Detected missing prefab error. Triggering recovery...");

                    Hax2.CoroutineHost.StartCoroutine(DelayedRecovery(1.0f));
                }
            }
        }

        private static IEnumerator DelayedRecovery(float delay)
        {
            yield return new WaitForSeconds(delay);

            Troll.SceneRecovery();

            yield return new WaitForSeconds(5.0f);
            hasTriggeredRecovery = false;
        }

        public static void Init()
        {
            try
            {
                Directory.CreateDirectory("C:\\temp");
                File.WriteAllText("C:\\temp\\inject_debug.txt", "Init() reached\n");

                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    string resourceName = args.Name.Split(',')[0] + ".dll";
                    var executingAssembly = Assembly.GetExecutingAssembly();
                    string fullName = executingAssembly
                        .GetManifestResourceNames()
                        .FirstOrDefault(r => r.EndsWith(resourceName));

                    if (fullName != null)
                    {
                        using (var stream = executingAssembly.GetManifestResourceStream(fullName))
                        {
                            if (stream != null)
                            {
                                byte[] buffer = new byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);
                                return Assembly.Load(buffer);
                            }
                        }
                    }

                    return null;
                };

                Load = new GameObject();
                Load.AddComponent<Hax2>();
                UnityEngine.Object.DontDestroyOnLoad(Load);
                Load.AddComponent<PatchDelay>();
                Application.logMessageReceived += HandleUnityLog;
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\temp\\inject_error.txt", ex.ToString());
            }
        }

        public static IEnumerator DelayedPatchRoutine()
        {
            while (Type.GetType("SpectateCamera, Assembly-CSharp") == null ||
                   Type.GetType("InputManager, Assembly-CSharp") == null)
            {
                yield return new WaitForSeconds(0.5f);
                File.AppendAllText("C:\\temp\\inject_debug.txt", "Waiting for types...\n");
            }

            try
            {
                File.AppendAllText("C:\\temp\\inject_debug.txt", "Types found, creating Harmony...\n");
                var harmony = new HarmonyLib.Harmony("dark_cheat");
                harmony.PatchAll(typeof(Patches).Assembly);
                File.AppendAllText("C:\\temp\\inject_debug.txt", "Harmony patches applied successfully\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\temp\\inject_debug.txt", "Harmony error: " + ex.ToString() + "\n");
            }
        }

        public static void UnloadCheat()
        {
            try
            {
                if (Load != null)
                {
                    UnityEngine.Object.Destroy(Load);
                    Load = null;
                }

                if (harmonyInstance != null)
                {
                    var harmonyType = harmonyInstance.GetType();
                    var unpatchSelf = harmonyType.GetMethod("UnpatchSelf");
                    unpatchSelf?.Invoke(harmonyInstance, null);
                    harmonyInstance = null;
                }

                GC.Collect();
                File.AppendAllText("C:\\temp\\inject_debug.txt", "UnloadCheat() completed\n");
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\temp\\unload_error.txt", ex.ToString());
            }
        }
    }

    public class PatchDelay : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(Loader.DelayedPatchRoutine());
        }
    }
}
