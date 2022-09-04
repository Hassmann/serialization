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

        [Fact]
        public void Complex()
        {
            ComplexObject thing = new ComplexObject
            {
                Name = "Complex",
                Integer = 42,

                Nothing = null,
                Something = new DerivedObject { Name = "Something" },
                KnownNothing = null,
                KnownSomething = new DerivedObject { Name = "Known Derived" },
            };

            var serialized = thing.ToBinaryStream(true);

            serialized.Position = 0;

            var deserialized = Binary.Deserialize(serialized) as ComplexObject;

            Assert.NotNull(deserialized);
            Assert.Equal(thing, deserialized);
        }

        [Theory]
        [InlineData(null)]
        [InlineData((String)"42")]
        [InlineData((Boolean)true)]
        [InlineData((Byte)42)]
        [InlineData((SByte)42)]
        [InlineData((Char)42)]
        //[InlineData((Decimal)42)]
        [InlineData((Double)42)]
        [InlineData((Single)42)]
        [InlineData((Int32)42)]
        [InlineData((UInt32)42)]
        [InlineData((Int64)42)]
        [InlineData((UInt64)42)]
        [InlineData((Int16)42)]
        [InlineData((UInt16)42)]
        public void GenericValue(object item)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            Binary.SerializeGeneric(item, writer);

            stream.Position = 0;

            var deserialized = Binary.DeserializeGeneric(stream);

            if (item is not null)
            {
                Assert.IsType(item.GetType(), deserialized);
            }

            Assert.Equal(item, deserialized);
        }

        [Fact]
        public void GenericInstance()
        {
            GenericValue(new DerivedObject { Name = "Generic" });
        }

        [Fact]
        public void GenericDecimal()
        {
            GenericValue((Decimal)42);
        }
    }
}