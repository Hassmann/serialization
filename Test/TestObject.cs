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

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(Integer);
        }

        public bool Equals(TestObject? other) 
            => other is not null 
            && this.Integer == other.Integer;
    }

    public class DerivedObject : TestObject, IEquatable<DerivedObject>
    {
        public DerivedObject()
        {

        }

        public DerivedObject(BinaryReader reader) : base(reader)
        {
            Name = reader.ReadString();
        }

        public string Name { get; set; }

        public bool Equals(DerivedObject? other) 
            => base.Equals(other)
            && this.Name == other.Name;

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Name);
        }
    }
}
