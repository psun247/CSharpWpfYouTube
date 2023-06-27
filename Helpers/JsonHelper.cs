using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CSharpWpfYouTube.Helpers
{
    public static class JsonHelper
    {
        public static string SerializeObjectAsJsonString(object objectToSerialize)
        {
            string jsonString = JsonSerializer.Serialize(objectToSerialize,
                        options: new JsonSerializerOptions
                        {                            
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            WriteIndented = true,
                        });
            return jsonString;
        }

        public static void SaveAsJsonToFile(object objectToSerialize, string filePath)
        {
            string jsonString = SerializeObjectAsJsonString(objectToSerialize);
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                File.WriteAllText(filePath, jsonString);
            }
        }

        public static T? DeserializeToClass<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return default(T);

            T? @class = JsonSerializer.Deserialize<T>(jsonString);
            return @class;
        }

        public static T? DeserializeFromFile<T>(string jsonfilePath)
        {
            string jsonString = File.ReadAllText(jsonfilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
                return default(T);

            T? @class = JsonSerializer.Deserialize<T>(jsonString);
            return @class;
        }
    }
}
