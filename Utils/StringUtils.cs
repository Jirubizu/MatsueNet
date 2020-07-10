using System;
using System.Collections;

namespace MatsueNet.Utils
{
    public static class StringUtils
    {
        public static IEnumerable SplitByLength(this string str, int maxLength)
        {
            for (var index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }
    }
}