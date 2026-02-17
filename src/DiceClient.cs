using DiceDB;
using DiceDB.Generated;

public sealed class DiceClient(string host="localhost", int port=7379)
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

    public async Task<bool> HandShakeAsync()
    {
        var cmd = new Command
        {
            Cmd = CommandList.HANDSHAKE,
            Args = { ClientID, "command" }
        };

        Result result = await _tcp.SendAsync(cmd);
        
        return result.Status == Status.Ok ;

    }

    public async Task ConnectAsync()
    {
        Console.WriteLine($"Try to Connect {host}:{port}");
        bool result = await HandShakeAsync();
        Console.WriteLine("Connected");
    }

    public async Task CloseAsync()
    {
        await _tcp.DisposeAsync();
    }

    private string ClientID{get;set;} = Guid.NewGuid().ToString() ;
}
