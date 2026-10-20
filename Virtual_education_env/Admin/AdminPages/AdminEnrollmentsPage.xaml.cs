using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class AdminEnrollmentsPage : Page
    {
        private List<EnrollmentRow> _allEnrollments = new List<EnrollmentRow>();

        public AdminEnrollmentsPage()
        {
            InitializeComponent();
            LoadCourses();
            LoadEnrollments();
        }

        private void LoadCourses()
        {
            var courses = App.Context.KURS_course
                .Select(c => new { id = c.id, title = c.title })
                .OrderBy(c => c.title)
                .ToList();

            // Добавляем "Все курсы" первым
            var list = new List<object>();
            list.Add(new { id = -1, title = "Все курсы" });
            foreach (var c in courses)
                list.Add(new { id = c.id, title = c.title });

            CourseFilterComboBox.DisplayMemberPath = "title";
            CourseFilterComboBox.SelectedValuePath = "id";
            CourseFilterComboBox.ItemsSource = list;
            CourseFilterComboBox.SelectedIndex = 0;

            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void LoadEnrollments()
        {
            _allEnrollments = App.Context.KURS_enrollments
                .Select(en => new EnrollmentRow
                {
                    id = en.id,
                    studentId = en.student_id,
                    student = en.KURS_student.KURS_user.last_name + " " +
                                en.KURS_student.KURS_user.name + " " +
                                en.KURS_student.KURS_user.middle_name,
                    group = en.KURS_student.KURS_group.name,
                    courseId = en.course_id,
                    course = en.KURS_course.title,
                    teacher = en.KURS_course.KURS_teacher.KURS_user.last_name + " " +
                                en.KURS_course.KURS_teacher.KURS_user.name + " " +
                                en.KURS_course.KURS_teacher.KURS_user.middle_name,
                    enrolledAt = en.enrolled_at,
                    status = en.status
                })
                .OrderByDescending(en => en.enrolledAt)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allEnrollments.AsEnumerable();

            // Фильтр по курсу
            if (CourseFilterComboBox.SelectedValue != null)
            {
                int courseId = (int)CourseFilterComboBox.SelectedValue;
                if (courseId != -1)
                {
                    filtered = filtered.Where(en => en.courseId == courseId);
                }
            }

            // Фильтр по статусу
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem selectedStatus)
            {
                string status = selectedStatus.Tag?.ToString();
                if (status != "all")
                {
                    filtered = filtered.Where(en =>
                        string.Equals(en.status, status, StringComparison.OrdinalIgnoreCase));
                }
            }

            EnrollmentsDataGrid.ItemsSource = filtered.ToList();
        }

        private void CourseFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            CourseFilterComboBox.SelectedIndex = 0;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void ExpelButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись в таблице.");
                return;
            }

            var selected = (EnrollmentRow)EnrollmentsDataGrid.SelectedItem;

            if (string.Equals(selected.status, "отчислен", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Этот студент уже отчислен с курса.");
                return;
            }

            var result = MessageBox.Show(
                $"Отчислить студента «{selected.student}» с курса «{selected.course}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var enrollment = App.Context.KURS_enrollments
                    .FirstOrDefault(en => en.id == selected.id);

                if (enrollment == null)
                {
                    MessageBox.Show("Запись не найдена.");
                    return;
                }

                enrollment.status = "отчислен";
                App.Context.SaveChanges();

                MessageBox.Show("Студент отчислен с курса.");
                LoadEnrollments();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnrollmentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись в таблице.");
                return;
            }

            var selected = (EnrollmentRow)EnrollmentsDataGrid.SelectedItem;

            if (string.Equals(selected.status, "активен", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Этот студент уже активен на курсе.");
                return;
            }

            var result = MessageBox.Show(
                $"Восстановить студента «{selected.student}» на курсе «{selected.course}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var enrollment = App.Context.KURS_enrollments
                    .FirstOrDefault(en => en.id == selected.id);

                if (enrollment == null)
                {
                    MessageBox.Show("Запись не найдена.");
                    return;
                }

                enrollment.status = "активен";
                enrollment.enrolled_at = DateTime.Now;
                App.Context.SaveChanges();

                MessageBox.Show("Студент восстановлен на курсе.");
                LoadEnrollments();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }

    public class EnrollmentRow
    {
        public int id { get; set; }
        public int studentId { get; set; }
        public string student { get; set; }
        public string group { get; set; }
        public int courseId { get; set; }
        public string course { get; set; }
        public string teacher { get; set; }
        public DateTime enrolledAt { get; set; }
        public string status { get; set; }
    }
}