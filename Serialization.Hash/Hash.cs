using System;
using System.IO;

namespace SLD.Serialization
{
    using XXHash3NET;

    public struct Hash : IBinarySerializable
    {
        public const int BitLength = 64;
        public const int Size = BitLength / 8;
        private readonly ulong _value;

        public Hash(ulong value)
        {
            _value = value;
        }

        public Hash(BinaryReader reader)
        {
            _value = reader.ReadUInt64();
        }

        public ulong Value64
            => _value;

        public byte[] Bytes
            => BitConverter.GetBytes(_value);

        public static Hash From(ReadOnlySpan<byte> data)
        {
            ulong xx = XXHash3.Hash64(data);

            return new Hash(xx);
        }

        //public static Hash From(Stream input)
        //{
        //	var xx = XXHash64.Create();

        //	byte[] bytes = xx.ComputeHash(input);

        //	return new Hash(bytes);
        //}

        public static Hash Deserialize(string serializedHash)
        {
            byte[] bytes = Convert.FromBase64String(serializedHash);

            return new Hash(BitConverter.ToUInt64(bytes, 0));
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Value64);
        }

        public override string ToString()
        {
            return Convert.ToBase64String(Bytes);
        }
    }
}