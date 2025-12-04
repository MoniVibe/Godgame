using System;
using Unity.Mathematics;

namespace Godgame.Environment
{
    /// <summary>
    /// Designer-friendly description for when an effect should be active across the 24-hour cycle.
    /// Converts to <see cref="TimeOfDayWindowData"/> for DOTS systems.
    /// </summary>
    [Serializable]
    public struct TimeOfDayWindow
    {
        public TimeOfDayWindowMode mode;
        public float startHour;
        public float endHour;

        public TimeOfDayWindowMode Mode => mode;

        public static TimeOfDayWindow Always => new()
        {
            mode = TimeOfDayWindowMode.Always,
            startHour = 0f,
            endHour = 24f
        };

        public static TimeOfDayWindow Daytime => new()
        {
            mode = TimeOfDayWindowMode.Daytime,
            startHour = 6f,
            endHour = 18f
        };

        public static TimeOfDayWindow Nighttime => new()
        {
            mode = TimeOfDayWindowMode.Nighttime,
            startHour = 18f,
            endHour = 6f
        };

        public TimeOfDayWindowData ToData()
        {
            var data = new TimeOfDayWindowData
            {
                Mode = mode,
                StartHour = math.clamp(startHour, 0f, 24f),
                EndHour = math.clamp(endHour, 0f, 24f)
            };

            if (data.Mode == TimeOfDayWindowMode.CustomRange && math.abs(data.StartHour - data.EndHour) < 1e-3f)
            {
                // Guard against empty ranges by defaulting to full-day.
                data.StartHour = 0f;
                data.EndHour = 24f;
                data.Mode = TimeOfDayWindowMode.Always;
            }

            return data;
        }
    }

    public enum TimeOfDayWindowMode : byte
    {
        Always = 0,
        Daytime = 1,
        Nighttime = 2,
        CustomRange = 3
    }

    /// <summary>
    /// DOTS-friendly representation of <see cref="TimeOfDayWindow"/>.
    /// </summary>
    public struct TimeOfDayWindowData
    {
        public TimeOfDayWindowMode Mode;
        public float StartHour;
        public float EndHour;
    }

    public static class TimeOfDayWindowUtility
    {
        public static bool IsActive(in TimeOfDayWindowData window, float timeOfDayHours)
        {
            switch (window.Mode)
            {
                case TimeOfDayWindowMode.Daytime:
                    return timeOfDayHours >= 5.5f && timeOfDayHours <= 18.5f;
                case TimeOfDayWindowMode.Nighttime:
                    return !(timeOfDayHours >= 5.5f && timeOfDayHours <= 18.5f);
                case TimeOfDayWindowMode.CustomRange:
                    return ContainsRange(window.StartHour, window.EndHour, timeOfDayHours);
                default:
                    return true;
            }
        }

        private static bool ContainsRange(float startHour, float endHour, float testHour)
        {
            var start = math.clamp(startHour, 0f, 24f);
            var end = math.clamp(endHour, 0f, 24f);
            var hour = math.clamp(testHour, 0f, 24f);

            if (math.abs(start - end) < 1e-3f)
            {
                return true;
            }

            return start <= end
                ? hour >= start && hour <= end
                : hour >= start || hour <= end;
        }
    }
}
