using nanoFramework.Hardware.Esp32;
using System.Diagnostics;

namespace Modicus.Helpers
{
    public class Diagnostics
    {
        /// <summary>Prints the available memory of the device.</summary>
        /// <param name="msg">The MSG.</param>
        public static void PrintMemory(string msg)
        {
            NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.Internal, out uint totalSize, out uint totalFree, out uint largestFree);
            Debug.WriteLine($"{msg} -> Internal Mem:  Total Internal: {totalSize} Free: {totalFree} Largest: {largestFree}");
            Debug.WriteLine($"nF Mem:  {nanoFramework.Runtime.Native.GC.Run(false)}");
        }
    }
}
