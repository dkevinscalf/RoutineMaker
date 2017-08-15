using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutineMaker
{
    public class Segment : INotifyPropertyChanged
    {
        public Segment()
        {
            Events = new List<Event>();
        }

        public int ID { get; set; }

        public double Duration { get; set; }

        public List<Event> Events { get; set; }

        public Event MusicEvent { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged("Name"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
