using System;
using System.Threading.Tasks;



internal partial class HmGoogleGemini
{
    static async Task Main()
    {
        WindowsShutDownNotifier();
        var content = await GenerateContent();
        Console.WriteLine($"Generated content: {content}");
    }
}
