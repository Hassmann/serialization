using SLD.Serialization;
using System.Drawing.Imaging;
using System.Text;
using Windows.Foundation.Metadata;

namespace Test
{
    public class CompressionTest
    {
        const int DefaultLength = 1000000;

        [Fact]
        public void RoundTrip()
        {
            // Create Original
            var originalStream = CreateOriginal(DefaultLength);

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

            decompressed.Position = 0;

            for (int i = 0; i < DefaultLength; i++)
            {
                var read = decompressed.ReadByte();

                Assert.Equal((byte)i, read);
            }
        }

        private static Stream CreateOriginal(long length)
        {
            var originalStream = new MemoryStream();
            for (int i = 0; i < length; i++)
            {
                originalStream.WriteByte((byte)i);
            }

            originalStream.Position = 0;

            return originalStream;
        }

        [Fact]
        public void CompressSamplePng()
        {
            var source = System.Drawing.Image.FromFile("Samples/background.png");

            var inmemory = new MemoryStream();
            source.Save(inmemory, ImageFormat.Png);

            inmemory.Position = 0;

            var compressed = new MemoryStream();
            using (var compressing = new CompressingStream(compressed))
            {
                //                inmemory.CopyTo(compressing);
                source.Save(compressing, ImageFormat.Png);
            }
        }
    }
}