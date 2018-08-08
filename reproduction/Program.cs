using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace reproduction
{
    public static class Program
    {
        public static void Main(string[] args) => BenchmarkRunner.Run<MinusBenchmark>();
    }

    [MemoryDiagnoser]
    [DisassemblyDiagnoser(true, true)]
    [Q3Column]
    public class MinusBenchmark
    {
        private const ushort length = 100;
        private const int baseInt = 1 << 30;
        private readonly byte[] _buffer = ArrayPool<byte>.Shared.Rent(short.MaxValue);

        [Benchmark(Baseline = true)]
        public int NoMinus()
        {
            var i = 0;
            for (; i < length; i++)
                BinaryPrimitives.WriteInt32BigEndian(_buffer, baseInt);
            return i;
        }

        [Benchmark]
        public int Minus()
        {
            var i = 0;
            for (; i < length; i++)
                BinaryPrimitives.WriteInt32BigEndian(_buffer, baseInt - i);
            return i;
        }

        [Benchmark]
        public int MinusLocalVar()
        {
            var i = 0;
            var local = baseInt;
            for (; i < length; i++)
                BinaryPrimitives.WriteInt32BigEndian(_buffer, local - i);
            return i;
        }

        [Benchmark]
        public int MinusLocalConst()
        {
            var i = 0;
            const int local = baseInt;
            for (; i < length; i++)
                BinaryPrimitives.WriteInt32BigEndian(_buffer, local - i);
            return i;
        }

        [Benchmark]
        public int MinusLocalNoPack()
        {
            var i = 0;
            var local = baseInt;
            for (; i < length; i++)
                local -= i;
            return local;
        }

        [Benchmark]
        public void CArray() => CNative.SerializeArray();

        [Benchmark]
        public void CArrayMinus() => CNative.SerializeArrayMinus();

        [Benchmark]
        public void CppArray() => CppNative.SerializeArray();

        [Benchmark]
        public void CppArrayMinus() => CppNative.SerializeArrayMinus();

        private static class CNative
        {
            [DllImport("libcMsgPack.so", EntryPoint = "serializeIntArray", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SerializeArray();

            [DllImport("libcMsgPack.so", EntryPoint = "serializeIntArrayMinus", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SerializeArrayMinus();
        }

        private static class CppNative
        {
            [DllImport("libcppMsgPack.so", EntryPoint = "serializeIntArray", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SerializeArray();

            [DllImport("libcMsgPack.so", EntryPoint = "serializeIntArrayMinus", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SerializeArrayMinus();
        }
    }
}
