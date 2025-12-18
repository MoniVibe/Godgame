using Godgame.Telemetry;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Scenarios;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Evaluates telemetry summaries against configured budgets and reports quality issues.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameTelemetryBudgetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!SystemAPI.HasComponent<GodgameTelemetrySummary>(telemetryEntity) ||
                !SystemAPI.HasComponent<GodgameTelemetryBudgets>(telemetryEntity))
            {
                state.Enabled = false;
                return;
            }

            var summary = SystemAPI.GetComponent<GodgameTelemetrySummary>(telemetryEntity);
            var budgets = SystemAPI.GetComponent<GodgameTelemetryBudgets>(telemetryEntity);
            if (budgets.Enforce == 0)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                var minutes = math.max(1f / 60f, timeState.WorldSeconds / 60f);
                summary.ResourcesPerMinute = summary.ResourceTotal / minutes;
                summary.TasksPerMinute = summary.TaskCount / math.max(1f, minutes);
                summary.CombatPerMinute = summary.CombatTotal / math.max(1f, minutes);
            }

            if (summary.ResourcesPerMinute < budgets.MinResourcesPerMinute && (summary.BudgetsFailed & 1) == 0)
            {
                ScenarioExitUtility.ReportQuality("Budget/ResourcesPerMinute", ScenarioSeverity.Error,
                    $"Resources/minute {summary.ResourcesPerMinute:F2} below budget {budgets.MinResourcesPerMinute:F2}");
                summary.BudgetsFailed |= 1;
                summary.SummaryDirty = 1;
            }

            if (summary.StarvationCount > budgets.MaxStarvation && (summary.BudgetsFailed & 2) == 0)
            {
                ScenarioExitUtility.ReportQuality("Budget/Starvation", ScenarioSeverity.Error,
                    $"Starvation count {summary.StarvationCount} exceeds {budgets.MaxStarvation}");
                summary.BudgetsFailed |= 2;
                summary.SummaryDirty = 1;
            }

            if (summary.OscillationCount > budgets.MaxOscillation && (summary.BudgetsFailed & 4) == 0)
            {
                ScenarioExitUtility.ReportQuality("Budget/Oscillation", ScenarioSeverity.Error,
                    $"Decision oscillations {summary.OscillationCount} exceed {budgets.MaxOscillation}");
                summary.BudgetsFailed |= 4;
                summary.SummaryDirty = 1;
            }

            SystemAPI.SetComponent(telemetryEntity, summary);
        }
    }
}
