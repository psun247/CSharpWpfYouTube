using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Web.WebView2.Wpf;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CSharpWpfYouTube.Helpers;
using CSharpWpfYouTube.Models;
using CSharpWpfYouTube.Services;

namespace CSharpWpfYouTube
{
    public partial class MainViewModel : ObservableRecipient
    {
        private readonly Uri _BlankUri = new Uri("about:blank");

        private AppSettings _appSettings;
        private MainService _mainService;
        private List<VideoInfo> _videoInfoList;

        public MainViewModel(string dbFilePath)
        {
            _appSettings = new AppSettings();
            _mainService = new MainService(dbFilePath);
            _videoInfoList = new List<VideoInfo>();
            InitializeWebView2();
            AppTitle = $"{App.AppName} - by Peter Sun (WPF WebView2, CommunityToolkit.Mvvm, " +
                        "EntityFrameworkCore.Sqlite, ModernWpfUI, RestoreWindowPlace)";
#if DEBUG
            AppTitle += " - Debug";
#endif
        }

        public string AppTitle { get; private set; } = string.Empty;
        // The one and only one WebView2Control
        public WebView2? WebView2Control { get; private set; }
        // Video address in the textbox (whenever navigated to)
        [ObservableProperty]
        string _currentVideoUri = string.Empty;
        [ObservableProperty]
        ObservableCollection<string> _videoGroupList = new ObservableCollection<string>();
        [ObservableProperty]
        string _selectedVideoGroup = string.Empty;
        [ObservableProperty]
        ObservableCollection<VideoInfo> _videoInfoByGroupList = new ObservableCollection<VideoInfo>();
        [ObservableProperty]
        VideoInfo? _selectedVideoInfo;
        [ObservableProperty]
        string _statusMessage = string.Empty;

