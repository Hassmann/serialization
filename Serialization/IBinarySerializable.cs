using System.IO;

namespace SLD.Serialization
{
    public interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
    }
}
