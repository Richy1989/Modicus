using System.Collections;

namespace Modicus.Extensions
{
    public static class CommonExtensions
    {
        public static void AddRange(this ArrayList arrayList, IList range)
        {
            foreach (var item in range)
            {
                arrayList.Add(item);
            }
        }

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