        public void ReloadAndRebindAll()
        {
            try
            {
                _mainService.LoadAppSettings(_appSettings);

                List<string> videoGroupList = _mainService.EnsureLoadVideoGroupList();
                VideoGroupList = new ObservableCollection<string>(videoGroupList);
                SelectedVideoGroup = _appSettings.SelectedVideoGroup;
                _videoInfoList = _mainService.GetVideoInfoList();
                SelectedVideoInfo = EnsureInitialVideoInfo(_videoInfoList, _appSettings.SelectedVideoLink);
                VideoInfoByGroupList = new ObservableCollection<VideoInfo>(_videoInfoList.Where(x => x.VideoGroup == _selectedVideoGroup));
                StatusMessage = "Right-click on a video in the left panel for actions";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void MoveToVideoGroup(string videoGroup, string? videoLink)
        {
            try
            {
                VideoInfo? videoInfo = _videoInfoList.FirstOrDefault(x => x.Link == videoLink);
                if (videoInfo != null)
                {
                    videoInfo.VideoGroup = videoGroup;
                    VideoInfoByGroupList.Remove(videoInfo);

                    if (VideoInfoByGroupList.Count > 0)
                    {
                        // Stay in the current group, select the first one
                        SelectedVideoInfo = VideoInfoByGroupList[0];
                    }
                    else
                    {
                        // The current group is empty, so go to the moved group
                        AutoSelectVideoGroupAndVideo(videoInfo);
                    }

                    _mainService.MoveToGroup(videoLink, videoGroup);

                    StatusMessage = $"Moved '{videoLink}' to group '{videoGroup}'";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        public void RemoveVideoInfo(VideoInfo videoInfo)
        {
            try
            {
                StopCurrentVideo(blankUriOnly: false);

                _mainService.RemoveVideoInfo(VideoInfoByGroupList, videoInfo.Link);

                // Also remove it from 'full' list
                _videoInfoList.RemoveAll(x => x.Link == videoInfo.Link);

                if (VideoInfoByGroupList.Count > 0)
                {
                    SelectedVideoInfo = VideoInfoByGroupList[0];
                }
                else
                {
                    // Videos in current group are empty, so select one (could be ramdon) in the master list                
                    VideoInfo? selectedVideoInfo = _videoInfoList.FirstOrDefault();
                    if (selectedVideoInfo != null)
                    {
                        AutoSelectVideoGroupAndVideo(selectedVideoInfo);
                    }
                }

                StatusMessage = $"Removed video from group '{videoInfo.VideoGroup}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error on removing video: {ex.Message}";
            }
        }

        public void Shutdown()
        {
            StopCurrentVideo(blankUriOnly: true);
        }

        // partial method hook (selected a video group in the top-left panel)
        partial void OnSelectedVideoGroupChanged(string value)
        {
            if (value.IsNotBlank())
            {
                if (_videoInfoList.IsNotEmpty())
                {
                    VideoInfoByGroupList = new ObservableCollection<VideoInfo>(_videoInfoList.Where(x => x.VideoGroup == value));
                }

                // Note: SelectedVideoInfo remains unchanged
                _mainService.UpdateAppSetting(AppSettings.SelectedVideoGroupName, value);

                StatusMessage = $"Group '{value}'";
            }
        }

        // partial method hook (selected a video in the left panel)
        partial void OnSelectedVideoInfoChanged(VideoInfo? value)
        {
            if (value != null)
            {
                if (Uri.IsWellFormedUriString(value.Link, UriKind.RelativeOrAbsolute))
                {
                    // Link is like https://www.youtube.com/watch?v=d_l-st8Q1S0, 
                    // https://www.youtube.com/results?search_query=....."                    
                    _currentVideoUri = value.Link;
                }
                else
                {                    
                    _currentVideoUri = VideoInfo.YouTubeHomeUri;
                }

                // YouTube tab (go to the last navigated video uri)
                BindWebView2Control(_currentVideoUri);                
                OnPropertyChanged(nameof(CurrentVideoUri));

                _mainService.UpdateAppSetting(AppSettings.SelectedVideoLinkName, _currentVideoUri);

                if (value.Description.IsNotBlank())
                {
                    StatusMessage = value.Description;
                }
                else
                {
                    StatusMessage = $"Selected video in group '{value.VideoGroup}'";
                }
            }
            else
            {
                StatusMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void GoToVideoUrl()
        {
            try
            {
                BindWebView2Control(CurrentVideoUri);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void ImportCurrentVideo()
        {
            if (!GeneralHelper.IsValidUri(_currentVideoUri))
            {
                StatusMessage = "A video needs to be navigated to for 'Import'";
                return;
            }

            if (!GeneralHelper.IsYouTubeVideoUri(_currentVideoUri))
            {
                StatusMessage = "Must be a YouTube video to be imported";
                return;
            }

            try
            {
                GeneralHelper.CleanYouTubeUri(ref _currentVideoUri);
                var videoInfo = new VideoInfo
                {
                    // With YouTubeVideoUri, from https://www.youtube.com/watch?v=d_l-st8Q1S0,
                    // make https://img.youtube.com/vi/d_l-st8Q1S0/0.jpg
                    CoverUrl = _currentVideoUri.Replace("www.youtube", "img.youtube")
                                           .Replace("watch?v=", "vi/") + "/0.jpg",
                    Link = _currentVideoUri,
                };

                _mainService.ImportVideo(_videoInfoList, _selectedVideoGroup, ref videoInfo,
                                            out bool isNewVideo, out string statusMessage);
                if (isNewVideo)
                {
                    VideoInfoByGroupList = new ObservableCollection<VideoInfo>(_videoInfoList.Where(x => x.VideoGroup == _selectedVideoGroup));

                    // Ensure the video is selected after it's added to the left panel                        
                    SelectedVideoInfo = videoInfo;
                }
                StatusMessage = statusMessage;
            }
            catch (NotSupportedException ex)
            {
                // SQLite exception
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async void OpenAtYouTubeWebSite()
        {
            try
            {
                if (_currentVideoUri.IsNotBlank() && GeneralHelper.IsValidUri(_currentVideoUri))
                {
                    await GeneralHelper.ExecuteOpenUrlCommandAsync(_currentVideoUri);
                }
                else
                {
                    await GeneralHelper.ExecuteOpenUrlCommandAsync(VideoInfo.YouTubeHomeUri);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void InitializeWebView2()
        {
            // One and only one WebView2Control
            WebView2Control = new WebView2
            {
                Name = "WebView2",
                Source = _BlankUri,
            };
            WebView2Control.SourceChanged += (s, e) =>
            {
                // Always update the textbox (so can copy to clipboard)
                // not the same as YouTube behavior (only update at the top)                
                CurrentVideoUri = WebView2Control.Source.AbsoluteUri;
            };
            OnPropertyChanged(nameof(WebView2Control));
        }

        // Ensure to avoid empty YouTube control
        private VideoInfo EnsureInitialVideoInfo(List<VideoInfo> videoInfoList, string selectedVideoLink)
        {
            VideoInfo? initialVideoInfo = null;            
            if (selectedVideoLink.IsNotBlank())
            {
                initialVideoInfo = videoInfoList.FirstOrDefault(x => x.Link == selectedVideoLink);
            }
            if (initialVideoInfo == null)
            {
                if (videoInfoList.Count > 0)
                {
                    initialVideoInfo = videoInfoList[0];
                }
                else
                {
                    // Create defaults on empty. Videos imported by a user (from UI) will be from YouTube.
                    _currentVideoUri = "https://github.com/psun247/CSharpWpfChatGPT";
                    initialVideoInfo = new VideoInfo
                    {
                        Description = "C# WPF ChatGPT by Peter Sun",
                        CoverUrl = "/CSharpWpfYouTube;component/Resources/Info.png",
                        Link = _currentVideoUri,
                    };
                    _mainService.ImportVideo(_videoInfoList, VideoInfo.HomeVideoGroup, ref initialVideoInfo,
                                             out bool _, out string _);
                    var videoInfo = new VideoInfo
                    {
                        Description = "French Open Novak Djokovic vs Carlos Alcaraz",
                        CoverUrl = "https://img.youtube.com/vi/K6VuFCwUMnQ/0.jpg",
                        Link = "https://www.youtube.com/watch?v=K6VuFCwUMnQ",
                    };
                    _mainService.ImportVideo(_videoInfoList, VideoInfo.HomeVideoGroup, ref videoInfo,
                                             out bool _, out string _);
                }
            }

            // Ensure SelectedVideoGroup but not triggering setting save
            _selectedVideoGroup = initialVideoInfo.VideoGroup;
            OnPropertyChanged(nameof(SelectedVideoGroup));

            return initialVideoInfo;
        }

        private void AutoSelectVideoGroupAndVideo(VideoInfo videoInfo)
        {
            SelectedVideoGroup = VideoGroupList.FirstOrDefault(x => x == videoInfo.VideoGroup) ?? string.Empty;
            SelectedVideoInfo = videoInfo;
        }

        private void BindWebView2Control(string youTubeWebView2Source)
        {
            if (WebView2Control != null &&
                (WebView2Control.Source == null || WebView2Control.Source.AbsoluteUri != youTubeWebView2Source))
            {
                WebView2Control.Source = new Uri(youTubeWebView2Source, UriKind.RelativeOrAbsolute);
            }
        }

        // blankUriOnly to avoid UI flickering on app exit
        private void StopCurrentVideo(bool blankUriOnly)
        {
            if (WebView2Control != null)
            {
                // Only stop the playing (if going on), but can't pause the video though because it's a website uri
                Uri uri = WebView2Control.Source;
                WebView2Control.Source = _BlankUri;
                if (!blankUriOnly)
                {
                    WebView2Control.Source = uri;
                }
            }
        }
    }
}
