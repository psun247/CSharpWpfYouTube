using CSharpWpfYouTube.Helpers;

namespace CSharpWpfYouTube.Models
{
    public class VideoInfo
    {
        public const string HomeVideoGroup = "Home";
        public const string YouTubeHomeUri = "https://www.youtube.com";

        public VideoInfo()
        {
        }

        // Group name in the top-left group list for binding VideoInfoList        
        public string VideoGroup { get; set; } = HomeVideoGroup;

        // From YouTube data service API
        public string Description { get; set; } = string.Empty;

        // https://www.youtube.com/watch?v=d_l-st8Q1S0
        public string Link { get; set; } = string.Empty;

        // Leave it as null (not string.Empty) as default because string.Empty
        // causes an exception as invalid Image's Source in XAML        
        public string? CoverUrl { get; set; }
        public string? Released { get; set; }

        public bool IsSameVideo(string link) =>
                        Link.IsNotBlank() &&
                            Link.Equals(link, System.StringComparison.InvariantCultureIgnoreCase);

        public SLVideoInfo ToSLVideoInfo() =>
                                new SLVideoInfo
                                {
                                    VideoGroup = VideoGroup,
                                    Description = Description,
                                    Link = Link,
                                    CoverUrl = CoverUrl,
                                };
    }
}
