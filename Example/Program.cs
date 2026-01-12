// See https://aka.ms/new-console-template for more information

using DiceDB;

DiceClient db = new();

await db.ConnectAsync().ConfigureAwait(false);

ResponseGet r = await db.GetCommandAsync("K1");
ResponseSet re= await db.SetCommandAsync("","");

await db.CloseAsync();
