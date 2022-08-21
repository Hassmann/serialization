#pragma warning disable xUnit2000 // Constants and literals should be the expected argument
#pragma warning disable xUnit2003 // Do not use equality check to test for null value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class TestObjectTests
    {
        [Fact]
        public void TestObjectEquality()
        {
            TestObject a1 = new TestObject
            {
                Integer = 42
            };

            TestObject a2 = new TestObject
            {
                Integer = 42
            };

            TestObject b = new TestObject
            {
                Integer = -1
            };

            Assert.Equal(a1, a2);
            Assert.NotEqual(a1, b);
            Assert.NotEqual(null, b);
            Assert.NotEqual(b, null);

            Assert.True(a1.Equals(a2));
            //Assert.True(object.Equals(a1, a2));
            //Assert.True(a1 == a2);
            Assert.False(a1.Equals(b));
            Assert.False(a1 == b);
        }

        [Fact]
        public void DerivedObjectEquality()
        {
            DerivedObject a1 = new DerivedObject
            {
                Name = "A"
            };

            DerivedObject a2 = new DerivedObject
            {
                Name = "A"
            };

            DerivedObject b = new DerivedObject
            {
                Name = "B"
            };

            Assert.Equal(a1, a2);
            Assert.NotEqual(a1, b);
            Assert.NotEqual(null, b);
            Assert.NotEqual(b, null);

            Assert.True(a1.Equals(a2));
            //Assert.True(object.Equals(a1, a2));
            //Assert.True(a1 == a2);
            Assert.False(a1.Equals(b));
            Assert.False(a1 == b);
        }

        [Fact]
        public void ComplexObjectEquality()
        {
            ComplexObject Create() => new ComplexObject
            {
                Name = "Complex",
                Integer = 42,

                //Nothing = null,
                Something = new DerivedObject { Name = "Something" },
                //KnownNothing = null,
                //KnownSomething = new DerivedObject { Name = "Known Derived" },
            };

            ComplexObject a1 = Create();
            ComplexObject a2 = Create();

            Assert.Equal(a1, a2);
        }

    }
}
