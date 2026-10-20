using System;
using System.Linq;
using System.Windows;
using Virtual_education_env.Teacher;

namespace Virtual_education_env.Teacher
{
    public partial class Teacher_window : Window
    {
        public Teacher_window()
        {
            InitializeComponent();

            LoadUserInfo();

            TeacherFrame.Navigate(new TeacherPages.TeacherCoursesPage());

            MenuMyCoursesButton.Click += (s, e) =>
                TeacherFrame.Navigate(new TeacherPages.TeacherCoursesPage());

            MenuEnrollStudentButton.Click += (s, e) =>
                TeacherFrame.Navigate(new TeacherPages.EnrollStudentPage());

            MenuAddGradeButton.Click += (s, e) =>
                TeacherFrame.Navigate(new TeacherPages.AddGradePage());

            MenuReportButton.Click += (s, e) =>
                TeacherFrame.Navigate(new TeacherPages.ReportPage());

            MenuLogoutButton.Click += MenuLogoutButton_Click;
        }

        private void LoadUserInfo()
        {
            var user = App.CurrentUser;

            var teacher = App.Context.KURS_teacher
                .FirstOrDefault(t => t.user_id == user.user_id);

            UserInfoTextBlock.Text =
                $"{user.last_name} {user.name} {user.middle_name} · " +
                (teacher?.position ?? "преподаватель");

            CurrentDateTextBlock.Text = DateTime.Now.ToString("dd MMMM yyyy");
        }

        private void MenuLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.Logout();
            var login = new MainWindow();
            login.Show();
            Close();
        }
    }
}