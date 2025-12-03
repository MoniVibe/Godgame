using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake InitiativeBandTable ScriptableObjects.
    /// </summary>
    public sealed class InitiativeBandTableAuthoring : MonoBehaviour
    {
        public InitiativeBandTable Table;
    }

    public sealed class InitiativeBandTableAuthoringBaker : Baker<InitiativeBandTableAuthoring>
    {
        public override void Bake(InitiativeBandTableAuthoring authoring)
        {
            var table = authoring.Table;
            if (table == null || table.Bands == null || table.Bands.Length == 0)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<InitiativeBandLookupBlob>();

            var bandsArray = builder.Allocate(ref root.Bands, table.Bands.Length);

            for (int i = 0; i < table.Bands.Length; i++)
            {
                var bandDef = table.Bands[i];
                bandsArray[i] = new InitiativeBandBlob
                {
                    BandId = new FixedString32Bytes(bandDef.BandId ?? $"Band_{i}"),
                    TickBudget = bandDef.TickBudget,
                    RecoveryHalfLife = bandDef.RecoveryHalfLife,
                    StressBreakpoints = new StressBreakpointsBlob
                    {
                        Panic = bandDef.StressBreakpoints.Panic,
                        Rally = bandDef.StressBreakpoints.Rally,
                        Frenzy = bandDef.StressBreakpoints.Frenzy
                    }
                };
            }

            var blobAsset = builder.CreateBlobAssetReference<InitiativeBandLookupBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new InitiativeBandLookupBlobComponent { Lookup = blobAsset });
        }
    }
}
