using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using CSharpWpfYouTube.Models;

namespace CSharpWpfYouTube
{
    public partial class MainWindow : Window
    {                
        private const string _Remove = "Remove";

        private MainViewModel _mainViewModel;
        private ContextMenu _videoListContextMenu;

        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            DataContext = _mainViewModel = mainViewModel;
            Loaded += MainWindow_Loaded;
            videoHistoryListDataGrid.PreviewMouseRightButtonUp += videoHistoryListDataGrid_PreviewMouseRightButtonUp;
            _videoListContextMenu = new ContextMenu();
            _videoListContextMenu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(VideoMenuOnClick));
            Closing += MainWindow_Closing;           
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ReloadAndRebindAll();
        }

        private void videoHistoryListDataGrid_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Hit test for image, text, blank space below text (Border)
            if (e.Device.Target is Grid ||
                e.Device.Target is Ellipse ||
                e.Device.Target is TextBlock ||
                e.Device.Target is Border)
            {
                VideoInfo? videoInfo = (e.Device.Target as FrameworkElement)?.DataContext as VideoInfo;
                if (videoInfo != null)
                {
                    ShowVideoListContextMenu(videoInfo);
                    e.Handled = true;
                }
            }
        }

        public void ShowVideoListContextMenu(VideoInfo videoInfo)
        {
            _videoListContextMenu.Tag = videoInfo;
            _videoListContextMenu.Items.Clear();
            
            _videoListContextMenu.Items.Add(new MenuItem
            {
                Header = "Move To",
                IsHitTestVisible = false,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });
            foreach (string videoGroup in _mainViewModel.VideoGroupList)
            {
                if (videoGroup != videoInfo.VideoGroup)
                {
                    _videoListContextMenu.Items.Add(new Separator());
                    _videoListContextMenu.Items.Add(new MenuItem { Header = videoGroup });
                }
            }            
            _videoListContextMenu.Items.Add(new Separator());
            _videoListContextMenu.Items.Add(new MenuItem
            {
                Header = "Command",
                IsHitTestVisible = false,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });
            _videoListContextMenu.Items.Add(new Separator());
            _videoListContextMenu.Items.Add(new MenuItem { Header = _Remove });

            _videoListContextMenu.IsOpen = true;
        }

        private void VideoMenuOnClick(object sender, RoutedEventArgs args)
        {
            MenuItem? mi = args.Source as MenuItem;
            VideoInfo? videoInfo = _videoListContextMenu.Tag as VideoInfo;
            if (mi != null && videoInfo != null)
            {                
                switch (mi.Header as string)
                {                                        
                    case _Remove: _mainViewModel.RemoveVideoInfo(videoInfo); break;
                    default: _mainViewModel.MoveToVideoGroup((string)mi.Header, videoInfo.Link); break;
                }
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _mainViewModel.Shutdown();                
            }
            catch (Exception)
            {
            }

            // Since Application.Current.ShutdownMode is OnLastWindowClose, don't need to call this:            
            // Application.Current.Shutdown();
        }
    }
}
