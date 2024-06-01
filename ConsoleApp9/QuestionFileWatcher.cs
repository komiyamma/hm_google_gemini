using Google.Cloud.AIPlatform.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class HmGoogleGemini
{
    // FileSystemWatcherオブジェクトの作成
    static FileSystemWatcher watcher = new FileSystemWatcher();

    static void StartFileWatchr()
    {
        string tempfolder = Path.GetTempPath();
        string saveFilePath = Path.Combine(tempfolder, "HmGoogleGemini.question.txt");


        // 監視するディレクトリを設定
        watcher.Path = tempfolder;

        // ファイル更新を監視する
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        // 監視するファイルを指定
        watcher.Filter = Path.GetFileName(saveFilePath);

        // 監視を開始
        watcher.EnableRaisingEvents = true;

        bool isConversationing = false;
        // 更新があった時の処理。ただし連続して同じファイルに複数回保存するエディタがあるので、0.2秒以内のものは無視する。
        watcher.Changed += async (sender, e) =>
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) { return; }

            try
            {
                Console.WriteLine("ファイルが更新されました: " + e.FullPath);
                ClearTextFile();

                string question_text = "";
                using (StreamReader reader = new StreamReader(saveFilePath, Encoding.UTF8))
                {
                    question_text = reader.ReadToEnd();
                }
                string prompt = question_text;
                Console.WriteLine($"\nUser: {prompt}");

                if (question_text == "HmGoogleGemini.Cancel()")
                {
                    chatSession.Cancel();
                    isConversationing = false;
                    return;
                }

                if (question_text == "HmGoogleGemini.Clear()")
                {
                    chatSession.Cancel();
                    chatSession.Clear();
                    isConversationing = false;
                    return;
                }

                if (isConversationing) { return; }
                isConversationing = true;
                var task = chatSession.SendMessageAsync(prompt);
                string response = task.Result;
                isConversationing = false;
                Console.WriteLine($"Response: {response}");
            }
            catch (Exception)
            {
            }
            finally
            {
                isConversationing = false;
            }
        };

    }
}

