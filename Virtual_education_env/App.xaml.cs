using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Virtual_education_env
{
    public partial class App : Application
    {
        public static user21_dbEntities Context { get; } = new user21_dbEntities();
        user21_dbEntities _context = new user21_dbEntities();

        public static KURS_user _currentUser { get; set; } = null;
        public static KURS_user CurrentUser
        {
            get
            {
                if (_currentUser == null)
                    throw new InvalidOperationException(
                        "Пользователь не авторизован. Сначала выполните вход.");
                return _currentUser;
            }
            set => _currentUser = value;
        }

        // для выхода — сбросить
        public static void Logout()
        {
            _currentUser = null;
        }
    }
}
