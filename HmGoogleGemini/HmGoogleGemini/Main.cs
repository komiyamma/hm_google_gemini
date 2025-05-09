﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;



internal partial class HmGoogleGemini
{
    static void IfOldProcessIsOtherDirectoryKillIt()
    {
        string currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string processName = Process.GetCurrentProcess().ProcessName;

        Process[] processes = Process.GetProcessesByName(processName);

        foreach (Process p in processes)
        {
            if (p.Id != Process.GetCurrentProcess().Id)
            {
                string processDirectory = Path.GetDirectoryName(p.MainModule.FileName);
                if (processDirectory != currentDirectory)
                {
                    p.Kill();
                }
            }
        }
    }

    static void IfProcessHasExistKillIt()
    {
        // 現在のプロセスの名前を取得
        string currentProcessName = Process.GetCurrentProcess().ProcessName;

        // 既に起動している同じプロセス名のプロセスを取得
        var runningProcesses = Process.GetProcessesByName(currentProcessName);

        // 起動しているプロセスが2つ以上ある場合は、
        if (runningProcesses.Length > 1)
        {
            // 新しいプロセス(今このプログラム行を実行しているプロセス = カレントプロセス)を終了させる
            Environment.Exit(0);
        }
    }

    static void KillExistsProcess()
    {
        string processName = Process.GetCurrentProcess().ProcessName;
        Process[] processes = Process.GetProcessesByName(processName);

        foreach (Process process in processes)
        {
            if (process.Id != Process.GetCurrentProcess().Id)
            {
                process.Kill();
            }
        }
    }

    static async Task Main(String[] args)
    {
        // 古いプロセスが他のディレクトリにある場合はKillする
        IfOldProcessIsOtherDirectoryKillIt();

        // クリアの命令をすると、先に実行していた方が先に閉じてしまうことがある。
        // よってマクロから明示的にClearする時は、引数にて「実行を継続するようなプロセスではないですよ」といった意味で
        // HmGoogleGemini.Clear という文字列を渡してある
        if (args.Length >= 1)
        {
            var command = args[0];
            if (command.Contains("HmGoogleGemini.Clear()"))
            {
                await Task.Delay(500); // 0.5秒まつ
                KillExistsProcess(); // 強制的に過去のものも削除
                return;
            }
            if (command.Contains("HmGoogleGemini.Cancel()"))
            {
                return;
            }
            if (command.Contains("HmGoogleGemini.Pop()"))
            {
                return;
            }
        }

        // 自分が2個目なら終了(2重起動しない)
        IfProcessHasExistKillIt();


        // Windowsがシャットダウンするときに呼び出される処理を登録等
        WindowsShutDownNotifier();

        // 会話エンジンを初期化
        GenerateContent();

        // ファイル監視を開始
        StartFileWatchr();

        await Task.Delay(-1); // 無期限で待機する
    }
}
