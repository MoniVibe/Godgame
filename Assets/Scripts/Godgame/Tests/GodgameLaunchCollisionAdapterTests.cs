using Godgame.Adapters.Launch;
using NUnit.Framework;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Launch;
using PureDOTS.Runtime.Physics;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Physics
{
    public class GodgameLaunchCollisionAdapterTests
    {
        private World _world;
        private EntityManager _entityManager;
        private EndSimulationEntityCommandBufferSystem _endSimEcb;
        private GodgameSlingshotCollisionAdapter _adapter;
        private Entity _timeEntity;
        private Entity _rewindEntity;
        private Entity _physicsConfigEntity;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(GodgameLaunchCollisionAdapterTests));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);

            _endSimEcb = _world.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            _adapter = _world.GetOrCreateSystemManaged<GodgameSlingshotCollisionAdapter>();

            EnsureTimeState();
            EnsureRewindState();
            EnsurePhysicsConfig();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void LaunchCollision_EmitsDamageAndConsumesProjectile()
        {
            SetRewindMode(RewindMode.Record);

            var launcher = _entityManager.CreateEntity();
            var target = _entityManager.CreateEntity();
            var projectile = _entityManager.CreateEntity(typeof(LaunchedProjectileTag));

            _entityManager.SetComponentData(projectile, new LaunchedProjectileTag
            {
                LaunchTick = 1,
                SourceLauncher = launcher
            });

            var collisions = _entityManager.AddBuffer<PhysicsCollisionEventElement>(projectile);
            collisions.Add(new PhysicsCollisionEventElement
            {
                OtherEntity = target,
                ContactPoint = float3.zero,
                ContactNormal = new float3(0f, 1f, 0f),
                Impulse = 10f,
                Tick = 1,
                EventType = PhysicsCollisionEventType.Collision
            });

            _adapter.Update(_world.Unmanaged);
            _endSimEcb.Update(_world.Unmanaged);

            Assert.IsFalse(_entityManager.HasComponent<LaunchedProjectileTag>(projectile), "Projectile should be consumed.");
            Assert.IsTrue(_entityManager.HasBuffer<DamageEvent>(target), "Target should receive a DamageEvent buffer.");

            var damageBuffer = _entityManager.GetBuffer<DamageEvent>(target);
            Assert.AreEqual(1, damageBuffer.Length, "Exactly one damage event should be emitted.");
            Assert.AreEqual(launcher, damageBuffer[0].SourceEntity);
            Assert.AreEqual(target, damageBuffer[0].TargetEntity);
            Assert.Greater(damageBuffer[0].RawDamage, 0f);
        }

        [Test]
        public void LaunchCollision_SkipsDuringRewindPlayback()
        {
            SetRewindMode(RewindMode.Playback);

            var launcher = _entityManager.CreateEntity();
            var target = _entityManager.CreateEntity();
            var projectile = _entityManager.CreateEntity(typeof(LaunchedProjectileTag));

            _entityManager.SetComponentData(projectile, new LaunchedProjectileTag
            {
                LaunchTick = 1,
                SourceLauncher = launcher
            });

            var collisions = _entityManager.AddBuffer<PhysicsCollisionEventElement>(projectile);
            collisions.Add(new PhysicsCollisionEventElement
            {
                OtherEntity = target,
                ContactPoint = float3.zero,
                ContactNormal = new float3(0f, 1f, 0f),
                Impulse = 10f,
                Tick = 1,
                EventType = PhysicsCollisionEventType.Collision
            });

            _adapter.Update(_world.Unmanaged);
            _endSimEcb.Update(_world.Unmanaged);

            Assert.IsTrue(_entityManager.HasComponent<LaunchedProjectileTag>(projectile), "Projectile should remain during rewind playback.");
            Assert.IsFalse(_entityManager.HasBuffer<DamageEvent>(target), "No damage should be emitted during rewind playback.");
        }

        private void EnsureTimeState()
        {
            var query = _entityManager.CreateEntityQuery(ComponentType.ReadWrite<TimeState>());
            _timeEntity = query.GetSingletonEntity();
            var time = _entityManager.GetComponentData<TimeState>(_timeEntity);
            time.IsPaused = false;
            time.FixedDeltaTime = 1f / 60f;
            time.Tick = 1;
            _entityManager.SetComponentData(_timeEntity, time);
        }

        private void EnsureRewindState()
        {
            var query = _entityManager.CreateEntityQuery(ComponentType.ReadWrite<RewindState>());
            _rewindEntity = query.GetSingletonEntity();
            var rewind = _entityManager.GetComponentData<RewindState>(_rewindEntity);
            rewind.Mode = RewindMode.Record;
            _entityManager.SetComponentData(_rewindEntity, rewind);
        }

        private void EnsurePhysicsConfig()
        {
            var query = _entityManager.CreateEntityQuery(ComponentType.ReadWrite<PhysicsConfig>());
            if (query.IsEmptyIgnoreFilter)
            {
                _physicsConfigEntity = _entityManager.CreateEntity(typeof(PhysicsConfig));
            }
            else
            {
                _physicsConfigEntity = query.GetSingletonEntity();
            }

            var config = _entityManager.GetComponentData<PhysicsConfig>(_physicsConfigEntity);
            config.ProviderId = PhysicsProviderIds.Entities;
            config.EnableGodgamePhysics = 1;
            config.PostRewindSettleFrames = 0;
            config.LastRewindCompleteTick = 0;
            _entityManager.SetComponentData(_physicsConfigEntity, config);
        }

        private void SetRewindMode(RewindMode mode)
        {
            var rewind = _entityManager.GetComponentData<RewindState>(_rewindEntity);
            rewind.Mode = mode;
            _entityManager.SetComponentData(_rewindEntity, rewind);
        }
    }
}
