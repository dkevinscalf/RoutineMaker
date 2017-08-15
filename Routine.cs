using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutineMaker
{
    public class Routine
    {
        public ObservableCollection<Segment> Segments { get; set; }

        public Routine DeepCopy()
        {
            var newRoutine = new Routine {Segments = new ObservableCollection<Segment>()};

            foreach (var segment in Segments)
            {
                newRoutine.Segments.Add(segment.DeepCopy());
            }

            return newRoutine;
        }
    }
}
