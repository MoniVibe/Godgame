#if LEGACY_RENDERING_ARCHIVE_DISABLED
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using PureDOTS.Demo.Rendering;

namespace Godgame.Scenario
{
    /// <summary>
    /// Populates the shared RenderMeshArray used by PureDOTS demo systems
    /// so that orbit cubes / village cubes have valid meshes & materials.
    /// Runs once in Initialization, then disables itself.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Demo.Rendering.SharedRenderBootstrap))]
    public partial struct GodgameDemoRenderSetupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Just enable; we'll do all checks in OnUpdate.
            state.Enabled = true;
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            // Find the entity that holds the RenderMeshArraySingleton shared component
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<RenderMeshArraySingleton>());
            if (query.IsEmptyIgnoreFilter)
            {
                Godgame.GodgameDebug.LogWarning("[GodgameDemoRenderSetupSystem] No RenderMeshArraySingleton entity found; skipping.");
                state.Enabled = false;
                return;
            }

            var rmaEntity = query.GetSingletonEntity();

            // IMPORTANT: RenderMeshArraySingleton is a *shared* managed component,
            // so we must use GetSharedComponentManaged, NOT GetComponentData.
            var renderMeshArraySingleton = em.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
            var renderMeshArray = renderMeshArraySingleton.Value;

            bool alreadyPopulated =
                renderMeshArray.MaterialReferences != null && renderMeshArray.MaterialReferences.Length > 0 &&
                renderMeshArray.MeshReferences != null && renderMeshArray.MeshReferences.Length > 0;

            if (alreadyPopulated)
            {
                Godgame.GodgameDebug.Log("[GodgameDemoRenderSetupSystem] RenderMeshArray already populated; skipping.");
                state.Enabled = false;
                return;
            }

            // ----- Build demo meshes & materials -----

            // 1) Mesh: built-in Unity cube
            var cubeMesh = UnityEngine.Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (cubeMesh == null)
            {
                Debug.LogError("[GodgameDemoRenderSetupSystem] Could not find builtin Cube mesh.");
                state.Enabled = false;
                return;
            }

            // 2) Base URP Simple Lit material
            var shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                Debug.LogError("[GodgameDemoRenderSetupSystem] Could not find URP Simple Lit shader.");
                state.Enabled = false;
                return;
            }

            var baseMat = new Material(shader) { color = Color.white };
            baseMat.enableInstancing = true;

            // We want 4 "slots":
            // 0: magenta big debug cube
            // 1: red orbiter
            // 2: green orbiter
            // 3: blue orbiter
            var materials = new Material[4];
            materials[0] = new Material(baseMat) { color = Color.magenta, enableInstancing = true };
            materials[1] = new Material(baseMat) { color = Color.red, enableInstancing = true };
            materials[2] = new Material(baseMat) { color = Color.green, enableInstancing = true };
            materials[3] = new Material(baseMat) { color = Color.blue, enableInstancing = true };

            var meshes = new Mesh[4];
            for (int i = 0; i < meshes.Length; i++)
                meshes[i] = cubeMesh;

            // Construct a new RenderMeshArray
            var newRma = new RenderMeshArray(materials, meshes);

            // Wrap it in RenderMeshArraySingleton and set as shared component
            var newSingleton = new RenderMeshArraySingleton { Value = newRma };
            em.SetSharedComponentManaged(rmaEntity, newSingleton);

            Godgame.GodgameDebug.Log(
                $"[GodgameDemoRenderSetupSystem] Populated RenderMeshArray with {materials.Length} materials and {meshes.Length} meshes."
            );

            // Run once
            state.Enabled = false;
        }
    }
}
#endif
