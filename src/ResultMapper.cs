using DiceDB.Generated;

namespace DiceDB;

internal static class ResultMapper
{
    public static ResponseGet MapGet(Result r)
    {
        EnsureOk(r);

        if (r.ResponseCase != Result.ResponseOneofCase.GETRes)
            throw new InvalidOperationException("Expected GETRes");

        return new ResponseGet
        {
            Fingerprint64 = r.Fingerprint64,
            Value = r.GETRes.Value
        };
    }

    public static ResponseSet MapSet(Result r)
    {
        EnsureOk(r);

        if (r.ResponseCase != Result.ResponseOneofCase.SETRes)
            throw new InvalidOperationException("Expected SETRes");

        return new ResponseSet
        {
            Fingerprint64 = r.Fingerprint64,
            Success = true
        };
    }

    private static void EnsureOk(Result r)
    {
        // if (r.Status != Status.Ok)
        //     throw new DiceDbException(r.Message ?? "Unknown error");
    }
}
