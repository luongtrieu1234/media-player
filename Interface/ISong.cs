using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Interface
{
    public class ISong : INotifyPropertyChanged, ICloneable

    {
        public string title { get; set; }

        public string singer { get; set; }

        public string image { get; set; }

        public string time { get; set; }

        public string path { get; set; }

        public double currentTime { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;


        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class IPlaylist : INotifyPropertyChanged, ICloneable
    {  

        public string name { get; set; }

        public ObservableCollection<ISong> listSongs { get; set; }

        public string date { get; set; }

        //public History history { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            return MemberwiseClone();

        }

    }

    public class ListSong : INotifyPropertyChanged, ICloneable
    {


        public BindingList<ISong> listSongs { get; set; }
        public int currentIndex { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            return MemberwiseClone();

        }

    }

    public class History:INotifyPropertyChanged, ICloneable
    {
        public ISong song { get; set; }

        public double time { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            return MemberwiseClone();

        }
    }
}
