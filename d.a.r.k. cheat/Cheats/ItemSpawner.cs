using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace dark_cheat
{
    public class MaterialPreserver : MonoBehaviour
    {
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private float checkInterval = 0.1f;
        private float timeUntilNextCheck = 0;
        private bool initialized = false;

        public void PreserveMaterials()
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true)) // Store all renderer material references
            {
                if (renderer != null && renderer.materials.Length > 0)
                {
                    Material[] materialCopies = new Material[renderer.materials.Length]; // Create deep copies of all materials
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        if (renderer.materials[i] != null)
                        {
                            materialCopies[i] = new Material(renderer.materials[i]);
                        }
                    }
                    originalMaterials[renderer] = materialCopies;
                }
            }

            initialized = true;
            enabled = true; // Enable the component

            RestoreMaterials(); // Force immediate check
        }

        private void RestoreMaterials()
        {
            foreach (var kvp in originalMaterials)
            {
                Renderer renderer = kvp.Key;
                Material[] materials = kvp.Value;

                if (renderer != null)
                {
                    bool needsRestore = false; // Check if current materials are different from our preserved ones

                    if (renderer.materials.Length != materials.Length)
                    {
                        needsRestore = true;
                    }
                    else
                    {
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (renderer.materials[i] == null ||
                                renderer.materials[i].shader != materials[i].shader ||
                                (renderer.materials[i].mainTexture == null && materials[i].mainTexture != null))
                            {
                                needsRestore = true;
                                break;
                            }
                        }
                    }

                    if (needsRestore)
                    {
                        renderer.materials = materials;
                        DLog.Log($"Restored materials for {renderer.gameObject.name}");
                    }
                }
            }
        }

        void Update()
        {
            if (!initialized) return;

            timeUntilNextCheck -= Time.deltaTime;
            if (timeUntilNextCheck <= 0)
            {
                RestoreMaterials();
                timeUntilNextCheck = checkInterval;
            }
        }

        private float preservationDuration = 10f; // Keep preserving for 10 seconds, then disable
        private float timeElapsed = 0f;

        void FixedUpdate()
        {
            if (!initialized) return;

            timeElapsed += Time.fixedDeltaTime;
            if (timeElapsed > preservationDuration)
            {
                enabled = false; // Disable to save performance
            }
        }
    }
    public class ItemSpawner : MonoBehaviourPunCallbacks
    {
        private static Dictionary<string, GameObject> itemPrefabCache = new Dictionary<string, GameObject>();
        private static List<string> availableItems = new List<string>();

        public static List<string> GetAvailableItems()
        {
            if (availableItems.Count == 0)
            {
                availableItems.Add("Item Cart Medium");
                availableItems.Add("Item Cart Small");
                availableItems.Add("Item Drone Battery");
                availableItems.Add("Item Drone Feather");
                availableItems.Add("Item Drone Indestructible");
                availableItems.Add("Item Drone Torque");
                availableItems.Add("Item Drone Zero Gravity");
                availableItems.Add("Item Extraction Tracker");
                availableItems.Add("Item Grenade Duct Taped");
                availableItems.Add("Item Grenade Explosive");
                availableItems.Add("Item Grenade Human");
                availableItems.Add("Item Grenade Shockwave");
                availableItems.Add("Item Grenade Stun");
                availableItems.Add("Item Gun Handgun");
                availableItems.Add("Item Gun Shotgun");
                availableItems.Add("Item Gun Tranq");
                availableItems.Add("Item Health Pack Large");
                availableItems.Add("Item Health Pack Medium");
                availableItems.Add("Item Health Pack Small");
                availableItems.Add("Item Melee Baseball Bat");
                availableItems.Add("Item Melee Frying Pan");
                availableItems.Add("Item Melee Inflatable Hammer");
                availableItems.Add("Item Melee Sledge Hammer");
                availableItems.Add("Item Melee Sword");
                availableItems.Add("Item Mine Explosive");
                availableItems.Add("Item Mine Shockwave");
                availableItems.Add("Item Mine Stun");
                availableItems.Add("Item Orb Zero Gravity");
                availableItems.Add("Item Power Crystal");
                availableItems.Add("Item Rubber Duck");
                availableItems.Add("Item Upgrade Map Player Count");
                availableItems.Add("Item Upgrade Player Stamina");
                availableItems.Add("Item Upgrade Player Extra Jump");
                availableItems.Add("Item Upgrade Player Grab Range");
                availableItems.Add("Item Upgrade Player Grab Strength");
                availableItems.Add("Item Upgrade Player Health");
                availableItems.Add("Item Upgrade Player Sprint Speed");
                availableItems.Add("Item Upgrade Player Tumble Launch");
                availableItems.Add("Item Valuable Tracker");
                availableItems.Add("Valuable Small");
                availableItems.Add("Valuable Medium");
                availableItems.Add("Valuable Large");
            }
            return availableItems;
        }

        public static void SpawnMoney(Vector3 position, int value = 45000)
        {
            try
            {
                SpawnItem("Valuable Small", position, value);
            }
            catch (Exception ex)
            {
                DLog.Log($"Error spawning money via Valuable Small: {ex.Message}");
                try
                {
                    DLog.Log("Attempting direct money spawn...");
                    CreateMoneyDirectly(position, value);
                }
                catch (Exception ex2)
                {
                    DLog.LogError($"Failed to spawn money via fallback method: {ex2.Message}");
                }
            }
        }

        private static void CreateMoneyDirectly(Vector3 position, int value)
        {
            bool isMultiplayer = SemiFunc.IsMultiplayer();
            GameObject moneyObj = new GameObject("Valuable_Spawned");
            moneyObj.transform.position = position;
            Rigidbody rb = moneyObj.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            BoxCollider collider = moneyObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.2f, 0.2f, 0.2f);
            collider.center = Vector3.zero;
            var physGrabObj = moneyObj.AddComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
            var valuableObj = moneyObj.AddComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableObj != null)
            {
                var dollarValueCurrentField = valuableObj.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                var dollarValueOriginalField = valuableObj.GetType().GetField("dollarValueOriginal", BindingFlags.Public | BindingFlags.Instance);
                var dollarValueSetField = valuableObj.GetType().GetField("dollarValueSet", BindingFlags.Public | BindingFlags.Instance);
                if (dollarValueCurrentField != null) dollarValueCurrentField.SetValue(valuableObj, (float)value);
                if (dollarValueOriginalField != null) dollarValueOriginalField.SetValue(valuableObj, (float)value);
                if (dollarValueSetField != null) dollarValueSetField.SetValue(valuableObj, true);
            }
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(moneyObj.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.84f, 0f);
                renderer.material.SetFloat("_Metallic", 1f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
            if (isMultiplayer)
            {
                ConfigureSyncComponents(moneyObj);
            }
            DLog.Log($"Created money object directly with value: {value}");
        }

        public static void SpawnItem(string itemName, Vector3 position, int value = 0)
        {
            try
            {
                DLog.Log($"Spawning item: {itemName} at position: {position}");
                bool isMultiplayer = SemiFunc.IsMultiplayer();

                if (isMultiplayer && !PhotonNetwork.IsMasterClient)
                {
                    SpawnItemNonHost(itemName, position, value);
                    return;
                }

                GameObject itemPrefab = GetItemPrefab(itemName);
                if (itemPrefab == null && itemName.Contains("Valuable"))
                {
                    DLog.Log($"Item prefab not found for: {itemName}, trying direct valuable creation");
                    if (CreateValuableDirectly(itemName, position, value))
                    {
                        return;
                    }
                }
                else if (itemPrefab == null)
                {
                    DLog.LogError($"Item prefab not found for: {itemName}");
                    return;
                }

                GameObject spawnedItem;
                object[] itemData = null;
                if (itemName.Contains("Valuable") && value > 0)
                {
                    itemData = new object[] { value };
                }

                if (!isMultiplayer)
                {
                    DLog.Log("Offline mode: Spawning locally.");
                    spawnedItem = UnityEngine.Object.Instantiate(itemPrefab, position, Quaternion.identity);
                    ConfigureSyncComponents(spawnedItem);
                    EnsureItemVisibility(spawnedItem);
                }
                else
                {
                    DLog.Log("Multiplayer mode: Spawning via Photon.");
                    string prefabPath = GetPrefabPath(itemName);
                    if (string.IsNullOrEmpty(prefabPath))
                    {
                        DLog.LogError($"Could not determine prefab path for: {itemName}");
                        return;
                    }
                    try
                    {
                        spawnedItem = PhotonNetwork.Instantiate(prefabPath, position, Quaternion.identity, 0, itemData);
                    }
                    catch (Exception ex)
                    {
                        DLog.LogError($"Photon instantiate failed: {ex.Message}. Falling back to local instantiate.");
                        spawnedItem = UnityEngine.Object.Instantiate(itemPrefab, position, Quaternion.identity);
                        ConfigureSyncComponents(spawnedItem);
                    }
                }

                if (itemName.Contains("Valuable") && value > 0)
                {
                    ConfigureValuableObject(spawnedItem, value, isMultiplayer);
                }

                ConfigurePhysicsProperties(spawnedItem, position, isMultiplayer);
                DLog.Log($"Successfully spawned {itemName}");
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error in SpawnItem: {ex.Message}\n{ex.StackTrace}");
                if (itemName.Contains("Valuable"))
                {
                    DLog.Log($"Attempting direct creation for valuable after failure");
                    CreateValuableDirectly(itemName, position, value);
                }
            }
        }

        private static void SpawnItemNonHost(string itemName, Vector3 position, int value)
        {
            try
            {
                DLog.Log($"Non-host spawning item: {itemName} at position: {position}");

                string prefabPath = GetPrefabPath(itemName); // Force a complete prefab load first
                GameObject prefabResource = Resources.Load<GameObject>(prefabPath);

                if (prefabResource == null)
                {
                    DLog.LogError($"Failed to load prefab resource for: {itemName}");
                    return;
                }

                var photonNetworkType = typeof(PhotonNetwork); // Now perform the network instantiation
                var instantiateMethod = photonNetworkType.GetMethod("NetworkInstantiate",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(InstantiateParameters), typeof(bool), typeof(bool) },
                    null);

                if (instantiateMethod == null)
                {
                    DLog.LogError("NetworkInstantiate method not found");
                    return;
                }

                var currentLevelPrefixField = photonNetworkType.GetField("currentLevelPrefix",
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (currentLevelPrefixField == null)
                {
                    DLog.LogError("currentLevelPrefix field not found");
                    return;
                }

                var currentLevelPrefix = currentLevelPrefixField.GetValue(null);

                var parameters = new InstantiateParameters(prefabPath,
                    position,
                    Quaternion.identity,
                    0,
                    null,
                    (byte)currentLevelPrefix,
                    null,
                    PhotonNetwork.LocalPlayer,
                    PhotonNetwork.ServerTimestamp);

                GameObject spawnedItem = (GameObject)instantiateMethod.Invoke(null, new object[] { parameters, true, false });

                if (spawnedItem != null)
                {
                    FindOrCreateCheatSync().StartCoroutine(SetupItemAndNotifyHost(spawnedItem, prefabResource, itemName, position, value));

                    DLog.Log($"Successfully spawned {itemName} as non-host"); // Process the local spawn first, delay the RPC
                }
                else
                {
                    DLog.LogError("NetworkInstantiate returned null");
                }
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error in SpawnItemNonHost: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static IEnumerator SetupItemAndNotifyHost(GameObject item, GameObject prefabResource, string itemName, Vector3 position, int value)
        {
            yield return new WaitForEndOfFrame(); // Wait frames for the object to be fully initialized

            try // Apply materials from the prefab directly
            {
                foreach (Renderer itemRenderer in item.GetComponentsInChildren<Renderer>(true))
                {
                    string rendererPath = GetPathRelativeToRoot(itemRenderer.transform);

                    Transform prefabTransform = prefabResource.transform; // Find the corresponding renderer in the prefab
                    foreach (string pathPart in rendererPath.Split('/'))
                    {
                        if (string.IsNullOrEmpty(pathPart)) continue;
                        prefabTransform = prefabTransform.Find(pathPart);
                        if (prefabTransform == null) break;
                    }

                    if (prefabTransform != null)
                    {
                        Renderer prefabRenderer = prefabTransform.GetComponent<Renderer>();
                        if (prefabRenderer != null && prefabRenderer.sharedMaterials.Length > 0)
                        {
                            Material[] newMaterials = new Material[prefabRenderer.sharedMaterials.Length]; // Create new material instances to avoid sharing
                            for (int i = 0; i < prefabRenderer.sharedMaterials.Length; i++)
                            {
                                if (prefabRenderer.sharedMaterials[i] != null)
                                {
                                    newMaterials[i] = new Material(prefabRenderer.sharedMaterials[i]);
                                }
                            }

                            itemRenderer.materials = newMaterials; // Apply the materials
                        }
                    }
                }

                MaterialPreserver preserver = item.AddComponent<MaterialPreserver>(); // Add the material preserver component
                preserver.PreserveMaterials();

                if (itemName.Contains("Valuable") && value > 0) // Setup valuable component
                {
                    var valuableComponent = item.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
                    if (valuableComponent != null)
                    {
                        SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
                        SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
                        SetFieldValue(valuableComponent, "dollarValueSet", true);

                        var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                        if (dollarValueRPC != null)
                        {
                            dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                        }
                    }
                }

                var physComponent = item.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp")); // Apply physics
                if (physComponent != null)
                {
                    SetFieldValue(physComponent, "spawnTorque", UnityEngine.Random.insideUnitSphere * 0.05f);
                }
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error setting up item: {ex.Message}\n{ex.StackTrace}");
            }

            yield return new WaitForSeconds(0.1f); // Wait a bit more to make sure our setup is complete - outside the try block

            try
            {
                PlayerCheatSync cheatSync = FindOrCreateCheatSync(); // Now notify the host
                if (cheatSync != null && cheatSync.photonView != null)
                {
                    cheatSync.photonView.RPC("SpawnItemMirrorRPC", RpcTarget.MasterClient,
                        itemName, position, value, PhotonNetwork.LocalPlayer.ActorNumber);
                }

                DLog.Log($"Successfully finalized spawn and notified host of {itemName}");
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error notifying host: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static IEnumerator ApplyMaterialsWithDelay(GameObject item, Dictionary<string, Material> materials, string itemName, int value)
        {
            yield return new WaitForEndOfFrame(); // Wait frames to ensure the object is fully initialized
            yield return new WaitForEndOfFrame();

            try
            {
                foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>(true)) // Apply materials to renderers
                {
                    if (renderer != null)
                    {
                        List<Material> newMaterials = new List<Material>();

                        foreach (Material originalMat in renderer.sharedMaterials) // Get original materials to determine names
                        {
                            if (originalMat != null && materials.ContainsKey(originalMat.name))
                            {
                                newMaterials.Add(materials[originalMat.name]); // Use our cached material
                            }
                            else
                            {
                                string bestMatch = null; // Try to find by partial name match
                                foreach (string matName in materials.Keys)
                                {
                                    if (originalMat != null && originalMat.name.Contains(matName) ||
                                        (matName.Contains(originalMat != null ? originalMat.name : "default")))
                                    {
                                        bestMatch = matName;
                                        break;
                                    }
                                }

                                if (bestMatch != null)
                                {
                                    newMaterials.Add(materials[bestMatch]);
                                }
                                else
                                { // Add a placeholder if needed
                                    newMaterials.Add(originalMat != null ? originalMat : new Material(Shader.Find("Standard")));
                                }
                            }
                        }

                        if (newMaterials.Count > 0) // Apply the materials
                        {
                            renderer.materials = newMaterials.ToArray();
                        }

                        renderer.enabled = false; // Force renderer to refresh
                        renderer.enabled = true;
                    }
                }

                if (itemName.Contains("Valuable") && value > 0) // Setup valuable component
                {
                    var valuableComponent = item.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
                    if (valuableComponent != null)
                    {
                        SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
                        SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
                        SetFieldValue(valuableComponent, "dollarValueSet", true);

                        var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                        if (dollarValueRPC != null)
                        {
                            dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                        }
                    }
                }

                var physComponent = item.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
                if (physComponent != null)
                {
                    SetFieldValue(physComponent, "spawnTorque", UnityEngine.Random.insideUnitSphere * 0.05f);
                }

                DLog.Log($"Successfully finalized spawn of {itemName}");
            }
            catch (Exception ex)
            {
                DLog.LogError($"Error applying materials: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void CopyRenderersFromPrefab(GameObject target, GameObject prefab)
        {
            if (target == null || prefab == null) return;

            var targetRenderers = target.GetComponentsInChildren<Renderer>(true); // Check if target has any renderers
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                var prefabRenderers = prefab.GetComponentsInChildren<Renderer>(true); // No renderers, need to copy from prefab
                foreach (var prefabRenderer in prefabRenderers)
                {
                    Transform targetChild = FindCorrespondingChild(target.transform, prefabRenderer.transform);
                    if (targetChild == null) // Find the corresponding child object in the target
                    {
                        targetChild = new GameObject(prefabRenderer.name).transform; // Create the child if it doesn't exist
                        targetChild.SetParent(target.transform);
                        targetChild.localPosition = prefabRenderer.transform.localPosition;
                        targetChild.localRotation = prefabRenderer.transform.localRotation;
                        targetChild.localScale = prefabRenderer.transform.localScale;
                    }

                    Renderer targetRenderer = targetChild.GetComponent<Renderer>(); // Add renderer component if missing
                    if (targetRenderer == null)
                    {
                        if (prefabRenderer is MeshRenderer) // Copy renderer type
                        {
                            targetRenderer = targetChild.gameObject.AddComponent<MeshRenderer>();

                            MeshFilter prefabMeshFilter = prefabRenderer.GetComponent<MeshFilter>(); // Add mesh filter if needed
                            if (prefabMeshFilter != null && prefabMeshFilter.sharedMesh != null)
                            {
                                MeshFilter targetMeshFilter = targetChild.GetComponent<MeshFilter>();
                                if (targetMeshFilter == null)
                                {
                                    targetMeshFilter = targetChild.gameObject.AddComponent<MeshFilter>();
                                }
                                targetMeshFilter.sharedMesh = prefabMeshFilter.sharedMesh;
                            }
                        }
                        else if (prefabRenderer is SkinnedMeshRenderer)
                        {
                            SkinnedMeshRenderer smr = targetChild.gameObject.AddComponent<SkinnedMeshRenderer>();
                            SkinnedMeshRenderer prefabSMR = prefabRenderer as SkinnedMeshRenderer;
                            if (prefabSMR.sharedMesh != null)
                            {
                                smr.sharedMesh = prefabSMR.sharedMesh;
                            }
                        }
                    }

                    if (targetRenderer != null && prefabRenderer.sharedMaterials.Length > 0)
                    {
                        Material[] materials = new Material[prefabRenderer.sharedMaterials.Length];
                        for (int i = 0; i < prefabRenderer.sharedMaterials.Length; i++) // Instead of just setting shared materials, create proper material instances
                        {
                            if (prefabRenderer.sharedMaterials[i] != null)
                            {
                                materials[i] = new Material(prefabRenderer.sharedMaterials[i]); // Create a new instance of the material

                                foreach (var property in materials[i].GetTexturePropertyNames()) // Force material to refresh its textures
                                {
                                    Texture texture = materials[i].GetTexture(property);
                                    if (texture != null)
                                    {
                                        materials[i].SetTexture(property, texture);
                                    }
                                }
                            }
                        }

                        targetRenderer.materials = materials; // Apply as materials (not shared) to ensure unique instance

                        targetRenderer.sharedMaterials = prefabRenderer.sharedMaterials;// Also set shared materials for consistency
                    }
                }
            }
        }

        private static Transform FindCorrespondingChild(Transform parent, Transform prefabChild)
        {
            Transform child = parent.Find(prefabChild.name); // Try to find by name
            if (child != null) return child;

            string path = GetPathRelativeToRoot(prefabChild); // Try to find by hierarchy path
            string[] pathParts = path.Split('/');

            Transform current = parent;
            for (int i = 0; i < pathParts.Length; i++)
            {
                Transform next = current.Find(pathParts[i]);
                if (next == null) return null;
                current = next;
            }

            return current;
        }

        private static string GetPathRelativeToRoot(Transform transform)
        {
            List<string> pathParts = new List<string>();
            Transform current = transform;
            while (current.parent != null)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }

            pathParts.Reverse();
            return string.Join("/", pathParts);
        }

        private static void SetFieldValue(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                DLog.LogError($"{fieldName} field not found");
            }
        }

        private static IEnumerator FinalizeItemSpawn(GameObject spawnedItem, GameObject prefab, string itemName, int value)
        {
            yield return new WaitForEndOfFrame(); // Give time for resources to load

            CopyRenderersFromPrefab(spawnedItem, prefab); // Apply components and materials

            if (itemName.Contains("Valuable") && value > 0) // Set up valuable component if needed
            {
                var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
                if (valuableComponent != null)
                {
                    SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
                    SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
                    SetFieldValue(valuableComponent, "dollarValueSet", true);

                    var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                    if (dollarValueRPC != null)
                    {
                        dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                    }
                }
            }

            foreach (Renderer renderer in spawnedItem.GetComponentsInChildren<Renderer>(true)) // Force material refresh
            {
                if (renderer != null && renderer.materials.Length > 0)
                {
                    Material[] mats = renderer.materials;
                    renderer.materials = mats; // Re-apply to force refresh
                }
            }

            EnsureItemVisibility(spawnedItem);
            DLog.Log($"Successfully finalized spawn of {itemName}");
        }

        private static PlayerCheatSync FindOrCreateCheatSync()
        {
            PlayerCheatSync cheatSync = UnityEngine.Object.FindObjectOfType<PlayerCheatSync>();
            if (cheatSync == null)
            {
                GameObject cheatSyncObj = new GameObject("CheatSync");
                cheatSync = cheatSyncObj.AddComponent<PlayerCheatSync>();
                UnityEngine.Object.DontDestroyOnLoad(cheatSyncObj);
            }
            return cheatSync;
        }

        private static bool CreateValuableDirectly(string itemName, Vector3 position, int value)
        {
            try
            {
                DLog.Log($"Creating valuable directly: {itemName} with value {value}");
                float sizeMultiplier = 1.0f;
                if (itemName.Contains("Medium")) sizeMultiplier = 1.5f;
                else if (itemName.Contains("Large")) sizeMultiplier = 2.0f;
                GameObject valuableObj = new GameObject(itemName + "_Spawned");
                valuableObj.transform.position = position;
                Rigidbody rb = valuableObj.AddComponent<Rigidbody>();
                rb.mass = 1f * sizeMultiplier;
                rb.drag = 0.1f;
                rb.angularDrag = 0.05f;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                BoxCollider collider = valuableObj.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.2f, 0.2f, 0.2f) * sizeMultiplier;
                collider.center = Vector3.zero;
                var physGrabObjType = Type.GetType("PhysGrabObject, Assembly-CSharp");
                if (physGrabObjType != null)
                {
                    var physGrabObj = valuableObj.AddComponent(physGrabObjType);
                    var midPointField = physGrabObjType.GetField("midPoint", BindingFlags.Public | BindingFlags.Instance);
                    if (midPointField != null) midPointField.SetValue(physGrabObj, position);
                    var targetPositionField = physGrabObjType.GetField("targetPosition", BindingFlags.Public | BindingFlags.Instance);
                    if (targetPositionField != null) targetPositionField.SetValue(physGrabObj, position);
                }
                var valuableObjType = Type.GetType("ValuableObject, Assembly-CSharp");
                if (valuableObjType != null)
                {
                    var valuableComponent = valuableObj.AddComponent(valuableObjType);
                    var dollarValueCurrentField = valuableObjType.GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                    var dollarValueOriginalField = valuableObjType.GetField("dollarValueOriginal", BindingFlags.Public | BindingFlags.Instance);
                    var dollarValueSetField = valuableObjType.GetField("dollarValueSet", BindingFlags.Public | BindingFlags.Instance);
                    if (dollarValueCurrentField != null) dollarValueCurrentField.SetValue(valuableComponent, (float)value);
                    if (dollarValueOriginalField != null) dollarValueOriginalField.SetValue(valuableComponent, (float)value);
                    if (dollarValueSetField != null) dollarValueSetField.SetValue(valuableComponent, true);
                    var excludeFromExtractionField = valuableObjType.GetField("excludeFromExtraction", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (excludeFromExtractionField != null)
                    {
                        excludeFromExtractionField.SetValue(valuableComponent, true);
                    }
                    var discoveredField = valuableObjType.GetField("discovered", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (discoveredField != null)
                    {
                        discoveredField.SetValue(valuableComponent, true);
                    }
                    var addedToHaulListField = valuableObjType.GetField("addedToDollarHaulList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (addedToHaulListField != null)
                    {
                        addedToHaulListField.SetValue(valuableComponent, true);
                    }
                    var dollarValueRPC = valuableObjType.GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
                    if (dollarValueRPC != null)
                    {
                        dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                    }
                }
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(valuableObj.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f) * sizeMultiplier;
                Renderer renderer = visual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(1f, 0.84f, 0f);
                    renderer.material.SetFloat("_Metallic", 1f);
                    renderer.material.SetFloat("_Glossiness", 0.8f);
                }
                if (SemiFunc.IsMultiplayer())
                {
                    ConfigureSyncComponents(valuableObj);
                }
                valuableObj.tag = "SpawnedValuable";
                var mapInstance = typeof(Map).GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (mapInstance != null)
                {
                    valuableObj.AddComponent<ExcludeFromMapTracking>();
                }
                DLog.Log($"Created valuable object directly: {itemName} with value: {value}");
                return true;
            }
            catch (Exception ex)
            {
                DLog.LogError($"Failed to create valuable directly: {ex.Message}");
                return false;
            }
        }

        private class ExcludeFromMapTracking : MonoBehaviour
        {
            void Start()
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        public static string GetPrefabPath(string itemName)
        {
            if (itemName.Contains("Valuable"))
            {
                return "Valuables/" + itemName;
            }
            else if (itemName.StartsWith("Item "))
            {
                return "Items/" + itemName;
            }
            return "Items/" + itemName;
        }

        private static GameObject GetItemPrefab(string itemName)
        {
            if (itemPrefabCache.ContainsKey(itemName) && itemPrefabCache[itemName] != null)
            {
                return itemPrefabCache[itemName];
            }
            GameObject prefab = null;
            if (itemName.Contains("Valuable"))
            {
                if (itemName.Contains("Valuable"))
                {
                    if (itemName == "Valuable Small" && AssetManager.instance.surplusValuableSmall != null)
                    {
                        prefab = AssetManager.instance.surplusValuableSmall;
                    }
                    else
                    {
                        string fieldName = null;
                        if (itemName == "Valuable Small") fieldName = "surplusValuableSmall";
                        else if (itemName == "Valuable Medium") fieldName = "surplusValuableMedium";
                        else if (itemName == "Valuable Large") fieldName = "surplusValuableLarge";
                        if (fieldName != null && AssetManager.instance != null)
                        {
                            var field = AssetManager.instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (field != null)
                            {
                                prefab = field.GetValue(AssetManager.instance) as GameObject;
                            }
                        }
                    }
                    if (prefab == null)
                    {
                        DLog.Log($"Trying to find an existing valuable in the scene for: {itemName}");
                        var valuableObjects = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
                        if (valuableObjects != null && valuableObjects.Length > 0)
                        {
                            foreach (var obj in valuableObjects)
                            {
                                GameObject go = (obj as MonoBehaviour)?.gameObject;
                                if (go != null)
                                {
                                    prefab = go;
                                    DLog.Log($"Using existing valuable object as template: {go.name}");
                                    break;
                                }
                            }
                        }
                    }
                    if (prefab == null)
                    {
                        string[] resourcePaths = {
                            "Valuables/Valuable",
                            "Valuables/ValuableSmall",
                            "Valuables/ValuableMedium",
                            "Valuables/ValuableLarge",
                            "Prefabs/Valuable",
                            "Prefabs/ValuableSmall"
                        };
                        foreach (string path in resourcePaths)
                        {
                            prefab = Resources.Load<GameObject>(path);
                            if (prefab != null)
                            {
                                DLog.Log($"Found valuable prefab at Resources path: {path}");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    prefab = Resources.Load<GameObject>("Valuables/" + itemName);
                }
            }
            else
            {
                var statsManager = StatsManager.instance;
                if (statsManager != null)
                {
                    var itemDictionaryField = statsManager.GetType().GetField("itemDictionary", BindingFlags.Public | BindingFlags.Instance);
                    if (itemDictionaryField != null)
                    {
                        var itemDictionary = itemDictionaryField.GetValue(statsManager) as Dictionary<string, Item>;
                        if (itemDictionary != null && itemDictionary.ContainsKey(itemName))
                        {
                            var item = itemDictionary[itemName];
                            if (item != null && item.prefab != null)
                            {
                                prefab = item.prefab;
                            }
                        }
                    }
                }
                if (prefab == null)
                {
                    prefab = Resources.Load<GameObject>("Items/" + itemName);
                }
            }
            if (prefab != null)
            {
                itemPrefabCache[itemName] = prefab;
            }
            else
            {
                DLog.LogError($"Could not find prefab for item: {itemName}");
            }
            return prefab;
        }

        private static void ConfigureSyncComponents(GameObject item)
        {
            PhotonView pv = item.GetComponent<PhotonView>() ?? item.AddComponent<PhotonView>();
            pv.ViewID = PhotonNetwork.AllocateViewID(0);
            DLog.Log("PhotonView added to item: " + pv.ViewID);
            PhotonTransformView transformView = item.GetComponent<PhotonTransformView>() ?? item.AddComponent<PhotonTransformView>();
            DLog.Log("PhotonTransformView added to item");
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                PhotonRigidbodyView rigidbodyView = item.GetComponent<PhotonRigidbodyView>() ?? item.AddComponent<PhotonRigidbodyView>();
                rigidbodyView.m_SynchronizeVelocity = true;
                rigidbodyView.m_SynchronizeAngularVelocity = true;
                DLog.Log("PhotonRigidbodyView added and configured on item");
            }
            if (item.GetComponent<ItemSync>() == null)
            {
                item.AddComponent<ItemSync>();
            }
            pv.ObservedComponents = new List<Component> { transformView };
            if (rb != null)
            {
                PhotonRigidbodyView rigidbodyView = item.GetComponent<PhotonRigidbodyView>();
                if (rigidbodyView != null)
                {
                    pv.ObservedComponents.Add(rigidbodyView);
                }
            }
            pv.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
            EnsureItemVisibility(item);
        }

        private static void ConfigureValuableObject(GameObject spawnedItem, int value, bool isMultiplayer)
        {
            var valuableComponent = spawnedItem.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableComponent == null)
            {
                DLog.LogError("ValuableObject component not found");
                return;
            }
            DLog.Log("ValuableObject component found");
            SetFieldValue(valuableComponent, "dollarValueOverride", value);
            SetFieldValue(valuableComponent, "dollarValueOriginal", (float)value);
            SetFieldValue(valuableComponent, "dollarValueCurrent", (float)value);
            SetFieldValue(valuableComponent, "dollarValueSet", true);
            SetFieldValue(valuableComponent, "excludeFromExtraction", true);
            var dollarHaulListField = valuableComponent.GetType().GetField("addedToDollarHaulList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (dollarHaulListField != null)
            {
                dollarHaulListField.SetValue(valuableComponent, true);
            }
            var discoveredField = valuableComponent.GetType().GetField("discovered", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (discoveredField != null)
            {
                discoveredField.SetValue(valuableComponent, true);
            }
            var dollarValueRPC = valuableComponent.GetType().GetMethod("DollarValueSetRPC", BindingFlags.Public | BindingFlags.Instance);
            if (dollarValueRPC != null)
            {
                dollarValueRPC.Invoke(valuableComponent, new object[] { (float)value });
                if (isMultiplayer)
                {
                    var photonView = valuableComponent.GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        photonView.RequestOwnership();
                        photonView.RPC("DollarValueSetRPC", RpcTarget.Others, (float)value);
                    }
                }
            }
            else
            {
                DLog.LogError("DollarValueSetRPC method not found");
            }
            try
            {
                var statsManager = StatsManager.instance;
                if (statsManager != null)
                {
                    string valuableIdentifier = "";
                    var instanceNameField = spawnedItem.GetComponent(Type.GetType("ItemAttributes, Assembly-CSharp"))?.GetType().GetField("instanceName");
                    if (instanceNameField != null)
                    {
                        valuableIdentifier = instanceNameField.GetValue(spawnedItem.GetComponent(Type.GetType("ItemAttributes, Assembly-CSharp"))) as string;
                    }
                    if (!string.IsNullOrEmpty(valuableIdentifier))
                    {
                        var removeValuableMethod = statsManager.GetType().GetMethod("RemoveValuableFromHaul", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (removeValuableMethod != null)
                        {
                            removeValuableMethod.Invoke(statsManager, new object[] { valuableIdentifier });
                            DLog.Log($"Removed valuable {valuableIdentifier} from extraction goal tracking");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DLog.Log($"Error trying to remove valuable from tracking: {ex.Message}");
            }
        }

        private static void ConfigurePhysicsProperties(GameObject spawnedItem, Vector3 position, bool isMultiplayer)
        {
            var physComponent = spawnedItem.GetComponent(Type.GetType("PhysGrabObject, Assembly-CSharp"));
            if (physComponent == null)
            {
                DLog.LogError("PhysGrabObject component not found");
                return;
            }
            DLog.Log("PhysGrabObject component found");
            SetFieldValue(physComponent, "spawnTorque", UnityEngine.Random.insideUnitSphere * 0.05f);
            if (isMultiplayer)
            {
                var photonView = spawnedItem.GetComponent<PhotonView>();
                photonView?.RPC("SetPositionRPC", RpcTarget.MasterClient, position, Quaternion.identity);
            }
        }

        private static void EnsureItemVisibility(GameObject item)
        {

            foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>(true)) // Add material texture verification
            {
                bool needsTextureRefresh = false; // Check if any material has missing textures
                foreach (Material mat in renderer.materials)
                {
                    if (mat != null)
                    {
                        if (mat.HasProperty("_MainTex") && mat.mainTexture == null) // Check for main texture
                        {
                            needsTextureRefresh = true;
                            break;
                        }
                    }
                }

                if (needsTextureRefresh)
                {
                    string itemName = item.name.Replace("(Clone)", "").Trim(); // Try reloading materials from prefab
                    GameObject prefab = Resources.Load<GameObject>(GetPrefabPath(itemName));
                    if (prefab != null)
                    {
                        Renderer prefabRenderer = prefab.GetComponentInChildren<Renderer>();
                        if (prefabRenderer != null)
                        {
                            renderer.sharedMaterials = prefabRenderer.sharedMaterials;
                        }
                    }
                }
            }
        }
        public static void SpawnSelectedItemMultiple(int count, List<string> availableItemsList, int selectedItemToSpawnIndex, int itemSpawnValue)
        {
            if (availableItemsList == null || availableItemsList.Count == 0 || selectedItemToSpawnIndex >= availableItemsList.Count)
            {
                DLog.LogError("No valid item selected to spawn multiple times.");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null)
            {
                DLog.LogError("Local player not found for multiple spawn.");
                return;
            }

            string itemName = availableItemsList[selectedItemToSpawnIndex];
            bool isValuable = itemName.Contains("Valuable");

            Vector3 basePos = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up * 1f;

            for (int i = 0; i < count; i++)
            {
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 1f;
                Vector3 spawnPos = basePos + offset;

                if (isValuable)
                    SpawnItem(itemName, spawnPos, itemSpawnValue);
                else
                    SpawnItem(itemName, spawnPos);
            }

            DLog.Log($"Spawned {count}x {itemName}");
        }
    }
}
