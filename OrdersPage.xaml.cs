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
using System.Data.Entity;

namespace MazinaShoes
{
    /// <summary>
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
        }

        private void LoadOrders()
        {
            try
            {
                OrdersList.ItemsSource = MazinaShoesEntities.GetContext().Order
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказы: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // список обновляется при возврате с других страниц
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                Dispatcher.BeginInvoke(new Action(LoadOrders)); // см. комментарий в ProductPage
        }

        // новый заказ собирается так же, как обычный: каталог -> товар -> корзина
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage());
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var order = OrdersList.SelectedItem as Order;
            if (order == null)
            {
                MessageBox.Show("Выберите заказ в списке.", "Заказы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Manager.MainFrame.Navigate(new OrderDetailsPage(order));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var order = OrdersList.SelectedItem as Order;
            if (order == null)
            {
                MessageBox.Show("Выберите заказ в списке.", "Заказы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show("Заказ № " + order.OrderID + " будет удалён, а товары вернутся на склад. Это действие нельзя отменить. Удалить?",
                "Удаление заказа", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                var ctx = MazinaShoesEntities.GetContext();
                foreach (var item in order.OrderItem.ToList())
                {
                    Manager.ReturnToStock(item);
                    ctx.OrderItem.Remove(item);
                }
                ctx.Order.Remove(order);
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить заказ: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadOrders();
        }
    }
}
