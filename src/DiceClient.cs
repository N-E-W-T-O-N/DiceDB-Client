using DiceDB;
using DiceDB.Generated;

public sealed class DiceClient(string host="localhost", int port=7839)
{
    private readonly DiceTcpHandler _tcp = new(host,port);

    public async Task<ResponseGet> GetCommandAsync(string key)
    {
        
        var cmd = new Command
        {
            Cmd = CommandList.GET,
            Args = { key }
        };

        Result result = await _tcp.SendAsync(cmd);
        return ResultMapper.MapGet(result);
    }

    public async Task<ResponseSet> SetCommandAsync(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var cmd = new Command
        {
            Cmd = CommandList.SET,
            Args = { key, value }
        };

        Result result = await _tcp.SendAsync(cmd);
        return ResultMapper.MapSet(result);
    }


    public async Task ConnectAsync()
    {
        throw new NotImplementedException();
    }

    public async Task CloseAsync()
    {
        await _tcp.DisposeAsync();
    }
}
using DiceDB.Generated;

namespace DiceDB
{
}
