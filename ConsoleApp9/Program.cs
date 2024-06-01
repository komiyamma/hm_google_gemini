
using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using static Google.Cloud.AIPlatform.V1.ReadFeatureValuesResponse.Types.EntityView.Types;
using System.IO;


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
        ChatSession chatSession = new ChatSession($"projects/{_projectId}/locations/{_location}/publishers/{_publisher}/models/{_model}", _location);

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
}