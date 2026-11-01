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
    /// Логика взаимодействия для OrderDetailsPage.xaml
    /// </summary>
    public partial class OrderDetailsPage : Page
    {
        private Order _order;

        public OrderDetailsPage(Order order)
        {
            InitializeComponent();
            _order = order;

            TBHeader.Text = "Заказ № " + _order.OrderID;
            TBClient.Text = "Клиент: " + _order.User.FullName;
            DPDate.SelectedDate = _order.OrderDate;

            // менеджер только смотрит, администратор ещё и редактирует
            if (!Manager.IsAdmin)
            {
                DPDate.IsEnabled = false;
                BtnSaveDate.Visibility = Visibility.Collapsed;
                BtnDeleteItem.Visibility = Visibility.Collapsed;
            }
            LoadItems();
        }

        private void LoadItems()
        {
            var items = _order.OrderItem.ToList();
            ItemsList.ItemsSource = items;
            TBTotal.Text = "Итого: " + items.Sum(i => i.LineSum).ToString("N2") + " руб.";
        }

        private void BtnSaveDate_Click(object sender, RoutedEventArgs e)
        {
            if (DPDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // время заказа остаётся прежним, меняется только дата
                _order.OrderDate = DPDate.SelectedDate.Value.Date + _order.OrderDate.TimeOfDay;
                MazinaShoesEntities.GetContext().SaveChanges();
                MessageBox.Show("Дата заказа изменена.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить дату: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ItemsList.SelectedItem as OrderItem;
            if (item == null)
            {
                MessageBox.Show("Выберите позицию в списке.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show("Позиция будет удалена из заказа, а пары вернутся на склад. Удалить?",
                "Удаление позиции", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
                return;

            try
            {
                var ctx = MazinaShoesEntities.GetContext();
                Manager.ReturnToStock(item);
                // итог пересчитываем без удаляемой позиции
                _order.OrderTotal = _order.OrderItem.Where(i => i != item).Sum(i => i.LineSum);
                ctx.OrderItem.Remove(item);
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить позицию: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadItems();
        }
    }
}
