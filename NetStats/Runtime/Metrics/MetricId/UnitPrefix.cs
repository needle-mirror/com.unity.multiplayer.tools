using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    internal enum UnitPrefix : byte
    {
        None,

        /// Base 10 prefix for 10^-9
        Nano,
        /// Base 10 prefix for 10^-6
        Micro,
        /// Base 10 prefix for 10^-3
        Milli,

        /// Base 10 prefix for 10^3
        Kilo,
        /// Base 10 prefix for 10^6
        Mega,
        /// Base 10 prefix for 10^9
        Giga,
        /// Base 10 prefix for 10^12
        Tera,

        /// Binary prefix for 1024
        Kibi,
        /// Binary prefix for 1024^2
        Mebi,
        /// Binary prefix for 1024^3
        Gibi,
        /// Binary prefix for 1024^4
        Tebi,
    }

    internal static class UnitPrefixExtensions
    {
        public static string GetSymbol(this UnitPrefix prefix)
        {
            switch (prefix)
            {
                case UnitPrefix.None: return "";

                case UnitPrefix.Nano:  return "n";
                case UnitPrefix.Micro: return "μ";
                case UnitPrefix.Milli: return "m";

                case UnitPrefix.Kilo: return "k";
                case UnitPrefix.Mega: return "M";
                case UnitPrefix.Giga: return "G";
                case UnitPrefix.Tera: return "T";

                case UnitPrefix.Kibi: return "ki";
                case UnitPrefix.Mebi: return "Mi";
                case UnitPrefix.Gibi: return "Gi";
                case UnitPrefix.Tebi: return "Ti";

                default:
                    throw new ArgumentException($"Unhandled UnitPrefix {prefix}");
            }
        }

        public static double GetValue(this UnitPrefix prefix)
        {
            switch (prefix)
            {
                case UnitPrefix.None: return 1;

                case UnitPrefix.Nano:  return 1e-9;
                case UnitPrefix.Micro: return 1e-6;
                case UnitPrefix.Milli: return 1e-3;

                case UnitPrefix.Kilo: return 1e3;
                case UnitPrefix.Mega: return 1e6;
                case UnitPrefix.Giga: return 1e9;
                case UnitPrefix.Tera: return 1e12;

                case UnitPrefix.Kibi: return 1024L;
                case UnitPrefix.Mebi: return 1024L * 1024L;
                case UnitPrefix.Gibi: return 1024L * 1024L * 1024L;
                case UnitPrefix.Tebi: return 1024L * 1024L * 1024L * 1024L;

                default:
                    throw new ArgumentException($"Unhandled UnitPrefix {prefix}");
            }
        }

    }
}