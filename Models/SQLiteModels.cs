using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Models for tables in SQLite database
namespace CSharpWpfYouTube.Models
{
    [Table("SLAppSetting")]
    public class SLAppSetting
    {
        // Default ctor needed for SQLite
        public SLAppSetting()
                : this(string.Empty, string.Empty)
        {
        }

        // Pair such as "SelectedVideoGroup" and "Home"
        public SLAppSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [Required]
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }

    [Table("SLVideoGroup")]
    public class SLVideoGroup
    {
        // Default ctor needed for SQLite
        public SLVideoGroup()
                : this(0, string.Empty)
        {
        }

        public SLVideoGroup(int id, string name)
        {
            Id = id;
            Name = name;
            ModifiedDateTime = DateTime.Now;
        }

        // Important so that the order will be 1 (Home), 2, 3, etc., o/w alphabetical order by Name
        [Required]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDateTime { get; set; }
    }

    [Table("SLVideoInfo")]
    public class SLVideoInfo
    {
        // Default ctor for SQLite
        public SLVideoInfo()
        {
        }

        [Required]
        [Key]
        public int Id { get; set; }

        // Match SLVideoGroup.Name, VideoInfo.VideoGroup        
        public string VideoGroup { get; set; } = string.Empty;

        // User input from UI, or auto-generated
        public string Description { get; set; } = string.Empty;

        // https://www.youtube.com/watch?v=d_l-st8Q1S0
        public string Link { get; set; } = string.Empty;

        // Leave it as null (not string.Empty) as default because string.Empty
        // causes an exception as invalid Image's Source in XAML        
        public string? CoverUrl { get; set; }

        public DateTime ModifiedDateTime { get; set; } = DateTime.Now;
    }
}
