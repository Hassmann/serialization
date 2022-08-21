using SLD.Serialization.Binary;

namespace Test
{
    public class BinaryTests
    {
        [Fact]
        public void Null()
        {
            TestObject? thing = null;

            var serialized = thing.ToBinaryStream();

            var deserialized = Binary.Deserialize<TestObject>(serialized);

            Assert.Null(deserialized);
        }

        [Fact]
        public void Simple()
        {
            TestObject thing = new TestObject
            {
                Integer = 42
            };

            var serialized = thing.ToBinaryStream();

            serialized.Position = 0;

            var deserialized = Binary.Deserialize<TestObject>(serialized);

            Assert.Equal(thing, deserialized);
        }

        [Fact]
        public void Derived()
        {
            DerivedObject thing = new DerivedObject
            {
                Name = "Derived",
                Integer = 42
            };

            var serialized = thing.ToBinaryStream(true);

            serialized.Position = 0;

            var deserialized = Binary.Deserialize(serialized) as DerivedObject;

            Assert.NotNull(deserialized);
            Assert.Equal(thing, deserialized);
        }

    }
}