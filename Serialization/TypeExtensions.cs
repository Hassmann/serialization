using System;
using System.IO;

namespace SLD.Serialization
{
    public static class TypeExtensions
    {
        public static void Write(this BinaryWriter writer, DateTime dateTime)
            => writer.Write(dateTime.Ticks);

        public static DateTime ReadDateTime(this BinaryReader reader)
            => new DateTime(reader.ReadInt64());
    }
}