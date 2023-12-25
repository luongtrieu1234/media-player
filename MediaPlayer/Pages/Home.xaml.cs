using System;
using System.Collections.Generic;
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

namespace MediaPlayer.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    
    

    public partial class Home : Page
    { 
        public string currentPlaying { get; set; }


        public Home(string _currentPlaying)
        {
            InitializeComponent();
            currentPlaying = _currentPlaying;
            DataContext = currentPlaying;

        }
        public void InitPlayer()
        {
            player.Source = new Uri(currentPlaying, UriKind.Absolute);
            player.Play();
            player.Stop();

        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {

                
            
            
        }
        
        public void play()
        {
            player.Source = new Uri(currentPlaying, UriKind.Absolute);
            player.Play();
       
        }

        public void pause()
        {
            player.Pause();
        }
    }
}
