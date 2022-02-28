using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStats
{
    /// TODO: MTT-1852 - Document this API
    [Serializable]
    internal struct MetricId : IEquatable<MetricId>
    {
        [field: SerializeField]
        internal int TypeIndex { get; set; }

        [field: SerializeField]
        internal int EnumValue { get; set; }

        internal Type EnumType => Types[TypeIndex];
        internal string Name => Enum.GetName(EnumType, EnumValue);

        T GetEnumMetadata<T>(T[][] array)
        {
            var index = Array.IndexOf(Values[TypeIndex], EnumValue);
            return array[TypeIndex][index];
        }
        internal string DisplayName => GetEnumMetadata(DisplayNames) ?? Name;
        internal MetricKind MetricKind => GetEnumMetadata(MetricKinds);
        internal CompositeUnit Unit => GetEnumMetadata(Units);

        internal static Type[] Types { get; }

        /// Array of the names of each type
        internal static string[][] Names { get; }

        /// Array of the values of each type
        internal static int[][] Values { get; }

        /// Array of the display names of each metric
        internal static string[][] DisplayNames { get; }

        /// Array of the MetricKind of each metric
        internal static MetricKind[][] MetricKinds { get; }

        /// Array of the Units of each metric
        internal static CompositeUnit[][] Units { get; }

        static MetricId()
        {
            Types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly
                    .GetTypes()
                    .Where(type =>
                        type.IsEnum &&
                        type.GetEnumUnderlyingType() == typeof(Int32) &&
                        type.GetCustomAttributes(typeof(MetricTypeEnumAttribute), true).Length > 0))
                .OrderBy(type => type.FullName)
                .ToArray();

            Names = new string[Types.Length][];
            Values = new int[Types.Length][];
            DisplayNames = new string[Types.Length][];
            MetricKinds = new MetricKind[Types.Length][];
            Units = new CompositeUnit[Types.Length][];

            for (var i = 0; i < Types.Length; ++i)
            {
                var enumType = Types[i];
                Names[i] = enumType.GetEnumNames();

                var values = enumType.GetEnumValues();
                Values[i] = new int[values.Length];
                DisplayNames[i] = new string[values.Length];
                MetricKinds[i] = new MetricKind[values.Length];
                Units[i] = new CompositeUnit[values.Length];
                for (var j = 0; j < values.Length; ++j)
                {
                    var name = Names[i][j];

                    var value = (values as IList)[j];
                    Values[i][j] = Convert.ToInt32(value);

                    var enumMemberInfo = enumType.GetMember(name).FirstOrDefault();

                    if (enumMemberInfo
                        ?.GetCustomAttributes(typeof(MetricMetadataAttribute), false)
                        .FirstOrDefault() is MetricMetadataAttribute metadata)
                    {
                        DisplayNames[i][j] = metadata.DisplayName;
                        MetricKinds[i][j] = metadata.MetricKind;
                        Units[i][j] = metadata.Units.GetCompositeUnit();
                    }
                    else
                    {
                        // The array entries will default to null, 0, or the enum value corresponding to zero
                    }
                    if (MetricKinds[i][j] == MetricKind.Counter)
                    {
                        Units[i][j].SecondsExponent -= 1;
                    }
                }
            }
        }

        internal MetricId(int typeIndex, int enumValue)
        {
            if (!(0 <= typeIndex && typeIndex < Types.Length))
            {
                throw new ArgumentOutOfRangeException(
                    $"Cannot construct {nameof(MetricId)} with out-of-range {nameof(TypeIndex)} {typeIndex}. " +
                    $"Value must be in range [0, {Types.Length}).");
            }
            TypeIndex = typeIndex;
            EnumValue = enumValue;
        }

        internal MetricId(Type enumType, int enumValue)
        {
            TypeIndex = Array.IndexOf(Types, enumType);
            EnumValue = enumValue;
        }

        public static MetricId Create<T>(T value)
            where T: struct, IConvertible
        {
            var enumType = typeof(T);
            var enumValue = value.ToInt32(CultureInfo.InvariantCulture);
            return new MetricId(enumType, enumValue);
        }

        public bool Equals(MetricId other)
        {
            return TypeIndex == other.TypeIndex && EnumValue == other.EnumValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MetricId)obj);
        }

        public override int GetHashCode()
        {
            return 173 * TypeIndex + 13 * EnumValue;
        }

        public override string ToString() => Name;

        public static implicit operator string(MetricId metricId) => metricId.ToString();
    }
}