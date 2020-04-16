using System;

namespace ExerciseDaemon
{
    public static class ExtensionMethods
    {
        public static string ToFormattedDistance(this float? source)
        {
            if (!source.HasValue || source.Value == 0)
            {
                return null;
            }

            if (source.Value > 1000)
            {
                return $"{Math.Round(source.Value / 1000, 1, MidpointRounding.AwayFromZero)}km";
            }

            return $"{Math.Round(source.Value, 0, MidpointRounding.AwayFromZero)}m";
        }

        public static string ToFormattedElevation(this float? source)
        {
            if (!source.HasValue || source.Value == 0)
            {
                return null;
            }

            return $"{Math.Round(source.Value, 0, MidpointRounding.AwayFromZero)}m";
        }
    }
}
