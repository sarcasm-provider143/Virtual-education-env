using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class AdminCoursesPage : Page
    {
        private List<CourseRow> _allCourses = new List<CourseRow>();

        public AdminCoursesPage()
        {
            InitializeComponent();
            LoadCourses();
        }

        private void LoadCourses()
        {
            _allCourses = App.Context.KURS_course
                .Select(c => new CourseRow
                {
                    id = c.id,
                    title = c.title,
                    teacher = c.KURS_teacher.KURS_user.last_name + " " +
                                c.KURS_teacher.KURS_user.name + " " +
                                c.KURS_teacher.KURS_user.middle_name,
                    specialty = c.KURS_specialities.name,
                    hours = c.hours,
                    startDate = c.start_date,
                    students = App.Context.KURS_enrollments
                                    .Count(en => en.course_id == c.id && en.status == "активен")
                })
                .OrderBy(c => c.title)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                CoursesDataGrid.ItemsSource = _allCourses;
                return;
            }

            var filtered = _allCourses
                .Where(c =>
                    (c.title ?? "").ToLower().Contains(query) ||
                    (c.teacher ?? "").ToLower().Contains(query) ||
                    (c.specialty ?? "").ToLower().Contains(query))
                .ToList();

            CoursesDataGrid.ItemsSource = filtered;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AdminCourseEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadCourses();
            }
        }

        private void EditCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс.");
                return;
            }

            var selected = (CourseRow)CoursesDataGrid.SelectedItem;

            var course = App.Context.KURS_course
                .FirstOrDefault(c => c.id == selected.id);

            if (course == null)
            {
                MessageBox.Show("Курс не найден.");
                return;
            }

            var editWindow = new AdminCourseEditWindow(course);
            if (editWindow.ShowDialog() == true)
            {
                LoadCourses();
            }
        }

        private void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс.");
                return;
            }

            var selected = (CourseRow)CoursesDataGrid.SelectedItem;

            // Проверка есть ли студенты на курсе
            bool hasEnrollments = App.Context.KURS_enrollments
                .Any(en => en.course_id == selected.id);

            if (hasEnrollments)
            {
                MessageBox.Show(
                    "Нельзя удалить курс, на который записаны студенты.\n\n" +
                    "Сначала отчислите всех студентов с курса.");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить курс «{selected.title}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var course = App.Context.KURS_course
                    .FirstOrDefault(c => c.id == selected.id);

                if (course == null) return;

                // Удаляем материалы курса
                var materials = App.Context.KURS_materials
                    .Where(m => m.course_id == selected.id)
                    .ToList();

                foreach (var m in materials)
                    App.Context.KURS_materials.Remove(m);

                App.Context.KURS_course.Remove(course);
                App.Context.SaveChanges();

                MessageBox.Show("Курс удалён.");
                LoadCourses();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                var inner = ex.InnerException?.InnerException?.Message
                         ?? ex.InnerException?.Message
                         ?? ex.Message;
                MessageBox.Show("Ошибка БД:\n" + inner);
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

    public class CourseRow
    {
        public int id { get; set; }
        public string title { get; set; }
        public string teacher { get; set; }
        public string specialty { get; set; }
        public int hours { get; set; }
        public DateTime startDate { get; set; }
        public int students { get; set; }
    }
}