using System.Threading.Channels;
using DiceDB.Generated;

namespace DiceDB;

public sealed class HandleResult
{
    private readonly Action<Result> _onResponse;

    internal HandleResult(Action<Result> onResponse)
        => _onResponse = onResponse;

    public async Task RunAsync(
        ChannelReader<byte[]> frames,
        CancellationToken ct)
    {
        await foreach (var frame in frames.ReadAllAsync(ct))
        {
            var response = Result.Parser.ParseFrom(frame);
            _onResponse(response);
        }
    }
}
