using SLD.Serialization;
using System.Text;
using XXHash3NET;

namespace Test
{
    public class HashTest
    {
        [Fact]
        public void Small()
        {
            var original = "Ein Neger mit Gazelle zagt im Regen nie";
            var originalBytes = Encoding.UTF8.GetBytes(original);
            var originalStream = new MemoryStream(originalBytes);

            // Hash
            var storage = new MemoryStream();
            var hashing = new HashingStream(storage);

            using (hashing)
            {
                originalStream.CopyTo(hashing);
            }

            // Stream unchanged
            Assert.Equal(originalStream.Length, storage.Length);

            storage.Position = 0;

            var restored = Encoding.UTF8.GetString(storage.ToArray());

            Assert.Equal(original, restored);

            Assert.Equal(XXHash3.Hash64(originalBytes), hashing.Hash.Value64);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(16)]
        [InlineData(100)]
        [InlineData(128)]
        [InlineData(200)]
        [InlineData(240)]
        [InlineData(1000)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(1040)]
        [InlineData(2000)]
        [InlineData(2047)]
        [InlineData(2048)]
        [InlineData(2049)]
        [InlineData(3000)]
        [InlineData(4096)]
        public void SingleSizes(int N)
        {
            var original = new byte[N];

            for (int i = 0; i < N; i++)
            {
                original[i] = (byte)i;
            }

            var originalStream = new MemoryStream(original);

            // Hash
            var storage = new MemoryStream();
            var hashing = new HashingStream(storage);

            using (hashing)
            {
                originalStream.CopyTo(hashing);
            }

            // Stream unchanged
            Assert.Equal(originalStream.Length, storage.Length);
            var restored = storage.ToArray();

            for (int i = 0; i < N; i++)
            {
                Assert.Equal(original[i], restored[i]);
            }

            Assert.Equal(XXHash3.Hash64(original), hashing.Hash.Value64);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(16)]
        [InlineData(100)]
        [InlineData(128)]
        [InlineData(200)]
        [InlineData(240)]
        [InlineData(1000)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(1040)]
        [InlineData(2000)]
        [InlineData(2047)]
        [InlineData(2048)]
        [InlineData(2049)]
        [InlineData(3000)]
        [InlineData(4096)]
        public void Batches(int k)
        {
            const int N = 4096;

            var original = new byte[N];

            for (int i = 0; i < N; i++)
            {
                original[i] = (byte)i;
            }


            // Hash
            var storage = new MemoryStream();
            var hashing = new HashingStream(storage);

            using (hashing)
            {
                for (int i = 0; i < N; i += k)
                {
                    var upper = Math.Min(i + k, N);

                    var slice = original[i..upper];

                    hashing.Write(slice);
                }
            }

            // Stream unchanged
            Assert.Equal(original.Length, storage.Length);
            var restored = storage.ToArray();

            for (int i = 0; i < N; i++)
            {
                Assert.Equal(original[i], restored[i]);
            }

            Assert.Equal(XXHash3.Hash64(original), hashing.Hash.Value64);
        }

    }
}