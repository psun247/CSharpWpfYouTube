using System;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpWpfYouTube.Helpers;
using CSharpWpfYouTube.Models;

namespace CSharpWpfYouTube.Services
{
    // Get video info such as snippet, release date, cover jpg, etc.
    public class YouTubeDataService
    {
        private const string _VideoIDPlaceHolder = "VideoIDPlaceHolder";
        private readonly string _VideoSnippetUrl;

        private HttpClient _httpClient;

        public YouTubeDataService(string youtubeDataApiKey)
        {
            _VideoSnippetUrl = $"https://www.googleapis.com/youtube/v3/videos?part=id%2C+snippet&id={_VideoIDPlaceHolder}&key={youtubeDataApiKey}";
            _httpClient = new HttpClient();
        }

        public async Task<VideoInfo> CreateYouTubeVideoMatch(string youTubeVideoUrl)
        {
            // https://img.youtube.com/vi/<insert-youtube-video-id-here>/0.jpg
            // https://img.youtube.com/vi/K6VuFCwUMnQ/0.jpg            

            YouTubeVideoInfo? videoInfo = await GetVideoInfo(youTubeVideoUrl);
            Snippet? snippet = videoInfo?.ItemList?.Count > 0 ? videoInfo.ItemList[0].Snippet : null;
            string title = snippet?.Title ?? "No Title";
            string released = snippet?.ReleaseDate?.Length > 10 ? // Format: p2009-10=03T...
                                snippet.ReleaseDate.Substring(0, 10) : "Unknown";
            return new VideoInfo
            {                
                Description = title,
                Released = released,
                // With YouTubeVideoUrl, from https://www.youtube.com/watch?v=K6VuFCwUMnQ,
                // make https://img.youtube.com/vi/K6VuFCwUMnQ/0.jpg
                CoverUrl = snippet?.ThumbNails?.Default?.Url ??
                                    youTubeVideoUrl.Replace("www.youtube", "img.youtube")
                                                    .Replace("watch?v=", "vi/") + "/0.jpg",
                Link = youTubeVideoUrl
            };
        }
        
        private async Task<YouTubeVideoInfo?> GetVideoInfo(string videoUrl)
        {
            string videoID = ParseVideoID(videoUrl);
            string url = _VideoSnippetUrl.Replace(_VideoIDPlaceHolder, videoID);
            string result = await _httpClient.GetStringAsync(url);
            return JsonHelper.DeserializeToClass<YouTubeVideoInfo>(result);
        }

        private string ParseVideoID(string videoUrl)
        {
            int index = videoUrl.IndexOf("youtube.com/watch?v=", StringComparison.InvariantCultureIgnoreCase);
            if (index > 0)
            {
                // https://www.youtube.com/watch?v=6DiWifGtyLo
                // https://www.youtube.com/watch?v=6DiWifGtyLo?t=15       
                return videoUrl.Split('?')[1].Remove(0, 2); // Last remove "v="
            }

            if (videoUrl.Contains("youtu.be/"))
            {
                // https://youtu.be/6DiWifGtyLo (right-click copy url from YouTube control) 
                return videoUrl.Split('/')[3];
            }

            throw new Exception($"Invalid video url (VideoID not found): {videoUrl}");
        }
    }
}
