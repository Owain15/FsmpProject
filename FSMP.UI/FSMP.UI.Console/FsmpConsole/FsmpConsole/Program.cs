using FsmpConsole;

try
{
    var app = new AppStartup(Console.In, Console.Out);
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    Console.Error.WriteLine(ex.ToString());
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey(intercept: true);
    Environment.Exit(1);
}
