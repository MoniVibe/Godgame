using Godgame.Mana;
using PureDOTS.Runtime.Power;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Godgame/Mana/Mana Module")]
    public sealed class GodgameManaModuleAuthoring : MonoBehaviour
    {
        [Header("Module")]
        public ManaModuleCategory Category = ManaModuleCategory.Weapons;
        public ManaAllocationState InitialState = ManaAllocationState.Normal;
        public string LimbId;

        [Header("Power Draw")]
        public float BaselineDrawMW = 40f;
        [Range(0f, 1f)] public float MinOperatingFraction = 0.2f;
        public byte Priority = 20;

        [Header("Heat")]
        public float HeatCapacity = 1200f;
        public float HeatDissipation = 300f;

        [Header("Spool")]
        public float RampUpPerSecond = 120f;
        public float RampDownPerSecond = 200f;

        [Header("Burnout")]
        public PowerQuality Quality = PowerQuality.Standard;
        public float BurnoutRiskMultiplier = 1f;

        public sealed class Baker : Baker<GodgameManaModuleAuthoring>
        {
            public override void Bake(GodgameManaModuleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);

                AddComponent(entity, new ManaModuleConfig
                {
                    Category = authoring.Category,
                    LimbId = new FixedString64Bytes(authoring.LimbId ?? string.Empty),
                    BaseBurnoutRiskMultiplier = math.max(0f, authoring.BurnoutRiskMultiplier),
                    RampUpPerSecond = math.max(0f, authoring.RampUpPerSecond),
                    RampDownPerSecond = math.max(0f, authoring.RampDownPerSecond)
                });

                AddComponent(entity, new ManaModuleState
                {
                    State = authoring.InitialState
                });

                AddComponent(entity, new PowerConsumer
                {
                    BaselineDraw = math.max(0f, authoring.BaselineDrawMW),
                    RequestedDraw = 0f,
                    MinOperatingFraction = math.clamp(authoring.MinOperatingFraction, 0f, 1f),
                    Priority = authoring.Priority,
                    AllocatedDraw = 0f,
                    Online = 0,
                    Starved = 0
                });

                AddComponent(entity, new PowerAllocationPercent
                {
                    Value = ManaAllocationUtility.ToPercent(authoring.InitialState)
                });

                AddComponent(entity, new PowerHeatProfile
                {
                    BaseHeatGeneration = math.max(0f, authoring.BaselineDrawMW)
                });

                AddComponent(entity, new HeatState
                {
                    CurrentHeat = 0f,
                    MaxHeatCapacity = math.max(0f, authoring.HeatCapacity),
                    PassiveDissipation = math.max(0f, authoring.HeatDissipation)
                });

                AddComponent(entity, new PowerBurnoutSettings
                {
                    Quality = authoring.Quality,
                    CooldownTicks = 0u,
                    RiskMultiplier = math.max(0f, authoring.BurnoutRiskMultiplier)
                });
            }
        }
    }
}
