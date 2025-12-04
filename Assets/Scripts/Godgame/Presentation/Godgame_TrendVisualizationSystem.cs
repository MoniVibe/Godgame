using Godgame.Villages;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Trend direction for aggregates.
    /// </summary>
    public enum AggregateTrend : byte
    {
        Improving = 0,
        Stable = 1,
        Declining = 2
    }

    /// <summary>
    /// Component storing trend data for visualization.
    /// </summary>
    public struct AggregateTrendData : IComponentData
    {
        public AggregateTrend PopulationTrend;
        public AggregateTrend WealthTrend;
        public AggregateTrend FoodTrend;
        public AggregateTrend FertilityTrend;
        public float TrendIntensity; // 0-1, how strong the trend is
    }

    /// <summary>
    /// System that calculates trends from aggregate history and updates visual state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_AggregateHistorySystem))]
    public partial struct Godgame_TrendVisualizationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AggregateHistory>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new CalculateTrendsJob();
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job that calculates trends from village history.
    /// </summary>
    [BurstCompile]
    public partial struct CalculateTrendsJob : IJobEntity
    {
        public void Execute(
            ref AggregateTrendData trendData,
            in DynamicBuffer<AggregateHistory> history,
            in VillageCenterVisualState visualState)
        {
            if (history.Length < 10)
            {
                // Not enough data
                trendData.PopulationTrend = AggregateTrend.Stable;
                trendData.WealthTrend = AggregateTrend.Stable;
                trendData.FoodTrend = AggregateTrend.Stable;
                trendData.TrendIntensity = 0f;
                return;
            }

            // Compare last 10 samples vs previous 10 samples
            int recentStart = math.max(0, history.Length - 10);
            int previousStart = math.max(0, history.Length - 20);

            // Calculate averages
            int recentPop = 0, previousPop = 0;
            int recentWealth = 0, previousWealth = 0;
            int recentFood = 0, previousFood = 0;

            for (int i = recentStart; i < history.Length; i++)
            {
                recentPop += history[i].Population;
                recentWealth += history[i].Wealth;
                recentFood += history[i].Food;
            }

            int recentCount = history.Length - recentStart;
            if (recentCount > 0)
            {
                recentPop /= recentCount;
                recentWealth /= recentCount;
                recentFood /= recentCount;
            }

            if (previousStart < history.Length - 10)
            {
                for (int i = previousStart; i < recentStart; i++)
                {
                    previousPop += history[i].Population;
                    previousWealth += history[i].Wealth;
                    previousFood += history[i].Food;
                }

                int previousCount = recentStart - previousStart;
                if (previousCount > 0)
                {
                    previousPop /= previousCount;
                    previousWealth /= previousCount;
                    previousFood /= previousCount;
                }
            }

            // Determine trends
            trendData.PopulationTrend = CalculateTrend(recentPop, previousPop);
            trendData.WealthTrend = CalculateTrend(recentWealth, previousWealth);
            trendData.FoodTrend = CalculateTrend(recentFood, previousFood);

            // Calculate overall trend intensity
            float popChange = math.abs(recentPop - previousPop) / math.max(1, previousPop);
            float wealthChange = math.abs(recentWealth - previousWealth) / math.max(1, previousWealth);
            float foodChange = math.abs(recentFood - previousFood) / math.max(1, previousFood);
            trendData.TrendIntensity = math.saturate((popChange + wealthChange + foodChange) / 3f);
        }

        private static AggregateTrend CalculateTrend(int recent, int previous)
        {
            if (recent > previous * 1.1f) return AggregateTrend.Improving; // 10% increase
            if (recent < previous * 0.9f) return AggregateTrend.Declining; // 10% decrease
            return AggregateTrend.Stable;
        }
    }

    /// <summary>
    /// Job that applies trend visualization to village centers.
    /// </summary>
    [BurstCompile]
    public partial struct ApplyVillageTrendVisualizationJob : IJobEntity
    {
        public void Execute(
            ref VillageCenterVisualState visualState,
            in AggregateTrendData trendData)
        {
            // Update visual state based on trends
            // Improving = brighter green tint
            // Declining = red tint
            // Stable = default phase color

            float4 trendTint = visualState.PhaseTint;

            if (trendData.PopulationTrend == AggregateTrend.Improving)
            {
                trendTint = math.lerp(trendTint, new float4(0f, 1f, 0f, 1f), trendData.TrendIntensity * 0.3f); // Green
            }
            else if (trendData.PopulationTrend == AggregateTrend.Declining)
            {
                trendTint = math.lerp(trendTint, new float4(1f, 0f, 0f, 1f), trendData.TrendIntensity * 0.3f); // Red
            }

            visualState.PhaseTint = trendTint;
        }
    }

    /// <summary>
    /// Job that applies trend visualization to regions.
    /// </summary>
    [BurstCompile]
    public partial struct ApplyRegionTrendVisualizationJob : IJobEntity
    {
        public void Execute(
            ref BiomePresentationData presentationData,
            in AggregateTrendData trendData)
        {
            // Update biome presentation data based on fertility trends
            // Improving = brighter colors
            // Declining = darker colors

            if (trendData.FertilityTrend == AggregateTrend.Improving)
            {
                // Increase saturation
                // (Would need to adjust material properties)
            }
            else if (trendData.FertilityTrend == AggregateTrend.Declining)
            {
                // Decrease saturation, darken
                // (Would need to adjust material properties)
            }
        }
    }
}

