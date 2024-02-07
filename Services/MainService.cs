using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CSharpWpfYouTube.Helpers;
using CSharpWpfYouTube.Models;

namespace CSharpWpfYouTube.Services
{
    public class MainService
    {
        private string _dbFilePath;

        public MainService(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
        }

        public void LoadAppSettings(AppSettings appSettings)
        {
            bool needToSaveChanges = false;

            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                var slAppSettingList = sqliteDbContext.SLAppSettings.ToList();
                SLAppSetting? selectedVideoGroup = slAppSettingList.FirstOrDefault(x => x.Name == AppSettings.SelectedVideoGroupName);
                if (selectedVideoGroup != null)
                {
                    appSettings.SelectedVideoGroup = selectedVideoGroup.Value;
                }
                else
                {
                    appSettings.SelectedVideoGroup = VideoInfo.HomeVideoGroup;
                    sqliteDbContext.SLAppSettings.Add(new SLAppSetting(AppSettings.SelectedVideoGroupName, appSettings.SelectedVideoGroup));
                    needToSaveChanges = true;
                }
                SLAppSetting? selectedVideoLink = slAppSettingList.FirstOrDefault(x => x.Name == AppSettings.SelectedVideoLinkName);
                if (selectedVideoLink != null)
                {
                    appSettings.SelectedVideoLink = selectedVideoLink.Value;
                }
                else
                {
                    appSettings.SelectedVideoLink = string.Empty;
                    sqliteDbContext.SLAppSettings.Add(new SLAppSetting(AppSettings.SelectedVideoLinkName, appSettings.SelectedVideoLink));
                    needToSaveChanges = true;
                }
                if (needToSaveChanges)
                {
                    sqliteDbContext.SaveChanges();
                }
            }
        }

        public void UpdateAppSetting(string settingName, string value)
        {
            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                SLAppSetting? setting = sqliteDbContext.SLAppSettings.FirstOrDefault(x => x.Name == settingName);
                if (setting != null)
                {
                    setting.Value = value;
                    sqliteDbContext.SaveChanges();
                }
            }
        }

        public List<string> EnsureLoadVideoGroupList()
        {
            List<string> videoGroupList;
            List<SLVideoGroup> slVideoGroupList;
            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                slVideoGroupList = sqliteDbContext.SLVideoGroups.ToList();
            }
            if (slVideoGroupList.IsNotEmpty())
            {
                videoGroupList = slVideoGroupList.Select(x => x.Name).ToList();
            }
            else
            {
                videoGroupList = new List<string> { VideoInfo.HomeVideoGroup, "Music", "Fun", "Other" };
                InsertVideoGroupList(videoGroupList, slVideoGroupList);
            }
            return videoGroupList;
        }

        private void InsertVideoGroupList(List<string> videoGroupList, List<SLVideoGroup> slVideoGroupList)
        {
            int id = 1; // Home
            foreach (string videoGroup in videoGroupList)
            {
                slVideoGroupList.Add(new SLVideoGroup(id, videoGroup)
                {
                    ModifiedDateTime = DateTime.Now
                });
                id++;
            }

            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                sqliteDbContext.SLVideoGroups.AddRange(slVideoGroupList);
                sqliteDbContext.SaveChanges();
            }
        }

        public List<VideoInfo> GetVideoInfoList()
        {
            List<SLVideoInfo> slVideoInfoList;
            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                slVideoInfoList = sqliteDbContext.SLVideoInfos.ToList();
            }
            return slVideoInfoList.Select(x => new VideoInfo
            {
                VideoGroup = x.VideoGroup,
                Description = x.Description,
                Link = x.Link,
                CoverUrl = x.CoverUrl,
            }).ToList();
        }

        private void AddVideoInfo(VideoInfo videoInfo)
        {
            SLVideoInfo slVideoInfo = videoInfo.ToSLVideoInfo();
            slVideoInfo.ModifiedDateTime = DateTime.Now;

            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                if (!sqliteDbContext.SLVideoInfos.Any(IsSameVideo(slVideoInfo.Link)))
                {
                    sqliteDbContext.SLVideoInfos.Add(slVideoInfo);
                    sqliteDbContext.SaveChanges();
                }
            }
        }

        public void MoveToGroup(string? videoLink, string videoGroup)
        {
            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                SLVideoInfo? slVideoInfo = sqliteDbContext.SLVideoInfos.FirstOrDefault(x => x.Link == videoLink);
                if (slVideoInfo != null)
                {
                    slVideoInfo.VideoGroup = videoGroup;
                    sqliteDbContext.SaveChanges();
                }
            }
        }

        private System.Linq.Expressions.Expression<Func<SLVideoInfo, bool>> IsSameVideo(string videoInfolink)
        {
            // Still can't call a function like input.CallSomething(), where input is SLVideoInfo, but can use ToLower()
            return input => !string.IsNullOrEmpty(input.Link) && input.Link.ToLower() == videoInfolink.ToLower();
        }

        public void ImportVideo(List<VideoInfo> videoInfoList, string videoGroup, ref VideoInfo videoInfo,
                                out bool isNewVideo, out string statusMessage)
        {
            // Need this to avoid: error CS1628: Cannot use ref, out, or in parameter 'videoInfo' inside an anonymous method,
            // lambda expression, query expression, or local function
            string link = videoInfo.Link;
            if (videoInfoList.Any(x => x.IsSameVideo(link)))
            {
                isNewVideo = false;
                statusMessage = "Already previously imported";
            }
            else
            {
                isNewVideo = true;
                videoInfo.VideoGroup = videoGroup;
                AddVideoInfo(videoInfo);
                videoInfoList.Add(videoInfo);

                // Because no description for the video, hence just the group
                statusMessage = $"Imported into group '{videoGroup}'";
            }
        }

        public void RemoveVideoInfo(ObservableCollection<VideoInfo> videoInfoList, string link)
        {
            var videoInfoToRemove = videoInfoList.FirstOrDefault(x => x.IsSameVideo(link));
            if (videoInfoToRemove == null)
            {
                throw new Exception("Video not found");
            }

            // Remove from UI and DB
            videoInfoList.Remove(videoInfoToRemove);
            RemoveVideoInfoFromSQLite(link);
        }

        private void RemoveVideoInfoFromSQLite(string link)
        {
            using (var sqliteDbContext = new SQLiteDbContext(_dbFilePath))
            {
                SLVideoInfo? slVideoInfo = sqliteDbContext.SLVideoInfos.FirstOrDefault(x => x.Link == link);
                if (slVideoInfo != null)
                {
                    sqliteDbContext.SLVideoInfos.Remove(slVideoInfo);
                    sqliteDbContext.SaveChanges();
                }
            }
        }
    }
}
