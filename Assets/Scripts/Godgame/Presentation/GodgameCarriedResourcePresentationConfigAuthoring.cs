using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    public struct GodgameCarriedResourcePresentationConfig : IComponentData
    {
        public float3 LocalOffset;
        public float BaseScale;
        public float MaxScale;
        public float Smoothing;
        public float BoundsExtents;

        public static GodgameCarriedResourcePresentationConfig Default => new GodgameCarriedResourcePresentationConfig
        {
            LocalOffset = new float3(0.25f, 0.35f, 0.05f),
            BaseScale = 0.22f,
            MaxScale = 0.4f,
            Smoothing = 10f,
            BoundsExtents = 0.5f
        };
    }

    public sealed class GodgameCarriedResourcePresentationConfigAuthoring : MonoBehaviour
    {
        public Vector3 LocalOffset = new Vector3(0.25f, 0.35f, 0.05f);
        public float BaseScale = GodgameCarriedResourcePresentationConfig.Default.BaseScale;
        public float MaxScale = GodgameCarriedResourcePresentationConfig.Default.MaxScale;
        public float Smoothing = GodgameCarriedResourcePresentationConfig.Default.Smoothing;
        public float BoundsExtents = GodgameCarriedResourcePresentationConfig.Default.BoundsExtents;
    }

    public sealed class GodgameCarriedResourcePresentationConfigBaker : Baker<GodgameCarriedResourcePresentationConfigAuthoring>
    {
        public override void Bake(GodgameCarriedResourcePresentationConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GodgameCarriedResourcePresentationConfig
            {
                LocalOffset = new float3(authoring.LocalOffset.x, authoring.LocalOffset.y, authoring.LocalOffset.z),
                BaseScale = math.max(0.001f, authoring.BaseScale),
                MaxScale = math.max(0.001f, authoring.MaxScale),
                Smoothing = math.max(0f, authoring.Smoothing),
                BoundsExtents = math.max(0.01f, authoring.BoundsExtents)
            });
        }
    }
}
