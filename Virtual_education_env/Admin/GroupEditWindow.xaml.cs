using System;
using System.Linq;
using System.Windows;

namespace Virtual_education_env.Admin
{
    public partial class GroupEditWindow : Window
    {
        private readonly KURS_group _editingGroup;

        public GroupEditWindow(KURS_group group)
        {
            InitializeComponent();
            _editingGroup = group;

            LoadSpecialties();

            if (group == null)
            {
                HeaderTextBlock.Text = "Новая группа";
            }
            else
            {
                HeaderTextBlock.Text = "Редактирование группы";
                NameTextBox.Text = group.name;
                SpecialtyComboBox.SelectedValue = group.specialty_id;
            }
        }

        private void LoadSpecialties()
        {
            var specialties = App.Context.KURS_specialities
                .Select(s => new { id = s.id, name = s.name })
                .OrderBy(s => s.name)
                .ToList();

            SpecialtyComboBox.DisplayMemberPath = "name";
            SpecialtyComboBox.SelectedValuePath = "id";
            SpecialtyComboBox.ItemsSource = specialties;

            if (specialties.Any())
            {
                SpecialtyComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название группы.");
                return;
            }

            if (SpecialtyComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите специальность.");
                return;
            }

            int specialtyId = (int)SpecialtyComboBox.SelectedValue;

            try
            {
                if (_editingGroup == null)
                {
                    bool exists = App.Context.KURS_group
                        .Any(g => g.name == name);

                    if (exists)
                    {
                        MessageBox.Show("Группа с таким названием уже существует.");
                        return;
                    }

                    var newGroup = new KURS_group
                    {
                        name = name,
                        specialty_id = specialtyId
                    };

                    App.Context.KURS_group.Add(newGroup);
                }
                else
                {
                    bool exists = App.Context.KURS_group
                        .Any(g => g.name == name && g.id != _editingGroup.id);

                    if (exists)
                    {
                        MessageBox.Show("Группа с таким названием уже существует.");
                        return;
                    }

                    _editingGroup.name = name;
                    _editingGroup.specialty_id = specialtyId;
                }

                App.Context.SaveChanges();

                DialogResult = true;
                Close();
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