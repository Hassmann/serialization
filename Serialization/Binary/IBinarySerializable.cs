using System.IO;

namespace SLD.Serialization.Binary
{
    public interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
    }
}
