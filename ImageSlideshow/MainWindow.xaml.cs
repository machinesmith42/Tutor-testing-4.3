using ImageSlideshow.TutorDataSetTableAdapters;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MsoTriState = Microsoft.Office.Core.MsoTriState;

namespace ImageSlideshow {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly DispatcherTimer timerImageChange;
        private readonly DispatcherTimer clockUpdate;
        private readonly Image[] ImageControls;
        public static List<ImageSource> Images = new List<ImageSource>();
        private static readonly string[] ValidImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        private static readonly string[] TransitionEffects = new[] { "Fade" };
        private string TransitionType;
        private readonly string strImagePath = "";
        public static int CurrentSourceIndex;
        private int CurrentCtrlIndex;
        private readonly int EffectIndex = 0;
        private readonly int IntervalTimer = 10;
        private static readonly Microsoft.Office.Interop.PowerPoint.Application application = new Microsoft.Office.Interop.PowerPoint.Application();
        private static readonly Presentations ppPresens = application.Presentations;
        private static readonly Presentation objPres = ppPresens.Open(AppDomain.CurrentDomain.BaseDirectory + "\\better powerpoint test v2.pptm", MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoTrue);
        private static readonly Slides objSlides = objPres.Slides;
        private static readonly TutorDataSet.AllTutorsDataTable tutorTable = new TutorDataSet.AllTutorsDataTable();
        private static readonly TutorDataSet.ScheduleDataTable scheduleTable = new TutorDataSet.ScheduleDataTable();
        private static readonly TutorDataSet.SubjectDataTable subjectTable = new TutorDataSet.SubjectDataTable();

        private const int tutorsSlide = 1;


