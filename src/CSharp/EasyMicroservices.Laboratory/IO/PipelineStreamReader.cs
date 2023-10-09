using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.IO
{
    internal class PipelineStreamReader : IDisposable
    {
        readonly NetworkStream _stream;
        public PipelineStreamReader(NetworkStream stream)
        {
            _stream = stream;
            ThreadPool.QueueUserWorkItem(StartReader);
        }

        bool isDisposed = false;
        public void Dispose()
        {
            isDisposed = true;
            _stream?.Dispose();
        }

        ConcurrentQueue<byte> bytes = new ConcurrentQueue<byte>();
        List<byte> all = new List<byte>();
        public async void StartReader(object alaki)
        {
            try
            {
                while (!isDisposed)
                {
                    byte[] buffer = new byte[1024 * 100];
                    var readCount = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    for (int i = 0; i < readCount; i++)
                    {
                        bytes.Enqueue(buffer[i]);
                        all.Add(buffer[i]);
                    }
                }
            }
            catch// (Exception ex)
            {
                _stream?.Dispose();
                //var qq = ex;
                // Debug.WriteLine(Convert.ToBase64String(all.ToArray()));
            }
        }

        public async Task<int> ReadAsync(byte[] buffer, int count)
        {
            int index = 0;
            do
            {

                if (bytes.TryDequeue(out byte result))
                {
                    buffer[index] = result;
                    index++;
                }
                else
                    await Task.Delay(100);
                if (index == count)
                    return count;
            }
            while (!isDisposed);
            return 0;
        }
    }
}
