﻿using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif
namespace SLD.Serialization.XXHash3NET
{
    public static partial class XXHash3
    {

        private const uint XXH_PRIME32_1 = 0x9E3779B1U;
        private const uint XXH_PRIME32_2 = 0x85EBCA77U;
        private const uint XXH_PRIME32_3 = 0xC2B2AE3DU;
        private const uint XXH_PRIME32_4 = 0x27D4EB2FU;
        private const uint XXH_PRIME32_5 = 0x165667B1U;

        private const ulong XXH_PRIME64_1 = 0x9E3779B185EBCA87UL;
        private const ulong XXH_PRIME64_2 = 0xC2B2AE3D27D4EB4FUL;
        private const ulong XXH_PRIME64_3 = 0x165667B19E3779F9UL;
        private const ulong XXH_PRIME64_4 = 0x85EBCA77C2B2AE63UL;
        private const ulong XXH_PRIME64_5 = 0x27D4EB2F165667C5UL;

        private const int XXH_STRIPE_LEN = 64;
        private const int XXH_ACC_NB = (XXH_STRIPE_LEN / sizeof(ulong));

        private static readonly byte[] XXH3_SECRET = new byte[192]
        {
            0xb8, 0xfe, 0x6c, 0x39, 0x23, 0xa4, 0x4b, 0xbe, 0x7c, 0x01, 0x81, 0x2c, 0xf7, 0x21, 0xad, 0x1c,
            0xde, 0xd4, 0x6d, 0xe9, 0x83, 0x90, 0x97, 0xdb, 0x72, 0x40, 0xa4, 0xa4, 0xb7, 0xb3, 0x67, 0x1f,
            0xcb, 0x79, 0xe6, 0x4e, 0xcc, 0xc0, 0xe5, 0x78, 0x82, 0x5a, 0xd0, 0x7d, 0xcc, 0xff, 0x72, 0x21,
            0xb8, 0x08, 0x46, 0x74, 0xf7, 0x43, 0x24, 0x8e, 0xe0, 0x35, 0x90, 0xe6, 0x81, 0x3a, 0x26, 0x4c,
            0x3c, 0x28, 0x52, 0xbb, 0x91, 0xc3, 0x00, 0xcb, 0x88, 0xd0, 0x65, 0x8b, 0x1b, 0x53, 0x2e, 0xa3,
            0x71, 0x64, 0x48, 0x97, 0xa2, 0x0d, 0xf9, 0x4e, 0x38, 0x19, 0xef, 0x46, 0xa9, 0xde, 0xac, 0xd8,
            0xa8, 0xfa, 0x76, 0x3f, 0xe3, 0x9c, 0x34, 0x3f, 0xf9, 0xdc, 0xbb, 0xc7, 0xc7, 0x0b, 0x4f, 0x1d,
            0x8a, 0x51, 0xe0, 0x4b, 0xcd, 0xb4, 0x59, 0x31, 0xc8, 0x9f, 0x7e, 0xc9, 0xd9, 0x78, 0x73, 0x64,
            0xea, 0xc5, 0xac, 0x83, 0x34, 0xd3, 0xeb, 0xc3, 0xc5, 0x81, 0xa0, 0xff, 0xfa, 0x13, 0x63, 0xeb,
            0x17, 0x0d, 0xdd, 0x51, 0xb7, 0xf0, 0xda, 0x49, 0xd3, 0x16, 0x55, 0x26, 0x29, 0xd4, 0x68, 0x9e,
            0x2b, 0x16, 0xbe, 0x58, 0x7d, 0x47, 0xa1, 0xfc, 0x8f, 0xf8, 0xb8, 0xd1, 0x7a, 0xd0, 0x31, 0xce,
            0x45, 0xcb, 0x3a, 0x8f, 0x95, 0x16, 0x04, 0x28, 0xaf, 0xd7, 0xfb, 0xca, 0xbb, 0x4b, 0x40, 0x7e,
        };

        public static ulong Hash64(ReadOnlySpan<byte> data, ulong seed = 0)
        {
            return Hash64(data, data.Length, XXH3_SECRET, XXH3_SECRET.Length, seed);
        }
        public static ulong Hash64(ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            return Hash64(data, length, XXH3_SECRET, XXH3_SECRET.Length, seed);
        }

        public static ulong Hash64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, int secretLength, ulong seed)
        {
            if (length <= 16)
            {
                return xxh3_0to16_64(data, length, secret, seed);
            }
            else if (length <= 128)
            {
                return xxh3_17to128_64(data, length, secret, secretLength, seed);
            }
            else if (length <= 240)
            {
                return xxh3_129to240_64(data, length, secret, secretLength, seed);
            }
            else
            {
                return xxh3_hashLong_64(data, length, secret, secretLength, seed);
            }
        }

