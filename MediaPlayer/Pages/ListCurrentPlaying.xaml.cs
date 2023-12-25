using Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MediaPlayer.Pages
{
    /// <summary>
    /// Interaction logic for ListCurrentPlaying.xaml
    /// </summary>
    public partial class ListCurrentPlaying : Page
    {   

        public ListCurrentPlaying(ListSong _listSong,MediaElement _player)
        {
            InitializeComponent();
            listSong =(ListSong) _listSong.Clone();
            DataContext = listSong;
            player = _player;
        }
        public delegate void SongListValueChangeHandler(ListSong newValue);
        public event SongListValueChangeHandler SongListChanged;

        MediaElement player = new MediaElement();
        ListSong listSong = new ListSong();
        BindingList <ISong> songs = new BindingList <ISong>();

        private void page_loaded(object sender, RoutedEventArgs e)
        {
            
            currentPlayingListView.ItemsSource = listSong.listSongs;
        }
        private void select_Playlists(object sender, SelectionChangedEventArgs e)
        {
        }
        BindingList<ISong> search = new();
        private void searchChanged(object sender, TextChangedEventArgs e)
        {
            if (nameMedia.Text.Length == 0)
            {
                labelHintName.Visibility = Visibility;
                currentPlayingListView.ItemsSource = listSong.listSongs;
            }
            else
            {
                search.Clear();
                labelHintName.Visibility = Visibility.Hidden;
                string name = nameMedia.Text;
                foreach(var song in listSong.listSongs)
                {
                    if (song.title.ToLower().Contains(name.ToLower()))
                    {
                        search.Add(song);
                    }
                }
                currentPlayingListView.ItemsSource = search;
            }

        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlayingListView.SelectedIndex < 0 || currentPlayingListView.SelectedIndex == listSong.currentIndex)
                return;
            
            listSong.listSongs[listSong.currentIndex].currentTime = player.Position.TotalSeconds;
            int index = listSong.listSongs.IndexOf(listSong.listSongs[listSong.currentIndex]);
            if (index > currentPlayingListView.SelectedIndex)
                listSong.currentIndex = index - 1;
            listSong.listSongs.Remove((ISong) currentPlayingListView.SelectedItem);

            SongListChanged?.Invoke(listSong);
        }


        private void select_item(object sender, MouseButtonEventArgs e)
        {
            if (currentPlayingListView.SelectedItems.Count == 0)
                return;
            listSong.currentIndex = listSong.listSongs.IndexOf((ISong)currentPlayingListView.SelectedItem);

            SongListChanged?.Invoke(listSong);
        }
    }
}
