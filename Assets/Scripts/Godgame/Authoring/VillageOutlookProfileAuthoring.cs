using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake VillageOutlookProfile ScriptableObjects.
    /// </summary>
    public sealed class VillageOutlookProfileAuthoring : MonoBehaviour
    {
        public VillageOutlookProfile Profile;
    }

    public sealed class VillageOutlookProfileAuthoringBaker : Baker<VillageOutlookProfileAuthoring>
    {
        public override void Bake(VillageOutlookProfileAuthoring authoring)
        {
            var profile = authoring.Profile;
            if (profile == null)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<VillageOutlookProfileBlob>();

            root.ProfileName = new FixedString64Bytes(profile.ProfileName ?? profile.name);

            var blendsArray = builder.Allocate(ref root.AxisBlends, profile.AxisBlends?.Length ?? 0);
            if (profile.AxisBlends != null)
            {
                for (int i = 0; i < profile.AxisBlends.Length; i++)
                {
                    blendsArray[i] = new AxisBlendBlob
                    {
                        AxisId = new FixedString64Bytes(profile.AxisBlends[i].AxisId ?? ""),
                        BlendValue = profile.AxisBlends[i].BlendValue
                    };
                }
            }

            root.DefaultInitiativeBand = (byte)profile.DefaultInitiativeBand;
            root.Governance = (byte)profile.Governance;

            var blobAsset = builder.CreateBlobAssetReference<VillageOutlookProfileBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VillageOutlookProfileBlobComponent { Profile = blobAsset });
        }
    }
}
