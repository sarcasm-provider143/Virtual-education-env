using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Teacher.TeacherPages
{
    public partial class AddGradePage : Page
    {
        public AddGradePage()
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

            LoadStudents(courseId);
            LoadMaterials(courseId);
        }

        private void LoadStudents(int courseId)
        {
            var students = App.Context.KURS_enrollments
                .Where(en => en.course_id == courseId && en.status == "активен")
                .Select(en => new
                {
                    id = en.student_id,
                    name = en.KURS_student.KURS_user.last_name + " " +
                           en.KURS_student.KURS_user.name + " " +
                           en.KURS_student.KURS_user.middle_name
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
            else
            {
                GradesDataGrid.ItemsSource = null;
            }
        }

        private void LoadMaterials(int courseId)
        {
            var materials = App.Context.KURS_materials
                .Where(m => m.course_id == courseId)
                .Select(m => new { id = m.id, title = m.title })
                .OrderBy(m => m.title)
                .ToList();

            MaterialComboBox.DisplayMemberPath = "title";
            MaterialComboBox.SelectedValuePath = "id";
            MaterialComboBox.ItemsSource = materials;

            if (materials.Any())
            {
                MaterialComboBox.SelectedIndex = 0;
            }
        }

        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStudentGrades();
        }

        private void LoadStudentGrades()
        {
            if (CourseComboBox.SelectedValue == null || StudentComboBox.SelectedValue == null)
            {
                GradesDataGrid.ItemsSource = null;
                return;
            }

            int courseId = (int)CourseComboBox.SelectedValue;
            int studentId = (int)StudentComboBox.SelectedValue;

            var enrollment = App.Context.KURS_enrollments
                .FirstOrDefault(en => en.course_id == courseId && en.student_id == studentId);

            if (enrollment == null)
            {
                GradesDataGrid.ItemsSource = null;
                return;
            }

            var grades = App.Context.KURS_results
                .Where(r => r.enrollment_id == enrollment.id)
                .Select(r => new
                {
                    materialId = r.material_id,
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
        }

        private void SaveGradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите курс.");
                return;
            }

            if (StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите студента. Возможно, на курсе нет активных студентов.");
                return;
            }

            if (MaterialComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите задание. Возможно, у курса нет материалов.");
                return;
            }

            if (GradeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите оценку.");
                return;
            }

            int courseId = (int)CourseComboBox.SelectedValue;
            int studentId = (int)StudentComboBox.SelectedValue;
            int materialId = (int)MaterialComboBox.SelectedValue;
            int grade = int.Parse(((ComboBoxItem)GradeComboBox.SelectedItem).Content.ToString());
            string comment = CommentTextBox.Text.Trim();

            if (grade < 2 || grade > 5)
            {
                MessageBox.Show("Оценка должна быть от 2 до 5.");
                return;
            }

            var enrollment = App.Context.KURS_enrollments
                .FirstOrDefault(en => en.course_id == courseId && en.student_id == studentId);

            if (enrollment == null)
            {
                MessageBox.Show("Студент не записан на этот курс.");
                return;
            }

            var existing = App.Context.KURS_results
                .FirstOrDefault(r => r.enrollment_id == enrollment.id
                                  && r.material_id == materialId);

            try
            {
                if (existing != null)
                {
                    existing.grade = grade;
                    existing.date = DateTime.Now;
                    existing.comment = string.IsNullOrWhiteSpace(comment) ? null : comment;
                    MessageBox.Show("Оценка обновлена.");
                }
                else
                {
                    var result = new KURS_results
                    {
                        enrollment_id = enrollment.id,
                        material_id = materialId,
                        grade = grade,
                        date = DateTime.Now,
                        comment = string.IsNullOrWhiteSpace(comment) ? null : comment
                    };

                    App.Context.KURS_results.Add(result);
                    MessageBox.Show("Оценка выставлена.");
                }

                App.Context.SaveChanges();

                CommentTextBox.Clear();
                GradeComboBox.SelectedIndex = -1;

                LoadStudentGrades();
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

        private void GradesDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GradesDataGrid.SelectedItem == null) return;

            dynamic selected = GradesDataGrid.SelectedItem;

            int materialId = selected.materialId;
            int grade = selected.grade;
            string comment = selected.comment;

            MaterialComboBox.SelectedValue = materialId;

            if (grade >= 2 && grade <= 5)
            {
                GradeComboBox.SelectedIndex = grade - 2;
            }

            CommentTextBox.Text = comment ?? "";
        }
    }
}