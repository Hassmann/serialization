using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
