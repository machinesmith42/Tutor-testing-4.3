using System;
using System.Collections.Generic;
using System.Data;
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
using ImageSlideshow;
using ImageSlideshow.TutorDataSetTableAdapters;
using System.Diagnostics;
using System.Globalization;

namespace ImageSlideshow.Views {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    
    public partial class Form : Window {
        private static readonly TutorDataSet.AllTutorsDataTable tutorTable = new TutorDataSet.AllTutorsDataTable();
        private static readonly TutorDataSet.ScheduleDataTable scheduleTable = new TutorDataSet.ScheduleDataTable();
        private static readonly TutorDataSet.SubjectDataTable subjectTable = new TutorDataSet.SubjectDataTable();
        
        public Form() {
            AllTutorsTableAdapter tutorTableAdapt = new AllTutorsTableAdapter();
            tutorTableAdapt.Fill(tutorTable);
            ScheduleTableAdapter scheduleAdapt = new ScheduleTableAdapter();
            scheduleAdapt.Fill(scheduleTable);
            SubjectTableAdapter subjectAdapt = new SubjectTableAdapter();
            subjectAdapt.Fill(subjectTable);
            tutorTableAdapt.Dispose();
            scheduleAdapt.Dispose();
            subjectAdapt.Dispose();
            InitializeComponent();
            Topmost = true;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Form_Load(object sender, RoutedEventArgs e) {
            subjects.Items.Add(new ComboBoxItem<string>("All", "all"));
            var query = (
                from subject in subjectTable.AsEnumerable()
                select new {
                    Name = subject.Field<string>("TutorSubject"),
                    Value = subject.Field<string>("TutorSubject")
                }
            ).Distinct();
            foreach(var q in query) {
                subjects.Items.Add(new ComboBoxItem<string>(q.Name, q.Value));
            }
            weekdays.Items.Add(new ComboBoxItem<int>("Sunday", 1));
            weekdays.Items.Add(new ComboBoxItem<int>("Monday", 2));
            weekdays.Items.Add(new ComboBoxItem<int>("Tuesday", 3));
            weekdays.Items.Add(new ComboBoxItem<int>("Wednesday", 4));
            weekdays.Items.Add(new ComboBoxItem<int>("Thursday", 5));
            weekdays.Items.Add(new ComboBoxItem<int>("Friday", 6));
            weekdays.Items.Add(new ComboBoxItem<int>("Saturday", 1));
        }
        private void Submit_Click(object sender, RoutedEventArgs e) {
            List<ListItem> andoverTutors = new List<ListItem>();
            List<ListItem> eldoradoTutors = new List<ListItem>();
            List<ListItem> onlineTutors = new List<ListItem>();
            List<ListItem> allTutors = new List<ListItem>();
            bool isCampusChecked = false;
            bool isSubjectSelected = false;
            bool isWeekdaySelected = false;
            bool isTimeSelected = false;
            string[] campus = new string[3]; 
            if ((bool)andoverCheck.IsChecked) {
              campus[0] = "Andover";
                isCampusChecked = true;
            }
            if ((bool)eldoradoCheck.IsChecked) {
                campus[1] = "El Dorado";
                isCampusChecked = true;
            }
            if ((bool)onlineCheck.IsChecked) {
                campus[2] = "Online";
                isCampusChecked = true;
            }
            if (subjects.SelectedItems.Count > 0) {
                isSubjectSelected = true;
            }
            if (weekdays.SelectedItems.Count > 0) {
                isWeekdaySelected = true;
            }
            DateTime datetime = new DateTime();
            if(time.Value.HasValue) {
                isTimeSelected = true;
                datetime = time.Value.GetValueOrDefault();
            }
            var query =
                from schedule in scheduleTable.AsEnumerable()
                join subject in subjectTable
                on schedule.Field<int>("ID") equals subject.Field<int>("ID")
                
                where (campus.Contains(schedule.Field<string>("campus")) && isCampusChecked || !isCampusChecked) && 
                      (subjects.SelectedValue.Split(',').ToList().Contains(subject.Field<string>("TutorSubject")) && isSubjectSelected || !isSubjectSelected) &&
                      (weekdays.SelectedValue.Split(',').ToList().Contains(schedule.Field<int>("Day").ToString()) && isWeekdaySelected || !isWeekdaySelected) &&
                      !isTimeSelected || (TimeBetween(datetime, schedule.Field<DateTime>("Start").TimeOfDay, schedule.Field<DateTime>("End").TimeOfDay) && isTimeSelected)
                orderby schedule.Field<int>("ID")
                select new {
                    TutorID = schedule.Field<int>("ID"),
                    
                };
            
            foreach(var q in query) {
                allTutors.Add(new ListItem() { Name = GetName(q.TutorID), Subjects = GetSubject(q.TutorID), Times = GetTimes(q.TutorID) });
            }
            foreach(ListItem Tutor in allTutors) {
                List<Time> Times = Tutor.Times.ToList();
                foreach(Time time in Times) {
                    if(time.Campus == "Andover") {
                        andoverTutors.Add(Tutor);
                    }
                    if (time.Campus == "El Dorado") {
                        eldoradoTutors.Add(Tutor);
                    }
                    if (time.Campus == "Online") {
                        onlineTutors.Add(Tutor);
                    }
                }
            }
            if (andoverTutors.Any()) {
                andover.ItemsSource = andoverTutors;
            }
            if (eldoradoTutors.Any()) {
                eldorado.ItemsSource = eldoradoTutors;
            }
            if (onlineTutors.Any()) {
                online.ItemsSource = onlineTutors;
            }


        }
        static string GetName(int ID) {
            var query = 
            from tutor in tutorTable.AsEnumerable()
                where tutor.Field<int>("ID") == ID
                select new {
                    Name = tutor.Field<string>("FirstName") + " " + tutor.Field<string>("LastName")
                };
            return query.First().Name;

        }
        static List<Subject> GetSubject(int ID) {
            List<Subject> subjects = new List<Subject>();
            var query =
                from sub in subjectTable.AsEnumerable()
                where sub.Field<int>("ID") == ID
                select new {
                    subject = sub.Field<string>("TutorSubject")
                };
            
            foreach (var q in query) {
                subjects.Add(new Subject() { Subjects = q.subject });
                
            }
            return subjects;
        }
        static List<Time> GetTimes(int ID) {
            List<Time> time = new List<Time>(); 
            var query =
                from times in scheduleTable.AsEnumerable()
                where times.Field<int>("ID") == ID
                orderby times.Field<int>("Day"), times.Field<DateTime>("Start")
                select new {
                    dayName = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[times.Field<int>("Day") - 1],
                    start =  times.Field<DateTime>("Start"),
                    end = times.Field<DateTime>("End"),
                    campus = times.Field<string>("Campus")
                };

            foreach (var q in query) {
                time.Add(new Time() { Start = q.start, End = q.end, DayName = q.dayName, Campus = q.campus });
            }
            return time;
            
        }
        static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end) {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
    }
    public class ComboBoxItem<T> {
        private string Text { get; set; }
        public T Value { get; set; }

        public override string ToString() {
            return Text;
        }

        public ComboBoxItem(string text, T value) {
            Text = text;
            Value = value;
        }
    }
     public class Time {
        

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Campus { get; set;}
        public string DayName { get; set;}

        

    }
    public class Subject {
        public string Subjects { get; set; }
    }
    public class ListItem {
        public List<Time> Times{ get; set; }
        public List<Subject> Subjects { get; set; }
        public string Name { get; set; }
        
    }


}
