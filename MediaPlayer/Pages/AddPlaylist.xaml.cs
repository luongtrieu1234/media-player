using Interface;
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

namespace MediaPlayer.Pages
{
    public partial class AddPlaylist : Window
    {
        public AddPlaylist(ObservableCollection<IPlaylist> _myplaylist)
        {
            InitializeComponent();
            myplaylist = _myplaylist;
            DataContext = _myplaylist;
        }

        ObservableCollection<IPlaylist> myplaylist = new();
        public delegate void PlaylistsValueChangeHandler(ObservableCollection<IPlaylist> newValue);
        public event PlaylistsValueChangeHandler PlaylistsChanged;

        
        private void namePlaylistChanged(object sender, TextChangedEventArgs e)
        {
            if(namePlaylist.Text.Length == 0)
            {
                labelHintName.Visibility = Visibility;
            }
            else
            {
                labelHintName.Visibility = Visibility.Hidden;
            }
        }

        private void createHandler(object sender, RoutedEventArgs e)
        {
            string name = namePlaylist.Text;
            if(name.Length == 0)
            {
                MessageBox.Show("Playlist's name not empty!", "Error");
                return;
            }
            else
            {
                string date = DateTime.Now.ToString("dd/MM/yyyy");

                var newPlaylist = new IPlaylist() { name = name, date = date, listSongs = null };
                myplaylist.Add(newPlaylist);
                PlaylistsChanged?.Invoke(myplaylist);
            }
            DialogResult = true;
        }

        private void cancleHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
