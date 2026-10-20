using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Virtual_education_env.User_windows.Pages
{
    
    public partial class MyCoursesPage : Page
    {
        public MyCoursesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var user = App.CurrentUser;

            var student = App.Context.KURS_student
                .FirstOrDefault(s => s.user_id == user.user_id);

            if (student == null)
            {
                return;
            }

            var courses = App.Context.KURS_enrollments
                .Where(e => e.student_id == student.id 
                                && e.status == "активен")
                .Select(e => new
                {
                    title = e.KURS_course.title,
                    teacher = e.KURS_course.KURS_teacher.KURS_user.last_name + " " +
                                 e.KURS_course.KURS_teacher.KURS_user.name + " " +
                                 e.KURS_course.KURS_teacher.KURS_user.middle_name,
                    hours = e.KURS_course.hours,
                    start_date = e.KURS_course.start_date,
                    status = e.status
                })
                .ToList();

            MyCoursesDataGrid.ItemsSource = courses;
        }
    }
}
