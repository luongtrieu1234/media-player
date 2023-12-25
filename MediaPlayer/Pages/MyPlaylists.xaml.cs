using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using System.Windows.Forms;
using Interface;
using Microsoft.Win32;
using Ulti;
using System.IO;
using Path = System.IO.Path;
using static MediaPlayer.Pages.Playlist;
using System.Numerics;
using System.Drawing.Imaging;
using System.Drawing;

namespace MediaPlayer.Pages
{
    /// <summary>
    /// Interaction logic for MyPlaylists.xaml
    /// </summary>
    public partial class MyPlaylists : Page
    {
        public MyPlaylists(ObservableCollection<IPlaylist> _myplaylist, ListSong _songList , MediaElement _player)
        {

            InitializeComponent();
            myplaylist =_myplaylist;
            DataContext = myplaylist;

            songList =(ListSong) _songList.Clone();
            player = _player;


        }
        MediaElement player = new MediaElement();
        ObservableCollection<IPlaylist> myplaylist = new();
        ListSong songList = new ListSong();

        public delegate void PlaylistsValueChangeHandler(ObservableCollection<IPlaylist> newValue);
        public event PlaylistsValueChangeHandler PlaylistsChanged;

        public delegate void SongListValueChangeHandler(ListSong newValue);
        public event SongListValueChangeHandler SongListChanged;

        MediaElement PreviewMedia = new MediaElement();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            PLaylistsListView.ItemsSource = myplaylist;

            PreviewMedia.LoadedBehavior = MediaState.Manual;
        }

        private void mouseleftdown(object sender, MouseButtonEventArgs e)
        {
          
        }


      

        private void Value_SongListChanged(ListSong newValue)
        {
            songList = (ListSong)newValue.Clone();
            SongListChanged?.Invoke(songList);
        }


        private void Value_PlaylistChanged(IPlaylist newValue)
        {
            int i = PLaylistsListView.SelectedIndex;
            if (i == -1)
                return;
            myplaylist[i] = (IPlaylist) newValue.Clone();

            PlaylistsChanged?.Invoke(myplaylist);
            

        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {

            var screen = new AddPlaylist(myplaylist);
            if(screen.ShowDialog() == true)
            {
                screen.PlaylistsChanged += Screen_PlaylistsChanged;
            }
            else
            {

            }
        }



        private void Screen_PlaylistsChanged(ObservableCollection<IPlaylist> newValue)
        {
            myplaylist = newValue;
            PlaylistsChanged?.Invoke(myplaylist);
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
        private void uploadPlaylist(object sender, RoutedEventArgs e)
        {
            var Folderdialog = new FolderBrowserDialog();
            var result = Folderdialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(Folderdialog.SelectedPath))
            {

                DirectoryInfo d = new DirectoryInfo(Folderdialog.SelectedPath) ; //Assuming Test is your Folder

                FileInfo[] Files = d.GetFiles();

                ObservableCollection<ISong> listSongs = new();

                foreach (FileInfo file in Files)
                {
                    string _path = file.FullName;

                    bool isExist = false;
                    foreach (var x in listSongs)
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
                        song = new ISong()
                        {
                            title = title.Trim('\0'),
                            singer = null,
                            time = "",
                            path = _path,
                            currentTime = 0,
                            image = "/Images/" + title.Trim('\0') + ".png"
                        };
                        listSongs.Add(song);
                    }
                    else if (identifyFileType(_path) == TypeFile.AUDIO)
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
                                time = "",
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
                                time = "",
                                path = _path,
                                currentTime = 0,
                                image = "/Images/" + info[0].Trim('\0') + ".png"
                            };
                        }
                        listSongs.Add(song);
                    }
                    else
                    {

                    }

                }
                IPlaylist newPlaylist = new IPlaylist()
                {
                    name = Path.GetFileName(Folderdialog.SelectedPath),
                    listSongs = listSongs,
                    date = DateTime.Now.ToString("dd/MM/yyyy"),
                };
                myplaylist.Add(newPlaylist);
                PlaylistsChanged?.Invoke(myplaylist);

            }
        }
        private void SaveAudioImg(string filename, string title)
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
              
            }

            return infos;
        }

        private void savePlaylist(object sender, RoutedEventArgs e)
        {
            int index = PLaylistsListView.SelectedIndex;
            if (index == -1)
                return;
            var Folderdialog = new FolderBrowserDialog();
            var result = Folderdialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(Folderdialog.SelectedPath))
            {

                string newfolder = Path.Combine(Folderdialog.SelectedPath, myplaylist[index].name);
                Directory.CreateDirectory(newfolder);
                foreach(var song in myplaylist[index].listSongs)
                {

                    string filename = song.path.Split('\\')[^1];
                    File.Copy(song.path, Path.Combine(newfolder, filename), true);
                }
            }
        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            int index = PLaylistsListView.SelectedIndex;
            if (index == -1)
                return;
            myplaylist.RemoveAt(index);
            PlaylistsChanged?.Invoke(myplaylist);
        }

        private void double_click(object sender, MouseButtonEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    int i = PLaylistsListView.SelectedIndex;
                    if (i == -1)
                        return;

                    var playlist = myplaylist[i];
                    var value = new Playlist((IPlaylist)playlist, songList, player);
                    value.PlaylistChanged += Value_PlaylistChanged;
                    value.SongListChanged += Value_SongListChanged; ; ;
                    (window as MainWindow).navframe.Navigate(value);
                    return;
                }
            }
        }
    }
}
