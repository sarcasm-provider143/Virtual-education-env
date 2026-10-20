using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.User_windows.Pages
{

    public class CourseItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public partial class GradesPage : Page
    {
        public GradesPage()
        {
            InitializeComponent();
            LoadCourses();
        }
        private void LoadCourses()
        {
            var user = App.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("Сессия истекла, войдите заново.");
                return;
            }

            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.user_id == user.user_id);

            if (student == null)
            {
                MessageBox.Show("Профиль студента не найден.");
                return;
            }

            var courses = App.Context.KURS_enrollments
                .Where(e => e.student_id == student.id
                && (e.status == "активен" || e.status == "завершён"))
                .Select(e => new CourseItem
                {
                    Id = e.KURS_course.id,
                    Title = e.KURS_course.title
                })
                .OrderBy(c => c.Title)
                .ToList();

            GradesCourseComboBox.DisplayMemberPath = "Title";
            GradesCourseComboBox.SelectedValuePath = "Id";
            GradesCourseComboBox.ItemsSource = courses;

            
            GradesCourseComboBox.SelectionChanged += GradesCourseComboBox_SelectionChanged;

            if (courses.Any())
            {
                GradesCourseComboBox.SelectedIndex = 0;  
            }
        }

        private void GradesCourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = GradesCourseComboBox.SelectedItem as CourseItem;

            if (selected == null)
            {
                GradesDataGrid.ItemsSource = null;
                ResetSummary();
                return;
            }

            int courseId = selected.Id;
            var user = App.CurrentUser;

            if (user == null)
            {
                GradesDataGrid.ItemsSource = null;
                ResetSummary();
                return;
            }

            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.user_id == user.user_id);

            if (student == null)
            {
                GradesDataGrid.ItemsSource = null;
                ResetSummary();
                return;
            }

            // Находим запись студента на выбранный курс
            var enrollment = App.Context.KURS_enrollments
                .FirstOrDefault(en => en.student_id == student.id
                                   && en.course_id == courseId);

            if (enrollment == null)
            {
                GradesDataGrid.ItemsSource = null;
                ResetSummary();
                SummaryCourseValue.Text = selected.Title;
                return;
            }

            // Берём все оценки по этой записи
            var grades = App.Context.KURS_results
                .Where(r => r.enrollment_id == enrollment.id)
                .Select(r => new
                {
                    material = r.KURS_materials.title,
                    type = r.KURS_materials.type,
                    grade = r.grade,
                    date = r.date,
                    comment = r.comment
                })
                .OrderBy(r => r.type)
                .ThenBy(r => r.material)
                .ToList();

            GradesDataGrid.ItemsSource = grades;

            // Сводка
            if (grades.Any())
            {
                var avg = grades.Average(g => g.grade);
                SummaryAvgValue.Text = avg.ToString("F2");
                SummaryCountValue.Text = grades.Count.ToString();
            }
            else
            {
                SummaryAvgValue.Text = "—";
                SummaryCountValue.Text = "0";
            }

            // Название курса берём из того же объекта что и Id
            SummaryCourseValue.Text = selected.Title;
        }

        private void ResetSummary()
        {
            SummaryAvgValue.Text = "—";
            SummaryCountValue.Text = "0";
            SummaryCourseValue.Text = "—";
        }
    }
}