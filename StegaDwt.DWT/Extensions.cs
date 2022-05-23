using System;
using System.Linq;

namespace StegaDwt.DWT
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this int[] intArray)
        {
            byte[] result = new byte[intArray.Length * sizeof(int)];
            Buffer.BlockCopy(intArray, 0, result, 0, result.Length);
            return result;
        }

        public static byte[] ToByteArray(this float[] floatArray)
        {
            var byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public static float[] ToFloatArray(this byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }

        public static double[] ToDoubleArray(this byte[] byteArray)
        {
            var doubleArray = new double[byteArray.Length / 8];
            Buffer.BlockCopy(byteArray, 0, doubleArray, 0, doubleArray.Count());
            return doubleArray;
        }

        public static byte[] ToByteArray(this double[] doubleArray)
        {
            var byteArray = new byte[doubleArray.Length * 8];
            Buffer.BlockCopy(doubleArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public static int[] ToIntArray(this byte[] byteArray)
        {
            int[] result = new int[byteArray.Length / sizeof(int)];
            Buffer.BlockCopy(byteArray, 0, result, 0, result.Count());
            return result;
        }

        public static T[] TakeLast<T>(this T[] source, int n)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (n > source.Length)
                throw new ArgumentOutOfRangeException(nameof(n), "Can't be bigger than the array");
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Can't be negative");

            var target = new T[n];
            Array.Copy(source, source.Length - n, target, 0, n);
            return target;
        }
    }
}
