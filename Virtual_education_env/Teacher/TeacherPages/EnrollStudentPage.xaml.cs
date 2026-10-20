using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Teacher.TeacherPages
{
    public partial class EnrollStudentPage : Page
    {
        public EnrollStudentPage()
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

            LoadFreeStudents(courseId);
            LoadEnrolledStudents(courseId);
        }

        private void LoadFreeStudents(int courseId)
        {
            // Находим специальность курса
            var course = App.Context.KURS_course
                .FirstOrDefault(c => c.id == courseId);

            if (course == null) return;

            int courseSpecialtyId = course.specialty_id;

            // Уже записанные на курс
            var enrolledIds = App.Context.KURS_enrollments
                .Where(en => en.course_id == courseId
                          && en.status == "активен")
                .Select(en => en.student_id)
                .ToList();

            // Свободные студенты той же специальности включая отчисленных и завершивших
            var students = App.Context.KURS_student
                .Where(s => !enrolledIds.Contains(s.id)
                         && s.KURS_group.specialty_id == courseSpecialtyId)
                .Select(s => new
                {
                    id = s.id,
                    name = s.KURS_user.last_name + " " +
                           s.KURS_user.name + " " +
                           s.KURS_user.middle_name
                })
                .OrderBy(s => s.name)
                .ToList();

            StudentComboBox.DisplayMemberPath = "name";
            StudentComboBox.SelectedValuePath = "id";
            StudentComboBox.ItemsSource = students;

            if (students.Any())
            {
                StudentComboBox.SelectedIndex = 0;
            }
        }

        private void LoadEnrolledStudents(int courseId)
        {
            var enrolled = App.Context.KURS_enrollments
                .Where(en => en.course_id == courseId && en.status == "активен")
                .Select(en => new
                {
                    enrollmentId = en.id,
                    student = en.KURS_student.KURS_user.last_name + " " +
                                   en.KURS_student.KURS_user.name + " " +
                                   en.KURS_student.KURS_user.middle_name,
                    group = en.KURS_student.KURS_group.name,
                    status = en.status
                })
                .OrderBy(en => en.student)
                .ToList();

            EnrolledDataGrid.ItemsSource = enrolled;
        }

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите курс.");
                return;
            }

            if (StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите студента. Возможно, все студенты уже записаны на этот курс.");
                return;
            }

            int courseId = (int)CourseComboBox.SelectedValue;
            int studentId = (int)StudentComboBox.SelectedValue;

            // ПРоверка соответствия специальностей
            var course = App.Context.KURS_course.FirstOrDefault(c => c.id == courseId);
            var student = App.Context.KURS_student.FirstOrDefault(s => s.id == studentId);

            if (course == null || student == null)
            {
                MessageBox.Show("Курс или студент не найден.");
                return;
            }

            var studentGroup = App.Context.KURS_group
                .FirstOrDefault(g => g.id == student.group_id);

            if (studentGroup == null || studentGroup.specialty_id != course.specialty_id)
            {
                MessageBox.Show(
                    "Студент не может быть записан на этот курс — специальность группы " +
                    "не соответствует специальности курса.");
                return;
            }

            var existing = App.Context.KURS_enrollments
                .FirstOrDefault(en => en.course_id == courseId && en.student_id == studentId);

            try
            {
                if (existing != null)
                {
                    bool isActive = string.Equals(existing.status, "активен",
                                                  StringComparison.OrdinalIgnoreCase);

                    if (isActive)
                    {
                        MessageBox.Show("Этот студент уже записан на курс.");
                        return;
                    }

                    // Восстанавливаем если был отчислен или завершен
                    existing.status = "активен";
                    existing.enrolled_at = DateTime.Now;
                    MessageBox.Show("Студент восстановлен на курсе.");
                }
                else
                {
                    var enrollment = new KURS_enrollments
                    {
                        student_id = studentId,
                        course_id = courseId,
                        status = "активен",
                        enrolled_at = DateTime.Now
                    };
                    App.Context.KURS_enrollments.Add(enrollment);
                    MessageBox.Show("Студент записан на курс.");
                }

                App.Context.SaveChanges();

                LoadFreeStudents(courseId);
                LoadEnrolledStudents(courseId);
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

        private void RemoveStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите курс.");
                return;
            }

            if (EnrolledDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента в таблице, чтобы отчислить.");
                return;
            }

            dynamic selected = EnrolledDataGrid.SelectedItem;
            int enrollmentId = selected.enrollmentId;

            var enrollment = App.Context.KURS_enrollments
                .FirstOrDefault(en => en.id == enrollmentId);

            if (enrollment == null)
            {
                MessageBox.Show("Запись не найдена.");
                return;
            }

            var result = MessageBox.Show(
                "Отчислить студента с курса?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                enrollment.status = "отчислен";
                App.Context.SaveChanges();

                MessageBox.Show("Студент отчислен с курса.");

                int courseId = (int)CourseComboBox.SelectedValue;
                LoadFreeStudents(courseId);
                LoadEnrolledStudents(courseId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}