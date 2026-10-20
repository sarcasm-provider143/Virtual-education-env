using System;
using System.Linq;
using System.Windows;
using Virtual_education_env.Admin.AdminPages;

namespace Virtual_education_env.Admin
{
    public partial class Admin_window : Window
    {
        public Admin_window()
        {
            InitializeComponent();

            LoadUserInfo();

            AdminFrame.Navigate(new AdminStudentsPage());

            MenuStudentsButton.Click += (s, e) =>
                AdminFrame.Navigate(new AdminStudentsPage());

            MenuTeachersButton.Click += (s, e) =>
                AdminFrame.Navigate(new AdminTeachersPage());

            MenuCoursesButton.Click += (s, e) =>
                AdminFrame.Navigate(new AdminCoursesPage());
            MenuGroupsButton.Click += (s, e) =>
                AdminFrame.Navigate(new AdminGroupsPage());

            MenuEnrollmentsButton.Click += (s, e) =>
                AdminFrame.Navigate(new AdminEnrollmentsPage());

            MenuLogoutButton.Click += MenuLogoutButton_Click;
        }

        private void LoadUserInfo()
        {
            var user = App.CurrentUser;
            if (user == null) return;

            UserInfoTextBlock.Text =
                $"{user.last_name} {user.name} {user.middle_name} · Администратор";

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