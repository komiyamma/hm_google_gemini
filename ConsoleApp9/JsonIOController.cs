
using Google.Cloud.AIPlatform.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;



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
                writer.WriteLine(text);
            }
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }

    static public void SaveContentsToJson(List<Content> _contents)
    {
        try
        {
            string tempfolder = Path.GetTempPath();
            string saveFilePath = Path.Combine(tempfolder, "HmGoogleGemini.json");
            string json = JsonConvert.SerializeObject(_contents);
            Console.WriteLine(saveFilePath);

            // JSONをファイルに書き込み
            File.WriteAllText(saveFilePath, json, Encoding.UTF8);
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }


    static void ImportData()
    {
        string tempfolder = Path.GetTempPath();
        string loadFilePath = Path.Combine(tempfolder, "HmGoogleGemini.json");
        string json = File.ReadAllText(loadFilePath);
        var data = JsonConvert.DeserializeObject<List<TData>>(json);

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
