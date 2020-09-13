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
            this.Topmost = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e) {
            bool isCampusChecked = false;
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

            var query =
                from schedule in scheduleTable.AsEnumerable()
                join subject in subjectTable
                on schedule.Field<int>("ID") equals subject.Field<int>("ID")
                let v = (campus.Contains(schedule.Field<string>("campus")))
                where (v && isCampusChecked || !isCampusChecked)
                orderby schedule.Field<int>("ID")
                select new {
                    TutorID = schedule.Field<int>("ID")
                };
            foreach(var q in query) {
                Debug.Print(q.TutorID.ToString()+"\n");
            }
        }
    }
    
}
