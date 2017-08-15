using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RoutineMaker
{
    public class Segment : INotifyPropertyChanged
    {
        public Segment()
        {
            Events = new ObservableCollection<Event>();
        }

        public int ID { get; set; }

        public double GlobalStartTime { get; set; }

        public double Duration { get; set; }

        public ObservableCollection<Event> Events { get; set; }

        [ScriptIgnore]
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

        public Segment DeepCopy()
        {
            var newSegment = new Segment {Name = this.Name, ID = this.ID, Duration = this.Duration, GlobalStartTime = this.GlobalStartTime, MusicEvent = this.MusicEvent.DeepCopy()};

            foreach (var eEvent in Events)
            {
                newSegment.Events.Add(eEvent.DeepCopy());
            }

            return newSegment;
        }
    }
}
