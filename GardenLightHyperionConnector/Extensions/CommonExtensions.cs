using System;
using System.Collections;

namespace GardenLightHyperionConnector.Extensions
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
    }
}
