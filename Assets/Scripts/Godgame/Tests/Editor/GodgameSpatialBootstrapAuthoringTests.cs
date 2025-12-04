#if UNITY_EDITOR && GODGAME_TESTS && PUREDOTS_AUTHORING
using Godgame.Registry.Authoring;
using NUnit.Framework;
using PureDOTS.Authoring;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Godgame.Tests.Editor
{
    public sealed class GodgameSpatialBootstrapAuthoringTests
    {
        private const string ScenePath = "Assets/Scenes/Godgame_VillagerDemo.unity";

        [Test]
        public void BootstrapSceneHasSpatialProfile()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var bootstrap = GameObject.Find("BootstrapConfig");
                Assert.That(bootstrap, Is.Not.Null, "BootstrapConfig GameObject not found.");

                var configAuthoring = bootstrap.GetComponent<PureDotsConfigAuthoring>();
                Assert.That(configAuthoring, Is.Not.Null, "PureDotsConfigAuthoring missing on BootstrapConfig.");

                var spatialAuthoring = bootstrap.GetComponent<GodgameSpatialBootstrapAuthoring>();
                Assert.That(spatialAuthoring, Is.Not.Null, "GodgameSpatialBootstrapAuthoring missing on BootstrapConfig.");
                Assert.That(spatialAuthoring.profile, Is.Not.Null, "Spatial profile reference is not assigned.");
                Assert.That(spatialAuthoring.profile.name, Is.EqualTo("GodgameSpatialProfile"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
#endif
