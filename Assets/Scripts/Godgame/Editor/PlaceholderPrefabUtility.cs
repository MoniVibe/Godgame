using Godgame.Authoring;
using Godgame.Modules;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor
{
    /// <summary>
    /// Creates placeholder prefabs (pure data + primitive meshes) for quick DOTS conversion and testing.
    /// </summary>
    public static class PlaceholderPrefabUtility
    {
        private const string RootFolder = "Assets/Placeholders";

        [MenuItem("Tools/Godgame/Create Placeholder Prefabs")]
        public static void CreateAllPlaceholders()
        {
            EnsureFolder(RootFolder);
            CreateVillagerPrefab();
            CreateBuildingPrefab();
            CreateWagonPrefab();
            CreateResourceNodePrefab();
            CreateModulePrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Godgame] Placeholder prefabs created under Assets/Placeholders");
        }

        private static void CreateVillagerPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // barrel-ish
            go.name = "Villager_Placeholder";
            go.AddComponent<VillagerAuthoring>();
            var prefabPath = $"{RootFolder}/Villager_Placeholder.prefab";
            SavePrefab(go, prefabPath);
        }

        private static void CreateBuildingPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Building_Placeholder";
            go.AddComponent<BuildingModuleSlotsAuthoring>();
            var prefabPath = $"{RootFolder}/Building_Placeholder.prefab";
            SavePrefab(go, prefabPath);
        }

        private static void CreateWagonPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Wagon_Placeholder";
            go.AddComponent<WagonModuleSlotsAuthoring>();
            var prefabPath = $"{RootFolder}/Wagon_Placeholder.prefab";
            SavePrefab(go, prefabPath);
        }

        private static void CreateResourceNodePrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ResourceNode_Placeholder";
            go.AddComponent<ResourceNodeAuthoring>();
            var prefabPath = $"{RootFolder}/ResourceNode_Placeholder.prefab";
            SavePrefab(go, prefabPath);
        }

        private static void CreateModulePrefabs()
        {
            var modulesFolder = $"{RootFolder}/Modules";
            EnsureFolder(modulesFolder);

            CreateModulePrefab(modulesFolder, "Module_Mainhand_Placeholder", ModuleSlotIds.MainHand, PrimitiveType.Capsule);
            CreateModulePrefab(modulesFolder, "Module_Offhand_Placeholder", ModuleSlotIds.OffHand, PrimitiveType.Cube);
            CreateModulePrefab(modulesFolder, "Module_Armor_Placeholder", ModuleSlotIds.TorsoArmor, PrimitiveType.Cube, maxCond: 200f);
            CreateModulePrefab(modulesFolder, "Module_Underlayer_Placeholder", ModuleSlotIds.TorsoUnderlayer, PrimitiveType.Quad, maxCond: 80f);
            CreateModulePrefab(modulesFolder, "Module_Amulet_Placeholder", ModuleSlotIds.NeckAmulet, PrimitiveType.Sphere, maxCond: 50f);
            CreateModulePrefab(modulesFolder, "Module_Ring_Placeholder", ModuleSlotIds.RingLeft, PrimitiveType.Sphere, maxCond: 40f);
            CreateModulePrefab(modulesFolder, "Module_Trinket_Placeholder", ModuleSlotIds.TrinketA, PrimitiveType.Sphere, maxCond: 60f);
            CreateModulePrefab(modulesFolder, "Module_Backpack_Placeholder", ModuleSlotIds.Backpack, PrimitiveType.Cube, maxCond: 120f);
            CreateModulePrefab(modulesFolder, "Module_Cloak_Placeholder", ModuleSlotIds.Cloak, PrimitiveType.Plane, maxCond: 90f);
        }

        private static void CreateModulePrefab(string folder, string name, FixedString64Bytes slotId, PrimitiveType primitive, float maxCond = 100f)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            var module = go.AddComponent<ModuleDefinitionAuthoring>();
            var so = new SerializedObject(module);
            so.FindProperty("moduleId").stringValue = name;
            so.FindProperty("slotType").stringValue = slotId.ToString();
            so.FindProperty("maxCondition").floatValue = maxCond;
            so.FindProperty("startingCondition").floatValue = maxCond;
            so.FindProperty("degradationPerSecond").floatValue = 0.05f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefabPath = $"{folder}/{name}.prefab";
            SavePrefab(go, prefabPath);
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void EnsureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var segments = folder.Split('/');
                var current = segments[0];
                for (int i = 1; i < segments.Length; i++)
                {
                    var next = $"{current}/{segments[i]}";
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, segments[i]);
                    }

                    current = next;
                }
            }
        }
    }
}
