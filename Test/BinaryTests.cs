using SLD.Serialization;
using System.Text;

namespace Test
{
    public class BinaryTests : IDisposable
    {
        private readonly MemoryStream MemoryStream;
        private readonly BinaryWriter Writer;
        private BinaryReader? _reader;

        public BinaryTests()
        {
            MemoryStream = new MemoryStream();
            Writer = new BinaryWriter(MemoryStream, Encoding.UTF8, true);
        }

        private BinaryReader Reader
        {
            get
            {
                if (_reader is null)
                {
                    MemoryStream.Position = 0;
                    _reader = new BinaryReader(MemoryStream);
                }

                return _reader;
            }
        }

        public void Dispose()
        {
            Writer.Dispose();
            MemoryStream.Dispose();
            _reader?.Dispose();
        }

        [Fact]
        public void Null()
        {
            TestObject? thing = null;

            Binary.Serialize(thing, false, Writer);

            var deserialized = Binary.Deserialize<TestObject>(Reader);

            Assert.Null(deserialized);
        }

        [Fact]
        public void Simple()
        {
            TestObject thing = new TestObject
            {
                Integer = 42
            };

            Binary.Serialize(thing, false, Writer);

            var deserialized = Binary.Deserialize<TestObject>(Reader);

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

            Binary.Serialize(thing, true, Writer);

            var deserialized = Binary.DeserializeDerived(Reader) as DerivedObject;

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

            Binary.Serialize(thing, true, Writer);

            var deserialized = Binary.DeserializeDerived(Reader) as ComplexObject;

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
            Binary.SerializeGeneric(item, Writer);

            var deserialized = Binary.DeserializeGeneric(Reader);

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

        [Fact]
        public void CustomSerializable()
        {
            TestObject thing = new DerivedObject
            {
                Name = "Derived",
                Integer = 2
            };

            Binary.SerializeCustom(thing, Writer, CustomSerializeType);

            var deserialized = Binary.DeserializeCustom(Reader, CustomDeserialize) as TestObject;

            Assert.NotNull(deserialized);
            Assert.Equal(thing, deserialized);
        }

        [Fact]
        public void SerializableEnumerable()
        {
            var things = new TestObject[]
            {
                new TestObject{Integer = 1},
                new TestObject{Integer = 2},
                new TestObject{Integer = 3},
            };

            Binary.SerializeAll(things, Writer);

            var deserialized = Binary.DeserializeAll<TestObject>(Reader).ToArray();

            Assert.NotNull(deserialized);
            Assert.True(things.SequenceEqual(deserialized));
        }

        [Fact]
        public void DerivedEnumerable()
        {
            var things = new TestObject[]
            {
                new TestObject
                {
                    Integer = 1
                },
                new DerivedObject
                {
                    Name = "Derived",
                    Integer = 2
                },
                new ComplexObject
                {
                    Name = "Complex",
                    Integer = 3,

                    Nothing = null,
                    Something = new DerivedObject { Name = "Something" },
                    KnownNothing = null,
                    KnownSomething = new DerivedObject { Name = "Known Derived" },
                }
            };

            Binary.SerializeAllDerived(things, Writer);

            var deserialized = Binary.DeserializeAllDerived<TestObject>(Reader).ToArray();

            Assert.NotNull(deserialized);
            Assert.True(things.SequenceEqual(deserialized));
        }

        [Fact]
        public void CustomSerializableEnumerable()
        {
            var things = new TestObject[]
            {
                new TestObject
                {
                    Integer = 1
                },
                new DerivedObject
                {
                    Name = "Derived",
                    Integer = 2
                },
                new ComplexObject
                {
                    Name = "Complex",
                    Integer = 3,

                    Nothing = null,
                    Something = new DerivedObject { Name = "Something" },
                    KnownNothing = null,
                    KnownSomething = new DerivedObject { Name = "Known Derived" },
                }
            };

            Binary.SerializeAllCustom(things, Writer, CustomSerializeType);

            var deserialized = Binary.DeserializeAllCustom<TestObject>(Reader, CustomDeserialize).ToArray();

            Assert.NotNull(deserialized);
            Assert.True(things.SequenceEqual(deserialized));
        }

        [Fact]
        public void GenericEnumerable()
        {
            var things = new object?[]
            {
                null,
                "Some text",
                '?',
                42
            };

            Binary.SerializeAllGeneric(things, Writer);

            var deserialized = Binary.DeserializeAllGeneric(Reader).ToArray();

            Assert.NotNull(deserialized);
            Assert.True(things.SequenceEqual(deserialized));
        }

        [Fact]
        public void Strings()
        {
            var strings = new []
            {
                "1",
                "2",
                "3",
            };

            Binary.SerializeAll(strings, Writer);

            var deserialized = Binary.DeserializeAllStrings(Reader).ToArray();

            Assert.NotNull(deserialized);
            Assert.True(strings.SequenceEqual(deserialized));
        }

        private void CustomSerializeType(IBinarySerializable? serializable, BinaryWriter writer)
        {
            byte tag = serializable switch
            {
                ComplexObject => 3,
                DerivedObject => 2,
                TestObject => 1,

                _ => throw new Exception()
            };

            writer.Write(tag);
        }

        private object CustomDeserialize(BinaryReader reader)
        {
            var tag = reader.ReadByte();

            return tag switch
            {
                1 => new TestObject(reader),
                2 => new DerivedObject(reader),
                3 => new ComplexObject(reader),

                _ => throw new Exception()
            };
        }
    }
}