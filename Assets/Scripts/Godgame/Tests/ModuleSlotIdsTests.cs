using Godgame.Modules;
using NUnit.Framework;
using Unity.Entities;

namespace Godgame.Tests.Modules
{
    /// <summary>
    /// Ensures module slot helpers seed deterministic buffers for Godgame entities.
    /// </summary>
    public class ModuleSlotIdsTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(ModuleSlotIdsTests));
            _entityManager = _world.EntityManager;
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
        public void VillagerSlotsSeedExpectedCount()
        {
            var entity = _entityManager.CreateEntity();
            var buffer = _entityManager.AddBuffer<ModuleSlot>(entity);
            ModuleSlotIds.AddVillagerSlots(buffer);

            Assert.AreEqual(23, buffer.Length);
            Assert.AreEqual(ModuleSlotIds.HeadHelm, buffer[0].SlotType);
            Assert.AreEqual(ModuleSlotIds.Mount, buffer[buffer.Length - 1].SlotType);
        }

        [Test]
        public void WagonSlotsSeedExpectedCount()
        {
            var entity = _entityManager.CreateEntity();
            var buffer = _entityManager.AddBuffer<ModuleSlot>(entity);
            ModuleSlotIds.AddWagonSlots(buffer);

            Assert.AreEqual(6, buffer.Length);
            Assert.AreEqual(ModuleSlotIds.WagonMountA, buffer[0].SlotType);
            Assert.AreEqual(ModuleSlotIds.WagonDecor, buffer[buffer.Length - 1].SlotType);
        }

        [Test]
        public void BuildingSlotsSeedExpectedCount()
        {
            var entity = _entityManager.CreateEntity();
            var buffer = _entityManager.AddBuffer<ModuleSlot>(entity);
            ModuleSlotIds.AddBuildingSlots(buffer);

            Assert.AreEqual(3, buffer.Length);
            Assert.AreEqual(ModuleSlotIds.BuildingMaterial, buffer[0].SlotType);
            Assert.AreEqual(ModuleSlotIds.BuildingDecorB, buffer[buffer.Length - 1].SlotType);
        }
    }
}
