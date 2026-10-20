using System;
using System.Linq;
using System.Windows;

namespace Virtual_education_env.Admin
{
    public partial class AdminCourseEditWindow : Window
    {
        private readonly KURS_course _editingCourse;

        public AdminCourseEditWindow(KURS_course course)
        {
            InitializeComponent();
            _editingCourse = course;

            LoadTeachers();
            LoadSpecialties();

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

                TeacherComboBox.SelectedValue = course.teacher_id;
                SpecialtyComboBox.SelectedValue = course.specialty_id;
            }
        }

        private void LoadTeachers()
        {
            var teachers = App.Context.KURS_teacher
                .Select(t => new
                {
                    id = t.id,
                    name = t.KURS_user.last_name + " " +
                           t.KURS_user.name + " " +
                           t.KURS_user.middle_name
                })
                .OrderBy(t => t.name)
                .ToList();

            TeacherComboBox.DisplayMemberPath = "name";
            TeacherComboBox.SelectedValuePath = "id";
            TeacherComboBox.ItemsSource = teachers;

            if (teachers.Any())
            {
                TeacherComboBox.SelectedIndex = 0;
            }
        }

        private void LoadSpecialties()
        {
            var specialties = App.Context.KURS_specialities
                .Select(s => new { id = s.id, name = s.name })
                .OrderBy(s => s.name)
                .ToList();

            SpecialtyComboBox.DisplayMemberPath = "name";
            SpecialtyComboBox.SelectedValuePath = "id";
            SpecialtyComboBox.ItemsSource = specialties;

            if (specialties.Any())
            {
                SpecialtyComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TeacherComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите преподавателя.");
                return;
            }

            if (SpecialtyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите специальность.");
                return;
            }

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

            int teacherId = (int)TeacherComboBox.SelectedValue;
            int specialtyId = (int)SpecialtyComboBox.SelectedValue;

            try
            {
                if (_editingCourse == null)
                {
                    bool exists = App.Context.KURS_course
                        .Any(c => c.teacher_id == teacherId && c.title == title);

                    if (exists)
                    {
                        MessageBox.Show("У этого преподавателя уже есть курс с таким названием.");
                        return;
                    }

                    var newCourse = new KURS_course
                    {
                        teacher_id = teacherId,
                        specialty_id = specialtyId,
                        title = title,
                        description = string.IsNullOrWhiteSpace(description) ? null : description,
                        hours = hours,
                        start_date = StartDatePicker.SelectedDate.Value
                    };

                    App.Context.KURS_course.Add(newCourse);
                }
                else
                {
                    bool exists = App.Context.KURS_course
                        .Any(c => c.teacher_id == teacherId
                               && c.title == title
                               && c.id != _editingCourse.id);

                    if (exists)
                    {
                        MessageBox.Show("У этого преподавателя уже есть курс с таким названием.");
                        return;
                    }

                    _editingCourse.teacher_id = teacherId;
                    _editingCourse.specialty_id = specialtyId;
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