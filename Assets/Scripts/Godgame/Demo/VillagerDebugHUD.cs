#if GODGAME_SCENARIO && GODGAME_DEVTOOLS
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
// using PureDOTS.Demo.Village; // legacy demo dependency

namespace Godgame.Scenario
{
    /// <summary>
    /// Simple HUD component for displaying villager counts in the village demo scene.
    /// Reads villager entity counts from ECS and displays them on screen.
    /// </summary>
    public class VillagerDebugHUD : MonoBehaviour
    {
        public Text villagerCountText;
        public Text homeLotCountText;
        public Text workLotCountText;

        private EntityManager entityManager;
        private EntityQuery villagerQuery;
        private EntityQuery homeLotQuery;
        private EntityQuery workLotQuery;
        private bool queriesInitialized;

        private void Update()
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;

            // Re-acquire if needed (e.g. domain reload or startup timing)
            if (entityManager == default || !entityManager.World.IsCreated)
            {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }

            if (!entityManager.WorldUnmanaged.IsCreated)
            {
                return;
            }

            EnsureQueries();
            if (!queriesInitialized)
                return;

            UpdateCounts();
        }

        private void EnsureQueries()
        {
            if (queriesInitialized)
                return;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return;

            entityManager = world.EntityManager;

            villagerQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Godgame.Villagers.VillagerTag>());
            homeLotQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Godgame.Villagers.HomeLot>());
            workLotQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Godgame.Villagers.WorkLot>());

            queriesInitialized = true;
        }

        private void UpdateCounts()
        {
            // Count villagers
            int villagerCount = 0;
            if (queriesInitialized)
            {
                villagerCount = villagerQuery.CalculateEntityCount();
            }

            // Count home lots
            int homeLotCount = 0;
            if (queriesInitialized)
            {
                homeLotCount = homeLotQuery.CalculateEntityCount();
            }

            // Count work lots
            int workLotCount = 0;
            if (queriesInitialized)
            {
                workLotCount = workLotQuery.CalculateEntityCount();
            }

            // Update UI text
            if (villagerCountText != null)
            {
                villagerCountText.text = $"Villagers: {villagerCount}";
            }

            if (homeLotCountText != null)
            {
                homeLotCountText.text = $"Home Lots: {homeLotCount}";
            }

            if (workLotCountText != null)
            {
                workLotCountText.text = $"Work Lots: {workLotCount}";
            }
        }
    }
}
#endif
