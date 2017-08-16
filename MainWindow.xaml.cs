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
using System.Web.Script.Serialization;
using System.Configuration;
using System.IO;

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

        public ObservableCollection<Segment> Segments = new ObservableCollection<Segment>();
        public ObservableCollection<Event> Events = new ObservableCollection<Event>(); 

        private bool draggingTrack = false;

        private string AutoSaveFile = null;
        private DateTime AutoSaveDate = DateTime.Now;
        private TimeSpan AutoSaveInterval = new TimeSpan(0, 1, 0);

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
            if (wplayer.currentMedia != null)
            {
                TrackSlider.Maximum = wplayer.currentMedia.duration;
                var segment = (Segment)(SegmentsBox.SelectedItem);
                if (segment.Duration == 0)
                {
                    var duration = Math.Round(wplayer.currentMedia.duration);
                    DurationTB.Text = duration.ToString();
                    segment.Duration = duration;
                }
            }
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
                
                GetStateByTime(TrackSlider.Value);
            }            

            if(MenuAutoSave.IsChecked && DateTime.Now.Subtract(AutoSaveDate) > AutoSaveInterval && AutoSaveFile != null)
            {
                SaveRoutine(AutoSaveFile);
                AutoSaveDate = DateTime.Now;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (wplayer.currentMedia != null)
            {
                wplayer.controls.currentPosition = TrackSlider.Value;
                wplayer.controls.play();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            wplayer.controls.stop();
        }

        private void TrackSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            draggingTrack = false;
            if (wplayer.currentMedia != null)
            {
                wplayer.controls.currentPosition = TrackSlider.Value;
            }

            GetStateByTime(TrackSlider.Value);
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
            ChangeSegment(newSegment);
        }

        private void ChangeSegment(Segment segment)
        {
            UpdateTotalTime();
        
            if (segment == null)
                return;
            if (segment.MusicEvent != null)
            {
                wplayer.URL = segment.MusicEvent.ActionParams;
                MusicFileText.Text = segment.MusicEvent.ActionParams;
            }
            wplayer.controls.stop();

            

            NameTB.Text = segment.Name;
            DurationTB.Text = segment.Duration.ToString();
            TrackSlider.Value = 0;
            GetStateByTime(0);

            EventsBox.ItemsSource = segment.Events;
        }

        private void UpdateTotalTime()
        {
            var totalTimeInSeconds = 0.0;
            foreach(var segment in Segments)
            {
                totalTimeInSeconds += segment.Duration;
            }

            var totalTimeInMinutes = Math.Floor(totalTimeInSeconds / 60.0);
            TotalTimeTB.Content = string.Format("{0}:{1} Total Time", totalTimeInMinutes, Math.Round(totalTimeInSeconds)%60);
        }

        private void NameTB_KeyUp(object sender, KeyEventArgs e)
        {
            var selected = (Segment) (SegmentsBox.SelectedItem);
            var box = (TextBox) sender;
            selected.Name = box.Text;
        }

        private void SegmentsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeSegment((Segment) SegmentsBox.SelectedItem);
            var segSelected = SegmentsBox.SelectedItem != null;
            

            NameTB.IsEnabled = segSelected;
            DurationTB.IsEnabled = segSelected;
            MusicFileButton.IsEnabled = segSelected;
            MusicFileText.IsEnabled = segSelected;
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
                var relativeFileName = openFileDialog.FileName;
                var currentSegment = (Segment) SegmentsBox.SelectedItem;
                MusicFileText.Text = relativeFileName;
                currentSegment.MusicEvent = new Event
                {
                    ActionParams = relativeFileName,
                    ActionType = ActionType.Music,
                    Time = 0.0
                };
                wplayer.URL = openFileDialog.FileName;
            }
        }

        //private string MakeRelativeToExe(string filePath)
        //{
        //    var fileUri = new Uri(filePath);
        //    var referenceUri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //    return referenceUri.MakeRelativeUri(fileUri).ToString();
        //}

        private string MakeRelativeToParentDirectory(string filePath)
        {
            var parentDir = new System.IO.FileInfo(AutoSaveFile).Directory.ToString() + "\\";
            var relativeFilePath = filePath.Replace(parentDir, "");
            return relativeFilePath.Replace("\\", "/");
        }

        private string MakeAbsoluteFromParentDirectory(string filePath)
        {
            var parentDir = new System.IO.FileInfo(AutoSaveFile).Directory;
            return System.IO.Path.Combine(parentDir.ToString(), filePath.Replace("/", "\\"));
        }

        private void UpsertEvent(Event thisEvent)
        {
            UpsertEvent(thisEvent.Time, thisEvent.ActionType, thisEvent.ActionParams);
        }

        private void UpsertEvent(double time, ActionType actionType, string inputParams)
        {
            if (SegmentsBox.SelectedItem == null)
                return;
            if (wplayer.status.StartsWith("Play"))
            {
                MessageBox.Show("Stop the player before editing, dummy!!");
                return;
            }

            var segment = (Segment)SegmentsBox.SelectedItem;

            var existingEvent = segment.Events.FirstOrDefault(e => e.Time == time && e.ActionType == actionType);

            if(existingEvent == null)
            {
                existingEvent = new Event { Time = time, ActionType = actionType };
                segment.Events.Add(existingEvent);
            }

            switch(actionType)
            {
                case ActionType.Activity:
                    existingEvent.ActionParams = GetEventParamsFromActivityForm();
                    break;
                default:
                    existingEvent.ActionParams = inputParams;
                    break;
            }

            EventsBox.ItemsSource = segment.Events;
       }

        private string GetEventParamsFromActivityForm()
        {
            var RPM = RPMText.Text;
            var Watt = WattText.Text;
            var Standing = (bool)StandCB.IsChecked ? "True" : "False";

            return string.Format("{0},{1},{2}", RPM, Watt, Standing);
        }

        private void RPMText_KeyUp(object sender, KeyEventArgs e)
        {
            UpsertEvent(TrackSlider.Value, ActionType.Activity, null);
        }

        private void WattText_KeyUp(object sender, KeyEventArgs e)
        {            
            UpsertEvent(TrackSlider.Value, ActionType.Activity, null);
        }

        private void StandCB_Click(object sender, RoutedEventArgs e)
        {
            UpsertEvent(TrackSlider.Value, ActionType.Activity, null);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            UpsertEvent(TrackSlider.Value, ActionType.Text, TextTB.Text);
        }

        private void GetStateByTime(double time)
        {
            TrackLabel.Content = Math.Round(TrackSlider.Value).ToString() + " / " + Math.Round(TrackSlider.Maximum).ToString();
            if (SegmentsBox.SelectedItem == null)
            {
                return;
            }

            var segment = (Segment)SegmentsBox.SelectedItem;

            if (segment.Events.Count == 0 || segment.Events.Count(e => e.ActionType == ActionType.Activity) == 0)
                return;

            Event ActivityEvent = null;
            Event TextEvent = null;
            Event ImageEvent = null;

            foreach(var ev in segment.Events.Where(e=>e.Time<= time))
            {
                switch(ev.ActionType)
                {
                    case ActionType.Activity:
                        if(ActivityEvent == null || ActivityEvent.Time < ev.Time)
                        {
                            ActivityEvent = ev;
                        }
                        break;
                    case ActionType.Text:
                        if (TextEvent == null || TextEvent.Time < ev.Time)
                        {
                            TextEvent = ev;
                        }
                        break;
                    case ActionType.Image:
                        if (ImageEvent == null || ImageEvent.Time < ev.Time)
                        {
                            ImageEvent = ev;
                        }
                        break;
                    case ActionType.Music:
                        //do nothing
                        break;
                    default:
                        return;
                }

                if(ActivityEvent!=null)
                {
                    var activityParams = ActivityEvent.ActionParams.Split(',');
                    RPMText.Text = activityParams[0];
                    WattText.Text = activityParams[1];
                    StandCB.IsChecked = activityParams[2] == "True";
                }

                if (TextEvent != null)
                {
                    TextTB.Text = TextEvent.ActionParams;
                }

                if (ImageEvent != null)
                {
                    ImageBox.Source = new BitmapImage(new Uri(ImageEvent.ActionParams, UriKind.Absolute));
                }
            }
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                SaveRoutine(saveFileDialog.FileName);
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                OpenRoutine(openFileDialog.FileName);
            }
        }

        private void SaveRoutine(string fileName)
        {
            AutoSaveFile = fileName;
            CleanUpActivities();
            CopyMedia(fileName);
            var routine = new Routine { Segments = Segments };
            var saveRoutine = routine.DeepCopy();
            saveRoutine = MakePathsRelative(saveRoutine);
            var json = new JavaScriptSerializer().Serialize(saveRoutine);
            System.IO.File.WriteAllText(fileName, json);
            
        }

        private Routine MakePathsRelative(Routine saveRoutine)
        {
            foreach (var segment in saveRoutine.Segments)
            {
                segment.MusicEvent.ActionParams = MakeRelativeToParentDirectory(segment.MusicEvent.ActionParams);
                foreach (var eEvent in segment.Events)
                {
                    if (eEvent.ActionType == ActionType.Image || eEvent.ActionType == ActionType.Music)
                    {
                        eEvent.ActionParams = MakeRelativeToParentDirectory(eEvent.ActionParams);
                    }
                }
            }

            return saveRoutine;
        }

        private Routine MakePathsAbsolute(Routine loadRoutine)
        {
            foreach (var segment in loadRoutine.Segments)
            {
                segment.MusicEvent.ActionParams = MakeAbsoluteFromParentDirectory(segment.MusicEvent.ActionParams);
                foreach (var eEvent in segment.Events)
                {
                    if (eEvent.ActionType == ActionType.Image || eEvent.ActionType == ActionType.Music)
                    {
                        eEvent.ActionParams = MakeAbsoluteFromParentDirectory(eEvent.ActionParams);
                    }
                }
            }

            return loadRoutine;
        }

        private void CopyMedia(string fileName)
        {
            var media = wplayer.currentMedia;
            var time = wplayer.controls.currentPosition;
            var imageSource = ImageBox.Source;
            wplayer.currentPlaylist.clear();
            ImageBox.Source = null;
            var saveDirectory = System.IO.Path.GetDirectoryName(fileName);

            var imgDirectory = System.IO.Path.Combine(saveDirectory, "images");
            if (!Directory.Exists(imgDirectory))
            {
                Directory.CreateDirectory(imgDirectory);
            }
            var relativeImgDirectory = imgDirectory.Replace(saveDirectory, "");
            var musicDirectory = System.IO.Path.Combine(saveDirectory, "music");
            if (!Directory.Exists(musicDirectory))
            {
                Directory.CreateDirectory(musicDirectory);
            }
            var relativeMusicDirectory = musicDirectory.Replace(saveDirectory, "");

            foreach (var segment in Segments)
            {
                foreach (var currEvent in segment.Events)
                {
                    if (currEvent.ActionType == ActionType.Music)
                    {
                        
                        CopyFile(musicDirectory, relativeMusicDirectory, currEvent);
                    }

                    if (currEvent.ActionType == ActionType.Image)
                    {
                        CopyFile(imgDirectory, relativeImgDirectory, currEvent);
                    }
                }
            }

            wplayer.URL = media.sourceURL;
            wplayer.controls.currentPosition = time;
            ImageBox.Source = imageSource;
        }

        private void CopyFile(string dir, string relDir, Event currEvent)
        {
            var oldFileName = currEvent.ActionParams;
            var newFileName = System.IO.Path.Combine(dir, System.IO.Path.GetFileName(oldFileName));
            if (oldFileName.ToUpper() != newFileName.ToUpper())
                File.Copy(oldFileName, newFileName, true);
            currEvent.ActionParams = newFileName;
        }

        private void OpenRoutine(string fileName)
        {
            AutoSaveFile = fileName;
            var json = System.IO.File.ReadAllText(fileName);
            var routine = new JavaScriptSerializer().Deserialize<Routine>(json);
            foreach(var segment in routine.Segments)
            {
                segment.MusicEvent = segment.Events.FirstOrDefault(e => e.ActionType == ActionType.Music);
                if (segment.MusicEvent != null)
                {
                    segment.Events.Remove(segment.MusicEvent);
                }
            }
            routine = MakePathsAbsolute(routine);
            Segments = routine.Segments;
            SegmentsBox.ItemsSource = Segments;
            SegmentsBox.SelectedItem = Segments[0];
            AutoSaveFile = fileName;
        }

        private void CleanUpActivities()
        {
            var globalStartTime = 0.0;
            foreach (var segment in Segments)
            {
                segment.GlobalStartTime = globalStartTime;
                globalStartTime += segment.Duration;
                RemoveMusicEvents(segment);
                segment.Events.Add(segment.MusicEvent);

                segment.Events = SortEvents(segment.Events);

                if (segment.Events.Count(e => e.Time == 0 && e.ActionType == ActionType.Activity) == 0)
                {
                    var firstEvent = (Event)segment.Events.FirstOrDefault(e => e.ActionType == ActionType.Activity);
                    if (firstEvent != null)
                    {
                        firstEvent.Time = 0;
                    }
                }

                int lastActivity = -1;

                for (var i = 0; i < segment.Events.Count; i++)
                {
                    if (segment.Events[i].ActionType != ActionType.Activity)
                        continue;

                    if (lastActivity == -1)
                    {
                        lastActivity = i;
                        continue;
                    }
                    var lastDuration = Math.Floor(segment.Events[i].Time - segment.Events[lastActivity].Time);
                    var ap = segment.Events[lastActivity].ActionParams.Split(',');
                    segment.Events[lastActivity].ActionParams = ap[0] + "," + ap[1] + "," + ap[2] + "," + lastDuration.ToString();
                    lastActivity = i;
                }

                if (lastActivity != -1)
                {
                    var finalDuration = Math.Floor(segment.Duration - segment.Events[lastActivity].Time);
                    var ap = segment.Events[lastActivity].ActionParams.Split(',');
                    segment.Events[lastActivity].ActionParams = ap[0] + "," + ap[1] + "," + ap[2] + "," + finalDuration.ToString();
                }
            }
        }

        private void RemoveMusicEvents(Segment segment)
        {
            for(var i = 0;i<segment.Events.Count; i++)
            {
                if(segment.Events[i].ActionType == ActionType.Music)
                {
                    segment.Events.RemoveAt(i);
                    i--;
                }
            }
        }

        private ObservableCollection<Event> SortEvents(ObservableCollection<Event> events)
        {
            var sortedList = new ObservableCollection<Event>();

            foreach(var sortedEvent in events.OrderBy(o=>o.Time))
            {
                sortedList.Add(sortedEvent);
            }

            return sortedList;
        }

        private void EventsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventsBox.SelectedItem == null)
                return;
            var selectedEvent = (Event)EventsBox.SelectedItem;

            TrackSlider.Value = selectedEvent.Time;
            GetStateByTime(selectedEvent.Time);
        }

        private void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
            var segment = (Segment)SegmentsBox.SelectedItem;
            var selectedEvent = (Event)EventsBox.SelectedItem;
            segment.Events.Remove(selectedEvent);
            ChangeSegment(segment);
        }

        private void DeleteSegmentButton_Click(object sender, RoutedEventArgs e)
        {
            var segment = (Segment)SegmentsBox.SelectedItem;
            Segments.Remove(segment);
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ImageBox.Source = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute));
                UpsertEvent(TrackSlider.Value, ActionType.Image, openFileDialog.FileName);
            }
        }

        private void AddRaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SegmentsBox.SelectedItem != null)
            {
                var raceWindow = new RaceOptions(TrackSlider.Value);
                if(raceWindow.ShowDialog() == true)
                {
                    UpsertEvent(raceWindow.RaceEvent);
                }
            }
        }        
    }
}
