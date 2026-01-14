using PureDOTS.Runtime.ComplexEntities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Systems
{
    /// <summary>
    /// Bridge system that upgrades existing Godgame entities (guilds, colonies, etc.) to use the complex entity system.
    /// This is a minimal stub - full implementation would convert specific Godgame entity types.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ComplexEntityGodgameBridgeSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Placeholder: In full implementation, this would:
            // 1. Find guilds/colonies/stations that need conversion
            // 2. Create ComplexEntityIdentity with appropriate EntityType
            // 3. Create ComplexEntityCoreAxes from existing component data
            // 4. Add enableable operational/narrative components (disabled by default)
            
            // Example for future implementation:
            // foreach (var (guild, entity, transform) in SystemAPI.Query<
            //     RefRO<GuildComponent>,
            //     Entity>()
            //     .WithAll<LocalTransform>()
            //     .WithNone<ComplexEntityIdentity>()
            //     .WithEntityAccess())
            // {
            //     var identity = new ComplexEntityIdentity
            //     {
            //         StableId = new FixedString64Bytes($"guild_{guild.ValueRO.GuildId}"),
            //         EntityType = ComplexEntityType.Guild,
            //         CreationTick = (uint)SystemAPI.Time.ElapsedTime
            //     };
            //     // ... create core axes, add components, etc.
            // }
        }
    }

    /// <summary>
    /// Helper methods for registering activation triggers for complex entities in Godgame.
    /// </summary>
    public static class ComplexEntityGodgameTriggerHelpers
    {
        /// <summary>
        /// Registers an entity as having player focus (selected/inspected).
        /// </summary>
        public static void RegisterFocusTarget(EntityCommandBuffer ecb, Entity entity)
        {
            ecb.AddComponent<FocusTargetTag>(entity);
        }

        /// <summary>
        /// Registers an inspection request for an entity (UI detail panel).
        /// </summary>
        public static void RegisterInspectionRequest(EntityCommandBuffer ecb, Entity entity)
        {
            ecb.AddComponent<InspectionRequest>(entity);
        }

        /// <summary>
        /// Removes an inspection request (closes detail panel).
        /// </summary>
        public static void RemoveInspectionRequest(EntityCommandBuffer ecb, Entity entity)
        {
            ecb.RemoveComponent<InspectionRequest>(entity);
        }
    }
}
