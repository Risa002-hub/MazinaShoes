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
using System.Data;
using System.Data.Entity;

namespace MazinaShoes
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();

            if (Manager.IsStaff)
            {
                ComboClient.ItemsSource = MazinaShoesEntities.GetContext().User.ToList();
                ComboClient.SelectedItem = Manager.CurrentUser;
            }
            else
            {
                PanelClient.Visibility = Visibility.Collapsed;
            }
            RefreshCart();
        }

        // список и итог перерисовываются после любого изменения заказа
        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = Manager.Cart;
            TBTotal.Text = "Итого: " + Manager.Cart.Sum(l => l.Sum).ToString("N2") + " руб.";
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var line = (sender as Button).DataContext as CartLine;
            if (line.Count > 1)
                line.Count--;
            RefreshCart();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var line = (sender as Button).DataContext as CartLine;
            if (line.Count < line.Stock.Count)
            {
                line.Count++;
            }
            else
            {
                MessageBox.Show("На складе больше нет пар этого размера: осталось " + line.Stock.Count + ".",
                    "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            RefreshCart();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var line = (sender as Button).DataContext as CartLine;
            Manager.Cart.Remove(line);
            RefreshCart();
        }

        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.Cart.Count > 0)
            {
                var result = MessageBox.Show("Все выбранные товары будут убраны из заказа. Отказаться от заказа?",
                    "Отказ от заказа", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            Manager.Cart.Clear();
            NavigationService.GoBack();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.Cart.Count == 0)
            {
                MessageBox.Show("Заказ пуст. Вернитесь в каталог и добавьте товары.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User client = Manager.IsStaff ? ComboClient.SelectedItem as User : Manager.CurrentUser;
            if (client == null)
            {
                MessageBox.Show("Выберите клиента, на которого оформляется заказ.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // остатки могли измениться, пока заказ собирался
            foreach (var line in Manager.Cart)
            {
                if (line.Count > line.Stock.Count)
                {
                    MessageBox.Show("Для «" + line.Product.ProductName + "» на складе осталось пар: " + line.Stock.Count
                        + ". Уменьшите количество в заказе.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var answer = MessageBox.Show("Оформить заказ на сумму " + Manager.Cart.Sum(l => l.Sum).ToString("N2") + " руб.?",
                "Подтверждение заказа", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
                return;

            var ctx = MazinaShoesEntities.GetContext();
            var order = new Order { OrderDate = DateTime.Now, UserID = client.UserID };
            foreach (var line in Manager.Cart)
            {
                order.OrderItem.Add(new OrderItem
                {
                    Product = line.Product,
                    SizeID = line.Stock.SizeID,
                    Count = line.Count,
                    PriceOneItem = line.Price
                });
                line.Stock.Count -= line.Count; // списываем со склада
            }
            order.OrderTotal = Manager.Cart.Sum(l => l.Sum);
            ctx.Order.Add(order);

            try
            {
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                // откатываем списание и убираем неудавшийся заказ из контекста
                foreach (var item in order.OrderItem.ToList())
                    ctx.Entry(item).State = EntityState.Detached;
                ctx.Entry(order).State = EntityState.Detached;
                foreach (var line in Manager.Cart)
                    ctx.Entry(line.Stock).Reload();

                MessageBox.Show("Не удалось оформить заказ: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Manager.Cart.Clear();
            MessageBox.Show("Заказ № " + order.OrderID + " оформлен.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.GoBack();
        }
    }
}
