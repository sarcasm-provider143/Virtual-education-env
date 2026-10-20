using System;
using System.Linq;
using System.Windows;

namespace Virtual_education_env.Teacher.TeacherPages
{
    public partial class CourseEditWindow : Window
    {
        private readonly KURS_course _editingCourse;

        public CourseEditWindow(KURS_course course)
        {
            InitializeComponent();

            _editingCourse = course;

            if (course == null)
            {
                HeaderTextBlock.Text = "Новый курс";
                StartDatePicker.SelectedDate = DateTime.Today;
            }
            else
            {
                HeaderTextBlock.Text = "Редактирование курса";
                TitleTextBox.Text = course.title;
                DescriptionTextBox.Text = course.description;
                HoursTextBox.Text = course.hours.ToString();
                StartDatePicker.SelectedDate = course.start_date;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string hoursStr = HoursTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название курса.");
                return;
            }

            if (!int.TryParse(hoursStr, out int hours) || hours <= 0)
            {
                MessageBox.Show("Введите корректное количество часов (целое число больше 0).");
                return;
            }

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату начала.");
                return;
            }

            var user = App.CurrentUser;
            var teacher = App.Context.KURS_teacher
                .FirstOrDefault(t => t.user_id == user.user_id);

            if (teacher == null)
            {
                MessageBox.Show("Профиль преподавателя не найден.");
                return;
            }

            try
            {
                if (_editingCourse == null)
                {
                    // Проверка дубля названия у этого преподавателя
                    bool exists = App.Context.KURS_course
                        .Any(c => c.teacher_id == teacher.id && c.title == title);

                    if (exists)
                    {
                        MessageBox.Show("У вас уже есть курс с таким названием.");
                        return;
                    }

                    var newCourse = new KURS_course
                    {
                        teacher_id = teacher.id,
                        title = title,
                        description = string.IsNullOrWhiteSpace(description) ? null : description,
                        hours = hours,
                        start_date = StartDatePicker.SelectedDate.Value
                    };

                    App.Context.KURS_course.Add(newCourse);
                }
                else
                {
                    // Проверка дубля (кроме самого себя)
                    bool exists = App.Context.KURS_course
                        .Any(c => c.teacher_id == teacher.id
                               && c.title == title
                               && c.id != _editingCourse.id);

                    if (exists)
                    {
                        MessageBox.Show("У вас уже есть курс с таким названием.");
                        return;
                    }

                    _editingCourse.title = title;
                    _editingCourse.description = string.IsNullOrWhiteSpace(description) ? null : description;
                    _editingCourse.hours = hours;
                    _editingCourse.start_date = StartDatePicker.SelectedDate.Value;
                }

                App.Context.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}