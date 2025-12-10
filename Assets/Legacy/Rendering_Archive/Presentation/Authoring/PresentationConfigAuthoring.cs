using Godgame.Presentation;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Presentation.Authoring
{
    public class PresentationConfigAuthoring : MonoBehaviour
    {
        public PresentationConfig Config;

        public class Baker : Baker<PresentationConfigAuthoring>
        {
            public override void Bake(PresentationConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, authoring.Config);
            }
        }
    }
}

