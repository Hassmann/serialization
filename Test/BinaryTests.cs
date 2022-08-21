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
            TestObject? thing = new TestObject
            {
                Integer = 42
            };

            var serialized = thing.ToBinaryStream();

            serialized.Position = 0;

            var deserialized = Binary.Deserialize<TestObject>(serialized);

            Assert.Equal(thing, deserialized);
        }


    }
}