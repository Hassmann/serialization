#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()


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

        public override bool Equals(object? obj)
            => obj is TestObject test
            && Equals(test);

        protected bool IsEqual<T>(T? a, T? b)
            => (a is null && b is null)
            || (a is not null && a.Equals(b));
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


        public override bool Equals(object? obj)
            => obj is DerivedObject test
            && Equals(test);

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Name);
        }
    }


    public class ComplexObject : DerivedObject, IEquatable<ComplexObject>
    {
        public ComplexObject()
        {

        }

        public ComplexObject(BinaryReader reader) : base(reader)
        {
            Nothing = (TestObject)reader.ReadGeneric();
            Something = (TestObject)reader.ReadGeneric();
            KnownNothing = reader.ReadDistinct<DerivedObject>();
            KnownSomething = reader.ReadDistinct<DerivedObject>();
        }

        public TestObject? Nothing { get; set; }
        public TestObject? Something { get; set; }
        public DerivedObject? KnownNothing { get; set; }
        public DerivedObject? KnownSomething { get; set; }


        public bool Equals(ComplexObject? other)
            => base.Equals(other)
            && IsEqual(Nothing, other.Nothing)
            && IsEqual(Something, other.Something)
            && IsEqual(KnownNothing, other.KnownNothing)
            && IsEqual(KnownSomething, other.KnownSomething)
            ;


        public override bool Equals(object? obj)
            => obj is ComplexObject test
            && Equals(test);

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.WriteGeneric(Nothing);
            writer.WriteGeneric(Something);
            writer.WriteDistinct(KnownNothing);
            writer.WriteDistinct(KnownSomething);
        }
    }



}
