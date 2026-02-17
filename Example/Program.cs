// See https://aka.ms/new-console-template for more information

using DiceDB;

DiceClient db = new();

await db.ConnectAsync().ConfigureAwait(false);

ResponseGet r = await db.GetCommandAsync("K1");
Console.WriteLine(r.Value,r.Message);
ResponseSet re= await db.SetCommandAsync("K1","va");
r = await db.GetCommandAsync("K1");

await db.CloseAsync();
