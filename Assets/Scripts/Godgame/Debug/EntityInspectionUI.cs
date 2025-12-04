using UnityEngine;
using Unity.Entities;
using Godgame.Input;

namespace Godgame.Debugging
{
    /// <summary>
    /// MonoBehaviour to display entity inspection data in a debug overlay.
    /// Toggle with I key (default).
    /// </summary>
    public class EntityInspectionUI : MonoBehaviour
    {
        [Header("Display Settings")]
        [Tooltip("Show inspection overlay")]
        public bool ShowInspection = false;

        private World _ecsWorld;
        private EntityInspectionData _cachedInspection;

        private void Start()
        {
            _ecsWorld = World.DefaultGameObjectInjectionWorld;
        }

        private void Update()
        {
            // Read input from ECS
            if (_ecsWorld != null && _ecsWorld.IsCreated)
            {
                var em = _ecsWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(DebugInput));
                if (!query.IsEmpty)
                {
                    var debugInput = query.GetSingleton<DebugInput>();
                    if (debugInput.ToggleEntityInspection == 1)
                    {
                        ShowInspection = !ShowInspection;
                    }
                }

                // Try to get inspection data from ECS
                var inspectionQuery = em.CreateEntityQuery(typeof(EntityInspectionData));
                if (!inspectionQuery.IsEmpty)
                {
                    _cachedInspection = inspectionQuery.GetSingleton<EntityInspectionData>();
                }
            }
        }

        private void OnGUI()
        {
            if (!ShowInspection || _cachedInspection.IsValid == 0)
            {
                return;
            }

            var boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginArea(new Rect(10, 250, 300, 200), boxStyle);

            GUILayout.Label("<b>Entity Inspection</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Space(5);

            GUILayout.Label(_cachedInspection.Summary.ToString());
            GUILayout.Space(5);

            GUILayout.Label("<i>Press I to toggle</i>", new GUIStyle(GUI.skin.label) { richText = true });

            GUILayout.EndArea();
        }
    }
}

