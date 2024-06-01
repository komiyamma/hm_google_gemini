
using Google.Cloud.AIPlatform.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;



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

    static public void SaveToJson(List<Content> _contents)
    {
        try
        {
            string json = JsonConvert.SerializeObject(_contents);

            // JSONをファイルに書き込み
            File.WriteAllText("data.json", json);
        }
        catch (Exception err)
        {

            Console.WriteLine(err);
        }
    }


    static void ImportData()
    {
        string json = File.ReadAllText("data.json");
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
