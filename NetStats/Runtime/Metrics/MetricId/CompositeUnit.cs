using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    struct CompositeUnit
    {
        internal UnitPrefix Prefix { get; set; }
        internal sbyte BitsExponent { get; set; }
        internal sbyte BytesExponent { get; set; }
        internal sbyte SecondsExponent { get; set; }

        public CompositeUnit(
            UnitPrefix prefix = UnitPrefix.None,
            sbyte bitsExponent = 0,
            sbyte bytesExponent = 0,
            sbyte secondsExponent = 0)
        {
            Prefix = prefix;
            BitsExponent = bitsExponent;
            BytesExponent = bytesExponent;
            SecondsExponent = secondsExponent;
        }

        public static CompositeUnit k_Hertz { get; } =
            Units.Hertz.GetCompositeUnit();

        public bool Equals(CompositeUnit other)
        {
            return Prefix == other.Prefix &&
                   BitsExponent == other.BitsExponent &&
                   BytesExponent == other.BytesExponent &&
                   SecondsExponent == other.SecondsExponent;
        }

        public override bool Equals(object obj)
        {
            return obj is CompositeUnit other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Note: System.Hashcode functionality isn't available in Unity < 2021.2,
            // so this custom hashing can be replaced when versions < 2021.2 no longer
            // need to be supported

            // By virtue of the fact that all four fields are 8 bits we can pack them all in
            // a 32 bit HashCode, and don't even need to hash.
            return
                ((byte)Prefix << 24) |
                ((byte)BitsExponent << 16) |
                ((byte)BytesExponent << 8) |
                (byte)SecondsExponent;
        }

        internal sbyte GetExponent(BaseUnit unit)
        {
            switch (unit)
            {
                case BaseUnit.Bit:
                    return BitsExponent;
                case BaseUnit.Byte:
                    return BytesExponent;
                case BaseUnit.Second:
                    return SecondsExponent;
                default:
                    throw new ArgumentException($"Unhandled BaseUnit {unit}");
            }
        }

        static readonly char[] k_Superscripts = new char[]
            { '⁰', '¹', '²', '³', '⁴', '⁵', '⁶', '⁷', '⁸', '⁹', };
        internal string DisplayString
        {
            get
            {
                var prefixSymbol = Prefix.GetSymbol();
                if (Equals(k_Hertz))
                {
                    // Special case handling for Hertz
                    return prefixSymbol + "Hz";
                }

                var numerator = "";
                var denominator = "";

                void AddUnit(BaseUnit unit, sbyte exponent, ref string str)
                {
                    str += unit.GetSymbol();
                    if (exponent <= 1)
                    {
                        return;
                    }
                    if (exponent >= 100)
                    {
                        str += k_Superscripts[exponent / 100];
                        exponent %= 100;
                    }
                    if (exponent >= 10)
                    {
                        str += k_Superscripts[exponent / 10];
                        exponent %= 10;
                    }
                    str += k_Superscripts[exponent / 10];
                }

                for (var unit = (BaseUnit)1;
                    (int)unit < BaseUnitConstants.k_BaseUnitCount;
                    ++unit)
                {
                    var exponent = GetExponent(unit);
                    if (exponent > 0)
                    {
                        AddUnit(unit, exponent, ref numerator);
                    }
                    else if (exponent < 0)
                    {
                        AddUnit(unit, Math.Abs(exponent), ref denominator);
                    }
                }
                return prefixSymbol + numerator +
                       (denominator == "" ? "" : "/" + denominator);
            }
        }
    }
}