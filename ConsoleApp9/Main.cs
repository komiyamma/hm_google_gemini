using System;
using System.Threading.Tasks;



internal partial class HmGoogleGemini
{
    static async Task Main()
    {
        WindowsShutDownNotifier();
        _ = Task.Run(() => StartPipe());
        var content = await GenerateContent();
        // Console.WriteLine($"Generated content: {content}");
    }
}
