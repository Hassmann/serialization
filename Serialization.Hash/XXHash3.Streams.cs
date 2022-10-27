using System;
using System.Diagnostics;
/* Unmerged change from project 'Serialization.Hash (netstandard2.0)'
Before:
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
After:
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
*/

/* Unmerged change from project 'Serialization.Hash (netstandard2.1)'
Before:
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
After:
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
*/

/* Unmerged change from project 'Serialization.Hash (netcoreapp3.1)'
Before:
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
After:
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
*/


namespace SLD.Serialization.XXHash3NET
{

    partial class XXHash3
    {
        public class ContinousHash
        {
            readonly int _blockLength;
            readonly int _stripesPerBlock;
            readonly byte[] _buffer;
            int _index;
            ulong[] _acc = null!;

            ulong _length;
            int _blockCount;

            public ContinousHash()
            {
                _stripesPerBlock = (XXH3_SECRET.Length - XXH_STRIPE_LEN) / 8;
                _blockLength = XXH_STRIPE_LEN * _stripesPerBlock;
                _buffer = new byte[_blockLength];
            }

            internal void Add(ReadOnlyMemory<byte> input)
            {
                if (_acc is not null)
                {
                    Accumulate(input);
                }
                else
                {
                    var space = new Memory<byte>(_buffer, _index, _blockLength - _index);

                    if (space.Length < input.Length)
                    {
                        // Switch to accumulating
                        _acc = new ulong[8]
                        {
                            XXH_PRIME32_3, XXH_PRIME64_1, XXH_PRIME64_2, XXH_PRIME64_3,
                            XXH_PRIME64_4, XXH_PRIME32_2, XXH_PRIME64_5, XXH_PRIME32_1
                        };

                        Accumulate(input);
                    }
                    else
                    {
                        input.CopyTo(space);

                        _index += input.Length;
                    }
                }

                _length += (ulong)input.Length;
            }

            private void Accumulate(ReadOnlyMemory<byte> input)
            {
                var next = input;

                do
                {
                    next = AccumulateNext(next);
                } while (next.Length > 0);
            }

            private ReadOnlyMemory<byte> AccumulateNext(ReadOnlyMemory<byte> input)
            {
                var space = new Memory<byte>(_buffer, _index, _blockLength - _index);

                var overhead = input.Length - space.Length;

                if (overhead <= 0)
                {
                    // Keep on filling up
                    input.CopyTo(space);
                    _index += input.Length;

                    return null;
                }
                else
                {
                    // At least one block can be added
                    var head = input.Slice(0, space.Length);

                    head.CopyTo(space);

                    _index += head.Length;

                    AddBlock();

                    return input.Slice(space.Length, overhead);
                }
            }

            void AddBlock()
            {
                Debug.Assert(_index == _blockLength);

                var readBuffer = new ReadOnlySpan<byte>(_buffer);
                var readSecret = new ReadOnlySpan<byte>(XXH3_SECRET);

                xxh3_accumulate(_acc, readBuffer, XXH3_SECRET, _stripesPerBlock);
                xxh3_scramble_acc_scalar(_acc, readSecret[(XXH3_SECRET.Length - XXH_STRIPE_LEN)..]);

                _index = 0;
                _blockCount++;
            }

            internal ulong Finalize()
            {
                if (_acc is not null)
                {
                    return FinalizeContinous(new ReadOnlySpan<byte>(_buffer, 0, _index));
                }
                else
                {
                    return XXHash3.Hash64(new ReadOnlySpan<byte>(_buffer, 0, _index));
                }
            }

            private ulong FinalizeContinous(ReadOnlySpan<byte> tail)
            {
                var tailBuffer = new byte[XXH_STRIPE_LEN].AsSpan();

                if (tail.Length >= tailBuffer.Length)
                {
                    tail[(tail.Length - XXH_STRIPE_LEN)..].CopyTo(tailBuffer);
                }
                else
                {
                    var diff = tailBuffer.Length - tail.Length;

                    var destination = tailBuffer[diff..];
                    tail.CopyTo(destination);

                    destination = tailBuffer[..diff];
                    var readBuffer = new ReadOnlySpan<byte>(_buffer, _buffer.Length - diff, diff);

                    readBuffer.CopyTo(destination);
                }

                var readSecret = new ReadOnlySpan<byte>(XXH3_SECRET);

                int stripeCount = ((int)(_length - 1) - (_blockLength * _blockCount)) / XXH_STRIPE_LEN;
                xxh3_accumulate(_acc, tail, readSecret, stripeCount);

                xxh3_accumulate_512_scalar(_acc, tailBuffer, readSecret[(readSecret.Length - XXH_STRIPE_LEN - 7)..]);

                return xxh3_merge_accs(_acc, readSecret[11..], (ulong)_length * XXH_PRIME64_1);
            }

        }

    }
}
