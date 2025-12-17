using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using Unity.Entities;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Ensures an ActiveRenderTheme singleton exists and defaults to theme 0.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameRenderThemeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ActiveRenderTheme>())
            {
                state.Enabled = false;
                return;
            }

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new ActiveRenderTheme { ThemeId = 0 });
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
