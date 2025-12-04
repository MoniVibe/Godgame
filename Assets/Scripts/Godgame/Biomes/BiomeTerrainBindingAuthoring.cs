using Godgame.Miracles.Presentation;
using Godgame.Presentation.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Biomes
{
    public struct BiomeTerrainBinding : IComponentData
    {
        public int RegionId;
    }

    /// <summary>
    /// Marks a swappable presentation binding so the <see cref="BiomeTerrainAgent"/> knows which entities should
    /// adopt its descriptor changes. Attach alongside <see cref="SwappablePresentationBindingAuthoring"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SwappablePresentationBindingAuthoring))]
    public sealed class BiomeTerrainBindingAuthoring : MonoBehaviour
    {
        [SerializeField] int regionId;

        public int RegionId => regionId;

        private sealed class BakerImpl : Baker<BiomeTerrainBindingAuthoring>
        {
            public override void Bake(BiomeTerrainBindingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BiomeTerrainBinding
                {
                    RegionId = math.max(0, authoring.regionId)
                });
            }
        }
    }
}
