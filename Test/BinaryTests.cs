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
            TestObject? thing = new TestObject();

            var serialized = thing.ToBinaryStream();

            var deserialized = Binary.Deserialize<TestObject>(serialized);

            Assert.Equal(thing, deserialized);
        }


    }
}