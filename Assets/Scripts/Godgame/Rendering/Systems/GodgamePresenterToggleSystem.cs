using PureDOTS.Input;
using PureDOTS.Rendering;
using Unity.Collections;
using Unity.Entities;
using UnityDebug = UnityEngine.Debug;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Debug harness that toggles MeshPresenter enable state (F7) to demonstrate enableable presenters.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgamePresenterToggleSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<RenderSemanticKey>(),
                    ComponentType.ReadOnly<MeshPresenter>(),
                    ComponentType.ReadOnly<DebugPresenter>()
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            if (!Hotkeys.F7Down())
                return;

            using var entities = _query.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
                return;

            var target = entities[0];
            var meshEnabled = state.EntityManager.IsComponentEnabled<MeshPresenter>(target);
            var debugEnabled = state.EntityManager.IsComponentEnabled<DebugPresenter>(target);

            if (meshEnabled || !debugEnabled)
            {
                state.EntityManager.SetComponentEnabled<MeshPresenter>(target, false);
                state.EntityManager.SetComponentEnabled<DebugPresenter>(target, true);
                UnityDebug.Log($"[GodgamePresenterToggleSystem] DebugPresenter enabled for entity {target.Index}.");
            }
            else
            {
                state.EntityManager.SetComponentEnabled<MeshPresenter>(target, true);
                state.EntityManager.SetComponentEnabled<DebugPresenter>(target, false);
                UnityDebug.Log($"[GodgamePresenterToggleSystem] MeshPresenter restored for entity {target.Index}.");
            }
#endif
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
