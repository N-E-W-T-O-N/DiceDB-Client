using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;


// where Command and Result live
 
namespace DiceDB
{
    internal sealed class TcpServer
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        
        public TcpServer(string host, int port)
        {
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();
        }

        internal void Send(Command command)
        {
            byte[] payload = command.ToByteArray();
            byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));

            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(payload, 0, payload.Length);
            _stream.Flush();
        }

        internal async Task SendAsync(Command command)
        {
            byte[] payload = command.ToByteArray();
            byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));

            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await _stream.WriteAsync(payload, 0, payload.Length);
            await _stream.FlushAsync();
        
        }
    
        public async Task ReceiveAsync()
        {
            byte[] lengthBytes = new byte[4];
            await ReadFullyAsync(_stream, lengthBytes, 4);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

            byte[] payload = new byte[length];
            await ReadFullyAsync(_stream, payload, length);

            Result r = Result.Parser.ParseFrom(payload);
            Print(r);
        }
        
        public void Receive()
        {
            byte[] lengthBytes = new byte[4];
            ReadFully(_stream, lengthBytes, 4);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBytes, 0));

            byte[] payload = new byte[length];
            ReadFully(_stream, payload, length);

            Result r = Result.Parser.ParseFrom(payload);
            Print(r);
        }

        private void ReadFully(Stream stream, byte[] buffer, int length)
        {
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
        }

        private async Task ReadFullyAsync(Stream stream, byte[] buffer, int length)
        {
            int offset = 0;
            while (offset < length)
            {
                int read = await stream.ReadAsync(buffer, offset, length - offset);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
        }
        private void Print(Result r)
        {
            Console.WriteLine(r.Fingerprint64);
            Console.WriteLine(r.ResponseCase);
            Console.WriteLine(r.Message);
            Console.WriteLine(r.Status);
            OneofAccessor(r);
        
        }

        private void OneofAccessor(Result result)
        {
            switch(result.ResponseCase )
            {
                case Result.ResponseOneofCase.GETRes : Console.WriteLine(result.GETRes.Value); break;
                case Result.ResponseOneofCase.DELRes : Console.WriteLine(result.DELRes.Count); break;
                case Result.ResponseOneofCase.EXISTSRes :Console.WriteLine(result.EXISTSRes.Count); break;

                default: Console.WriteLine("Null");
                    break;
            }

        }
        public void Close() => _client.Close();
    }
}
