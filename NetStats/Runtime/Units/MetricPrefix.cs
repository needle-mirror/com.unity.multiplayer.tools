using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    internal enum MetricPrefix : sbyte
    {
        /// Metric prefix for 1000^-4
        Pico  = -4,
        /// Metric prefix for 1000^-3
        Nano  = -3,
        /// Metric prefix for 1000^-2
        Micro = -2,
        /// Metric prefix for 1000^-1
        Milli = -1,

        None = 0,

        /// Metric prefix for 1000^1
        Kilo = 1,
        /// Metric prefix for 1000^2
        Mega = 2,
        /// Metric prefix for 1000^3
        Giga = 3,
        /// Metric prefix for 1000^4
        Tera = 4,
    }

    internal static class UnitPrefixExtensions
    {
        public static string GetSymbol(this MetricPrefix prefix)
        {
            switch (prefix)
            {

                case MetricPrefix.Nano:  return "n";
                case MetricPrefix.Micro: return "μ";
                case MetricPrefix.Milli: return "m";

                case MetricPrefix.None: return "";

                case MetricPrefix.Kilo: return "k";
                case MetricPrefix.Mega: return "M";
                case MetricPrefix.Giga: return "G";
                case MetricPrefix.Tera: return "T";

                default:
                    throw new ArgumentException($"Unhandled {nameof(MetricPrefix)} {prefix}");
            }
        }

        public static float GetValueFloat(this MetricPrefix prefix)
        {
            switch (prefix)
            {
                case MetricPrefix.Pico:  return 1e-12f;
                case MetricPrefix.Nano:  return 1e-9f;
                case MetricPrefix.Micro: return 1e-6f;
                case MetricPrefix.Milli: return 1e-3f;

                case MetricPrefix.None: return 1;

                case MetricPrefix.Kilo: return 1e3f;
                case MetricPrefix.Mega: return 1e6f;
                case MetricPrefix.Giga: return 1e9f;
                case MetricPrefix.Tera: return 1e12f;

                default:
                    throw new ArgumentException($"Unhandled {nameof(MetricPrefix)} {prefix}");
            }
        }

    }
}