using Godgame.Resources;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for per-tree felling hazard profiles.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TreeFellingProfileAuthoring : MonoBehaviour
    {
        [Header("Felling Hazard Shape")]
        [Range(0.5f, 20f)] public float fallLength = 6f;
        [Range(0.2f, 8f)] public float fallWidth = 1.5f;
        [Range(1f, 20f)] public float awarenessRadius = 8f;

        [Header("Impact Damage")]
        [Range(0f, 100f)] public float baseDamage = 15f;

        [Header("Work Difficulty")]
        [Range(0.1f, 4f)] public float chopDifficulty = 1f;

        private sealed class Baker : Baker<TreeFellingProfileAuthoring>
        {
            public override void Bake(TreeFellingProfileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TreeFellingProfile
                {
                    FallLength = Mathf.Max(0.1f, authoring.fallLength),
                    FallWidth = Mathf.Max(0.05f, authoring.fallWidth),
                    BaseDamage = Mathf.Max(0f, authoring.baseDamage),
                    ChopDifficulty = Mathf.Max(0.1f, authoring.chopDifficulty),
                    AwarenessRadius = Mathf.Max(0.1f, authoring.awarenessRadius)
                });
            }
        }
    }
}
