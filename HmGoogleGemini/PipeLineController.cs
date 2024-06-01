#if HMGOOGLEGEMINI_PIPELINE

using System;
using System.IO.Pipes;
using System.Text;


partial class HmGoogleGemini { 
    static async void StartPipe()
    {
        // ネームドパイプは中身が壊れやすいので、とにかく1回ごとに破棄。
        while (true)
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("HmGoogleGemini", PipeDirection.InOut))
            {
                Console.WriteLine("パイプサーバーを起動しました。");

                Console.WriteLine("クライアントからの接続を待機中...");

                await pipeServer.WaitForConnectionAsync();
                Console.WriteLine("クライアントが接続しました。");

                byte[] buffer = new byte[1024];
                int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("クライアントが切断しました。");
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("受信したメッセージ: " + message);

                if (message == "HmGoogleGemini.Clear()")
                {
                    chatSession.Cancel();
                    chatSession.Clear();
                    ClearTextFile();
                    Console.WriteLine("テキストファイルをクリアしました。");
                }
                else if (message == "HmGoogleGemini.Exit()")
                {
                    Console.WriteLine("受信したコマンドが終了命令のため、通信を終了します。");
                }
            }
        }
    }
}

#endif