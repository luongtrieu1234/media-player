using Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace MediaPlayer.Pages
{
    /// <summary>
    /// Interaction logic for RecentlyPlayed.xaml
    /// </summary>
    public partial class RecentlyPlayed : Page
    {
        public RecentlyPlayed(BindingList<ISong> _recentlyList, ListSong _listSong)
        {
            InitializeComponent();
            recentlyList = _recentlyList;
            DataContext = recentlyList;
            listSong = _listSong;   
        }
        BindingList<ISong> recentlyList = new();
        ListSong listSong = new();

        public delegate void SongListValueChangeHandler(ListSong newValue);
        public event SongListValueChangeHandler SongListChanged;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = recentlyList;
            if (listSong.listSongs == null)
            {
                listSong = new ListSong()
                {

                    listSongs = new BindingList<ISong>(),
                    currentIndex = 0,
                };
            }
        }

      

        private void play_recently(object sender, RoutedEventArgs e)
        {
            if (recentlyList.Count == 0)
                return;

            listSong.listSongs.Clear();
            foreach (var song in recentlyList)
            {
                listSong.listSongs.Add(song);
            }
            listSong.currentIndex = 0;
            SongListChanged?.Invoke(listSong);
        }

        private void select_recently(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedIndex == -1)
                return;

            listSong.listSongs.Clear();

            listSong.listSongs.Add((ISong)dataGrid.SelectedItem);
            listSong.currentIndex = 0;
            SongListChanged?.Invoke(listSong);
        }
    }
}
