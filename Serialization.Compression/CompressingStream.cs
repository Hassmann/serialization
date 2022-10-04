using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
