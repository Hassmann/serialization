using System.IO;
using ZstdSharp;

namespace SLD.Serialization
{
    public class CompressingStream : CompressionStream
    {
        public CompressingStream(Stream destination) : base(destination)
        {
        }
    }
}
