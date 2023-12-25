using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ulti;
using Interface;
using MediaPlayer.Pages;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Threading;
using System.Numerics;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = System.Windows.Window;
//using System.Drawing;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //sidebar.SelectedIndex = 0;
        }
        ObservableCollection<IPlaylist> _myPlaylists = new ObservableCollection<IPlaylist>();


        public bool isPlay = false;
        MyPlaylists myPlaylist;
        RecentlyPlayed recentlyPlayed;
        Teams team = new Teams();

        ListSong listSong = new();
        ISong CurrentPlaying = new ISong();

        BindingList<ISong> recentlyList = new BindingList<ISong>();
        DispatcherTimer _timer;

        bool isPlayShuffle = false;
        bool isPlayRePeat = false;

        private void sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = sidebar.SelectedItem as NavButton;
            int index = sidebar.SelectedIndex;

            navframe.Visibility = Visibility;
            if (index == 1)
                navframe!.NavigationService.Navigate(myPlaylist);
            else if (index == 2)
                navframe!.NavigationService.Navigate(recentlyPlayed);
            else if (index == 3)
                navframe!.NavigationService.Navigate(team);
            else
            {
                navframe.Visibility = Visibility.Hidden;
            }

        }
        public void saveRecently()
        {
            if (CurrentPlaying.path == null)
                return;

            ISong newFile = (ISong)CurrentPlaying.Clone();
            newFile.currentTime = player.Position.TotalSeconds % player.NaturalDuration.TimeSpan.TotalSeconds;
            
            ISong temp = null;
            foreach (var song in recentlyList)
            {
                if (newFile.path == song.path)
                {
                    temp = song;
                    break;
                }
            }
           if(temp != null)
                recentlyList.Remove(temp);
            recentlyList.Insert(0,newFile);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            sidebar.SelectedIndex = 0;
            player.Volume = (double)volumeSlider.Value;

            _myPlaylists = new ObservableCollection<IPlaylist>()
            {
                new IPlaylist() { name = "Love Song", date = "20/12/2022", listSongs = null, }
            };

            myPlaylist = new MyPlaylists(_myPlaylists, listSong,player);
            myPlaylist.PlaylistsChanged += MyPlaylist_PlaylistsChanged;
            myPlaylist.SongListChanged += MyPlaylist_SongListChanged; 

            recentlyPlayed = new RecentlyPlayed(recentlyList,listSong);
            recentlyPlayed.SongListChanged += RecentlyPlayed_SongListChanged;

        }

        private void RecentlyPlayed_SongListChanged(ListSong newValue)
        {
            listSong = (ListSong) newValue.Clone();
            saveRecently();
            CurrentPlaying = listSong.listSongs[listSong.currentIndex];
            playMediaFile();
        }

        private void MyPlaylist_SongListChanged(ListSong newValue)
        {
            if (newValue.listSongs.Count > 0)
            {
                listSong = (ListSong)newValue.Clone();
                
                saveRecently();
                CurrentPlaying = listSong.listSongs[listSong.currentIndex];

                playMediaFile();

            }
        }

        private void MyPlaylist_PlaylistsChanged(ObservableCollection<IPlaylist> newValue)
        {
            _myPlaylists = newValue;
        }

        public void playMediaFile()
        {

            isPlay = true;
            player.Source = new Uri(CurrentPlaying.path, UriKind.Absolute);
            player.Play();
            player.Stop();

            //BitmapImage image = new BitmapImage(new Uri(CurrentPlaying.image, UriKind.Relative));

            title.Text = CurrentPlaying.title;
            perform.Text = CurrentPlaying.singer;
            songicon.DataContext = CurrentPlaying;
            music_img.DataContext = CurrentPlaying;

            titleHome.Text = CurrentPlaying.title;
            performHome.Content = CurrentPlaying.singer;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timer.Tick += _timer_Tick;

            PlayButton_Path.Data = Geometry.Parse("M6 5h4v14H6zm8 0h4v14h-4z");

            player.Play();
            _timer.Start();
            TimeSpan newPosition = TimeSpan.FromSeconds(CurrentPlaying.currentTime);
            player.Position = newPosition;
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPlaying.path == null)
                return;

            if (isPlay == false)
            {
                playMediaFile();
            }
            else
            {
                PlayButton_Path.Data = Geometry.Parse("M4610 6399 l0 -2881 43 25 c195 114 4144 2392 4494 2593 339 194 448 262 440 270 -7 7 -743 434 -1637 949 -894 516 -2001 1155 -2460 1420 -459 265 -845 487 -857 494 l-23 12 0 -2882z");
                isPlay = false;
                CurrentPlaying.currentTime = player.Position.TotalSeconds;
                player.Pause();
                _timer.Stop();
            }
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            int hours = player.Position.Hours;
            int minutes = player.Position.Minutes;
            int seconds = player.Position.Seconds;
            currentPosition.Text = $"{hours}:{minutes}:{seconds}";

            if (player.Position.TotalSeconds > 0)
            {
               
                    progressSlider.Value = player.Position.TotalSeconds;

                
            }
        }


        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!end)
            {
                double value = progressSlider.Value;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                player.Position = newPosition;
            }
        }

        bool end = false;
        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            
            int hours = player.NaturalDuration.TimeSpan.Hours;
            int minutes = player.NaturalDuration.TimeSpan.Minutes;
            int seconds = player.NaturalDuration.TimeSpan.Seconds;
            totalPosition.Text = $"{hours}:{minutes}:{seconds}"; 
            progressSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
            end = false;

        }

       
        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            end = true;
            next();
        }
        private void nextButton(object sender, RoutedEventArgs e)
        {
            next();
        }

        public void next()
        {
            
            if (isPlayShuffle)
            {
                int index = listSong.currentIndex;
                while (index == listSong.currentIndex)
                {
                    Random r = new Random();
                    index = r.Next(0, listSong.listSongs.Count);

                }
                listSong.listSongs[listSong.currentIndex].currentTime = 0;
                listSong.currentIndex = index;

            }
            else
            {
                if (listSong.currentIndex == listSong.listSongs.Count - 1)
                {
                    if (isPlayRePeat)
                    {

                        listSong.listSongs[listSong.currentIndex].currentTime = 0;
                        listSong.currentIndex = 0;
                    }
                    else
                    {
                        listSong.currentIndex -= 1;
                        return;
                    }

                }
                else
                {
                    listSong.listSongs[listSong.currentIndex].currentTime = 0;
                    listSong.currentIndex += 1;
                }
            }

            saveRecently();
            CurrentPlaying = listSong.listSongs[listSong.currentIndex];
            playMediaFile();
        }
        private void prevButton(object sender, RoutedEventArgs e)
        {
            if (listSong.listSongs == null)
                return;

            if (isPlayShuffle)
            {
                int index = listSong.currentIndex;
                while (index == listSong.currentIndex)
                {
                    Random r = new Random();
                    index = r.Next(0, listSong.listSongs.Count);

                }
                listSong.listSongs[listSong.currentIndex].currentTime = 0;
                listSong.currentIndex = index;

            }
            else
            {
                listSong.currentIndex -= 1;
                if (listSong.currentIndex < 0)
                {
                    listSong.currentIndex += 1;
                    return;
                }
                listSong.listSongs[listSong.currentIndex + 1].currentTime = 0;
            }

            saveRecently();
            CurrentPlaying = listSong.listSongs[listSong.currentIndex];

            playMediaFile();
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = (double)volumeSlider.Value;
            if (volumeSlider.Value == 0)
            {
                Volumn_Path.Data = Geometry.Parse("M155.51465,24.81348a7.99427,7.99427,0,0,0-8.42578.87207L77.25488,80H32A16.01833,16.01833,0,0,0,16,96v64a16.01833,16.01833,0,0,0,16,16H77.25488l69.834,54.31445A7.99958,7.99958,0,0,0,160,224V32A7.9997,7.9997,0,0,0,155.51465,24.81348ZM32,96H72v64H32ZM144,207.64258,88,164.08789V91.91211l56-43.55469Zm101.65723-61.29981a8.00053,8.00053,0,0,1-11.31446,11.31446L216,139.31445l-18.34277,18.34278a8.00053,8.00053,0,0,1-11.31446-11.31446L204.68555,128l-18.34278-18.34277a8.00053,8.00053,0,0,1,11.31446-11.31446L216,116.68555l18.34277-18.34278a8.00053,8.00053,0,0,1,11.31446,11.31446L227.31445,128Z");
            }
            else
            {
                Volumn_Path.Data = Geometry.Parse("M247.9707,128a79.47687,79.47687,0,0,1-23.43164,56.56934,8.00053,8.00053,0,0,1-11.31445-11.31446,63.99788,63.99788,0,0,0,0-90.50976,8.00053,8.00053,0,0,1,11.31445-11.31446A79.47687,79.47687,0,0,1,247.9707,128ZM160,32V224a7.99935,7.99935,0,0,1-12.91113,6.31445L77.25488,176H32a16.01833,16.01833,0,0,1-16-16V96A16.01833,16.01833,0,0,1,32,80H77.25488l69.834-54.31445A7.99958,7.99958,0,0,1,160,32ZM32,160H72V96H32ZM144,48.35742,88,91.91211v72.17578l56,43.55469ZM184.94043,99.7168a7.99914,7.99914,0,0,0,.001,11.31347,23.99835,23.99835,0,0,1,0,33.93946,7.99983,7.99983,0,1,0,11.3125,11.31445,39.99722,39.99722,0,0,0,0-56.56836A7.99827,7.99827,0,0,0,184.94043,99.7168Z");
            }
        }

        bool volume = false;
        private void volumn_click(object sender, RoutedEventArgs e)
        {
            if (volume == false) {
                volumeSlider.Visibility = Visibility.Visible;
                volume = true;
            }
            else {
                volumeSlider.Visibility = Visibility.Hidden;
                volume = false;
              
            }

        }


        bool isMuted = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P))
            {
                PlayButton_Click(sender, e);
            }

            if (e.Key == Key.Left)
            {
                double value = progressSlider.Value - 5;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                player.Position = newPosition;

            }
            if (e.Key == Key.Right)
            {
                double value = progressSlider.Value + 5;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                player.Position = newPosition;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.M)
            {
                isMuted = !isMuted;
                player.IsMuted = isMuted;
                if (isMuted)
                {
                    Volumn_Path.Data = Geometry.Parse("M155.51465,24.81348a7.99427,7.99427,0,0,0-8.42578.87207L77.25488,80H32A16.01833,16.01833,0,0,0,16,96v64a16.01833,16.01833,0,0,0,16,16H77.25488l69.834,54.31445A7.99958,7.99958,0,0,0,160,224V32A7.9997,7.9997,0,0,0,155.51465,24.81348ZM32,96H72v64H32ZM144,207.64258,88,164.08789V91.91211l56-43.55469Zm101.65723-61.29981a8.00053,8.00053,0,0,1-11.31446,11.31446L216,139.31445l-18.34277,18.34278a8.00053,8.00053,0,0,1-11.31446-11.31446L204.68555,128l-18.34278-18.34277a8.00053,8.00053,0,0,1,11.31446-11.31446L216,116.68555l18.34277-18.34278a8.00053,8.00053,0,0,1,11.31446,11.31446L227.31445,128Z");
                }
                else
                {
                    Volumn_Path.Data = Geometry.Parse("M247.9707,128a79.47687,79.47687,0,0,1-23.43164,56.56934,8.00053,8.00053,0,0,1-11.31445-11.31446,63.99788,63.99788,0,0,0,0-90.50976,8.00053,8.00053,0,0,1,11.31445-11.31446A79.47687,79.47687,0,0,1,247.9707,128ZM160,32V224a7.99935,7.99935,0,0,1-12.91113,6.31445L77.25488,176H32a16.01833,16.01833,0,0,1-16-16V96A16.01833,16.01833,0,0,1,32,80H77.25488l69.834-54.31445A7.99958,7.99958,0,0,1,160,32ZM32,160H72V96H32ZM144,48.35742,88,91.91211v72.17578l56,43.55469ZM184.94043,99.7168a7.99914,7.99914,0,0,0,.001,11.31347,23.99835,23.99835,0,0,1,0,33.93946,7.99983,7.99983,0,1,0,11.3125,11.31445,39.99722,39.99722,0,0,0,0-56.56836A7.99827,7.99827,0,0,0,184.94043,99.7168Z");
                }
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Right)
            {
                nextButton(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Left)
            {
                prevButton(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Up)
            {
                volumeSlider.Value += 0.05;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Down)
            {
                volumeSlider.Value -= 0.05;
            }
            if (IsFullscreen && e.Key == Key.Escape)
            {
                FullscreenToggle();
            }
        }

        bool showlistCurrent = false;
        private void listCurrentClick(object sender, RoutedEventArgs e)
        {
            if (!showlistCurrent)
            {
                navframe2.Visibility = Visibility.Visible;
                ListCurrentPlaying listCurrentPlaying = new ListCurrentPlaying(listSong,player);
                navframe2!.NavigationService.Navigate(listCurrentPlaying);
                listCurrentPlaying.SongListChanged += ListCurrentPlaying_SongListChanged;
                showlistCurrent = true;
            }
            else
            {
                navframe2.Visibility = Visibility.Hidden;
                showlistCurrent = false;
            }


        }

        private void ListCurrentPlaying_SongListChanged(ListSong newValue)
        {

            listSong = (ListSong)newValue.Clone();
            CurrentPlaying = listSong.listSongs[listSong.currentIndex];


            playMediaFile();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            navframe2.Visibility = Visibility.Hidden;
            showlistCurrent = false;

            volumeSlider.Visibility = Visibility.Hidden;
            volume = false;



        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayShuffle == false)
            {
                ShuffleButton_Border.Background = (Brush)new BrushConverter().ConvertFrom("#a1e7f0");
                isPlayShuffle = true;
                if (isPlayRePeat)
                {
                    RepeatButton_Border.Background = Brushes.Transparent;
                    isPlayRePeat = false;
                }
            }
            else
            {
                ShuffleButton_Border.Background = Brushes.Transparent;
                isPlayShuffle = false;

            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayRePeat == false)
            {
                RepeatButton_Border.Background = (Brush)new BrushConverter().ConvertFrom("#a1e7f0");
                isPlayRePeat = true;
                if (isPlayShuffle)
                {
                    ShuffleButton_Border.Background = Brushes.Transparent;
                    isPlayShuffle = false;
                }
            }
            else
            {
                RepeatButton_Border.Background = Brushes.Transparent;
                isPlayRePeat = false;
            }
        }

        //bool _isFullScreen = false;

        //private void FullscreenBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_isFullScreen)
        //    {
        //        player.Width = 1820;
        //        player.Height = 700;

        //        _isFullScreen = true;
        //    }
        //    else
        //    {
        //        player.Width = 1850;
        //        player.Height = 1850;
        //        _isFullScreen = false;
        //    }

        //}
        // You should use the MediaElement.IsFullWindow property instead
        // to enable and disable full window rendering.
        private bool _isFullscreenToggle = false;
        public bool IsFullscreen
        {
            get { return _isFullscreenToggle; }
            set { _isFullscreenToggle = value; }
        }

        private Size _previousVideoContainerSize = new Size();

        private void FullscreenToggle()
        {
            this.IsFullscreen = !this.IsFullscreen;

            if (this.IsFullscreen)
            {
                //TransportControlsPanel.Visibility = Visibility.Collapsed;

                _previousVideoContainerSize.Width = videoContainer.ActualWidth;
                _previousVideoContainerSize.Height = videoContainer.ActualHeight;

                var width = 700;
                var height = 480;

                videoContainer.Width = width;
                videoContainer.Height = height;
                player.Width = width;
                player.Height = height;
            }
            else
            {
                videoContainer.Width = _previousVideoContainerSize.Width;
                videoContainer.Height = _previousVideoContainerSize.Height;
                player.Width = _previousVideoContainerSize.Width;
                player.Height = _previousVideoContainerSize.Height;
            }
        }

        private void FullscreenBtn_Click(object sender, RoutedEventArgs e)
        {
            FullscreenToggle();
        }

        //private void VideoContainer_KeyUp(object sender, KeyRoutedEventArgs e)
        //{
        //    if (IsFullscreen && e.Key == Windows.System.VirtualKey.Escape)
        //    {
        //        FullscreenToggle();
        //    }

        //    e.Handled = true;
        //}
    }
}
