using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.User_windows.Pages
{
    public partial class SearchPage : Page
    {
        private int _studentSpecialtyId;

        public SearchPage()
        {
            InitializeComponent();

            LoadStudentSpecialty();
            LoadAllCourses();
        }

        private void LoadStudentSpecialty()
        {
            var user = App.CurrentUser;
            if (user == null) return;

            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.user_id == user.user_id);

            if (student == null) return;

            var group = App.Context.KURS_group
                .FirstOrDefault(g => g.id == student.group_id);

            if (group == null) return;

            _studentSpecialtyId = group.specialty_id;
        }

        private void LoadAllCourses()
        {
            var courses = App.Context.KURS_course
                .Where(c => c.specialty_id == _studentSpecialtyId)   
                .Select(c => new
                {
                    title = c.title,
                    teacher = c.KURS_teacher.KURS_user.last_name + " " +
                                  c.KURS_teacher.KURS_user.name + " " +
                                  c.KURS_teacher.KURS_user.middle_name,
                    description = c.description,
                    hours = c.hours,
                    start_date = c.start_date
                })
                .OrderBy(c => c.title)
                .ToList();

            SearchResultsDataGrid.ItemsSource = courses;
        }

        private void SearchSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                LoadAllCourses();
                return;
            }

            int mode = SearchModeComboBox.SelectedIndex;

            var q = App.Context.KURS_course
                .Where(c => c.specialty_id == _studentSpecialtyId);   

            if (mode == 0)
            {
                q = q.Where(c => c.title.Contains(query));
            }
            else
            {
                q = q.Where(c => c.KURS_teacher.KURS_user.last_name.Contains(query));
            }

            var results = q
                .Select(c => new
                {
                    title = c.title,
                    teacher = c.KURS_teacher.KURS_user.last_name + " " +
                                  c.KURS_teacher.KURS_user.name + " " +
                                  c.KURS_teacher.KURS_user.middle_name,
                    description = c.description,
                    hours = c.hours,
                    start_date = c.start_date
                })
                .OrderBy(c => c.title)
                .ToList();

            SearchResultsDataGrid.ItemsSource = results;

            if (!results.Any())
            {
                MessageBox.Show("По вашему запросу ничего не найдено.");
            }
        }
    }
}