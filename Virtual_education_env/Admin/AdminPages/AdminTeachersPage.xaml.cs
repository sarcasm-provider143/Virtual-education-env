using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class AdminTeachersPage : Page
    {
        private List<TeacherRow> _allTeachers = new List<TeacherRow>();

        public AdminTeachersPage()
        {
            InitializeComponent();
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            _allTeachers = App.Context.KURS_teacher
                .Select(t => new TeacherRow
                {
                    id = t.id,
                    userId = t.user_id,
                    lastName = t.KURS_user.last_name,
                    firstName = t.KURS_user.name,
                    middleName = t.KURS_user.middle_name,
                    position = t.position,
                    degree = t.degree,
                    login = t.KURS_user.login,
                    email = t.KURS_user.email
                })
                .OrderBy(t => t.lastName)
                .ThenBy(t => t.firstName)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                TeachersDataGrid.ItemsSource = _allTeachers;
                return;
            }

            var filtered = _allTeachers
                .Where(t =>
                    (t.lastName ?? "").ToLower().Contains(query) ||
                    (t.firstName ?? "").ToLower().Contains(query) ||
                    (t.middleName ?? "").ToLower().Contains(query) ||
                    (t.position ?? "").ToLower().Contains(query) ||
                    (t.login ?? "").ToLower().Contains(query))
                .ToList();

            TeachersDataGrid.ItemsSource = filtered;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void AddTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TeacherEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadTeachers();
            }
        }

        private void EditTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите преподавателя.");
                return;
            }

            var selected = (TeacherRow)TeachersDataGrid.SelectedItem;

            var editWindow = new TeacherEditWindow(selected.id);
            if (editWindow.ShowDialog() == true)
            {
                LoadTeachers();
            }
        }

        private void DeleteTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeachersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите преподавателя.");
                return;
            }

            var selected = (TeacherRow)TeachersDataGrid.SelectedItem;

            bool hasCourses = App.Context.KURS_course
                .Any(c => c.teacher_id == selected.id);

            if (hasCourses)
            {
                MessageBox.Show(
                    "Нельзя удалить преподавателя — у него есть курсы.\n\n" +
                    "Сначала удалите его курсы на странице «Курсы».");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить преподавателя «{selected.lastName} {selected.firstName} {selected.middleName}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var teacher = App.Context.KURS_teacher
                    .FirstOrDefault(t => t.id == selected.id);

                if (teacher == null) return;

                int userId = teacher.user_id;

                App.Context.KURS_teacher.Remove(teacher);

                var user = App.Context.KURS_user
                    .FirstOrDefault(u => u.user_id == userId);

                if (user != null)
                    App.Context.KURS_user.Remove(user);

                App.Context.SaveChanges();

                MessageBox.Show("Преподаватель удалён.");
                LoadTeachers();
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

        private void TeachersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditTeacherButton_Click(sender, e);
        }
    }

    public class TeacherRow
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string position { get; set; }
        public string degree { get; set; }
        public string login { get; set; }
        public string email { get; set; }
    }
}