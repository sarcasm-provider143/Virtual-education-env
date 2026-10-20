using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Teacher.TeacherPages
{
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
            LoadCourses();
        }

        private void LoadCourses()
        {
            var user = App.CurrentUser;
            if (user == null) return;

            var teacher = App.Context.KURS_teacher
                .FirstOrDefault(t => t.user_id == user.user_id);

            if (teacher == null) return;

            var courses = App.Context.KURS_course
                .Where(c => c.teacher_id == teacher.id)
                .Select(c => new { id = c.id, title = c.title })
                .OrderBy(c => c.title)
                .ToList();

            CourseComboBox.DisplayMemberPath = "title";
            CourseComboBox.SelectedValuePath = "id";
            CourseComboBox.ItemsSource = courses;

            if (courses.Any())
            {
                CourseComboBox.SelectedIndex = 0;
            }
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseComboBox.SelectedValue == null) return;

            int courseId = (int)CourseComboBox.SelectedValue;
            LoadReport(courseId);
        }

        private void LoadReport(int courseId)
        {
            var enrollments = App.Context.KURS_enrollments
                .Where(en => en.course_id == courseId && en.status == "активен")
                .ToList();

            if (!enrollments.Any())
            {
                ReportDataGrid.ItemsSource = null;
                ResetSummary();
                return;
            }

            var report = enrollments
                .Select(en => new
                {
                    student = en.KURS_student.KURS_user.last_name + " " +
                              en.KURS_student.KURS_user.name + " " +
                              en.KURS_student.KURS_user.middle_name,
                    group = en.KURS_student.KURS_group.name,
                    count = en.KURS_results.Count(),
                    avgGrade = en.KURS_results.Any()
                        ? Math.Round(en.KURS_results.Average(r => (double)r.grade), 2)
                        : 0.0,
                    finalGrade = en.KURS_results.Any()
                        ? Math.Round(en.KURS_results.Average(r => (double)r.grade), MidpointRounding.AwayFromZero).ToString("0")
                        : "—"
                })
                .OrderByDescending(r => r.avgGrade)
                .ThenBy(r => r.student)
                .ToList();

            ReportDataGrid.ItemsSource = report;

            // Сводка
            SummaryStudentsValue.Text = report.Count.ToString();

            // Сумма оценок по всем студентам
            int totalGrades = report.Sum(r => r.count);
            SummaryGradesValue.Text = totalGrades.ToString();

            // Средний балл по курсу (среднее из средних баллов студентов)
            var studentAverages = report
                .Where(r => r.count > 0)
                .Select(r => r.avgGrade)
                .ToList();

            if (studentAverages.Any())
            {
                double avgAll = Math.Round(studentAverages.Average(), 2);
                SummaryAvgValue.Text = avgAll.ToString("F2");
            }
            else
            {
                SummaryAvgValue.Text = "—";
            }

            var best = report
                .Where(r => r.count > 0)
                .OrderByDescending(r => r.avgGrade)
                .FirstOrDefault();

            SummaryBestValue.Text = best != null ? best.student : "—";
        }

        private void ResetSummary()
        {
            SummaryStudentsValue.Text = "0";
            SummaryAvgValue.Text = "—";
            SummaryGradesValue.Text = "0";
            SummaryBestValue.Text = "—";
        }
    }
}