using System;



internal partial class HmGoogleGemini
{
    static ChatSession chatSession;
    static void GenerateContent()
    {
        string _projectId = "";  // "new-project-20240307" とかそういうパターン
        string _location = "";   // "us-central1" とかそういうパターン
        string _model = "";      // "gemini-1.0-pro" とかそういうパターン
        string _publisher = "google";
        string _proxy_url = "";

        try
        {
            // main以外の場所でコマンドライン引数を取得する
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length >= 4)
            {
                // Console.WriteLine("_projectId:" + commandLineArgs[1]);
                _projectId = commandLineArgs[1];
                // Console.WriteLine("_location:" + commandLineArgs[2]);

                _location = commandLineArgs[2];
                // Console.WriteLine("_model:" + commandLineArgs[3]);
                _model = commandLineArgs[3];
            }
            if (commandLineArgs.Length >= 5)
            {
                // Console.WriteLine("_proxy_url:" + commandLineArgs[4]);
                _proxy_url = commandLineArgs[4];
            }
        }
        catch (Exception e)
        {
        }

        ClearAnswerFile();

        try
        {
            // コンテキストを追跡するためにチャットセッションを作成する
            chatSession = new ChatSession($"projects/{_projectId}/locations/{_location}/publishers/{_publisher}/models/{_model}", _location, _proxy_url);
        }
        catch (Exception e)
        {
            SaveAllTextToAnswerFile("\r\n\r\n" + e.GetType().Name + "\r\n\r\n" + e.Message + "\r\n");
            chatSession.Cancel();
            // Console.WriteLine("問い合わせをキャンセルしました。" + e);
            // Console.WriteLine("アプリを終了します。");
            Environment.Exit(0);
        }
        /*
        string prompt = "こんにちわ。私は日本語で会話します。";
        Console.WriteLine($"\nUser: {prompt}");

        string response = await chatSession.SendMessageAsync(prompt);
        Console.WriteLine($"Response: {response}");
        */

        /*
        prompt = "それを2倍すると？";
        Console.WriteLine($"\nUser: {prompt}");

        response = await chatSession.SendMessageAsync(prompt);
        Console.WriteLine($"Response: {response}");
        */
    }

}
