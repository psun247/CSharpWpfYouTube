using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharpWpfYouTube.Helpers
{
    public static class GeneralHelper
    {
        public static bool IsValidUri(string uri) =>
                            Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);

        // https://www.youtube.com/watch?v=0fT2amS9KZ4
        // https://youtu.be/0fT2amS9KZ4
        // https://youtu.be/0fT2amS9KZ4?t=17         
        public static bool IsYouTubeVideoUri(string input) =>
                            input.IsNotBlank() &&
                                (input.Contains("youtube.com/watch?v=") || input.Contains("youtu.be/"));

        public static void CleanYouTubeUri(ref string youTubeVideoUri)
        {
            // Remove & and the rest (could be &t=24, &list=..., etc.)
            // https://www.youtube.com/watch?v=0fT2amS9KZ4&index=1
            int index = youTubeVideoUri.IndexOf("&");
            if (index > 0)
            {
                youTubeVideoUri = youTubeVideoUri.Remove(index, youTubeVideoUri.Length - index);
            }
        }

        public static async Task ExecuteOpenUrlCommandAsync(string url, bool lanuchMSEdge = false)
        {
            if (!string.IsNullOrEmpty(url))
            {                             
                await Task.Run(() =>
                {
                    if (lanuchMSEdge)
                    {
                        url = $"microsoft-edge:{url}";
                    }

                    // https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/                        
                    // Suppressing the second command prompt,
                    // and escaping the “&” with “^&” so the shell does not treat them as command separators.                       
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                    {
                        UseShellExecute = false, // Already false in .NET 6
                        CreateNoWindow = true
                    });
                });
            }
        }
    }
}
