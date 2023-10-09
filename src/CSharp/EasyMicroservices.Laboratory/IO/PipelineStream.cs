using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.IO
{
    /// <summary>
    /// 
    /// </summary>
    internal class PipelineStream : Stream
    {
        PipelineStreamReader pipelineStreamReader;
        NetworkStream _stream;
        public PipelineStream(NetworkStream stream)
        {
            _stream = stream;
            pipelineStreamReader = new PipelineStreamReader(_stream);
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return pipelineStreamReader.ReadAsync(buffer, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
            pipelineStreamReader.Dispose();
        }
    }
}
