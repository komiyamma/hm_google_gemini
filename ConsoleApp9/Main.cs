using System;
using System.Threading.Tasks;



internal partial class HmGoogleGemini
{
    static async Task Main()
    {
        WindowsShutDownNotifier();
        GenerateContent();
        // _ = Task.Run(() => StartPipe());
        StartFileWatchr();
        await Task.Delay(-1); // 無期限で待機する
    }
}
