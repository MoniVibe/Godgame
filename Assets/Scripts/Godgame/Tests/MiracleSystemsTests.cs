#if GODGAME_TESTS && PUREDOTS_INPUT
using Godgame.Miracles;
using NUnit.Framework;
using Unity.Entities;

namespace Godgame.Tests
{
    /// <summary>
    /// Basic coverage for miracle systems to ensure source-generated types compile and can be created.
    /// </summary>
    public class MiracleSystemsTests
    {
        private World _world;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null)
            {
                _world = new World("Test World");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.Name == "Test World")
            {
                _world.Dispose();
            }
        }

        [Test]
        public void GodgameMiracleInputSystem_Exists()
        {
            var systemHandle = _world.GetOrCreateSystem<GodgameMiracleInputSystem>();
            Assert.AreNotEqual(SystemHandle.Null, systemHandle, "GodgameMiracleInputSystem should exist");
        }

        [Test]
        public void GodgameMiracleReleaseSystem_Exists()
        {
            var systemHandle = _world.GetOrCreateSystem<GodgameMiracleReleaseSystem>();
            Assert.AreNotEqual(SystemHandle.Null, systemHandle, "GodgameMiracleReleaseSystem should exist");
        }

        [Test]
        public void GodgameMiracleBootstrapSystem_Exists()
        {
            var systemHandle = _world.GetOrCreateSystem<GodgameMiracleBootstrapSystem>();
            Assert.AreNotEqual(SystemHandle.Null, systemHandle, "GodgameMiracleBootstrapSystem should exist");
        }
    }
}
#endif
