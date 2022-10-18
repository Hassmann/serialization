using System.IO;
using ZstdNet;

namespace SLD.Serialization
{
    public class DecompressingStream : DecompressionStream
    {
        public DecompressingStream(Stream source) : base(source)
        {
        }
    }
}
