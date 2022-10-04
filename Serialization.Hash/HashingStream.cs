using System;
using System.IO;

namespace SLD.Serialization
{
    public class HashingStream : Stream
    {
        private readonly Stream _underlying;

        public HashingStream(Stream underlying)
        {
            _underlying = underlying;
        }

        public Hash Hash { get; private set; }

        public override bool CanRead
            => _underlying.CanRead;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => _underlying.CanWrite;

        public override long Length
            => _underlying.Length;

        public override long Position
        {
            get => _underlying.Position;
            set => _underlying.Position = value;
        }

        public override void Flush()
            => _underlying.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}