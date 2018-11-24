using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;

namespace Hasher
{
    public enum HasherMode : byte
    {
        Bit8 = 8, Bit4 = 4, Bit2 = 2
    }
    public static class Hasher
    {
        private const int readLimit = 64;
        private static readonly BigInteger modulus = new BigInteger(1) << ((readLimit - 1) * 8);
        public static byte CalculateCheckSum(Stream stream, HasherMode mode = HasherMode.Bit8)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("sosi pisos so svoim nechitayemim strimom");
            }

            var power = readLimit;
            var buffer = new byte[readLimit];
            BigInteger sum = 0;
            var oldPosition = stream.Position;
            while (stream.Position < stream.Length)
            {
                stream.Read(buffer, 0, buffer.Length);

                var curr = new BigInteger(buffer);
                sum = sum + BigInteger.ModPow(curr, power, modulus);
            }
            stream.Seek(oldPosition, SeekOrigin.Begin);

            byte checkSum = 0;
            var byteSum = sum.ToByteArray();
            foreach (var b in byteSum)
            {
                //checkSum += (byte)((((b & 128) >> 7) + ((b & 64) >> 6) + ((b & 32) >> 5) + ((b & 16) >> 4)
                //    + ((b & 8) >> 3) + ((b & 4) >> 2) + ((b & 2) >> 1) + (b & 1)) & 1);
                checkSum ^= b;
            }

            return ShortenCheckSum(checkSum, mode);
        }

        public static byte ShortenCheckSum(byte checkSum, HasherMode mode)
        {
            if (mode < HasherMode.Bit8)
            {
                var bigCheckSum = checkSum;
                checkSum = 0;
                var step = (byte)(8 / (int)mode);
                for (byte i = 0, bits = (byte)mode; i < bits; ++i)
                {
                    byte bitSum = 0;
                    for (byte j = i; j < 8; j += step)
                    {
                        bitSum += (byte)((bigCheckSum >> j) & 1);
                    }
                    checkSum = (byte)((checkSum << 1) + (bitSum & 1));//(byte)(~bitSum & checkSum);
                }
            }

            return checkSum;
        }
    }
}
