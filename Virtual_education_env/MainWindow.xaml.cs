using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Virtual_education_env.User_windows;

namespace Virtual_education_env
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClearFields()
        {
            LoginTextBox.Clear();
            PasswordBox.Clear();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if(string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля");
            }
            else
            {
                var user = App.Context.KURS_user.FirstOrDefault(u => u.login == login);
                if (user == null)
                {
                    MessageBox.Show("Пользователя с таким логином не существует");
                    ClearFields();
                    return;
                }
                // Хеширование введенного пароля
                var enteredHash = PasswordHasher.Hash(password);

                if (enteredHash != user.password_hash)
                {
                    MessageBox.Show("Неверный пароль!");
                    return;
                }

                MessageBox.Show($"Здравствуйте, {user.name + ' ' + user.middle_name}!");
                App.CurrentUser = user;
                if(user.role_id == 3)
                {
                    User_windows.User_Hub userHub = new User_windows.User_Hub();
                    userHub.Show();
                    this.Close();
                }
                else if(user.role_id == 2)
                {
                    Teacher.Teacher_window teachWin = new Teacher.Teacher_window();
                    teachWin.Show();
                    this.Close();
                }
                else
                {
                    Admin.Admin_window admWin = new Admin.Admin_window();
                    admWin.Show();
                    this.Close();
                }
                
            }

        }

        private void PassBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegWin reg = new RegWin();
            reg.Show();
            this.Close();
        }
    }
}
