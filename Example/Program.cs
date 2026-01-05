// See https://aka.ms/new-console-template for more information

using DiceDB;

DiceClient db = new();

// map Result -> key (adapt to your proto fields)
Func<Response, string> keySelector = r => r.Message; // change to actual key field

await using var client = new DiceClient("localhost", 7379, keySelector);

// Subscribe / watch a key
var cts = new CancellationTokenSource();
IAsyncEnumerable<Response> stream = client.WatchAsync("my-key", cts.Token);

_ = Task.Run(async () =>
{
    await foreach (var ev in stream)
    {
        Console.WriteLine("Event for key: " + ev);
    }
});

// send a command
var cmd = new Request { Command = Commands.GET , Args = ["K"]};
await client.SendAsync(cmd);

// When done:
cts.Cancel();         // stop subscriber
await client.CloseAsync();


//// map Result -> key (adapt to your proto fields)
//Func<Result, string> keySelector = r => r.Message; // change to actual key field

//await using var client = new DiceTcpClient("localhost", 7379, keySelector);

//// Subscribe / watch a key
//var cts = new CancellationTokenSource();
//IAsyncEnumerable<Result> stream = client.WatchAsync("my-key", cts.Token);

//_ = Task.Run(async () =>
//{
//    await foreach (var ev in stream)
//    {
//        Console.WriteLine("Event for key: " + ev);
//    }
//});

//// send a command
//var cmd = new Command { /* populate */ };
//await client.SendAsync(cmd);

//// When done:
//cts.Cancel();         // stop subscriber
//await client.CloseAsync();
