using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aiplugs.PoshApp.Web
{
    public class WebSocketStreamConnecter
    {
        private readonly WebSocket _socket;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public WebSocketStreamConnecter(WebSocket socket, StreamReader reader, StreamWriter writer)
        {
            _socket = socket;
            _reader = reader;
            _writer = writer;
        }

        public Task StartAsync()
        {
            return Task.WhenAny(
                CreateReceivingTaskAsync(),
                CreateSendingTaskAsync()
            );
        }

        private Task CreateReceivingTaskAsync()
        {
            return Task.Run(async () =>
            {
                var buffer = new byte[200];
                while (!_socket.CloseStatus.HasValue)
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), CancellationToken.None);

                    if (result.CloseStatus.HasValue)
                        return;

                    await _writer.BaseStream.WriteAsync(buffer, 0, result.Count);
                    await _writer.BaseStream.FlushAsync();
                }
            });

        }

        private Task CreateSendingTaskAsync()
        {
            return Task.Run(async () =>
            {
                var buffer = new byte[1024 * 8];
                while (!_socket.CloseStatus.HasValue && _reader.BaseStream.CanRead)
                {
                    var count = await _reader.BaseStream.ReadAsync(buffer, 0, buffer.Length);

                    await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, count), WebSocketMessageType.Binary, true, CancellationToken.None);   
                }
            });
        }
    }

    class Utils
    {
        public static string HexDump(byte[] bytes, int bytesLength, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
    }
}
