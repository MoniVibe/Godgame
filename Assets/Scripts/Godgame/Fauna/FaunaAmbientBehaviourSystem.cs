using Godgame.Environment;
using PureDOTS.Environment;
using PureDOTS.Runtime.Time;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Fauna
{
    /// <summary>
    /// Simple DOTS-driven patrol/idle loops for spawned fauna.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FaunaAmbientSpawnSystem))]
    public partial struct FaunaAmbientBehaviourSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FaunaAmbientAgent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = state.WorldUnmanaged.Time.DeltaTime;
            var hasTimeOfDay = SystemAPI.TryGetSingleton(out TimeOfDayState timeOfDayState);
            var timeOfDay = hasTimeOfDay ? math.frac(timeOfDayState.TimeOfDayNorm) * 24f : 12f;

            foreach (var (agentRW, transformRW) in SystemAPI.Query<RefRW<FaunaAmbientAgent>, RefRW<LocalTransform>>())
            {
                var agent = agentRW.ValueRO;
                if ((agent.Flags & FaunaAmbientFlags.ExternalController) != 0)
                {
                    continue;
                }

                var isActive = TimeOfDayWindowUtility.IsActive(agent.ActivityWindow, timeOfDay);
                if (!isActive)
                {
                    agent.BehaviourState = FaunaAmbientBehaviour.Dormant;
                    var lt = transformRW.ValueRO;
                    lt.Position = math.lerp(lt.Position, agent.HomePosition, math.saturate(deltaTime * 2f));
                    transformRW.ValueRW = lt;
                    agentRW.ValueRW = agent;
                    continue;
                }

                var random = new Unity.Mathematics.Random(agent.RandomState == 0 ? 1u : agent.RandomState);

                switch (agent.BehaviourState)
                {
                    case FaunaAmbientBehaviour.Dormant:
                    case FaunaAmbientBehaviour.Idle:
                        agent.IdleTimer -= deltaTime;
                        if (agent.IdleTimer <= 0f)
                        {
                            agent.TargetPosition = PickTarget(agent.HomePosition, agent.WanderRadius, ref random);
                            agent.IdleTimer = random.NextFloat(1.5f, 4f);
                            agent.BehaviourState = FaunaAmbientBehaviour.Moving;
                        }
                        break;
                    case FaunaAmbientBehaviour.Moving:
                        var lt = transformRW.ValueRO;
                        var toTarget = agent.TargetPosition - lt.Position;
                        var distanceSq = math.lengthsq(toTarget);
                        if (distanceSq <= 0.05f)
                        {
                            agent.BehaviourState = FaunaAmbientBehaviour.Idle;
                            agent.IdleTimer = random.NextFloat(1.5f, 4f);
                            lt.Position = agent.TargetPosition;
                        }
                        else
                        {
                            var direction = math.normalize(toTarget);
                            lt.Position += direction * agent.MoveSpeed * deltaTime;
                        }

                        transformRW.ValueRW = lt;
                        break;
                }

                agent.RandomState = random.state;
                agentRW.ValueRW = agent;
            }
        }

        private static float3 PickTarget(float3 home, float wanderRadius, ref Unity.Mathematics.Random random)
        {
            var angle = random.NextFloat(0f, math.PI * 2f);
            var distance = random.NextFloat(0.5f, math.max(1f, wanderRadius));
            return home + new float3(math.cos(angle) * distance, 0f, math.sin(angle) * distance);
        }
    }
}
