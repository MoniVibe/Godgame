using Godgame.Presentation;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Presentation;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Seeds a minimal presentation binding so placeholder effects can play even without authored bindings.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PresentationBootstrapSystem))]
    public partial struct GodgamePresentationBindingBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationCommandQueue>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            Entity bindingEntity;

            if (!SystemAPI.TryGetSingletonEntity<PureDOTS.Runtime.Components.PresentationBindingReference>(out bindingEntity))
            {
                bindingEntity = entityManager.CreateEntity(typeof(PureDOTS.Runtime.Components.PresentationBindingReference));
            }

            var bindingRef = entityManager.GetComponentData<PureDOTS.Runtime.Components.PresentationBindingReference>(bindingEntity);
            if (bindingRef.Binding.IsCreated && 
                HasEffect(bindingRef.Binding, GodgamePresentationIds.MiraclePingEffectId) &&
                HasEffect(bindingRef.Binding, GodgamePresentationIds.JobsiteGhostEffectId) &&
                HasEffect(bindingRef.Binding, GodgamePresentationIds.ModuleRefitSparksEffectId) &&
                HasEffect(bindingRef.Binding, GodgamePresentationIds.HandAffordanceEffectId))
            {
                state.Enabled = false;
                return;
            }

            var blob = BuildBindingBlob();

            if (bindingRef.Binding.IsCreated)
            {
                bindingRef.Binding.Dispose();
            }

            bindingRef.Binding = blob;
            entityManager.SetComponentData(bindingEntity, bindingRef);
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out PureDOTS.Runtime.Components.PresentationBindingReference binding) && binding.Binding.IsCreated)
            {
                binding.Binding.Dispose();
            }
        }

        private static BlobAssetReference<PresentationBindingBlob> BuildBindingBlob()
        {
            var builder = new BlobBuilder(Allocator.Persistent);
            ref var root = ref builder.ConstructRoot<PresentationBindingBlob>();

            var effects = builder.Allocate(ref root.Effects, 4);
            effects[0] = new PresentationEffectBinding
            {
                EffectId = GodgamePresentationIds.MiraclePingEffectId,
                Kind = PresentationKind.Particle,
                Style = BuildStyleBlock(GodgamePresentationIds.MiraclePingStyle)
            };
            effects[1] = new PresentationEffectBinding
            {
                EffectId = GodgamePresentationIds.JobsiteGhostEffectId,
                Kind = PresentationKind.Particle,
                Style = BuildStyleBlock(GodgamePresentationIds.JobsiteGhostStyle)
            };
            effects[2] = new PresentationEffectBinding
            {
                EffectId = GodgamePresentationIds.ModuleRefitSparksEffectId,
                Kind = PresentationKind.Particle,
                Style = BuildStyleBlock(GodgamePresentationIds.ModuleRefitSparksStyle)
            };
            effects[3] = new PresentationEffectBinding
            {
                EffectId = GodgamePresentationIds.HandAffordanceEffectId,
                Kind = PresentationKind.Particle,
                Style = BuildStyleBlock(GodgamePresentationIds.HandAffordanceStyle)
            };

            builder.Allocate(ref root.Companions, 0);
            return builder.CreateBlobAssetReference<PresentationBindingBlob>(Allocator.Persistent);
        }

        private static PresentationStyleBlock BuildStyleBlock(in PresentationStyleOverride styleOverride)
        {
            return new PresentationStyleBlock
            {
                Style = styleOverride.Style,
                PaletteIndex = styleOverride.HasPalette ? (byte)styleOverride.PaletteIndex : (byte)0,
                Size = styleOverride.HasSize ? styleOverride.Size : 0f,
                Speed = styleOverride.HasSpeed ? styleOverride.Speed : 0f
            };
        }

        private static bool HasEffect(BlobAssetReference<PresentationBindingBlob> binding, int effectId)
        {
            ref var blob = ref binding.Value;
            ref var effects = ref blob.Effects;
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].EffectId == effectId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
