namespace SLD.Serialization.Binary
{
    public static class BinaryExtensions
    {
        public static Stream ToBinaryStream(this object? source, bool withTypeInfo = false)
            => Binary.Serialize(source, withTypeInfo);
    }
}
