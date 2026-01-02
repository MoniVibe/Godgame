using Godgame.Mana;
using PureDOTS.Runtime.Power;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Godgame/Mana/Mana Core")]
    public sealed class GodgameManaCoreAuthoring : MonoBehaviour
    {
        [Header("Mana Core")]
        public float OutputMW = 80f;
        [Range(0f, 2.5f)] public float OutputPercent = 1f;
        [Range(0f, 1f)] public float Efficiency = 0.9f;
        public byte TechLevel = 8;

        [Header("Distribution")]
        [Range(0f, 1f)] public float DistributionEfficiency = 0.95f;
        [Range(0f, 1f)] public float ConduitDamage = 0f;
        public byte DistributionTechLevel = 8;

        [Header("Mana Pool (Battery)")]
        public float CapacityMWs = 2500f;
        public float StartingStoredMWs = 2500f;
        public float MaxChargeRateMW = 80f;
        public float MaxDischargeRateMW = 200f;
        public float SelfDischargeRate = 0.0001f;
        [Range(0f, 1f)] public float ChargeEfficiency = 0.95f;
        [Range(0f, 1f)] public float DischargeEfficiency = 0.95f;
        public byte BatteryTechLevel = 8;

        [Header("Mana Capacitor")]
        public bool AddCapacitorBank = false;
        public float CapacitorCapacityMWs = 1000f;
        public float CapacitorStoredMWs = 0f;
        public float CapacitorChargeRateMW = 20f;
        public float CapacitorDischargeRateMW = 200f;
        public float CapacitorSelfDischargeRate = 0f;
        [Range(0f, 1f)] public float CapacitorChargeEfficiency = 0.95f;
        [Range(0f, 1f)] public float CapacitorDischargeEfficiency = 0.95f;
        public byte CapacitorTechLevel = 8;

        [Header("Focus")]
        public ManaFocusMode DefaultFocus = ManaFocusMode.Balanced;

        public sealed class Baker : Baker<GodgameManaCoreAuthoring>
        {
            public override void Bake(GodgameManaCoreAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);

                AddComponent(entity, new PowerGenerator
                {
                    TheoreticalMaxOutput = math.max(0f, authoring.OutputMW),
                    CurrentOutputPercent = math.saturate(authoring.OutputPercent),
                    Efficiency = math.saturate(authoring.Efficiency),
                    DegradationLevel = 0f,
                    WasteHeat = 0f,
                    TechLevel = authoring.TechLevel
                });

                AddComponent(entity, new PowerDistribution
                {
                    InputPower = 0f,
                    OutputPower = 0f,
                    TransmissionLoss = 0f,
                    DistributionEfficiency = math.saturate(authoring.DistributionEfficiency),
                    ConduitDamage = math.saturate(authoring.ConduitDamage),
                    TechLevel = authoring.DistributionTechLevel
                });

                var capacity = math.max(0f, authoring.CapacityMWs);
                AddComponent(entity, new PowerBattery
                {
                    MaxCapacity = capacity,
                    CurrentStored = math.clamp(authoring.StartingStoredMWs, 0f, capacity),
                    MaxChargeRate = math.max(0f, authoring.MaxChargeRateMW),
                    MaxDischargeRate = math.max(0f, authoring.MaxDischargeRateMW),
                    SelfDischargeRate = math.max(0f, authoring.SelfDischargeRate),
                    ChargeEfficiency = math.saturate(authoring.ChargeEfficiency),
                    DischargeEfficiency = math.saturate(authoring.DischargeEfficiency),
                    Health = 1f,
                    CycleCount = 0,
                    MaxCycles = 5000,
                    TechLevel = authoring.BatteryTechLevel
                });

                AddComponent(entity, new ManaFocus { Mode = authoring.DefaultFocus });

                if (authoring.AddCapacitorBank)
                {
                    var bankEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var bankCapacity = math.max(0f, authoring.CapacitorCapacityMWs);
                    AddComponent(bankEntity, new PowerBattery
                    {
                        MaxCapacity = bankCapacity,
                        CurrentStored = math.clamp(authoring.CapacitorStoredMWs, 0f, bankCapacity),
                        MaxChargeRate = math.max(0f, authoring.CapacitorChargeRateMW),
                        MaxDischargeRate = math.max(0f, authoring.CapacitorDischargeRateMW),
                        SelfDischargeRate = math.max(0f, authoring.CapacitorSelfDischargeRate),
                        ChargeEfficiency = math.saturate(authoring.CapacitorChargeEfficiency),
                        DischargeEfficiency = math.saturate(authoring.CapacitorDischargeEfficiency),
                        Health = 1f,
                        CycleCount = 0,
                        MaxCycles = 10000,
                        TechLevel = authoring.CapacitorTechLevel
                    });
                    AddComponent(bankEntity, new PowerBankTag());
                    AddComponent(bankEntity, new PowerDomainRef { Value = entity });
                }
            }
        }
    }
}