        public MainWindow() {
            InitializeComponent();
            AllTutorsTableAdapter tutorTableAdapt = new AllTutorsTableAdapter();
            tutorTableAdapt.Fill(tutorTable);
            ScheduleTableAdapter scheduleAdapt = new ScheduleTableAdapter();
            scheduleAdapt.Fill(scheduleTable);
            SubjectTableAdapter subjectAdapt = new SubjectTableAdapter();
            subjectAdapt.Fill(subjectTable);
            tutorTableAdapt.Dispose();
            scheduleAdapt.Dispose();
            subjectAdapt.Dispose();
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Images");
            foreach (FileInfo file in dir.EnumerateFiles()) {
                file.Delete();
            }
            for (int i = 4; i < objSlides.Count; i++) {
                objSlides[i].Export(AppDomain.CurrentDomain.BaseDirectory + "\\Images\\" + (i).ToString("D2", CultureInfo.CurrentCulture) + ".jpg", "JPG");

            }

            //Initialize Image control, Image directory path and Image timer.
            IntervalTimer = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalTime"], CultureInfo.CurrentCulture);
            strImagePath = ConfigurationManager.AppSettings["ImagePath"];
            ImageControls = new[] { myImage, myImage2 };

            //LoadImageFolder(strImagePath);

            timerImageChange = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, IntervalTimer)
            };
            timerImageChange.Tick += new EventHandler(TimerImageChange_Tick);
            clockUpdate = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 1)
            };
            clockUpdate.Tick += new EventHandler(ClockUpdate_Tick);
        }

        private void ClockUpdate_Tick(object sender, EventArgs e) {
            DateTime d;

            d = DateTime.Now;

            clock.Content = d.ToString("h:mm:ss tt", CultureInfo.CurrentCulture);
            date.Content = d.ToString("dddd, \nMMMM dd, yyyy", CultureInfo.CurrentCulture);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LoadImageFolder(strImagePath);
            if (Images.Count == 0)
                return;
            CurrentSourceIndex = Images.Count - 1;
            PlaySlideShow();
            timerImageChange.IsEnabled = true;
            clockUpdate.IsEnabled = true;
        }

        private void LoadImageFolder(string folder) {
            ErrorText.Visibility = Visibility.Collapsed;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!Path.IsPathRooted(folder))
                folder = Path.Combine(Environment.CurrentDirectory, folder);
            if (!Directory.Exists(folder)) {
                ErrorText.Text = "The specified folder does not exist: " + Environment.NewLine + folder;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            var sources = from file in new DirectoryInfo(folder).GetFiles().AsParallel()
                          where ValidImageExtensions.Contains(file.Extension, StringComparer.InvariantCultureIgnoreCase)
                          orderby file.Name
                          select CreateImageSource(file.FullName, true);
            Images.Clear();
            Images.AddRange(sources);
            sw.Stop();

        }

        private static ImageSource CreateImageSource(string file, bool forcePreLoad) {
            if (forcePreLoad) {
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(file, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                return src;
            } else {
                var src = new BitmapImage(new Uri(file, UriKind.Absolute));
                src.Freeze();
                return src;
            }
        }

        private void TimerImageChange_Tick(object sender, EventArgs e) {
            PlaySlideShow();
            if (CurrentSourceIndex == 1) {
                Refresh();
            }
        }

        private void PlaySlideShow() {

            LoadImageFolder(strImagePath);
            if (Images.Count == 0)
                return;
            var oldCtrlIndex = CurrentCtrlIndex;
            CurrentCtrlIndex = (CurrentCtrlIndex + 1) % 2;
            CurrentSourceIndex = (CurrentSourceIndex + 1) % Images.Count;

            Image imgFadeOut = ImageControls[oldCtrlIndex];
            Image imgFadeIn = ImageControls[CurrentCtrlIndex];
            ImageSource newSource = Images[CurrentSourceIndex];
            imgFadeIn.Source = newSource;

            TransitionType = TransitionEffects[EffectIndex].ToString(CultureInfo.CurrentCulture);

            Storyboard StboardFadeOut = (Resources[string.Format(CultureInfo.CurrentCulture, "{0}Out", TransitionType.ToString(CultureInfo.CurrentCulture))] as Storyboard).Clone();
            StboardFadeOut.Begin(imgFadeOut);
            Storyboard StboardFadeIn = Resources[string.Format(CultureInfo.CurrentCulture, "{0}In", TransitionType.ToString(CultureInfo.CurrentCulture))] as Storyboard;
            StboardFadeIn.Begin(imgFadeIn);

        }
        static void Refresh() {
            DeleteSlides();
            DisplayTutors();
        }
        static void DisplayTutors() {
            DateTime currentDayTime = DateTime.Now;
            var query =
                from tutor in tutorTable.AsEnumerable()
                join schedule in scheduleTable
                on tutor.Field<int>("ID") equals schedule.Field<int>("ID")
                where schedule.Field<int>("Day") == (int)currentDayTime.DayOfWeek + 1 &&
                schedule.Field<DateTime>("Start").TimeOfDay <= currentDayTime.TimeOfDay &&
                schedule.Field<DateTime>("End").TimeOfDay >= currentDayTime.TimeOfDay
                select new {
                    TutorID = tutor.Field<int>("ID"),
                    Name = tutor.Field<string>("FirstName") + " " + tutor.Field<string>("LastName")
                };
            int i = 23;
            foreach (var q in query) {
                SlideRange slide = CreateSlide(tutorsSlide);
                WriteToTextbox(slide, "TutorName", q.Name);
                GetSubject(q.TutorID, slide);
                slide.Export(AppDomain.CurrentDomain.BaseDirectory + "\\Images\\" + i.ToString(CultureInfo.CurrentCulture) + ".jpg", "JPG");
                i++;
            }
        }
        static void GetSubject(int ID, SlideRange slide) {
            var query =
                from sub in subjectTable.AsEnumerable()
                where sub.Field<int>("ID") == ID
                select new {
                    subject = sub.Field<string>("TutorSubject")
                };
            string subjects = "";
            foreach (var q in query) {
                subjects += q.subject + "\n";
            }
            WriteToTextbox(slide, "SubjectsTutored", subjects);
        }
        static SlideRange CreateSlide(int copyOfIndex) {
            SlideRange newSlide = objSlides[copyOfIndex].Duplicate();
            newSlide.Tags.Add("isCreated", "true");
            newSlide.MoveTo(objSlides.Count);
            return newSlide;
        }
        static string WriteToTextbox(SlideRange slide, string textboxName, string inputString) {
            slide.Shapes[textboxName].TextFrame.TextRange.Text = inputString;
            return inputString;
        }
        static int DeleteSlides() {
            int i = 23;
            while (objSlides[objSlides.Count].Tags["isCreated"] == "true") {
                objSlides[objSlides.Count].Delete();
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\Images\\" + i.ToString(CultureInfo.CurrentCulture) + ".jpg");
                i++;
            }
            return i;
        }

    }
}
