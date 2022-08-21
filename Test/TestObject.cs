using SLD.Serialization.Binary;

namespace Test
{

    public class TestObject : IBinarySerializable, IEquatable<TestObject>
    {
        public TestObject()
        {
        }

        public TestObject(BinaryReader reader)
        {
            Integer = reader.ReadInt32();
        }

        public int Integer { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Integer);
        }

        public bool Equals(TestObject? other) 
            => other is not null 
            && this.Integer == other.Integer;
    }
}
