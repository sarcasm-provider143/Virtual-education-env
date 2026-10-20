using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Virtual_education_env.Admin.AdminPages
{
    public partial class AdminGroupsPage : Page
    {
        private List<GroupRow> _allGroups = new List<GroupRow>();

        public AdminGroupsPage()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            _allGroups = App.Context.KURS_group
                .Select(g => new GroupRow
                {
                    id = g.id,
                    name = g.name,
                    specialty = g.KURS_specialities.name,
                    students = App.Context.KURS_student
                                    .Count(s => s.group_id == g.id)
                })
                .OrderBy(g => g.name)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                GroupsDataGrid.ItemsSource = _allGroups;
                return;
            }

            var filtered = _allGroups
                .Where(g =>
                    (g.name ?? "").ToLower().Contains(query) ||
                    (g.specialty ?? "").ToLower().Contains(query))
                .ToList();

            GroupsDataGrid.ItemsSource = filtered;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new GroupEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadGroups();
            }
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу.");
                return;
            }

            var selected = (GroupRow)GroupsDataGrid.SelectedItem;

            var group = App.Context.KURS_group
                .FirstOrDefault(g => g.id == selected.id);

            if (group == null)
            {
                MessageBox.Show("Группа не найдена.");
                return;
            }

            var editWindow = new GroupEditWindow(group);
            if (editWindow.ShowDialog() == true)
            {
                LoadGroups();
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу.");
                return;
            }

            var selected = (GroupRow)GroupsDataGrid.SelectedItem;

            // Проверка есть ли студенты в группе
            bool hasStudents = App.Context.KURS_student
                .Any(s => s.group_id == selected.id);

            if (hasStudents)
            {
                MessageBox.Show(
                    "Нельзя удалить группу — в ней есть студенты.\n\n" +
                    "Сначала переведите студентов в другие группы\n" +
                    "или удалите их на странице «Студенты».");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить группу «{selected.name}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var group = App.Context.KURS_group
                    .FirstOrDefault(g => g.id == selected.id);

                if (group == null) return;

                App.Context.KURS_group.Remove(group);
                App.Context.SaveChanges();

                MessageBox.Show("Группа удалена.");
                LoadGroups();
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

        private void GroupsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditGroupButton_Click(sender, e);
        }
    }

    public class GroupRow
    {
        public int id { get; set; }
        public string name { get; set; }
        public string specialty { get; set; }
        public int students { get; set; }
    }
}