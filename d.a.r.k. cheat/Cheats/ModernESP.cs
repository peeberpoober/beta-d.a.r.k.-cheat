using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace dark_cheat
{
    public class ModernESP : MonoBehaviour
    {
        private static Dictionary<ValuableObject, TextMeshPro> trackedItems = new Dictionary<ValuableObject, TextMeshPro>();
        private static Dictionary<Enemy, TextMeshPro> trackedEnemies = new Dictionary<Enemy, TextMeshPro>();

        public static float itemTextSize = 3f;
        public static float enemyTextSize = 3f;
        public static float sortFromPrice = 0;
        public static float sortToPrice = 9999;
        public static bool sortByPrice = false;

        public static void Render()
        {
            if (Hax2.useModernESP)
            {
                RenderItems();
                RenderEnemies();
            }
            else
            {
                ClearItemLabels();
                ClearEnemyLabels();
            }
        }

        // ================= ITEM ESP =================

        private static void RenderItems()
        {
            if (!DebugCheats.drawItemEspBool)
            {
                ClearItemLabels();
                return;
            }

            var valuableList = ValuableDirector.instance?.valuableList;
            if (valuableList == null) return;

            Camera cam = Camera.main;

            foreach (var item in valuableList)
            {
                if (item == null || item.gameObject == null) continue;

                if (!trackedItems.TryGetValue(item, out var label))
                    CreateItemLabel(item, cam);
                else
                    UpdateItemLabel(item, label, cam);
            }
        }

        private static void CreateItemLabel(ValuableObject item, Camera cam)
        {

            GameObject labelObj = new GameObject("ESP_Label_Item_" + item.name);
            labelObj.transform.SetParent(item.transform, false);

            TextMeshPro text = labelObj.AddComponent<TextMeshPro>();
            text.fontSize = 3f;
            text.color = new Color(1f, 1f, 1f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.isOverlay = true;
            text.fontSharedMaterial = text.font.material;

            trackedItems[item] = text;

            if (cam != null)
            {
                labelObj.transform.LookAt(cam.transform);
                labelObj.transform.Rotate(0, 180, 0);
            }

            UpdateItemLabel(item, text, cam);
        }

        private static void UpdateItemLabel(ValuableObject item, TextMeshPro label, Camera cam)
        {
            float dist = Vector3.Distance(cam.transform.position, item.transform.position);
            float size = Mathf.Clamp((0.2f + dist) - 1, 0.2f, itemTextSize);

            bool inPriceRange = item.dollarValueCurrent >= sortFromPrice &&
                                item.dollarValueCurrent <= sortToPrice;

            label.fontSize = size;
            label.color = new Color(1f, 1f, 1f, 1f);
            label.text = sortByPrice && !inPriceRange ? "" : GetItemInfo(item);

            if (cam != null)
                label.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
        }

        private static string GetItemInfo(ValuableObject item)
        {
            string name = item.name.Replace("Valuable", "").Replace("(Clone)", "").Trim();
            string info = "□";

            if (DebugCheats.showItemNames) info += $"\n{name}";
            if (DebugCheats.showItemValue) info += $"\n<b>{item.dollarValueCurrent}$</b>";

            return info;
        }

        public static void ClearItemLabels()
        {
            foreach (var label in trackedItems.Values)
            {
                if (label != null)
                    GameObject.Destroy(label.gameObject);
            }
            trackedItems.Clear();
        }

        // ================= ENEMY ESP =================

        private static void RenderEnemies()
        {
            if (!DebugCheats.drawEspBool)
            {
                ClearEnemyLabels();
                return;
            }

            var enemyParents = EnemyDirector.instance?.enemiesSpawned;
            if (enemyParents == null || enemyParents.Count == 0) return;

            Camera cam = Camera.main;

            foreach (var enemyParent in enemyParents)
            {
                var enemy = enemyParent.GetComponentInChildren<Enemy>();
                if (enemy == null || enemy.gameObject == null) continue;

                if (!trackedEnemies.TryGetValue(enemy, out var label))
                    CreateEnemyLabel(enemy, cam);
                else
                    UpdateEnemyLabel(enemyParent, enemy, label, cam);
            }
        }

        private static void CreateEnemyLabel(Enemy enemy, Camera cam)
        {
            GameObject labelObj = new GameObject("ESP_Label_Enemy_" + enemy.name);
            labelObj.transform.SetParent(enemy.transform, false);

            TextMeshPro text = labelObj.AddComponent<TextMeshPro>();
            text.fontSize = 3f;
            text.color = new Color(1f, 1f, 1f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.isOverlay = true;
            text.fontSharedMaterial = text.font.material;

            trackedEnemies[enemy] = text;

            if (cam != null)
            {
                labelObj.transform.LookAt(cam.transform);
                labelObj.transform.Rotate(0, 180, 0);
            }

            UpdateEnemyLabel(null, enemy, text, cam);
        }

        private static void UpdateEnemyLabel(EnemyParent enemyParent, Enemy enemy, TextMeshPro label, Camera cam)
        {
            float dist = Vector3.Distance(cam.transform.position, enemy.transform.position);
            float size = Mathf.Clamp((0.2f + dist) - 1, 0.2f, enemyTextSize);

            label.fontSize = size;
            label.color = new Color(1f, 1f, 1f, 1f);
            label.text = GetEnemyInfo(enemyParent, enemy);

            if (cam != null)
                label.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
        }

        private static string GetEnemyInfo(EnemyParent enemyParent, Enemy enemy)
        {
            if (enemyParent == null) return "";

            string name = enemyParent.enemyName.Replace("Enemy -", "").Replace("(Clone)", "").Trim();
            string info = "□";

            if (DebugCheats.showEnemyNames) info += $"\n{name}";

            if (DebugCheats.showEnemyHP)
            {
                object hp = typeof(EnemyHealth)
                    .GetField("healthCurrent", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(enemy.GetComponent<EnemyHealth>());
                info += $"\n<b>{hp}HP</b>";
            }

            return info;
        }

        public static void ClearEnemyLabels()
        {
            foreach (var label in trackedEnemies.Values)
            {
                if (label != null)
                    GameObject.Destroy(label.gameObject);
            }
            trackedEnemies.Clear();
        }
    }
}
