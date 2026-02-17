using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using DiceDB.Generated;
using Google.Protobuf;

namespace DiceDB;

public sealed class DiceTcpHandler : IAsyncDisposable
{
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly CancellationTokenSource _cts = new();

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly ConcurrentQueue<TaskCompletionSource<Result>> _pending = new();
    private readonly ConcurrentDictionary<string, Channel<Result>> _watch = new();

    private readonly PipeHandler _pipeline;
    private readonly HandleResult _decoder;

    public DiceTcpHandler(string host, int port)
    {
        _tcp = new TcpClient(host, port) { NoDelay = true };
        _stream = _tcp.GetStream();

        _pipeline = new PipeHandler(_stream);
        _decoder = new HandleResult(Dispatch);

        _ = Task.Run(() => _pipeline.RunAsync(_cts.Token));
        _ = Task.Run(() => _decoder.RunAsync(_pipeline.Frames, _cts.Token));
    }

    // ================= SEND =================

    internal async Task<Result> SendAsync(
        Command cmd,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Result>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _pending.Enqueue(tcs);

        byte[] payload = cmd.ToByteArray();
        int len = IPAddress.HostToNetworkOrder(payload.Length);
        byte[] header = BitConverter.GetBytes(len);

        await _sendLock.WaitAsync(ct);
        try
        {
            await _stream.WriteAsync(header, ct);
            await _stream.WriteAsync(payload, ct);
            await _stream.FlushAsync(ct);
        }
        finally
        {
            _sendLock.Release();
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (timeout.HasValue)
            cts.CancelAfter(timeout.Value);

        try
        {
            return await tcs.Task;
        }
        catch
        {
            // If timeout, we must drain one slot to keep ordering sane
            _pending.TryDequeue(out _);
            throw;
        }
    }


    // ================= DISPATCH =================

    private void Dispatch(Result r)
    {
        if (r.Fingerprint64 == 0 &&
            _pending.TryPeek( out var tcs))
        {
            tcs.TrySetResult(r);
            return;
        }

        if (IsWatch(r, out var key) &&
            _watch.TryGetValue(key, out var ch))
        {
            ch.Writer.TryWrite(r);
        }
    }

    private bool IsWatch(Result r, out string key)
    {
        key = r.Message ?? "";
        return ResponseWatchCode.Contains(r.ResponseCase) ;
    }

    // ================= WATCH =================

    internal IAsyncEnumerable<Result> WatchAsync(string key, CancellationToken ct)
    {
        var ch = _watch.GetOrAdd(key, _ => Channel.CreateUnbounded<Result>());
        return ch.Reader.ReadAllAsync(ct);
    }

    // ================= CLEANUP =================

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();

        foreach (var kv in _pending)
            kv.TrySetException(
                new OperationCanceledException("Connection closed"));

        _stream.Close();
        _tcp.Close();
        _sendLock.Dispose();

        await Task.CompletedTask;
    }

    private readonly Result.ResponseOneofCase[] ResponseWatchCode =
    [
        Result.ResponseOneofCase.GETWATCHRes,
        Result.ResponseOneofCase.ZCOUNTWATCHRes,
        Result.ResponseOneofCase.ZCARDWATCHRes,
        Result.ResponseOneofCase.ZRANKWATCHRes
    ];
    private static ulong GenerateRequestId()
        => (ulong)DateTime.UtcNow.Ticks;
}
