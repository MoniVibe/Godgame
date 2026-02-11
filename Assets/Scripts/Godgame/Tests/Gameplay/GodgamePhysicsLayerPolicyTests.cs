using Godgame.Physics;
using Godgame.Villagers;
using NUnit.Framework;

namespace Godgame.Tests.Gameplay
{
    public class GodgamePhysicsLayerPolicyTests
    {
        [Test]
        public void GroundUnits_DoNotCollideWithOtherGroundUnits()
        {
            var mask = GodgamePhysicsLayers.GetCollidesWithMask(GodgamePhysicsLayer.GroundUnit);
            var groundBit = 1u << GodgamePhysicsLayers.GroundUnit;
            Assert.AreEqual(0u, mask & groundBit);
        }

        [Test]
        public void GroundUnits_StillCollideWithTerrainAndProjectiles()
        {
            var mask = GodgamePhysicsLayers.GetCollidesWithMask(GodgamePhysicsLayer.GroundUnit);
            var terrainBit = 1u << GodgamePhysicsLayers.Terrain;
            var projectileBit = 1u << GodgamePhysicsLayers.Projectile;

            Assert.AreNotEqual(0u, mask & terrainBit);
            Assert.AreNotEqual(0u, mask & projectileBit);
        }

        [Test]
        public void MovementTuning_DefaultsEnableCrowdingGhostBias()
        {
            var defaults = VillagerMovementTuning.Default;
            Assert.Greater(defaults.CrowdingForwardBias, 0f);
            Assert.Greater(defaults.CrowdingSpeedBoost, 0f);
            Assert.Greater(defaults.StuckEscapeRadiusMultiplier, 1f);
            Assert.Greater(defaults.StuckEscapeSeparationDamp, 0f);
        }
    }
}
