using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Fauna
{
    /// <summary>
    /// Plays optional ambience clips defined on spawn rules.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FaunaAmbientBehaviourSystem))]
    public partial struct FaunaAmbientSoundSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FaunaAmbientSound>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (soundRW, transformRO) in SystemAPI.Query<RefRW<FaunaAmbientSound>, RefRO<LocalToWorld>>())
            {
                var sound = soundRW.ValueRO;
                if (!sound.Clip.IsValid() || sound.Clip.Value == null || sound.IntervalSeconds <= 0f)
                {
                    continue;
                }

                sound.Cooldown -= deltaTime;
                if (sound.Cooldown <= 0f)
                {
                    AudioSource.PlayClipAtPoint(sound.Clip.Value, transformRO.ValueRO.Position);
                    sound.Cooldown = sound.IntervalSeconds;
                }

                soundRW.ValueRW = sound;
            }
        }
    }
}
