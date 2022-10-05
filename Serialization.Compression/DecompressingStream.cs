using System.IO;
using ZstdSharp;

namespace SLD.Serialization
{
    public class DecompressingStream : DecompressionStream
    {
        public DecompressingStream(Stream source) : base(source)
        {
        }
    }
}
