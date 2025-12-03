using System.Collections;
using Godgame.MoveAct;
using Godgame.Presentation;
using NUnit.Framework;
using PureDOTS.Input;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using PureDOTS.Systems.Input;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.TestTools;

namespace Godgame.Tests
{
    public class MoveActPlayModeTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World("MoveActPlayModeTests");
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);

            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = false;
            timeState.FixedDeltaTime = 0.2f;
            timeState.Tick = 1;
            _entityManager.SetComponentData(timeEntity, timeState);

            var rewindEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>()).GetSingletonEntity();
            var rewindState = _entityManager.GetComponentData<RewindState>(rewindEntity);
            rewindState.Mode = RewindMode.Record;
            _entityManager.SetComponentData(rewindEntity, rewindState);

            _world.GetOrCreateSystem<PresentationBootstrapSystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<GodgamePresentationBindingBootstrapSystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<MoveActBootstrapSystem>().Update(_world.Unmanaged);
        }

        [TearDown]
        public void TearDown()
        {
            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator BandSpawnAndPing_ProducesRegistryEntryAndEffectRequest()
        {
            var timeState = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>())
                .GetSingleton<TimeState>();

            var handEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<DivineHandTag>())
                .GetSingletonEntity();
            var handInput = _entityManager.GetComponentData<DivineHandInput>(handEntity);
            var spawnPosition = new float3(2f, 0f, -3f);
            handInput.CursorWorldPosition = spawnPosition;
            handInput.SampleTick = timeState.Tick;
            _entityManager.SetComponentData(handEntity, handInput);

            var handEdges = _entityManager.GetBuffer<HandInputEdge>(handEntity);
            handEdges.Clear();
            handEdges.Add(new HandInputEdge
            {
                Button = InputButton.Primary,
                Kind = InputEdgeKind.Down,
                Tick = timeState.Tick,
                PointerPosition = float2.zero
            });

#if PUREDOTS_LEGACY_CAMERA
            _world.GetOrCreateSystem<IntentMappingSystem>().Update(_world.Unmanaged);
#endif
            _world.GetOrCreateSystem<BandSpawnInputSystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<BandSpawnSystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<BandRegistrySystem>().Update(_world.Unmanaged);

            var bandQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandId>());
            Assert.AreEqual(1, bandQuery.CalculateEntityCount());
            var bandEntity = bandQuery.GetSingletonEntity();

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandRegistry>())
                .GetSingletonEntity();
            var entries = _entityManager.GetBuffer<BandRegistryEntry>(registryEntity);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(bandEntity, entries[0].BandEntity);
            Assert.AreEqual(spawnPosition, entries[0].Position);

            var selection = _entityManager.GetComponentData<BandSelection>(
                _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandSelection>()).GetSingletonEntity());
            Assert.AreEqual(bandEntity, selection.Selected);

            var hotkeyEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandActionHotkeyState>())
                .GetSingletonEntity();
            var hotkey = _entityManager.GetComponentData<BandActionHotkeyState>(hotkeyEntity);
            hotkey.PlayPingRequested = 1;
            _entityManager.SetComponentData(hotkeyEntity, hotkey);

            _world.GetOrCreateSystem<BandActionEffectSystem>().Update(_world.Unmanaged);

            var presentationEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PresentationRequestHub>())
                .GetSingletonEntity();
            var effects = _entityManager.GetBuffer<PlayEffectRequest>(presentationEntity);
            Assert.AreEqual(1, effects.Length);
            Assert.AreEqual(GodgamePresentationIds.MiraclePingEffectId, effects[0].EffectId);
            Assert.AreEqual(bandEntity, effects[0].Target);
            Assert.AreEqual(spawnPosition, effects[0].Position);

            Assert.AreEqual(0, _entityManager.GetComponentData<BandActionHotkeyState>(hotkeyEntity).PlayPingRequested);

            yield return null;
        }
    }
}
