using SLD.Serialization;
using System.Text;
using XXHash3NET;

namespace Test
{
    public class HashTest
    {
        [Fact]
        public void RoundTrip()
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
    }
}