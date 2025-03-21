﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

internal partial class HmGoogleGemini
{
    // ユーザーからの質問ファイルを監視する。
    static FileSystemWatcher questionFileWatcher = new FileSystemWatcher();

    static bool isConversationing = false;

    public static string questionFilePath = "";

    // 今回のこのプロセス起動で、はじめて質問ファイルをチェックするかどうか
    static Boolean isQuestionFileFirstCheck = true;

    static void StartFileWatchr()
    {
        questionFilePath = Path.Combine(targetDir, "HmGoogleGemini.question.txt");

        // 監視するディレクトリを設定
        questionFileWatcher.Path = targetDir;

        // ファイル更新を監視する
        questionFileWatcher.NotifyFilter = NotifyFilters.LastWrite;

        // 監視するファイルを指定
        questionFileWatcher.Filter = Path.GetFileName(questionFilePath);

        // 監視を開始
        questionFileWatcher.EnableRaisingEvents = true;

        // 1回実行
        CheckQuestionFile(questionFilePath);

        // 更新があった時の処理。ただし連続して同じファイルに複数回保存するエディタがあるので、0.2秒以内のものは無視する。
        questionFileWatcher.Changed += QuestionFileWatcher_Changed;
    }

    static void QuestionFileWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed) { return; }
        if (isQuestionFileFirstCheck) { isQuestionFileFirstCheck = false; return; }
        CheckQuestionFile(e.FullPath);
    }


    static int lastQuestionNumber = 0;
    static void CheckQuestionFile(string filepath)
    {
        try
        {
            // Console.WriteLine("ファイルが更新されました: " + filepath);

            // ファイルが変更されたので、ファイルの内容を読み込む
            string question_text = "";
            using (StreamReader reader = new StreamReader(questionFilePath, Encoding.UTF8))
            {
                question_text = reader.ReadToEnd();
            }

            // 1行目にコマンドと質問がされた時刻に相当するTickCount相当の値が入っている
            // これによって値が進んでいることがわかる。
            // 正規表現を使用して数値を抽出
            Regex regex = new Regex(@"HmGoogleGemini\.(Message|Clear|Cancel|Pop)\((\d+)\)");
            Match match = regex.Match(question_text);

            // コマンドの種類の格納場所(Message, Clear, Cancel, Pop)
            string commandName = "";
            int questionNumber = 0;
            if (match.Success)
            {
                // コマンド
                commandName = match.Groups[1].Value;

                    // 質問がされた時刻に相当するTickCount
                    string strnumber = match.Groups[2].Value;
                questionNumber = int.Parse(strnumber);

                // 前回の投稿と番号が同じとかなら同一のものを指している。複数回 QuestionFileWatcher_Changed が反応してしまっているが、これを処理する必要はない。
                if (lastQuestionNumber == questionNumber)
                {
                    // Console.WriteLine("★前回と同じファイルだ");
                    return;
                }
                else
                {
                    // １行目がコマンド、2行目以降が質問内容。
                    // よって１行目の部分を削除する。
                    string[] lines = question_text.Split('\n');
                    question_text = string.Join("\n", lines, 1, lines.Length - 1);

                    // 最後に確認したtickCountとして更新
                    lastQuestionNumber = questionNumber;
                }
            }
            else
            {
                
            }

            // キャンセルコマンドなら、AIの応答を途中キャンセルする
            if (commandName == "Cancel")
            {
                chatSession.Cancel();
                isConversationing = false;
                return;
            }

            // クリアなら、キャンセルや会話履歴をクリアしつつ、質問内容をクリアして、このプロセスは終了
            else if (commandName == "Clear")
            {
                chatSession.Cancel();
                chatSession.Clear();
                isConversationing = false;
                ClearAnswerFile();
                Environment.Exit(0);
                return;
            }

            // 現在まだＡＩの応答中なら、新たな質問は受け付けない
            if (isConversationing) { return; }

            // 最後の「質問と応答」の履歴を削除
            if (commandName == "Pop")
            {
                chatSession.PopCotent();
                return;
            }

            // ブロックフラグ
            isConversationing = true;

            {
                // 回答内容のファイルをクリアして、
                ClearAnswerFile();

                // AIに質問を投げる
                string prompt = question_text;
                // Console.WriteLine($"\nUser: {prompt}");
                // 回答がStreamで返ってくるので全部終わるのを待つ(全部終わるまでは次の質問を受け付けない)
                var task = chatSession.SendMessageAsync(prompt, questionNumber);
                string response = task.Result;
            }

            // ブロックフラグ解除
            isConversationing = false;
        }
        catch (Exception)
        {
        }
        finally
        {
            // エラーが起きた際でも会話ブロックはとにかく解除
            isConversationing = false;
        }
    }

}

