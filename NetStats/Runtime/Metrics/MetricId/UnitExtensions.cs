using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    static class UnitExtensions
    {
        internal static CompositeUnit GetCompositeUnit(this Units units)
        {
            switch (units)
            {
                case Units.None:
                    return new CompositeUnit();

                case Units.Bits:
                    return new CompositeUnit(bitsExponent: 1);
                case Units.BitsPerSecond:
                    return new CompositeUnit(bitsExponent: 1, secondsExponent: -1);
                case Units.KiloBitsPerSecond:
                    return new CompositeUnit(prefix: UnitPrefix.Kilo, bitsExponent: 1, secondsExponent: -1);
                case Units.MegaBitsPerSecond:
                    return new CompositeUnit(prefix: UnitPrefix.Mega, bitsExponent: 1, secondsExponent: -1);

                case Units.Bytes:
                    return new CompositeUnit(bytesExponent: 1);
                case Units.KiloBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Kilo, bytesExponent: 1);
                case Units.MegaBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Mega, bytesExponent: 1);
                case Units.GigaBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Giga, bytesExponent: 1);
                case Units.TeraBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Tera, bytesExponent: 1);

                case Units.KibiBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Kibi, bytesExponent: 1);
                case Units.MebiBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Mebi, bytesExponent: 1);
                case Units.GibiBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Gibi, bytesExponent: 1);
                case Units.TebiBytes:
                    return new CompositeUnit(prefix: UnitPrefix.Tebi, bytesExponent: 1);

                case Units.BytesPerSecond:
                    return new CompositeUnit(bytesExponent: 1, secondsExponent: -1);
                case Units.KiloBytesPerSecond:
                    return new CompositeUnit(prefix: UnitPrefix.Kilo, bytesExponent: 1, secondsExponent: -1);
                case Units.MegaBytesPerSecond:
                    return new CompositeUnit(prefix: UnitPrefix.Mega, bytesExponent: 1, secondsExponent: -1);
                case Units.GigaBytesPerSecond:
                    return new CompositeUnit(prefix: UnitPrefix.Giga, bytesExponent: 1, secondsExponent: -1);

                case Units.NanoSeconds:
                    return new CompositeUnit(prefix: UnitPrefix.Nano, secondsExponent: 1);
                case Units.MicroSeconds:
                    return new CompositeUnit(prefix: UnitPrefix.Micro, secondsExponent: 1);
                case Units.MilliSeconds:
                    return new CompositeUnit(prefix: UnitPrefix.Milli, secondsExponent: 1);
                case Units.Seconds:
                    return new CompositeUnit(secondsExponent: 1);

                case Units.Hertz:
                    return new CompositeUnit(secondsExponent: -1);
                case Units.KiloHertz:
                    return new CompositeUnit(prefix: UnitPrefix.Kilo, secondsExponent: -1);
                case Units.MegaHertz:
                    return new CompositeUnit(prefix: UnitPrefix.Mega, secondsExponent: -1);
                case Units.GigaHertz:
                    return new CompositeUnit(prefix: UnitPrefix.Giga, secondsExponent: -1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(units), units, null);
            }
        }
    }
}