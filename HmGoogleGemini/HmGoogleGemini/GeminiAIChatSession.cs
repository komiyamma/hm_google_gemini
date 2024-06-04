
using Google.Api.Gax.Grpc;
using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


internal class ChatSession
{
    private string _modelPath;
    private PredictionServiceClient _predictionServiceClient;

    static List<Content> _contents;
    static int conversationUpdateCount = 1;
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

    // AIからの返答がどうも進んでいない、といったことを判定する。５秒進んでいないようだと、キャンセルを発動する。
    bool conversationUpdateCancel = false;
    async Task conversationUpdateCheck()
    {
        conversationUpdateCancel = false;
        int lastConversationUpdateCount = conversationUpdateCount;
        long iTickCount = 0;
        while (true)
        {
            if (conversationUpdateCancel)
            {
                // Console.WriteLine("今回の会話タスクが終了したため、conversationUpdateCheckを終了");
                break;
            }
            await Task.Delay(100); // 5秒ごとにチェック

            if (lastConversationUpdateCount == conversationUpdateCount)
            {
                iTickCount++;
            }
            else
            {
                lastConversationUpdateCount = conversationUpdateCount;
                iTickCount = 0;
            }

            if (iTickCount > 50)
            {
                iTickCount = 0;
                // Console.WriteLine("AIからの応答の進捗がみられないため、キャンセル発行");
                this.Cancel();
                break;
            }
        }
    }


    // 質問してAIの応答の途中でキャンセルするためのトークン
    static CancellationTokenSource _cst;

    // 会話履歴全部クリア
    public void Clear()
    {
        _contents.Clear();
    }

    // AIの返答を途中キャンセル
    public void Cancel()
    {
        _cst.Cancel();
    }

    private void CancelCheck()
    {
        string question_text = "";
        using (StreamReader reader = new StreamReader(HmGoogleGemini.questionFilePath, Encoding.UTF8))
        {
            question_text = reader.ReadToEnd();
        }

        // 1行目にコマンドと質問がされた時刻に相当するTickCount相当の値が入っている
        // これによって値が進んでいることがわかる。
        // 正規表現を使用して数値を抽出
        Regex regex = new Regex(@"HmGoogleGemini\.Cancel");
        Match match = regex.Match(question_text);
        if (match.Success)
        {
            this.Cancel();
            conversationUpdateCancel = true;
        }
    }

    public async Task<string> SendMessageAsync(string prompt, int questionNumber)
    {
        var task = conversationUpdateCheck();

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
                MaxOutputTokens = 4096,
            }
        };
        generateContentRequest.Contents.AddRange(_contents);

        _cst = new CancellationTokenSource();

        try
        {
            /*
            // ストリーミングではなく、全体を一気にリクエストをし、レスポンスを得る。
            GenerateContentResponse response = await _predictionServiceClient.GenerateContentAsync(generateContentRequest, _cst.Token);
            // Console.WriteLine("終了");

            // レスポンスの内容を保存する。
            _contents.Add(response.Candidates[0].Content);

            this.SaveContentsToJson();
            // テキストを返す
            return response.Candidates[0].Content.Parts[0].Text;
            */

            // ストリーミングでリクエストをし、レスポンスを得る。
            var response = _predictionServiceClient.StreamGenerateContent(generateContentRequest);


            // 1回の返答はこまごま返ってくるので、返答全部を１つにまとまる用途
            StringBuilder fullText = new StringBuilder();
            AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
            await foreach (GenerateContentResponse responseItem in responseStream)
            {
                // 途中で分詰まりを検知するための進捗カウンタ
                conversationUpdateCount++;

                // 毎回じゃ重いので、適当に間引く
                if (conversationUpdateCount % 5 == 0)
                {
                    CancelCheck();
                }

                if (_cst.IsCancellationRequested)
                {
                    throw new OperationCanceledException("AIの応答をキャンセルしました。");
                }

                var text = responseItem.Candidates[0].Content.Parts[0].Text;
                fullText.Append(text);
                SaveAddTextToFile(text);
                // Console.WriteLine(text);
            }

            var alltext = fullText.ToString();
            // 最後に念のために、全体のテキストとして1回上書き保存しておく。
            // 細かく保存していた際に、ファイルIOで欠損がある可能性がわずかにあるため。
            SaveAllTextToFile(alltext);
            SaveCompleteFile(questionNumber);

            // こまごまと返ってきた返答をまとめて１つにして「AIの返答」として１つで登録する
            var answer = new Content
            {
                Role = "model"
            };
            answer.Parts.AddRange(new List<Part>()
            {
                new() {
                    Text = alltext
                }
            });
            _contents.Add(answer);
            return alltext;
        }
        catch (Exception e)
        {
            SaveAddTextToFile("\r\n\r\n" + e.GetType().Name + "\r\n\r\n" + e.Message + "\r\n");
            conversationUpdateCancel = true;
            this.Cancel();
            // Console.WriteLine("問い合わせをキャンセルしました。" + e);
            // Console.WriteLine("アプリを終了します。");
            Environment.Exit(0);
        }
        finally
        {
            conversationUpdateCancel = true;
        }

        return "";
    }


    // Streamでちょこちょこと返答が返ってくるので、ちょこちょこと返答内容をファイルに追加保存する。
    private void SaveAddTextToFile(string text)
    {
        HmGoogleGemini.SaveAddTextToAnswerFile(text);
    }

    private void SaveAllTextToFile(string text)
    {
        HmGoogleGemini.SaveAllTextToAnswerFile(text);
    }

    private void SaveCompleteFile(int number)
    {
        HmGoogleGemini.SaveCompleteFile(number);
    }

}
