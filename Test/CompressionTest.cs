using SLD.Serialization;
using System.Drawing.Imaging;
using System.Text;
using Windows.Foundation.Metadata;

namespace Test
{
    public class CompressionTest
    {
        [Fact]
        public void RoundTrip()
        {
            // Create Original
            const int length = 1000000;
            var originalStream = new MemoryStream();

            for (int i = 0; i < length; i++)
            {
                originalStream.WriteByte((byte)i);
            }

            originalStream.Position = 0;

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

            for (int i = 0; i < length; i++)
            {
                var read = decompressed.ReadByte();

                Assert.Equal((byte)i, read);
            }
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