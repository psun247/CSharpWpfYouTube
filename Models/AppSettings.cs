namespace CSharpWpfYouTube.Models
{
    // Each property is a row in SLAppSetting table as name-value
    public class AppSettings
    {
        // Name column in SLAppSetting table
        public const string SelectedVideoGroupName = "SelectedVideoGroup";
        public const string SelectedVideoLinkName = "SelectedVideoLink";        

        public string SelectedVideoGroup { get; set; } = string.Empty;
        public string SelectedVideoLink { get; set; } = string.Empty;
    }
}
