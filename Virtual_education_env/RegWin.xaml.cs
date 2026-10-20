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
using System.Windows.Shapes;

namespace Virtual_education_env
{

    public partial class RegWin : Window
    {
        public RegWin()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            GroupComboBox.ItemsSource = App.Context.KURS_group.ToList();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string last_name = LastNameTextBox.Text.Trim();
            string name = FirstNameTextBox.Text.Trim();
            string middle_name = MiddleNameTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            int group = (int)GroupComboBox.SelectedValue;
            string pass = PasswordBox.Password.Trim();
            string confPass = ConfirmPasswordBox.Password.Trim();

            if(last_name == "" || name == "" || login == "" || email == "" || phone == "" || 
                pass == "" || confPass == "" || group == 0)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            if (pass != confPass)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            if(pass.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов");
                return;
            }

            var existingLogin = App.Context.KURS_user.FirstOrDefault(eu => eu.login == login);
            var existingEmail = App.Context.KURS_user.FirstOrDefault(eu => eu.email == email);

            if (existingLogin != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }
            if (existingEmail != null)
            {
                MessageBox.Show("Данный адрес электронной почты уже зарегистрирован");
                return;
            }

            var newUser = new KURS_user
            {
                last_name = last_name,
                name = name,
                middle_name = middle_name,
                login = login,
                password_hash = PasswordHasher.Hash(pass),
                email = email,
                role_id = 3
            };

            try
            {
                App.Context.KURS_user.Add(newUser);
                App.Context.SaveChanges();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

            var newStudent = new KURS_student
            {
                user_id = newUser.user_id,
                group_id = group,
                phone = phone
            };

            try
            {
                App.Context.KURS_student.Add(newStudent);
                App.Context.SaveChanges();

                MessageBox.Show("Вы успешно зарегистрированы.\n Теперь Вы можете авторизоваться");
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
