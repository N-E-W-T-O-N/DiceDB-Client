using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiceDB
{
    public sealed class DiceClient
    {
        private readonly TcpServer _tcp;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 1);
        
        public DiceClient(string host="localhost", int port=7379)
        {

            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Please provide a host `IP ADDRESS`");
            if(port<0)
                throw new ArgumentOutOfRangeException(nameof(port),"Please provide a valid port number");
            
            _tcp = new TcpServer(host, port);           
            
        }

        public async Task SendAsync(Command command)
        { 
            await _semaphore.WaitAsync();
            
            try{}
            finally
            {
                _semaphore.Release();
            }
           
        }
        
        public void Close()
        {
            _tcp.Close();
        }
        
    }
}
