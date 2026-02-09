using FsmpConsole;

var app = new AppStartup(Console.In, Console.Out);
await app.RunAsync();
