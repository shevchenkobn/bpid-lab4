using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System.Collections.ObjectModel;

namespace Hasher
{
    class Program
    {
        const int Iterations = 100;
        const int MinimumBatchSize = 1_000;
        const int MaximumBatchSize = 10_000;
        static string ByteToString(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        static void Main(string[] args)
        {
            //CheckHasher();
            var dict = GetCollidedHashes(new BigInteger(1) << 48, new BigInteger(1) << 49);

            Console.ReadKey();
        }

        static void CheckHasher()
        {
            var rnd = new Random();
            var failed8bitCount = 0;
            var failed4bitCount = 0;
            var failed2bitCount = 0;
            for (var i = 0; i < Iterations; i++)
            {
                var bytes = new byte[rnd.Next(MinimumBatchSize, MaximumBatchSize)];
                Console.WriteLine("Created byte array of {0}. Generating...", bytes.Length);
                rnd.NextBytes(bytes);
                var alteredBytes = new byte[bytes.Length];
                Buffer.BlockCopy(bytes, 0, alteredBytes, 0, bytes.Length);
                alteredBytes[rnd.Next(0, bytes.Length)] = (byte)rnd.Next(256);

                Console.WriteLine("Generated. Calculating checksum for source and altered arrays...");
                var checkSum = Hasher.CalculateCheckSum(new MemoryStream(bytes));
                var alteredCheckSum = Hasher.CalculateCheckSum(new MemoryStream(alteredBytes));

                Console.WriteLine("\n8 bits:\n{0}\n{1}", ByteToString(checkSum), ByteToString(alteredCheckSum));
                if (GetChangePercent(checkSum, alteredCheckSum, HasherMode.Bit8) < 0.3)
                {
                    failed8bitCount++;
                }

                var smallSum = Hasher.ShortenCheckSum(checkSum, HasherMode.Bit4);
                var otherSmallSum = Hasher.ShortenCheckSum(alteredCheckSum, HasherMode.Bit4);
                Console.WriteLine("\n4 bits:\n{0}\n{1}", ByteToString(smallSum), ByteToString(otherSmallSum));
                if (GetChangePercent(smallSum, otherSmallSum, HasherMode.Bit4) < 0.3)
                {
                    failed4bitCount++;
                }

                smallSum = Hasher.ShortenCheckSum(checkSum, HasherMode.Bit2);
                otherSmallSum = Hasher.ShortenCheckSum(alteredCheckSum, HasherMode.Bit2);
                Console.WriteLine("\n2 bits:\n{0}\n{1}", ByteToString(smallSum), ByteToString(otherSmallSum));
                if (GetChangePercent(checkSum, alteredCheckSum, HasherMode.Bit2) < 0.3)
                {
                    failed2bitCount++;
                }
                Console.WriteLine("\n{0}\n", new string('*', 16));
            }

            Console.WriteLine(
                "\n8 bit nodiffs: {0}, 4 bit nodiffs {1}, 2 bit nodiffs {2}; Total {3}",
                failed8bitCount,
                failed4bitCount, 
                failed2bitCount, 
                Iterations
            );
        }

        static double GetChangePercent(byte checkSum, byte otherCheckSum, HasherMode mode)
        {
            var diffCount = 0;
            var limit = (int)mode;
            for (var i = 0; i < limit; i++)
            {
                var bitMask = 1 << i;
                if ((checkSum & i) != (otherCheckSum & i))
                {
                    diffCount++;
                }
            }
            return (double)diffCount / limit;
        }

        static Dictionary<HasherMode, Dictionary<byte, List<byte[]>>> GetCollidedHashes(BigInteger start, BigInteger end)
        {
            var dict = new Dictionary<HasherMode, Dictionary<byte, List<byte[]>>>();
            var bit8dict = new Dictionary<byte, List<byte[]>>();

            dict[HasherMode.Bit8] = bit8dict;
            for (var curr = start; curr < end; curr++)
            {
                var checkSum = Hasher.CalculateCheckSum(new MemoryStream(curr.ToByteArray()));
                if (!bit8dict.ContainsKey(checkSum))
                {
                    bit8dict[checkSum] = new List<byte[]>();
                }
                bit8dict[checkSum].Add(curr.ToByteArray());
            }

            dict[HasherMode.Bit4] = new Dictionary<byte, List<byte[]>>();
            dict[HasherMode.Bit2] = new Dictionary<byte, List<byte[]>>();
            foreach (var pair in bit8dict)
            {
                foreach (var mode in new [] { HasherMode.Bit4, HasherMode.Bit2 }) {
                    var checkSum = Hasher.ShortenCheckSum(pair.Key, mode);
                    if (dict[mode].ContainsKey(checkSum))
                    {
                        dict[mode][checkSum].AddRange(pair.Value);
                    }
                    else
                    {
                        dict[mode][checkSum] = pair.Value;
                    }
                }
            }
            return dict;
        }
    }
}
