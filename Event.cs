using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutineMaker
{
    public class Event
    {
        public double Time { get; set; }
        public ActionType ActionType { get; set; }
        public string ActionParams { get; set; }
    }

    public enum ActionType
    {
        Activity = 1,
        Music = 2, 
        Image = 3,
        Text = 4
    }
}
