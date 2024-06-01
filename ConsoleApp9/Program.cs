
using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using static Google.Cloud.AIPlatform.V1.ReadFeatureValuesResponse.Types.EntityView.Types;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;


internal class ChatSession
{
    private string _modelPath;
    private PredictionServiceClient _predictionServiceClient;

    static List<Content> _contents;

    public ChatSession(string modelPath, string location)
    {
        _modelPath = modelPath;

        // 予測サービス・クライアントを作成する。
        _predictionServiceClient = new PredictionServiceClientBuilder
        {
            Endpoint = $"{location}-aiplatform.googleapis.com",
        }.Build();

        InitContents();
    }

    private void InitContents()
    {
        // リクエスト毎に送信する内容を初期化する。
        _contents = new List<Content>();
    }

    CancellationTokenSource _cst;

    public void clear()
    {
        _contents.Clear();
    }

    public void cancel()
    {
        _cst.Cancel();
    }

    public async Task<string> SendMessageAsync(string prompt)
    {

        // Initialize the content with the prompt.
        var content = new Content
        {
            Role = "USER"
        };
        content.Parts.AddRange(new List<Part>()
        {
            new() {
                Text = prompt
            }
        });
        _contents.Add(content);
        /*
                var content2 = new Content
                {
                    Role = "model"
                };
                content.Parts.AddRange(new List<Part>()
                {
                    new() {
                        Text = "4です"
                    }
                });
                _contents.Add(content2);

                var content3 = new Content
                {
                    Role = "model"
                };
                content.Parts.AddRange(new List<Part>()
                {
                    new() {
                        Text = "それを2倍すると？"
                    }
                });
                _contents.Add(content3);
        */

        // コンテンツ生成のリクエストを作成する。
        var generateContentRequest = new GenerateContentRequest
        {
            Model = _modelPath,
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.9f,
                TopP = 1,
                TopK = 32,
                CandidateCount = 1,
                MaxOutputTokens = (prompt.Length) * 2 + 4096
            }
        };
        generateContentRequest.Contents.AddRange(_contents);

        _cst = new CancellationTokenSource();

        Console.WriteLine("開始");
        try
        {
            // ストリーミングではなく、全体を一気にリクエストをし、レスポンスを得る。
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(generateContentRequest, _cst.Token);
            Console.WriteLine("終了");

            // レスポンスの内容を保存する。
            _contents.Add(response.Candidates[0].Content);

            SaveToJson();
            // テキストを返す
            return response.Candidates[0].Content.Parts[0].Text;
        }
        catch (Exception e)
        {
            Console.WriteLine("問い合わせをキャンセルしました。");
        }

            return "";
    }

    private void SaveToJson()
    {
        try
        {
            string json = JsonConvert.SerializeObject(_contents);

            // JSONをファイルに書き込み
            File.WriteAllText("data.json", json);
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }
}

public class MultiTurnChatSample
{
    static ChatSession chatSession;
    static async Task<string> GenerateContent()
    {

        string _projectId = "new-project-";
        string _location = "us-central1";
        string _publisher = "google";
        string _model = "gemini-1.5-pro";

        try
        {
            // main以外の場所でコマンドライン引数を取得する
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length >= 4)
            {
                _projectId = commandLineArgs[1];
                _location = commandLineArgs[2];
                _model = commandLineArgs[3];
            }
        }
        catch (Exception e)
        {
        }

        // コンテキストを追跡するためにチャットセッションを作成する
        chatSession = new ChatSession($"projects/{_projectId}/locations/{_location}/publishers/{_publisher}/models/{_model}", _location);

        /*
        string prompt = "こんにちわ。私は日本語で会話します。";
        Console.WriteLine($"\nUser: {prompt}");

        string response = await chatSession.SendMessageAsync(prompt);
        Console.WriteLine($"Response: {response}");
        */

        string prompt = "1+1は？";
        Console.WriteLine($"\nUser: {prompt}");

        string response = await chatSession.SendMessageAsync(prompt);
        Console.WriteLine($"Response: {response}");

        prompt = "それを2倍すると？";
        Console.WriteLine($"\nUser: {prompt}");

        var ret = chatSession.SendMessageAsync(prompt);
        chatSession.cancel();
        response = ret.Result;
        Console.WriteLine($"Response: {response}");


        return response;
    }


    static async Task Main()
    {
        WindowsShutDownNotifier();
        var content = await GenerateContent();
        Console.WriteLine($"Generated content: {content}");
    }

    /*
    static void Main(string[] args)
    {
        ImportData();
    }
    */

    public class TPart
    {
        public string Text { get; set; }
    }
    public class Data
    {
        public string Role { get; set; }
        public List<TPart> Parts { get; set; }
    }

    static void ImportData()
    {
        string json = File.ReadAllText("data.json");
        var data = JsonConvert.DeserializeObject<List<Data>>(json);

        foreach (var item in data)
        {
            Console.WriteLine($"Role: {item.Role}");

            foreach (var part in item.Parts)
            {
                Console.WriteLine($"Text: {part.Text}");
            }
            Console.WriteLine();
        }
    }

    bool isHidemaruExist()
    {
        return isNativeHidemaruIsExist() || isStoreHidemaruIsExist();
    }

    bool isNativeHidemaruIsExist()
    {
        // ネイティブはHidemaru32Class で
        IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Hidemaru32Class", IntPtr.Zero);
        if (hWnd == IntPtr.Zero)
        {
            // Trace.WriteLine("Hidemaru32Classなし");
            return false;
        }

        // 「常駐秀丸」は、「Hidemaru32Class」の下に子ウィンドウは持たないので、この判定が効く
        IntPtr hChild = FindWindowEx(hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (hChild == IntPtr.Zero)
        {
            // Trace.WriteLine("これは有効な編集エリアを持つ有効な秀丸エディタのウィンドウハンドルではない。常駐秀丸か何かである");
            return false; // 終わる
        }

        return true;
    }

    bool isStoreHidemaruIsExist()
    {
        // (ちなみに、ストアアプリ版は、HHidemaru32Class_Appx
        IntPtr hWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Hidemaru32Class", IntPtr.Zero);
        if (hWnd == IntPtr.Zero)
        {
            // Trace.WriteLine("Hidemaru32Classなし");
            return false;
        }

        // 常駐秀丸は、「Hidemaru32Class」の下に子ウィンドウは持たないので、この判定が効く
        IntPtr hChild = FindWindowEx(hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (hChild == IntPtr.Zero)
        {
            // Trace.WriteLine("これは有効な編集エリアを持つ有効な秀丸エディタのウィンドウハンドルではない。常駐秀丸か何かである");
            return false; // 終わる
        }

        return true;
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, IntPtr strWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, IntPtr lpClassName, IntPtr strWindowName);

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

                if (message == "exit")
                {
                    Console.WriteLine("受信したコマンドが終了命令のため、通信を終了します。");
                }
            }
        }
    }

    static void WindowsShutDownNotifier()
    {
        SystemEvents.SessionEnding += SystemEvents_SessionEnding;

    }

    private static async void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
    {
        if (e.Reason == SessionEndReasons.SystemShutdown)
        {
            chatSession.cancel();
            await Task.Delay(100); // ミリ秒単位で待つ時間を指定
            Console.WriteLine("Windowsがシャットダウンしています。ここで必要な操作を行ってください。");
        }
    }

}