        private static ulong xxh3_0to16_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, ulong seed)
        {
            if (length > 8) return xxh3_len_9to16_64(data, length, secret, seed);
            else if (length >= 4) return xxh3_len_4to8_64(data, length, secret, seed);
            else if (length > 0) return xxh3_len_1to3_64(data, length, secret, seed);
            else return xxh3_avalanche(seed ^ (read_le64(secret[56..]) ^ read_le64(secret[64..])));


            static ulong xxh3_len_9to16_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, ulong seed)
            {
                ulong bitflip1 = (read_le64(secret[24..]) ^ read_le64(secret[32..])) + seed;
                ulong bitflip2 = (read_le64(secret[40..]) ^ read_le64(secret[48..])) - seed;
                ulong input_low = read_le64(data) ^ bitflip1;
                ulong input_high = read_le64(data[(length - 8)..]) ^ bitflip2;
                ulong acc = (ulong)length + swap64(input_low) + input_high + xxh3_mul128_fold64(input_low, input_high);

                return xxh3_avalanche(acc);
            }
            static ulong xxh3_len_4to8_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, ulong seed)
            {
                seed ^= (ulong)swap32((uint)seed) << 32;

                uint input1 = read_le32(data);
                uint input2 = read_le32(data[(length - 4)..]);
                ulong bitflip = (read_le64(secret[8..]) ^ read_le64(secret[16..])) - seed;
                ulong input64 = input2 + (((ulong)input1) << 32);
                ulong keyed = input64 ^ bitflip;

                return xxh3_rrmxmx(keyed, (ulong)length);
            }
            static ulong xxh3_len_1to3_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, ulong seed)
            {
                byte c1 = data[0];
                byte c2 = data[length >> 1];
                byte c3 = data[length - 1];
                uint combined = ((uint)c1 << 16) | ((uint)c2 << 24) | ((uint)c3 << 0) | ((uint)length << 8);
                ulong bitflip = (read_le32(secret) ^ read_le32(secret[4..])) + seed;
                ulong keyed = (ulong)combined ^ bitflip;

                return xxh64_avalanche(keyed);
            }
        }
        private static ulong xxh3_17to128_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, int secretLength, ulong seed)
        {
            ulong acc = (ulong)length * XXH_PRIME64_1;

            if (length > 32)
            {
                if (length > 64)
                {
                    if (length > 96)
                    {
                        acc += xxh3_mix16B(data[48..], secret[96..], seed);
                        acc += xxh3_mix16B(data[(length - 64)..], secret[112..], seed);
                    }

                    acc += xxh3_mix16B(data[32..], secret[64..], seed);
                    acc += xxh3_mix16B(data[(length - 48)..], secret[80..], seed);
                }

                acc += xxh3_mix16B(data[16..], secret[32..], seed);
                acc += xxh3_mix16B(data[(length - 32)..], secret[48..], seed);
            }

            acc += xxh3_mix16B(data, secret, seed);
            acc += xxh3_mix16B(data[(length - 16)..], secret[16..], seed);

            return xxh3_avalanche(acc);
        }
        private static ulong xxh3_129to240_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, int secretLength, ulong seed)
        {
            ulong acc = (ulong)length * XXH_PRIME64_1;

            int round_count = length / 16;
            for (int i = 0; i < 8; i++)
            {
                acc += xxh3_mix16B(data[(16 * i)..], secret[(16 * i)..], seed);
            }

            acc = xxh3_avalanche(acc);

            for (int i = 8; i < round_count; i++)
            {
                acc += xxh3_mix16B(data[(16 * i)..], secret[((16 * (i - 8)) + 3)..], seed);
            }

            acc += xxh3_mix16B(data[(length - 16)..], secret[(136 - 17)..], seed);

            return xxh3_avalanche(acc);
        }

        private static ulong xxh3_hashLong_64(ReadOnlySpan<byte> data, int length, ReadOnlySpan<byte> secret, int secretLength, ulong seed)
        {
            ulong[] acc = new ulong[8]
            {
                XXH_PRIME32_3, XXH_PRIME64_1, XXH_PRIME64_2, XXH_PRIME64_3,
                XXH_PRIME64_4, XXH_PRIME32_2, XXH_PRIME64_5, XXH_PRIME32_1
            };

            int stripesPerBlock = (secretLength - XXH_STRIPE_LEN) / 8;
            int blockLength = XXH_STRIPE_LEN * stripesPerBlock;
            int blockCount = (length - 1) / blockLength;

            for (int n = 0; n < blockCount; n++)
            {
                xxh3_accumulate(acc, data[(n * blockLength)..], secret, stripesPerBlock);
                xxh3_scramble_acc_scalar(acc, secret[(secretLength - XXH_STRIPE_LEN)..]);
            }

            int stripeCount = ((length - 1) - (blockLength * blockCount)) / XXH_STRIPE_LEN;
            xxh3_accumulate(acc, data[(blockCount * blockLength)..], secret, stripeCount);

            ReadOnlySpan<byte> p = data[(length - XXH_STRIPE_LEN)..];
            xxh3_accumulate_512_scalar(acc, p, secret[(secretLength - XXH_STRIPE_LEN - 7)..]);

            return xxh3_merge_accs(acc, secret[11..], (ulong)length * XXH_PRIME64_1);
        }

        private static ulong xxh3_merge_accs(Span<ulong> acc, ReadOnlySpan<byte> secret, ulong start)
        {
            ulong result = start;

            for (int i = 0; i < 4; i++)
            {
                result += xxh3_mix2accs(acc[(2 * i)..], secret[(16 * i)..]);
            }

            return xxh3_avalanche(result);
        }
        private static ulong xxh3_mix2accs(Span<ulong> acc, ReadOnlySpan<byte> secret)
        {
            return xxh3_mul128_fold64(acc[0] ^ read_le64(secret), acc[1] ^ read_le64(secret[8..]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh3_avalanche(ulong hash)
        {
            hash = xxh_xorshift64(hash, 37);
            hash *= 0x165667919E3779F9UL;
            hash = xxh_xorshift64(hash, 32);

            return hash;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh64_avalanche(ulong hash)
        {
            hash ^= hash >> 33;
            hash *= XXH_PRIME64_2;
            hash ^= hash >> 29;
            hash *= XXH_PRIME64_3;
            hash ^= hash >> 32;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh3_rrmxmx(ulong h64, ulong len)
        {
            h64 ^= rotl64(h64, 49) ^ rotl64(h64, 24);
            h64 *= 0x9FB21C651E98DF25UL;
            h64 ^= (h64 >> 35) + len;
            h64 *= 0x9FB21C651E98DF25UL;

            return xxh_xorshift64(h64, 28);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh3_mix16B(ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret, ulong seed)
        {
            ulong input_low = read_le64(data);
            ulong input_high = read_le64(data[8..]);

            return xxh3_mul128_fold64(
                input_low ^ (read_le64(secret) + seed),
                input_high ^ (read_le64(secret[8..]) - seed));
        }

        private static unsafe void xxh3_accumulate(ulong[] acc, ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret, int stripeCount)
        {
            for (int i = 0; i < stripeCount; i++)
            {
                xxh3_accumulate_512_scalar(acc, data[(i * XXH_STRIPE_LEN)..], secret[(i * 8)..]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void xxh3_accumulate_512_scalar(ulong[] acc, ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret)
        {
            for (int i = 0; i < XXH_ACC_NB; i++)
            {
                ulong data_val = read_le64(data[(8 * i)..]);
                ulong data_key = data_val ^ read_le64(secret[(i * 8)..]);

                acc[i ^ 1] += data_val;
                acc[i] += xxh_mul32to64(data_key & 0xFFFFFFFF, data_key >> 32);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void xxh3_scramble_acc_scalar(ulong[] acc, ReadOnlySpan<byte> secret)
        {
            for (int i = 0; i < XXH_ACC_NB; i++)
            {
                ulong key64 = read_le64(secret[(8 * i)..]);
                ulong acc64 = acc[i];

                acc64 = xxh_xorshift64(acc64, 47);
                acc64 ^= key64;
                acc64 *= XXH_PRIME32_1;

                acc[i] = acc64;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh_xorshift64(ulong v64, int shift)
        {
            return v64 ^ (v64 >> shift);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong xxh3_mul128_fold64(ulong lhs, ulong rhs)
        {
#if NETCOREAPP
            ulong low;
            ulong high = Bmi2.X64.MultiplyNoFlags(lhs, rhs, &low);

            return low ^ high;
#else
            return xxh3_mul128_fold64_slow(rhs, lhs);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong xxh3_mul128_fold64_slow(ulong lhs, ulong rhs)
        {
            uint lhsHigh = (uint)(lhs >> 32);
            uint rhsHigh = (uint)(rhs >> 32);
            uint lhsLow = (uint)lhs;
            uint rhsLow = (uint)rhs;

            ulong high = xxh_mul32to64(lhsHigh, rhsHigh);
            ulong middleOne = xxh_mul32to64(lhsLow, rhsHigh);
            ulong middleTwo = xxh_mul32to64(lhsHigh, rhsLow);
            ulong low = xxh_mul32to64(lhsLow, rhsLow);

            ulong t = low + (middleOne << 32);
            ulong carry1 = t < low ? 1u : 0u;

            low = t + (middleTwo << 32);
            ulong carry2 = low < t ? 1u : 0u;
            high = high + (middleOne >> 32) + (middleTwo >> 32) + carry1 + carry2;

            return high + low;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong xxh_mul32to64(ulong x, ulong y)
        {
            return (ulong)(uint)x * (ulong)(uint)y;
        }


        // -------------- UTILITY METHODS -------------- \\
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint read_le32(ReadOnlySpan<byte> data)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(data);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong read_le64(ReadOnlySpan<byte> data)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint swap32(uint value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong swap64(ulong value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rotl64(ulong value, int shift)
        {
            return (value << shift) | (value >> (64 - shift));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong rotr64(ulong value, int shift)
        {
            return (value << (64 - shift)) | (value >> shift);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte _mm_shuffle(byte p3, byte p2, byte p1, byte p0)
        {
            return (byte)((p3 << 6) | (p2 << 4) | (p1 << 2) | p0);
        }
    }
}
