using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace RoutineMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WMPLib.WindowsMediaPlayer wplayer;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;

        private int SegmentIDSeed = 0;
        private int EventIDSeed = 0;

        public ObservableCollection<Segment> Segments = new ObservableCollection<Segment>();
        public ObservableCollection<Event> Events = new ObservableCollection<Event>(); 

        private bool draggingTrack = false;

        public MainWindow()
        {
            InitializeComponent();

            Segments.Add(new Segment { ID = SegmentIDSeed, Name = "test" });
            SegmentIDSeed++;

            SegmentsBox.ItemsSource = Segments;

            wplayer = new WMPLib.WindowsMediaPlayer();
            wplayer.StatusChange += WplayerOnStatusChange;

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        private void WplayerOnStatusChange()
        {
            TrackSlider.Maximum = wplayer.currentMedia.duration;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (draggingTrack)
            {
                return;
            }

            if (wplayer.currentMedia != null && wplayer.status.StartsWith("Play"))
            {
                TrackSlider.Maximum = wplayer.currentMedia.duration;
                TrackSlider.Value = wplayer.controls.currentPosition;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            wplayer.controls.currentPosition = TrackSlider.Value;
            wplayer.URL = @"C:\Users\kscalf\Music\Electronic\The Chemical Brothers - Go.mp3";
            wplayer.controls.play();
        }

        private void TrackSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            draggingTrack = false;
            if (wplayer.currentMedia != null)
            {
                wplayer.controls.currentPosition = TrackSlider.Value;
            }
        }

        private void TrackSlider_DragEnter(object sender, DragEventArgs e)
        {
            draggingTrack = true;
        }

        private void AddSegmentButton_Click(object sender, RoutedEventArgs e)
        {
            var newSegment = new Segment {ID = SegmentIDSeed, Name="NewSegment"};
            SegmentIDSeed++;
            Segments.Add(newSegment);
            SegmentsBox.ItemsSource = Segments;
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NameTB_KeyUp(object sender, KeyEventArgs e)
        {
            var selected = (Segment) (SegmentsBox.SelectedItem);
            var box = (TextBox) sender;
            selected.Name = box.Text;
        }

        private void SegmentsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindSegmentData((Segment) SegmentsBox.SelectedItem);
        }

        private void BindSegmentData(Segment segment)
        {
            NameTB.Text = segment.Name;
            DurationTB.Text = segment.Duration.ToString();
        }

        private void DurationTB_KeyUp(object sender, KeyEventArgs e)
        {
            var selected = (Segment)(SegmentsBox.SelectedItem);
            var box = (TextBox)sender;
            selected.Duration = Convert.ToDouble(box.Text);
        }

        private void MusicFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                MusicFileText.Text = openFileDialog.FileName;
                var currentSegment = (Segment) SegmentsBox.SelectedItem;
                currentSegment.MusicEvent = new Event
                {
                    ActionParams = openFileDialog.FileName,
                    ActionType = ActionType.Music,
                    Time = 0.0
                };
                wplayer.URL = openFileDialog.FileName;
            }
        }
    }
}
