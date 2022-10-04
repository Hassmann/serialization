using SLD.Serialization;
using System.Text;

namespace Test
{
    public class CompressionTest
    {
        [Fact]
        public void RoundTrip()
        {
            var original = "Ein Neger mit Gazelle zagt im Regen nie";
            var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(original));

            // Compress
            var storage = new MemoryStream();

            using (var compressor = new CompressingStream(storage))
            {
                originalStream.CopyTo(compressor);
            }


            // Decompress
            var decompressed = new MemoryStream();

            storage.Position = 0;

            using (var decompressor = new DecompressingStream(storage))
            {
                decompressor.CopyTo(decompressed);
            }

            Assert.Equal(originalStream.Length, decompressed.Length);

            var restored = Encoding.UTF8.GetString(decompressed.ToArray());

            Assert.Equal(original, restored);
        }
    }
}