using Godgame.Logistics;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component to mark an entity as a logistics hauler.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LogisticsHaulerAuthoring : MonoBehaviour
    {
        [SerializeField] private float carryCapacity = 20f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float interactRange = 2f;
        [SerializeField] private uint claimCooldownTicks = 10;

        private sealed class Baker : Unity.Entities.Baker<LogisticsHaulerAuthoring>
        {
            public override void Bake(LogisticsHaulerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<LogisticsHaulerTag>(entity);
                AddComponent(entity, new LogisticsHauler
                {
                    CarryCapacity = authoring.carryCapacity,
                    MoveSpeed = authoring.moveSpeed,
                    InteractRange = authoring.interactRange,
                    ClaimCooldownTicks = authoring.claimCooldownTicks
                });
                AddComponent(entity, new LogisticsHaulState
                {
                    Phase = LogisticsHaulPhase.Idle,
                    BoardEntity = Entity.Null,
                    SiteEntity = Entity.Null,
                    SourceEntity = Entity.Null,
                    ResourceTypeIndex = ushort.MaxValue,
                    ReservedUnits = 0f,
                    CarryingUnits = 0f,
                    ReservationId = 0,
                    LastClaimTick = 0,
                    LastProgressTick = 0
                });
            }
        }
    }
}
