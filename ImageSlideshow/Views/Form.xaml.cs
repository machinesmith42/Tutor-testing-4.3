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
            this.Close();
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
            bool isCampusChecked = false;
            bool isSubjectSelected = false;
            bool isWeekdaySelected = false;
            bool isTimeSelected = false;
            string[] campus = new string[3]; 
            if ((bool)andoverCheck.IsChecked) {
              campus[0] = "Andover";
                isCampusChecked = true;
            }
            if ((bool)eldorado.IsChecked) {
                campus[1] = "El Dorado";
                isCampusChecked = true;
            }
            if ((bool)online.IsChecked) {
                campus[2] = "Online";
                isCampusChecked = true;
            }
            if (subjects.SelectedItems.Count > 0) {
                isSubjectSelected = true;
            }
            if (weekdays.SelectedItems.Count > 0) {
                isWeekdaySelected = true;
            }
            if(time.Value.HasValue) {
                isTimeSelected = true;
            }
            var query =
                from schedule in scheduleTable.AsEnumerable()
                join subject in subjectTable
                on schedule.Field<int>("ID") equals subject.Field<int>("ID")
                where (campus.Contains(schedule.Field<string>("campus")) && isCampusChecked || !isCampusChecked) && 
                      (subjects.SelectedValue.Split(',').ToList().Contains(subject.Field<string>("TutorSubject")) && isSubjectSelected || !isSubjectSelected) &&
                      (weekdays.SelectedValue.Split(',').ToList().Contains(schedule.Field<int>("Day").ToString()) && isWeekdaySelected || !isWeekdaySelected) &&
                      (TimeBetween((DateTime)time.Value, schedule.Field<DateTime>("Start").TimeOfDay, schedule.Field<DateTime>("End").TimeOfDay) && isTimeSelected || !isTimeSelected)
                orderby schedule.Field<int>("ID")
                select new {
                    TutorID = schedule.Field<int>("ID")
                };
            foreach(var q in query) {
                Debug.Print(q.TutorID.ToString()+"\n");
            }
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


}
