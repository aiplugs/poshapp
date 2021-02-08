using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Web
{
    public class WebSocketStream : Stream
    {
        private readonly WebSocket _webSocket;
        
        public WebSocketStream(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var context = new Microsoft.VisualStudio.Threading.JoinableTaskContext();
            var jtf = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(context);
            return jtf.Run(() => ReadAsync(buffer, offset, count));
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return (await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken)).Count;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return (await _webSocket.ReceiveAsync(buffer, cancellationToken)).Count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var context = new Microsoft.VisualStudio.Threading.JoinableTaskContext();
            var jtf = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(context);
            jtf.Run(() => WriteAsync(buffer, offset, count));
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Text, false, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _webSocket.SendAsync(buffer, WebSocketMessageType.Text, false, cancellationToken);
        }
    }
}
