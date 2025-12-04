using System;
using System.Linq;
using Godgame.Presentation;
using Godgame.Presentation.Authoring;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.Presentation
{
    /// <summary>
    /// Provides a descriptor dropdown for SwappablePresentationBindingAuthoring so designers can pick
    /// from the Godgame registry instead of typing raw keys.
    /// </summary>
    [CustomEditor(typeof(SwappablePresentationBindingAuthoring))]
    public sealed class SwappablePresentationBindingEditor : UnityEditor.Editor
    {
        private const string RegistryPath = "Assets/Data/Presentation/GodgameSwappablePresentationRegistry.asset";

        SerializedProperty _descriptorKeyProp;
        SerializedProperty _markDirtyProp;

        string[] _descriptorOptions = Array.Empty<string>();
        PresentationRegistry _cache;

        void OnEnable()
        {
            _descriptorKeyProp = serializedObject.FindProperty("DescriptorKey");
            _markDirtyProp = serializedObject.FindProperty("markDirtyOnBake");
            LoadRegistry();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescriptorField();
            if (_markDirtyProp != null)
            {
                EditorGUILayout.PropertyField(_markDirtyProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawDescriptorField()
        {
            if (_descriptorOptions.Length == 0)
            {
                EditorGUILayout.PropertyField(_descriptorKeyProp);
                EditorGUILayout.HelpBox("No descriptor registry loaded. Create/assign GodgameSwappablePresentationRegistry to enable dropdown selection.", MessageType.Info);
                if (GUILayout.Button("Reload Registry"))
                {
                    LoadRegistry();
                }
                return;
            }

            var currentValue = _descriptorKeyProp.stringValue;
            var currentIndex = Array.IndexOf(_descriptorOptions, currentValue);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            var nextIndex = EditorGUILayout.Popup("Descriptor Key", currentIndex, _descriptorOptions);
            if (EditorGUI.EndChangeCheck())
            {
                _descriptorKeyProp.stringValue = _descriptorOptions[Mathf.Clamp(nextIndex, 0, _descriptorOptions.Length - 1)];
            }
        }

        void LoadRegistry()
        {
            _cache = AssetDatabase.LoadAssetAtPath<PresentationRegistry>(RegistryPath);
            if (_cache == null)
            {
                _descriptorOptions = Array.Empty<string>();
                return;
            }

            _descriptorOptions = _cache.Keys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim())
                .Distinct()
                .OrderBy(key => key)
                .ToArray();
        }
    }
}
