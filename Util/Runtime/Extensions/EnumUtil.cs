using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Multiplayer.Tools.Util
{
    internal static class EnumUtil {
        public static T[] GetValues<T>() {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}