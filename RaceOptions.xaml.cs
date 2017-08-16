using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RoutineMaker
{
    /// <summary>
    /// Interaction logic for RaceOptions.xaml
    /// </summary>
    public partial class RaceOptions : Window
    {
        public Event RaceEvent { get; set; }
        public RaceOptions(double? startTime)
        {
            InitializeComponent();

            RaceEvent = new Event { ActionType = ActionType.Race };

            if (startTime != null)
            {
                RaceStartTimeText.Text = startTime.ToString();
                RaceDurationText.Text = "10";//Default 10.. whatever
            }
        }

        private void RaceSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            double timeTry;
            if (double.TryParse(RaceStartTimeText.Text, out timeTry))
            {
                RaceEvent.Time = timeTry;
            } else
            {
                MessageBox.Show("Not a valid Time Value");
                return;
            }

            int durationTry;
            if (int.TryParse(RaceDurationText.Text, out durationTry))
            {
                RaceEvent.Duration = durationTry;
                RaceEvent.ActionParams = durationTry.ToString();
            }
            else
            {
                MessageBox.Show("Not a valid Duration Value");
                return;
            }

            this.DialogResult = true;
            this.Hide();
        }
    }
}
