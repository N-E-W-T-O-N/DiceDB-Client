using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;

namespace DiceDB;

internal sealed class PipeHandler(Stream stream)
{
    private readonly Pipe _pipe = new();
    private readonly Channel<byte[]> _frames = Channel.CreateUnbounded<byte[]>();

    public ChannelReader<byte[]> Frames => _frames.Reader;

    public async Task RunAsync(CancellationToken ct)
    {
        var fill = FillAsync(ct);
        var read = ReadAsync(ct);
        await Task.WhenAll(fill, read);
    }

    private async Task FillAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                Memory<byte> mem = _pipe.Writer.GetMemory(4096);
                int read = await stream.ReadAsync(mem, ct);
                if (read == 0) break;

                _pipe.Writer.Advance(read);
                await _pipe.Writer.FlushAsync(ct);
            }
        }
        finally
        {
            await _pipe.Writer.CompleteAsync();
        }
    }

    private async Task ReadAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _pipe.Reader.ReadAsync(ct);
            var buffer = result.Buffer;

            while (TryReadFrame(ref buffer, out var frame))
            {
                _frames.Writer.TryWrite(frame.ToArray());
            }

            _pipe.Reader.AdvanceTo(buffer.Start, buffer.End);
            if (result.IsCompleted) break;
        }

        _frames.Writer.TryComplete();
        await _pipe.Reader.CompleteAsync();
    }

    private static bool TryReadFrame(
        ref ReadOnlySequence<byte> buffer,
        out ReadOnlySequence<byte> frame)
    {
        frame = default;
        if (buffer.Length < 4) return false;

        Span<byte> lenBuf = stackalloc byte[4];
        buffer.Slice(0, 4).CopyTo(lenBuf);
        int len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf));

        if (buffer.Length < 4 + len) return false;

        frame = buffer.Slice(4, len);
        buffer = buffer.Slice(4 + len);
        return true;
    }
}
