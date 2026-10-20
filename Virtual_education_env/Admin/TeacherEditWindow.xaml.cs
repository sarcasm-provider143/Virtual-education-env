using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin
{
    public partial class TeacherEditWindow : Window
    {
        private readonly int? _teacherId;
        private readonly int _currentUserId;

        public TeacherEditWindow(int? teacherId)
        {
            InitializeComponent();
            _teacherId = teacherId;

            if (teacherId.HasValue)
            {
                var teacher = App.Context.KURS_teacher
                    .FirstOrDefault(t => t.id == teacherId.Value);

                _currentUserId = teacher?.user_id ?? -1;
            }
            else
            {
                _currentUserId = -1;
            }

            if (teacherId == null)
            {
                HeaderTextBlock.Text = "Новый преподаватель";
            }
            else
            {
                HeaderTextBlock.Text = "Редактирование преподавателя";
                PasswordHint.Visibility = Visibility.Visible;
                LoadTeacher(teacherId.Value);
            }
        }

        private void LoadTeacher(int teacherId)
        {
            var teacher = App.Context.KURS_teacher
                .FirstOrDefault(t => t.id == teacherId);

            if (teacher == null)
            {
                MessageBox.Show("Преподаватель не найден.");
                Close();
                return;
            }

            var user = teacher.KURS_user;

            LastNameTextBox.Text = user.last_name;
            FirstNameTextBox.Text = user.name;
            MiddleNameTextBox.Text = user.middle_name;
            LoginTextBox.Text = user.login;
            EmailTextBox.Text = user.email;
            PositionTextBox.Text = teacher.position;
            DegreeTextBox.Text = teacher.degree;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string lastName = LastNameTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string middleName = MiddleNameTextBox.Text.Trim();
            string position = PositionTextBox.Text.Trim();
            string degree = DegreeTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(position) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните обязательные поля: Фамилия, Имя, Должность, Логин, Email.");
                return;
            }

            // Уникальность логина
            bool loginExists = _teacherId == null
                ? App.Context.KURS_user.Any(u => u.login == login)
                : App.Context.KURS_user.Any(u => u.login == login
                    && u.user_id != _currentUserId);

            if (loginExists)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                return;
            }

            // Уникальность email
            bool emailExists = _teacherId == null
                ? App.Context.KURS_user.Any(u => u.email == email)
                : App.Context.KURS_user.Any(u => u.email == email
                    && u.user_id != _currentUserId);

            if (emailExists)
            {
                MessageBox.Show("Пользователь с таким email уже существует.");
                return;
            }

            try
            {
                if (_teacherId == null)
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Введите пароль для нового преподавателя.");
                        return;
                    }

                    if (password.Length < 6)
                    {
                        MessageBox.Show("Пароль должен быть не менее 6 символов.");
                        return;
                    }

                    var newUser = new KURS_user
                    {
                        last_name = lastName,
                        name = firstName,
                        middle_name = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                        login = login,
                        email = email,
                        password_hash = PasswordHasher.Hash(password),
                        role_id = 2
                    };

                    App.Context.KURS_user.Add(newUser);
                    App.Context.SaveChanges();

                    var newTeacher = new KURS_teacher
                    {
                        user_id = newUser.user_id,
                        position = position,
                        degree = string.IsNullOrWhiteSpace(degree) ? null : degree
                    };

                    App.Context.KURS_teacher.Add(newTeacher);
                    App.Context.SaveChanges();
                }
                else
                {
                    var teacher = App.Context.KURS_teacher
                        .FirstOrDefault(t => t.id == _teacherId.Value);

                    if (teacher == null)
                    {
                        MessageBox.Show("Преподаватель не найден.");
                        return;
                    }

                    var user = teacher.KURS_user;

                    user.last_name = lastName;
                    user.name = firstName;
                    user.middle_name = string.IsNullOrWhiteSpace(middleName) ? null : middleName;
                    user.login = login;
                    user.email = email;

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        if (password.Length < 6)
                        {
                            MessageBox.Show("Пароль должен быть не менее 6 символов.");
                            return;
                        }
                        user.password_hash = PasswordHasher.Hash(password);
                    }

                    teacher.position = position;
                    teacher.degree = string.IsNullOrWhiteSpace(degree) ? null : degree;

                    App.Context.SaveChanges();
                }

                DialogResult = true;
                Close();
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}