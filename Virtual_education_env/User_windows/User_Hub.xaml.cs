using System;
using System.Linq;
using System.Windows;
using Virtual_education_env.User_windows.Pages;

namespace Virtual_education_env.User_windows
{
    public partial class User_Hub : Window
    {
        public User_Hub()
        {
            InitializeComponent();

            LoadUserInfo();

            StudentFrame.Navigate(new MyCoursesPage());

            MenuMyCoursesButton.Click += (s, e) =>
                StudentFrame.Navigate(new MyCoursesPage());

            MenuMaterialsButton.Click += (s, e) =>
                StudentFrame.Navigate(new MaterialsPage());

            MenuGradesButton.Click += (s, e) =>
                StudentFrame.Navigate(new GradesPage());

            MenuSearchButton.Click += (s, e) =>
                StudentFrame.Navigate(new SearchPage());

            MenuLogoutButton.Click += MenuLogoutButton_Click;
        }

        private void LoadUserInfo()
        {
            var user = App.CurrentUser;

            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.user_id == user.user_id);

            var group = student != null
                ? App.Context.KURS_group.FirstOrDefault(g => g.id == student.group_id)
                : null;

            UserInfoTextBlock.Text =
                $"{user.last_name} {user.name} {user.middle_name} · " +
                (group?.name ?? "без группы");

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