using System.IO;
using ZstdNet;

namespace SLD.Serialization
{
    public class CompressingStream : CompressionStream
    {
        public CompressingStream(Stream destination) : base(destination)
        {
        }
    }
}
