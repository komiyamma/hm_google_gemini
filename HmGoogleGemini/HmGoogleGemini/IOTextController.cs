﻿using System;
using System.IO;
using System.Reflection;
using System.Text;



partial class HmGoogleGemini
{
    static string targetDir = System.AppContext.BaseDirectory;

    // 質問内容をファイルに保存してあるが、ファイル内容をクリアする
    static public void ClearQuestionFile()
    {
        try
        {
            string saveFilePath = Path.Combine(targetDir, "HmGoogleGemini.question.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(saveFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine("");
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

    // AIの回答内容をファイルに保存してあるが、ファイル内容をクリアする
    static public void ClearAnswerFile()
    {
        try
        {
            string saveFilePath = Path.Combine(targetDir, "HmGoogleGemini.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(saveFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine("");
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

    // Streamでちょこちょこと返答が返ってくるので、ちょこちょこと返答内容をファイルに追加保存する。
    static public void SaveAddTextToAnswerFile(string text)
    {
        try
        {
            string saveFilePath = Path.Combine(targetDir, "HmGoogleGemini.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(saveFilePath, true, Encoding.UTF8))
            {
                // Console.WriteLine("追加書き込み");
                writer.Write(text);
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

    // 最後に全体を念のために保存する
    static public void SaveAllTextToAnswerFile(string text)
    {
        try
        {
            string saveFilePath = Path.Combine(targetDir, "HmGoogleGemini.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(saveFilePath, false, Encoding.UTF8))
            {
                // Console.WriteLine("追加書き込み");
                writer.Write(text);
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

    static public void SaveCompleteFile(int number)
    {
        try
        {
            string completeFilePath = Path.Combine(targetDir, "HmGoogleGemini.complete.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(completeFilePath, false, Encoding.UTF8))
            {
                // Console.WriteLine("追加書き込み");
                writer.Write($"HmGoogleGemini.MessageComplete({number})");
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

}
