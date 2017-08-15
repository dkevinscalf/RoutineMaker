using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RoutineMaker
{
    public class Event : INotifyPropertyChanged
    {
        private double _time;
        public double Time
        {
            get { return _time; }
            set { _time = value; NotifyPropertyChanged("Time"); }
        }

        [ScriptIgnore]
        public double DisplayTime
        {
            get { return Math.Round(_time); }
        }

        private ActionType _actionType;
        public ActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; NotifyPropertyChanged("ActionType"); }
        }

        private string _actionParams;
        public string ActionParams
        {
            get { return _actionParams; }
            set { _actionParams = value; NotifyPropertyChanged("ActionParams"); }
        }

        [ScriptIgnore]
        public int Duration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Event DeepCopy()
        {
            var newEvent = new Event
            {
                ActionParams = this.ActionParams,
                Duration = this.Duration,
                Time = this.Time,
                ActionType = this.ActionType
            };

            return newEvent;
        }
    }

    public enum ActionType
    {
        Activity = 1,
        Music = 2, 
        Image = 3,
        Text = 4
    }
}
