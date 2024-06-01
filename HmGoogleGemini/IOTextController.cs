using System;
using System.Collections.Generic;
using System.IO;
using System.Text;



partial class HmGoogleGemini
{
    public class TPart
    {
        public string Text { get; set; }
    }
    public class TData
    {
        public string Role { get; set; }
        public List<TPart> Parts { get; set; }
    }

    static public void ClearTextFile()
    {
        try
        {
            string tempfolder = Path.GetTempPath();
            string saveFilePath = Path.Combine(tempfolder, "HmGoogleGemini.txt");

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

    static public void SaveAddTextToFile(string text)
    {
        try
        {
            string tempfolder = Path.GetTempPath();
            string saveFilePath = Path.Combine(tempfolder, "HmGoogleGemini.txt");

            // ファイルが存在しない場合は新規にファイルを作成し、ファイルが存在する場合は追記モードで開く
            using (StreamWriter writer = new StreamWriter(saveFilePath, true, Encoding.UTF8))
            {
                Console.WriteLine("追加書き込み");
                writer.Write(text);
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }
}
