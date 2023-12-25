using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Interface;
using Microsoft.Win32;
using Ulti;

using TagLib;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace MediaPlayer.Pages
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : Page
    {

        public Playlist(IPlaylist playlist, ListSong _songList, MediaElement _player  )
        {
            InitializeComponent();
            _playlist = (IPlaylist) playlist.Clone();
            DataContext = _playlist;
            songList = (ListSong) _songList.Clone();
            player = _player;
        }
        public delegate void PlaylistValueChangeHandler(IPlaylist newValue);
        public event PlaylistValueChangeHandler PlaylistChanged;

        public delegate void SongListValueChangeHandler(ListSong newValue);
        public event SongListValueChangeHandler SongListChanged;
        MediaElement player = new MediaElement();
        ListSong songList = new ListSong()
        {

            listSongs = null,
            currentIndex = 0,
        };


        IPlaylist _playlist { get; set; }
        MediaElement PreviewMedia = new MediaElement();
        string _time = "";
        DispatcherTimer _timer;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PreviewMedia.MediaOpened += PreviewMedia_MediaOpened;
            PreviewMedia.LoadedBehavior = MediaState.Manual;
            dataGrid.ItemsSource = _playlist.listSongs;

            if(_playlist.listSongs == null)
            {
                _playlist.listSongs = new ObservableCollection<ISong>();
            }
            if(songList.listSongs == null)
            {
                songList = new ListSong()
                {

                    listSongs = new BindingList<ISong>(),
                    currentIndex = 0,
                };
            }
            
        }

        private void PreviewMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = PreviewMedia.NaturalDuration.TimeSpan.Hours;
            int minutes = PreviewMedia.NaturalDuration.TimeSpan.Minutes;
            int seconds = PreviewMedia.NaturalDuration.TimeSpan.Seconds;
             _time = $"{hours}:{minutes}:{seconds}";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

        }

        public enum TypeFile
        {
            VIDEO,
            AUDIO,
            OTHER
        }

        private TypeFile identifyFileType(String path)
        {
            string[] videoExtensions = {
                                           ".AVI", ".MP4", ".DIVX", ".WMV", //etc  // Video
                                       };

            string[] audioExtensions = {
                                           ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc // Audio
                                       };


            if (Array.IndexOf(videoExtensions, System.IO.Path.GetExtension(path).ToUpperInvariant()) != -1)
            {
                return TypeFile.VIDEO;
            }

            if (Array.IndexOf(audioExtensions, System.IO.Path.GetExtension(path).ToUpperInvariant()) != -1)
            {
                return TypeFile.AUDIO;
            }

            return TypeFile.OTHER;
        }
        private void addMediaFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog screen = new OpenFileDialog();
            screen.Multiselect = true;
            if (screen.ShowDialog() == true)
            {
                string[] paths = screen.FileNames; 

                foreach(var _path in paths)
                {
                    bool isExist = false;
                    foreach (var x in _playlist.listSongs)
                    {
                        if (x.path == _path)
                        {
                            isExist = true; break;
                        }

                    }
                    if (isExist)
                        continue;

                    PreviewMedia.Source = new Uri(_path, UriKind.Absolute);
                    PreviewMedia.Play();
                    PreviewMedia.Stop();
                    ISong song;
                    
                    
                    if (identifyFileType(_path) == TypeFile.VIDEO)
                    {
                        string title = _path.Split('\\')[^1];
                        SaveAudioImg(_path, title);

                        PreviewMedia.MediaOpened += PreviewMedia_MediaOpened;
                        song = new ISong()
                        {
                            title = title.Trim('\0'),
                            singer = null,
                            time = _time, 
                            path = _path,
                            currentTime = 0,
                            image = "/Images/" + title.Trim('\0') + ".png"
                        };
                        _playlist.listSongs.Add(song);
                    }
                    else if(identifyFileType(_path) == TypeFile.AUDIO)
                    {
                       
                        string[] info = GetAudioFileInfo(_path);

                        if (info[0] == null)
                        {
                            string title = _path.Split('\\')[^1];
                            SaveAudioImg(_path, title);

                            song = new ISong()
                            {
                                title = title.Trim('\0'),
                                singer = null,
                                time = _time,
                                path = _path,
                                currentTime = 0,
                                image = "/Images/" + title.Trim('\0') + ".png"
                            };
                        }
                        else
                        {
                            SaveAudioImg(_path, info[0]);
                            song = new ISong()
                            {
                                title = info[0].Trim('\0'),
                                singer = info[1],
                                time = _time,
                                path = _path,
                                currentTime = 0,
                                image = "/Images/" + info[0].Trim('\0') + ".png"
                            };
                        }

                        _playlist.listSongs.Add(song);
                    }
                    else
                    {

                    }

                    
                }
               
                dataGrid.ItemsSource =null;
                dataGrid.ItemsSource = _playlist.listSongs;
                PlaylistChanged?.Invoke(_playlist);
            }
        }
        private void SaveAudioImg (string filename, string title)
        {

            TagLib.File file = TagLib.Mpeg4.File.Create(filename);
            var mStream = new MemoryStream();
            var firstPicture = file.Tag.Pictures.FirstOrDefault();
            if (firstPicture != null)
            {
                byte[] pData = firstPicture.Data.Data;
                mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
                var bm = new Bitmap(mStream, false);
                //mStream.Dispose();
                //mStream= null;
                bm.SetResolution(600, 600);
                //coverPictureBox.Image = bm;
                string path = System.IO.Directory.GetCurrentDirectory();
                //bm.Dispose();
                string rs = path + "\\Images\\" + title.Trim('\0') + ".png";
                bm.Save(rs, ImageFormat.Png);
            }
            else
            {
                //return null;
                // set "no cover" image
            }
        }
        public string[] GetAudioFileInfo(string path)
        {
            path = Uri.UnescapeDataString(path);    

            byte[] b = new byte[128];
            string[] infos = new string[5]; //Title; Singer; Album; Year; Comm;
            bool isSet = false;


            //Read bytes
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                fs.Seek(-128, SeekOrigin.End);
                fs.Read(b, 0, 128);
                //Set flag
                String sFlag = System.Text.Encoding.Default.GetString(b, 0, 3);
                if (sFlag.CompareTo("TAG") == 0) isSet = true;

                if (isSet)
                {
                    infos[0] = System.Text.Encoding.Default.GetString(b, 3, 30); //Title
                    infos[1] = System.Text.Encoding.Default.GetString(b, 33, 30); //Singer
                    //infos[2] = System.Text.Encoding.Default.GetString(b, 63, 30); //Album
                    //infos[3] = System.Text.Encoding.Default.GetString(b, 93, 4); //Year
                    //infos[4] = System.Text.Encoding.Default.GetString(b, 97, 30); //Comm
                }
                fs.Close();
                fs.Dispose();
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }

            return infos;
        }


        private void Button_Play(object sender, RoutedEventArgs e)
        {
           
            if (_playlist.listSongs.Count == 0)
                return;
            songList.listSongs.Clear();
            foreach (ISong song in _playlist.listSongs)
            {
                songList.listSongs.Add((ISong)song.Clone());
            }

            songList.currentIndex = 0;
            SongListChanged?.Invoke(songList);

        }

        private void PlayItemInPlaylist(object sender, MouseButtonEventArgs e)
        {
           
        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            ISong clickedItem = dataGrid.SelectedItem as ISong;
            _playlist.listSongs.Remove(clickedItem);

            PlaylistChanged?.Invoke(_playlist);

        }

        private void DeleteAllItem_Click(object sender, RoutedEventArgs e)
        {
            _playlist.listSongs.Clear();

            PlaylistChanged?.Invoke(_playlist);

        }

        private void play_Click(object sender, RoutedEventArgs e)
        {

            ISong temp = (ISong)dataGrid.SelectedItem;
            if (temp== null)
                return;
            songList.listSongs.Add((ISong) temp.Clone());
        
             
            songList.currentIndex = songList.listSongs.Count -1;
            SongListChanged?.Invoke(songList);
         }

        private void addtoCP_Click(object sender, RoutedEventArgs e)
        {
            ISong temp = (ISong)dataGrid.SelectedItem;
            if (temp == null)
                return;
            songList.listSongs.Add((ISong)temp.Clone());

            songList.listSongs[songList.currentIndex].currentTime = player.Position.TotalSeconds;

            SongListChanged?.Invoke(songList);
        }

        private void Backbutton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    
    }
}
