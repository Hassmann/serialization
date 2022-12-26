using System;
using System.IO;
using XXHash3NET;

namespace SLD.Serialization
{
    public class HashingStream : Stream
    {
        readonly XXHash3 _hash = XXHash3.Create();

        private readonly Stream _destination;

        public HashingStream(Stream destination)
        {
            _destination = destination;
        }

        public Hash Hash { get; private set; }

        public override bool CanRead
            => false;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => _destination.CanWrite;

        public override long Length
            => _destination.Length;

        public override long Position
        {
            get => _destination.Position;
            set => _destination.Position = value;
        }

        public override void Flush()
            => _destination.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _destination.Write(buffer, offset, count);

            _hash.Update(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        protected override void Dispose(bool disposing)
        {
            Hash = new Hash(_hash.Digest64());

            base.Dispose(disposing);
        }
    }
}