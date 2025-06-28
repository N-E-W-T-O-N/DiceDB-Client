using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;

namespace DiceDB
{
    public sealed class DiceClient
    {
        private readonly TcpServer _tcp;
        private readonly SemaphoreSlim _semophore = new   SemaphoreSlim(1, 1);
        
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

        public async Task SendAsync(string command)
        {
            List<string> cmdArr = command.Split(" ").ToList();
            var c = cmdArr.RemoveAt(0);
            
            Command cmd = new Command(){Cmd = cmdArr[0] ,Args={cmdArr[0]} }; 
                
            
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
