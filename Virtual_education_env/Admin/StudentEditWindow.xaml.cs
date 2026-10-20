using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class StudentEditWindow : Window
    {
        private readonly int? _studentId;
        private readonly int _currentUserId;

        public StudentEditWindow(int? studentId)
        {
            InitializeComponent();
            _studentId = studentId;

            // Вычисляем user_id
            if (studentId.HasValue)
            {
                var student = App.Context.KURS_student
                    .FirstOrDefault(s => s.id == studentId.Value);

                _currentUserId = student?.user_id ?? -1;
            }
            else
            {
                _currentUserId = -1;
            }

            LoadGroups();

            if (studentId == null)
            {
                HeaderTextBlock.Text = "Новый студент";
            }
            else
            {
                HeaderTextBlock.Text = "Редактирование студента";
                PasswordHint.Visibility = Visibility.Visible;
                LoadStudent(studentId.Value);
            }
        }

        private void LoadGroups()
        {
            var groups = App.Context.KURS_group
                .Select(g => new { id = g.id, name = g.name })
                .OrderBy(g => g.name)
                .ToList();

            GroupComboBox.DisplayMemberPath = "name";
            GroupComboBox.SelectedValuePath = "id";
            GroupComboBox.ItemsSource = groups;

            if (groups.Any())
            {
                GroupComboBox.SelectedIndex = 0;
            }
        }

        private void LoadStudent(int studentId)
        {
            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.id == studentId);

            if (student == null)
            {
                MessageBox.Show("Студент не найден.");
                Close();
                return;
            }

            var user = student.KURS_user;

            LastNameTextBox.Text = user.last_name;
            FirstNameTextBox.Text = user.name;
            MiddleNameTextBox.Text = user.middle_name;
            LoginTextBox.Text = user.login;
            EmailTextBox.Text = user.email;
            PhoneTextBox.Text = student.phone;

            GroupComboBox.SelectedValue = student.group_id;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string lastName = LastNameTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string middleName = MiddleNameTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните обязательные поля: Фамилия, Имя, Логин, Email.");
                return;
            }

            if (GroupComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите группу.");
                return;
            }

            int groupId = (int)GroupComboBox.SelectedValue;

            // Проверка уникальности логина (исключая себя)
            bool loginExists = _studentId == null
                ? App.Context.KURS_user.Any(u => u.login == login)
                : App.Context.KURS_user.Any(u => u.login == login
                    && u.user_id != _currentUserId);

            if (loginExists)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                return;
            }

            // Проверка уникальности email (исключая себя)
            bool emailExists = _studentId == null
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
                if (_studentId == null)
                {
                    // Создрание нового студента

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Введите пароль для нового студента.");
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
                        role_id = 3
                    };

                    App.Context.KURS_user.Add(newUser);
                    App.Context.SaveChanges();

                    var newStudent = new KURS_student
                    {
                        user_id = newUser.user_id,
                        group_id = groupId,
                        phone = string.IsNullOrWhiteSpace(phone) ? null : phone
                    };

                    App.Context.KURS_student.Add(newStudent);
                    App.Context.SaveChanges();
                }
                else
                {
                    // Редактирование студента

                    var student = App.Context.KURS_student
                        .FirstOrDefault(s => s.id == _studentId.Value);

                    if (student == null)
                    {
                        MessageBox.Show("Студент не найден.");
                        return;
                    }

                    var user = student.KURS_user;

                    user.last_name = lastName;
                    user.name = firstName;
                    user.middle_name = string.IsNullOrWhiteSpace(middleName) ? null : middleName;
                    user.login = login;
                    user.email = email;

                    // Пароль меняем только если поле заполнено
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        if (password.Length < 6)
                        {
                            MessageBox.Show("Пароль должен быть не менее 6 символов.");
                            return;
                        }
                        user.password_hash = PasswordHasher.Hash(password);
                    }

                    // Смена группы и телефона
                    student.group_id = groupId;
                    student.phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

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