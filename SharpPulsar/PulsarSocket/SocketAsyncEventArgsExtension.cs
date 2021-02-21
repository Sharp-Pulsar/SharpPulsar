using Akka.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SharpPulsar.PulsarSocket
{
    public static class SocketAsyncEventArgsExtension
    {

        public static void SetBuffer(this SocketAsyncEventArgs args, IEnumerable<PulsarByteString> dataCollection)
        {
            if (RuntimeDetector.IsMono)
            {
                // Mono doesn't support BufferList - falback to compacting ByteString
                var dataList = dataCollection.ToList();
                var totalSize = dataList.SelectMany(d => d.Buffers).Sum(d => d.Count);
                var bytes = new byte[totalSize];
                var position = 0;
                foreach (var byteString in dataList)
                {
                    var copied = byteString.CopyTo(bytes, position, byteString.Count);
                    position += copied;
                }
                args.SetBuffer(bytes, 0, bytes.Length);
            }
            else
            {
                if (args.Buffer != null)
                {
                    args.SetBuffer(null, 0, 0);
                }
                args.BufferList = dataCollection.SelectMany(d => d.Buffers).ToList();
            }
        }
    }
}
