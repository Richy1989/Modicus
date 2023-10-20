using System.Collections;

namespace Modicus.Extensions
{
    public static class CommonExtensions
    {
        /// <summary>Adds the range to the ArrayList.</summary>
        /// <param name="arrayList">The array list.</param>
        /// <param name="range">The range.</param>
        public static void AddRange(this ArrayList arrayList, IList range)
        {
            foreach (var item in range)
            {
                arrayList.Add(item);
            }
        }

        /// <summary>Determines whether this instance is number.</summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified value is number; otherwise, <c>false</c>.</returns>
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double;
        }
    }
}