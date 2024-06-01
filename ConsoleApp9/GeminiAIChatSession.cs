
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;


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

    static CancellationTokenSource _cst;

    public void Clear()
    {
        _contents.Clear();
    }

    public void Cancel()
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
                MaxOutputTokens = (prompt.Length) * 2 + 4096,
            }
        };
        generateContentRequest.Contents.AddRange(_contents);

        _cst = new CancellationTokenSource();

        Console.WriteLine("開始");
        try
        {
            /*
            // ストリーミングではなく、全体を一気にリクエストをし、レスポンスを得る。
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(generateContentRequest, _cst.Token);
            Console.WriteLine("終了");

            // レスポンスの内容を保存する。
            _contents.Add(response.Candidates[0].Content);

            this.SaveContentsToJson();
            // テキストを返す
            return response.Candidates[0].Content.Parts[0].Text;
            */

            var response = _predictionServiceClient.StreamGenerateContent(generateContentRequest);

            StringBuilder fullText = new StringBuilder();
            AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
            await foreach (GenerateContentResponse responseItem in responseStream)
            {
                if (_cst.IsCancellationRequested)
                {
                    Console.WriteLine("問い合わせをキャンセルしました。" + e);
                    break;
                }
                fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);
                Console.WriteLine(responseItem.Candidates[0].Content.Parts[0].Text);
            }
            var answer = new Content
            {
                Role = "model"
            };
            answer.Parts.AddRange(new List<Part>()
            {
            new() {
                Text = fullText.ToString()
            }
             });
            _contents.Add(answer);

            return fullText.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine("問い合わせをキャンセルしました。" + e);
        }

            return "";
    }

    private void SaveContentsToJson()
    {
        HmGoogleGemini.SaveContentsToJson(_contents);
    }
}
