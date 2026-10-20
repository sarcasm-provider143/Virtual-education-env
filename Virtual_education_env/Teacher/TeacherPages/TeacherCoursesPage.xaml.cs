using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Teacher.TeacherPages
{
    public partial class TeacherCoursesPage : Page
    {
        public TeacherCoursesPage()
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

            if (teacher == null)
            {
                MessageBox.Show("Профиль преподавателя не найден.");
                return;
            }

            var courses = App.Context.KURS_course
                .Where(c => c.teacher_id == teacher.id)
                .Select(c => new
                {
                    id = c.id,
                    title = c.title,
                    description = c.description,
                    hours = c.hours,
                    start_date = c.start_date,
                    students = App.Context.KURS_enrollments
                                    .Count(e => e.course_id == c.id && e.status == "активен")
                })
                .OrderBy(c => c.title)
                .ToList();

            CoursesDataGrid.ItemsSource = courses;
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new CourseEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadCourses();
            }
        }

        private void EditCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс для редактирования.");
                return;
            }

            dynamic selected = CoursesDataGrid.SelectedItem;
            int courseId = selected.id;

            var course = App.Context.KURS_course.FirstOrDefault(c => c.id == courseId);
            if (course == null)
            {
                MessageBox.Show("Курс не найден.");
                return;
            }

            var editWindow = new CourseEditWindow(course);
            if (editWindow.ShowDialog() == true)
            {
                LoadCourses();
            }
        }

        private void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс для удаления.");
                return;
            }

            dynamic selected = CoursesDataGrid.SelectedItem;
            int courseId = selected.id;

            var course = App.Context.KURS_course.FirstOrDefault(c => c.id == courseId);
            if (course == null)
            {
                MessageBox.Show("Курс не найден.");
                return;
            }

            // Проверяем есть ли записи студентов на курс
            bool hasEnrollments = App.Context.KURS_enrollments
                .Any(en => en.course_id == courseId);

            if (hasEnrollments)
            {
                MessageBox.Show("Нельзя удалить курс, на который записаны студенты.\n" +
                                "Сначала отчислите всех студентов с курса.");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить курс «{course.title}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Удаляем материалы курса
                var materials = App.Context.KURS_materials
                    .Where(m => m.course_id == courseId)
                    .ToList();

                foreach (var m in materials)
                    App.Context.KURS_materials.Remove(m);

                App.Context.KURS_course.Remove(course);
                App.Context.SaveChanges();

                MessageBox.Show("Курс удалён.");
                LoadCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void CoursesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditCourseButton_Click(sender, e);
        }
    }
}