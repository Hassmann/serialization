using SLD.Serialization;
using System.Text;

namespace Test
{
    public class TypeTests
    {
        [Fact]
        public void SerializeDateTime()
        {
            var original = DateTime.Now;

            var stream = new MemoryStream();

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(original);
            }

            stream.Position = 0;

            DateTime deserialized;

            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                deserialized = reader.ReadDateTime();
            }

            Assert.Equal(original, deserialized);
        }
    }
}