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

namespace MazinaShoes
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // Вход только по логину, пароль не нужен
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.Trim();
            if (login == "")
            {
                MessageBox.Show("Введите логин.", "Вход в систему", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = MazinaShoesEntities.GetContext().User.FirstOrDefault(u => u.Login == login);
                if (user == null)
                {
                    MessageBox.Show("Пользователь с логином «" + login + "» не найден. Проверьте логин и повторите ввод.",
                        "Вход в систему", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Manager.CurrentUser = user;
                Manager.MainFrame.Navigate(new ProductPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить вход: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentUser = null;
            Manager.MainFrame.Navigate(new ProductPage());
        }
    }
}
