using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

internal partial class HmGoogleGemini
{
    // FileSystemWatcherオブジェクトの作成
    static FileSystemWatcher watcher = new FileSystemWatcher();

    static bool isConversationing = false;

    static string tempfolder = "";
    static string saveFilePath = "";

    static Boolean isFirst = true;

    static void StartFileWatchr()
    {
        tempfolder = getTargetDir;
        saveFilePath = Path.Combine(tempfolder, "HmGoogleGemini.question.txt");


        // 監視するディレクトリを設定
        watcher.Path = tempfolder;

        // ファイル更新を監視する
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        // 監視するファイルを指定
        watcher.Filter = Path.GetFileName(saveFilePath);

        // 監視を開始
        watcher.EnableRaisingEvents = true;

        Watcher_helper(saveFilePath);

        // 更新があった時の処理。ただし連続して同じファイルに複数回保存するエディタがあるので、0.2秒以内のものは無視する。
        watcher.Changed += Watcher_ChangedHandler;
    }

    static int lastTickCount = 0;
    static void Watcher_helper(string filepath)
    {
        try
        {

            // Console.WriteLine("ファイルが更新されました: " + filepath);

            string question_text = "";
            using (StreamReader reader = new StreamReader(saveFilePath, Encoding.UTF8))
            {
                question_text = reader.ReadToEnd();
            }

            // 正規表現を使用して数値を抽出
            Regex regex = new Regex(@"HmGoogleGemini\.(Message|Clear|Cancel)\((\d+)\)");
            Match match = regex.Match(question_text);

            string command = "";
            if (match.Success)
            {
                command = match.Groups[1].Value;
                string strnumber = match.Groups[2].Value;
                int number = int.Parse(strnumber);

                // 前回の投稿と番号が同じとかならダメ。ファイルが変わっていない
                if (lastTickCount >= number)
                {
                    // Console.WriteLine("★前回と同じファイルだ");
                    return;
                } else
                {
                    // 文字列を改行文字で分割し、2行目以降を取得
                    string[] lines = question_text.Split(new[] { '\n' });
                    question_text = string.Join("\n", lines, 1, lines.Length - 1);

                    lastTickCount = number;
                }
            }
            else
            {
                
            }


            if (command == "Cancel")
            {
                chatSession.Cancel();
                isConversationing = false;
                return;
            }

            else if (command == "Clear")
            {
                chatSession.Cancel();
                chatSession.Clear();
                isConversationing = false;
                ClearAnswerFile();
                Environment.Exit(0);
                return;
            }

            if (isConversationing) { return; }
            isConversationing = true;
            ClearAnswerFile();
            string prompt = question_text;
            // Console.WriteLine($"\nUser: {prompt}");
            var task = chatSession.SendMessageAsync(prompt);
            string response = task.Result;
            isConversationing = false;
        }
        catch (Exception)
        {
        }
        finally
        {
            isConversationing = false;
        }
    }

    static void Watcher_ChangedHandler(object sender, FileSystemEventArgs e)
    {

        if (e.ChangeType != WatcherChangeTypes.Changed) { return; }
        if (isFirst) { isFirst = false; return; }
        Watcher_helper(e.FullPath);
    }
}

