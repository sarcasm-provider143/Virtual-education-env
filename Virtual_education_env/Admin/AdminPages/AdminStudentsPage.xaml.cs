using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class AdminStudentsPage : Page
    {
        private List<StudentRow> _allStudents = new List<StudentRow>();

        public AdminStudentsPage()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void LoadStudents()
        {

            var c = App.Context.KURS_course.First();
            int sp = c.specialty_id;
            _allStudents = App.Context.KURS_student
                .Select(s => new StudentRow
                {
                    id = s.id,
                    userId = s.user_id,
                    lastName = s.KURS_user.last_name,
                    firstName = s.KURS_user.name,
                    middleName = s.KURS_user.middle_name,
                    group = s.KURS_group.name,
                    login = s.KURS_user.login,
                    email = s.KURS_user.email,
                    phone = s.phone
                })
                .OrderBy(s => s.lastName)
                .ThenBy(s => s.firstName)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                StudentsDataGrid.ItemsSource = _allStudents;
                return;
            }

            var filtered = _allStudents
                .Where(s =>
                    (s.lastName ?? "").ToLower().Contains(query) ||
                    (s.firstName ?? "").ToLower().Contains(query) ||
                    (s.middleName ?? "").ToLower().Contains(query) ||
                    (s.group ?? "").ToLower().Contains(query) ||
                    (s.login ?? "").ToLower().Contains(query))
                .ToList();

            StudentsDataGrid.ItemsSource = filtered;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new StudentEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadStudents();
            }
        }

        private void EditStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента.");
                return;
            }

            var selected = (StudentRow)StudentsDataGrid.SelectedItem;

            var editWindow = new StudentEditWindow(selected.id);
            if (editWindow.ShowDialog() == true)
            {
                LoadStudents();
            }
        }

        private void DeleteStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента.");
                return;
            }

            var selected = (StudentRow)StudentsDataGrid.SelectedItem;

            bool hasEnrollments = App.Context.KURS_enrollments
                .Any(en => en.student_id == selected.id);

            if (hasEnrollments)
            {
                MessageBox.Show(
                    "Нельзя удалить студента — у него есть записи на курсы.\n\n" +
                    "Сначала отчислите его со всех курсов\n" +
                    "(страница «Записи на курсы»).");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить студента «{selected.lastName} {selected.firstName} {selected.middleName}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var student = App.Context.KURS_student
                    .FirstOrDefault(s => s.id == selected.id);

                if (student == null) return;

                int userId = student.user_id;

                App.Context.KURS_student.Remove(student);

                var user = App.Context.KURS_user
                    .FirstOrDefault(u => u.user_id == userId);

                if (user != null)
                    App.Context.KURS_user.Remove(user);

                App.Context.SaveChanges();

                MessageBox.Show("Студент удалён.");
                LoadStudents();
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

        private void StudentsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditStudentButton_Click(sender, e);
        }
    }

    public class StudentRow
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string group { get; set; }
        public string login { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }
}