using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    enum BaseUnit
    {
        Bit,
        Byte,
        Second,
    }

    static class BaseUnitConstants
    {
        internal const int k_BaseUnitCount = 3;
    }

    static class BaseUnitExtensions
    {
        public static string GetSymbol(this BaseUnit unit)
        {
            switch (unit)
            {
                case BaseUnit.Bit: return "b";
                case BaseUnit.Byte: return "B";
                case BaseUnit.Second: return "s";
                default:
                    throw new ArgumentException($"Unhandled BaseUnit {unit}");
            }
        }
    }
}