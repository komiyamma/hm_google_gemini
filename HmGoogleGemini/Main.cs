using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;



internal partial class HmGoogleGemini
{
    static void ifProcessHasExistKillIt()
    {
        // 現在のプロセスの名前を取得
        string currentProcessName = Process.GetCurrentProcess().ProcessName;

        // 既に起動している同じプロセス名のプロセスを取得
        var runningProcesses = Process.GetProcessesByName(currentProcessName);

        // 起動しているプロセスが2つ以上ある場合は、新しいプロセスを終了させる
        if (runningProcesses.Length > 1)
        {
            foreach (var process in runningProcesses.Where(p => p.Id == Process.GetCurrentProcess().Id))
            {
                process.Kill();
            }
            return;
        }
    }

    static async Task Main()
    {
        ifProcessHasExistKillIt();
        WindowsShutDownNotifier();
        GenerateContent();
        // _ = Task.Run(() => StartPipe());
        StartFileWatchr();
        await Task.Delay(-1); // 無期限で待機する
    }
}
