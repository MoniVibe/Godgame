using System.Collections.Generic;
using Godgame.Environment;
using Godgame.Miracles.Presentation;
using Godgame.Presentation;
using PureDOTS.Environment;
using PureDOTS.Runtime.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Fauna
{
    /// <summary>
    /// Spawns ambient fauna tokens based on biome/time-of-day windows.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FaunaAmbientBehaviourSystem))]
    public partial class FaunaAmbientSpawnSystem : SystemBase
    {
        private ComponentLookup<LocalToWorld> _localToWorldLookup;

        protected override void OnCreate()
        {
            RequireForUpdate<FaunaAmbientVolume>();
            _localToWorldLookup = GetComponentLookup<LocalToWorld>(true);
        }

        protected override void OnUpdate()
        {
            _localToWorldLookup.Update(this);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var hasBiome = SystemAPI.TryGetSingleton(out Godgame.Environment.BiomeGrid biomeGrid);
            var hasTimeOfDay = SystemAPI.TryGetSingleton(out TimeOfDayState timeOfDayState);
            var elapsedTime = (float)SystemAPI.Time.ElapsedTime;

            foreach (var (volumeRO, runtimeRW, agentBuffer, entity) in SystemAPI
                         .Query<RefRO<FaunaAmbientVolume>, RefRW<FaunaAmbientVolumeRuntime>, DynamicBuffer<FaunaAmbientActiveAgent>>()
                         .WithEntityAccess())
            {
                ref var runtime = ref runtimeRW.ValueRW;
                if (runtime.ActiveAgents >= volumeRO.ValueRO.MaxAgents)
                {
                    continue;
                }

                if (elapsedTime < runtime.NextSpawnTime)
                {
                    continue;
                }

                var profile = volumeRO.ValueRO.Profile.Value;
                if (profile == null || profile.Rules == null || profile.Rules.Count == 0)
                {
                    runtime.NextSpawnTime = elapsedTime + volumeRO.ValueRO.SpawnIntervalSeconds;
                    continue;
                }

                var random = new Unity.Mathematics.Random(runtime.RandomState == 0 ? 1u : runtime.RandomState);
                var center = _localToWorldLookup.HasComponent(entity) ? _localToWorldLookup[entity].Position : float3.zero;
                if (volumeRO.ValueRO.AlignToGround == 0)
                {
                    center.y += volumeRO.ValueRO.SpawnHeightOffset;
                }

                // Placeholder for biome sampling until BiomeGrid has SampleNearest
                var biome = BiomeType.Unknown;
                var timeOfDay = hasTimeOfDay ? math.frac(timeOfDayState.TimeOfDayNorm) * 24f : 12f;

                if (!TryPickRule(profile.Rules, biome, timeOfDay, ref random, out var rule, out var ruleIndex))
                {
                    runtime.NextSpawnTime = elapsedTime + volumeRO.ValueRO.SpawnIntervalSeconds;
                    runtime.RandomState = random.state;
                    continue;
                }

                var availableSlots = math.max(0, volumeRO.ValueRO.MaxAgents - runtime.ActiveAgents);
                if (availableSlots == 0)
                {
                    runtime.NextSpawnTime = elapsedTime + volumeRO.ValueRO.SpawnIntervalSeconds;
                    continue;
                }

                var targetSpawnCount = math.clamp(random.NextInt(rule.spawnCount.x, rule.spawnCount.y + 1), 1, availableSlots);

                for (int i = 0; i < targetSpawnCount; i++)
                {
                    var spawnPosition = PickSpawnPosition(center, volumeRO.ValueRO.Radius, rule.spawnRadius, ref random);
                    spawnPosition.y += volumeRO.ValueRO.SpawnHeightOffset;

                    var agentEntity = CreateAgent(ref ecb, spawnPosition, in volumeRO.ValueRO, rule, ruleIndex, entity, ref random);
                    agentBuffer.Add(new FaunaAmbientActiveAgent { Agent = agentEntity });
                    runtime.ActiveAgents++;
                }

                runtime.NextSpawnTime = elapsedTime + volumeRO.ValueRO.SpawnIntervalSeconds;
                runtime.RandomState = random.state;
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private static bool TryPickRule(IReadOnlyList<FaunaAmbientProfile.SpawnRule> rules, BiomeType biome, float timeOfDay, ref Unity.Mathematics.Random random, out FaunaAmbientProfile.SpawnRule rule, out int ruleIndex)
        {
            var candidates = new List<int>();
            for (int i = 0; i < rules.Count; i++)
            {
                var entry = rules[i];
                if (entry == null)
                {
                    continue;
                }

                if (!entry.allowedBiomes.Allows(biome))
                {
                    continue;
                }

                if (!TimeOfDayWindowUtility.IsActive(entry.activityWindow.ToData(), timeOfDay))
                {
                    continue;
                }

                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                rule = null;
                ruleIndex = -1;
                return false;
            }

            var pickIndex = candidates[random.NextInt(0, candidates.Count)];
            rule = rules[pickIndex];
            ruleIndex = pickIndex;
            return true;
        }

        private static float3 PickSpawnPosition(float3 center, float maxRadius, float ruleRadius, ref Unity.Mathematics.Random random)
        {
            var radius = math.min(maxRadius, math.max(1f, ruleRadius));
            var angle = random.NextFloat(0f, math.PI * 2f);
            var distance = random.NextFloat(0.5f, radius);
            var offset = new float3(math.cos(angle) * distance, 0f, math.sin(angle) * distance);
            return center + offset;
        }

        private static Entity CreateAgent(ref EntityCommandBuffer ecb,
            float3 spawnPosition,
            in FaunaAmbientVolume volume,
            FaunaAmbientProfile.SpawnRule rule,
            int ruleIndex,
            Entity volumeEntity,
            ref Unity.Mathematics.Random random)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(spawnPosition, quaternion.identity, 1f));

            var descriptor = ToFixedString(rule.descriptorKey);
            var miracleDescriptor = ToFixedString(rule.miracleDescriptorKey);

            var agent = new FaunaAmbientAgent
            {
                Volume = volumeEntity,
                RuleIndex = ruleIndex,
                HomePosition = spawnPosition,
                TargetPosition = spawnPosition,
                IdleTimer = math.max(0.5f, rule.baseIdleDuration),
                MoveSpeed = math.max(0.5f, rule.moveSpeed),
                WanderRadius = math.max(1f, rule.wanderRadius),
                ActivityWindow = rule.activityWindow.ToData(),
                RandomState = random.NextUInt(1, uint.MaxValue),
                AmbientCooldown = rule.ambientSoundInterval,
                Flags = rule.nocturnal ? FaunaAmbientFlags.Nocturnal : FaunaAmbientFlags.None,
                BehaviourState = FaunaAmbientBehaviour.Idle,
                BaseDescriptor = descriptor,
                MiracleDescriptor = miracleDescriptor
            };

            if (rule.useEmeraldAi)
            {
                agent.Flags |= FaunaAmbientFlags.ExternalController;
            }

            ecb.AddComponent(entity, agent);

            ecb.AddComponent(entity, new SwappablePresentationBinding
            {
                DescriptorKey = descriptor,
                DescriptorHash = ComputeHash(descriptor)
            });
            ecb.AddComponent<SwappablePresentationDirtyTag>(entity);

            if (rule.fallbackPrefab != null)
            {
                ecb.AddComponent(entity, new StaticVisualPrefab
                {
                    LocalOffset = float3.zero,
                    LocalRotationOffset = quaternion.identity,
                    ScaleMultiplier = 1f,
                    InheritParentScale = 1
                });
                ecb.AddComponent(entity, new StaticVisualPrefabReference
                {
                    Prefab = new UnityObjectRef<GameObject> { Value = rule.fallbackPrefab }
                });
            }

            if (rule.ambientClip != null && rule.ambientSoundInterval > 0f)
            {
                ecb.AddComponent(entity, new FaunaAmbientSound
                {
                    Clip = new UnityObjectRef<AudioClip> { Value = rule.ambientClip },
                    IntervalSeconds = math.max(0.5f, rule.ambientSoundInterval),
                    Cooldown = random.NextFloat(0.5f, math.max(0.5f, rule.ambientSoundInterval))
                });
            }

            return entity;
        }

        private static FixedString64Bytes ToFixedString(string key)
        {
            FixedString64Bytes result = default;
            if (string.IsNullOrWhiteSpace(key))
            {
                return result;
            }

            result.CopyFromTruncated(key.Trim());
            return result;
        }

        private static int ComputeHash(in FixedString64Bytes key)
        {
            var keyString = key.ToString();
            if (string.IsNullOrEmpty(keyString))
            {
                return 0;
            }

            return Animator.StringToHash(keyString);
        }
    }
}
