using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Shared UI for editing visual presentation assets.
    /// </summary>
    public static class VisualPresentationEditor
    {
        /// <summary>
        /// Draws UI for editing visual presentation.
        /// </summary>
        public static void DrawVisualPresentationEditor(VisualPresentation presentation)
        {
            if (presentation == null)
            {
                EditorGUILayout.HelpBox("Visual presentation is null. Template may need migration.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.LabelField("Visual Presentation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // Prefab Asset
            presentation.prefabAsset = (GameObject)EditorGUILayout.ObjectField(
                "Prefab Asset",
                presentation.prefabAsset,
                typeof(GameObject),
                false
            );

            // Mesh Asset
            presentation.meshAsset = (Mesh)EditorGUILayout.ObjectField(
                "Mesh Asset",
                presentation.meshAsset,
                typeof(Mesh),
                false
            );

            // Sprite Asset
            presentation.spriteAsset = (Sprite)EditorGUILayout.ObjectField(
                "Sprite Asset",
                presentation.spriteAsset,
                typeof(Sprite),
                false
            );

            // Material Override
            presentation.materialOverride = (Material)EditorGUILayout.ObjectField(
                "Material Override",
                presentation.materialOverride,
                typeof(Material),
                false
            );

            EditorGUILayout.Space();

            // Transform settings
            EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
            presentation.positionOffset = EditorGUILayout.Vector3Field("Position Offset", presentation.positionOffset);
            presentation.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", presentation.rotationOffset);
            presentation.scale = EditorGUILayout.Vector3Field("Scale", presentation.scale);

            EditorGUILayout.Space();

            // Primitive fallback
            EditorGUILayout.LabelField("Fallback", EditorStyles.boldLabel);
            presentation.usePrimitiveFallback = EditorGUILayout.Toggle("Use Primitive Fallback", presentation.usePrimitiveFallback);
            
            if (presentation.usePrimitiveFallback)
            {
                presentation.primitiveType = (PrimitiveType)EditorGUILayout.EnumPopup("Primitive Type", presentation.primitiveType);
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing VFX presentation.
        /// </summary>
        public static void DrawVFXPresentationEditor(VFXPresentation vfxPresentation)
        {
            if (vfxPresentation == null)
            {
                EditorGUILayout.HelpBox("VFX presentation is null. Template may need migration.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.LabelField("VFX Presentation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // VFX Asset
            vfxPresentation.vfxAsset = (UnityEngine.VFX.VisualEffectAsset)EditorGUILayout.ObjectField(
                "VFX Graph Asset",
                vfxPresentation.vfxAsset,
                typeof(UnityEngine.VFX.VisualEffectAsset),
                false
            );

            if (vfxPresentation.vfxAsset != null)
            {
                EditorGUILayout.Space();

                // Transform settings
                EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
                vfxPresentation.positionOffset = EditorGUILayout.Vector3Field("Position Offset", vfxPresentation.positionOffset);
                vfxPresentation.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", vfxPresentation.rotationOffset);

                EditorGUILayout.Space();

                // Playback settings
                EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
                vfxPresentation.playOnAwake = EditorGUILayout.Toggle("Play On Awake", vfxPresentation.playOnAwake);
                vfxPresentation.loop = EditorGUILayout.Toggle("Loop", vfxPresentation.loop);
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a VFX Graph asset to add visual effects to this prefab.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing presentation ID.
        /// </summary>
        public static void DrawPresentationIdEditor(PrefabTemplate template)
        {
            EditorGUILayout.LabelField("Presentation ID", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            template.presentationId = EditorGUILayout.TextField("ID", template.presentationId);
            
            if (string.IsNullOrEmpty(template.presentationId))
            {
                EditorGUILayout.HelpBox(
                    "Presentation ID is optional. Used for runtime binding to presentation systems.",
                    MessageType.Info
                );
            }

            EditorGUI.indentLevel--;
        }
    }
}


