using System;
using System.IO;

namespace Hasher
{
    class Program
    {
        const int Iterations = 100;
        const int MinimumBatchSize = 100_000;
        const int MaximumBatchSize = 1_000_000;
        static string ByteToString(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        static void Main(string[] args)
        {
            var rnd = new Random();
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
                Console.WriteLine(
                    "\n4 bits:\n{0}\n{1}",
                    ByteToString(Hasher.ShortenCheckSum(checkSum, HasherMode.Bit4)),
                    ByteToString(Hasher.ShortenCheckSum(alteredCheckSum, HasherMode.Bit4))
                );
                Console.WriteLine(
                    "\n2 bits:\n{0}\n{1}",
                    ByteToString(Hasher.ShortenCheckSum(checkSum, HasherMode.Bit2)),
                    ByteToString(Hasher.ShortenCheckSum(alteredCheckSum, HasherMode.Bit2))
                );
                Console.WriteLine("\n{0}\n", new string('*', 16));
            }

            Console.ReadKey();
        }
    }
}
