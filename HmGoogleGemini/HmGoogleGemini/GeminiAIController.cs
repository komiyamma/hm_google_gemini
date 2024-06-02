using System;



internal partial class HmGoogleGemini
{
    static ChatSession chatSession;
    static void GenerateContent()
    {

        string _projectId = "";
        string _location = "";
        string _model = "";
        string _publisher = "google";

        try
        {
            // main以外の場所でコマンドライン引数を取得する
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length >= 4)
            {
                Console.WriteLine("_projectId:" + commandLineArgs[1]);
                _projectId = commandLineArgs[1];
                Console.WriteLine("_location:" + commandLineArgs[2]);

                _location = commandLineArgs[2];
                Console.WriteLine("_model:" + commandLineArgs[3]);
                _model = commandLineArgs[3];
                
            }
        }
        catch (Exception e)
        {
        }

        ClearAnswerFile();

        // コンテキストを追跡するためにチャットセッションを作成する
        chatSession = new ChatSession($"projects/{_projectId}/locations/{_location}/publishers/{_publisher}/models/{_model}", _location);

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
