using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.User_windows.Pages
{
    public partial class MaterialsPage : Page
    {
        public MaterialsPage()
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
                                && e.status == "активен")
                .Select(e => new
                {
                    id = e.KURS_course.id,
                    title = e.KURS_course.title
                })
                .OrderBy(c => c.title)
                .ToList();

            MaterialsCourseComboBox.ItemsSource = courses;
            MaterialsCourseComboBox.DisplayMemberPath = "title";
            MaterialsCourseComboBox.SelectedValuePath = "id";

            if (courses.Any())
            {
                MaterialsCourseComboBox.SelectedIndex = 0;
            }
        }

        
        private void MaterialsCourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialsCourseComboBox.SelectedValue == null)
            {
                MaterialsDataGrid.ItemsSource = null;
                return;
            }

            int courseId = (int)MaterialsCourseComboBox.SelectedValue;

            var materials = App.Context.KURS_materials
                .Where(m => m.course_id == courseId) 
                .Select(m => new
                {
                    type = m.type,
                    title = m.title,
                    link = m.link
                })
                .OrderBy(m => m.type)
                .ThenBy(m => m.title)
                .ToList();

            MaterialsDataGrid.ItemsSource = materials;
        }
    }
}