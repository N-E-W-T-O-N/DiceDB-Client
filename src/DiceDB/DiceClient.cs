using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiceDB
{
    public sealed class DiceClient
    {
        private readonly TcpServer _tcp;
        private SemaphoreSlim _semophore = new   SemaphoreSlim(1, 1);
        
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
            await _semophore.WaitAsync();
            
            try{}
            finally
            {
                _semophore.Release();
                
            }
           
        }
        
        public void Close()
        {
            _tcp.Close();
        }
        
    }
}
