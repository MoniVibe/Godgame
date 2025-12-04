using Godgame.Biomes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Godgame.Editor.Biomes
{
    [CustomEditor(typeof(BiomeTerrainProfile))]
    public sealed class BiomeTerrainProfileEditor : UnityEditor.Editor
    {
        SerializedProperty _displayNameProp;
        SerializedProperty _biomeProp;
        SerializedProperty _moistureRangeProp;
        SerializedProperty _temperatureRangeProp;
        SerializedProperty _descriptorKeyProp;
        SerializedProperty _descriptorHashProp;
        SerializedProperty _groundProp;
        SerializedProperty _vegetationProp;
        SerializedProperty _ambientProp;
        SerializedProperty _vfxProp;

        ReorderableList _vegetationList;
        ReorderableList _ambientList;
        ReorderableList _vfxList;

        void OnEnable()
        {
            _displayNameProp = serializedObject.FindProperty("displayName");
            _biomeProp = serializedObject.FindProperty("biome");
            _moistureRangeProp = serializedObject.FindProperty("moistureRange");
            _temperatureRangeProp = serializedObject.FindProperty("temperatureRange");
            _descriptorKeyProp = serializedObject.FindProperty("descriptorKey");
            _descriptorHashProp = serializedObject.FindProperty("descriptorHash");
            _groundProp = serializedObject.FindProperty("ground");
            _vegetationProp = serializedObject.FindProperty("vegetationClusters");
            _ambientProp = serializedObject.FindProperty("ambientProps");
            _vfxProp = serializedObject.FindProperty("vfxBindings");

            BuildLists();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawIdentitySection();
            EditorGUILayout.Space();
            DrawGroundSection();
            EditorGUILayout.Space();

            _vegetationList.DoLayoutList();
            EditorGUILayout.Space();
            _ambientList.DoLayoutList();
            EditorGUILayout.Space();
            _vfxList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawIdentitySection()
        {
            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(_biomeProp);
            DrawFloatRange(_moistureRangeProp, "Moisture Range (0-100)", 0f, 100f);
            DrawFloatRange(_temperatureRangeProp, "Temperature Range (°C)", -80f, 80f);
            EditorGUILayout.PropertyField(_descriptorKeyProp);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Descriptor Hash", _descriptorHashProp.intValue);
            }
        }

        void DrawGroundSection()
        {
            if (_groundProp == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Ground", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("surfaceMode"));

            var mode = (GroundSurfaceMode)_groundProp.FindPropertyRelative("surfaceMode").enumValueIndex;
            switch (mode)
            {
                case GroundSurfaceMode.ProceduralPrefab:
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("proceduralPrefab"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("spawnOffset"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("spawnRotationEuler"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("spawnScale"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("fallbackMaterial"));
                    break;

                case GroundSurfaceMode.MicroSplatMaterial:
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("microSplatMaterial"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("microSplatProfileKeyword"));
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("fallbackMaterial"));
                    break;

                case GroundSurfaceMode.MaterialOnly:
                    EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("fallbackMaterial"));
                    break;
            }

            EditorGUILayout.PropertyField(_groundProp.FindPropertyRelative("allowRuntimeHeightBlending"));
        }

        void BuildLists()
        {
            _vegetationList = BuildDefaultList(_vegetationProp, "Vegetation Clusters");
            _ambientList = BuildDefaultList(_ambientProp, "Ambient Props");
            _vfxList = BuildDefaultList(_vfxProp, "Ground FX / Atmospherics");
        }

        ReorderableList BuildDefaultList(SerializedProperty property, string header)
        {
            var list = new ReorderableList(serializedObject, property, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, header)
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = property.GetArrayElementAtIndex(index);
                rect.height = EditorGUI.GetPropertyHeight(element, includeChildren: true);
                EditorGUI.PropertyField(rect, element, GUIContent.none, includeChildren: true);
            };

            list.elementHeightCallback = index =>
            {
                var element = property.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, includeChildren: true) + EditorGUIUtility.standardVerticalSpacing;
            };

            return list;
        }

        static void DrawFloatRange(SerializedProperty property, string label, float minLimit, float maxLimit)
        {
            if (property == null)
            {
                return;
            }

            var minProp = property.FindPropertyRelative("min");
            var maxProp = property.FindPropertyRelative("max");
            if (minProp == null || maxProp == null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
                return;
            }

            float minValue = minProp.floatValue;
            float maxValue = maxProp.floatValue;

            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            var sliderRect = EditorGUILayout.GetControlRect();
            EditorGUI.MinMaxSlider(sliderRect, GUIContent.none, ref minValue, ref maxValue, minLimit, maxLimit);
            EditorGUILayout.BeginHorizontal();
            minValue = EditorGUILayout.FloatField("Min", minValue);
            maxValue = EditorGUILayout.FloatField("Max", maxValue);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            if (maxValue < minValue)
            {
                (minValue, maxValue) = (maxValue, minValue);
            }

            minProp.floatValue = Mathf.Clamp(minValue, minLimit, maxLimit);
            maxProp.floatValue = Mathf.Clamp(maxValue, minProp.floatValue, maxLimit);
        }
    }
}
