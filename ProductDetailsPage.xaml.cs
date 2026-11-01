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
    /// Логика взаимодействия для ProductDetailsPage.xaml
    /// </summary>
    public partial class ProductDetailsPage : Page
    {
        private Product _product;

        public ProductDetailsPage(Product product)
        {
            InitializeComponent();
            _product = product;
            DataContext = _product;

            // размерный ряд модели и размеры, которые ещё есть на складе
            var allSizes = _product.StockItem.OrderBy(s => s.Size.SizeValue).ToList();
            TBSizes.Text = "Размерный ряд: " + string.Join(", ", allSizes.Select(s => s.Size.SizeValue.ToString("0.#")));

            var available = allSizes.Where(s => s.Count > 0).ToList();
            ComboSize.ItemsSource = available;
            if (available.Count == 0)
            {
                TBSizes.Text += ". Нет в наличии";
                BtnAdd.IsEnabled = false;
            }
        }

        // в поле количества можно вводить только цифры
        private void TBCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var stock = ComboSize.SelectedItem as StockItem;
            if (stock == null)
            {
                MessageBox.Show("Выберите размер.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int count;
            if (!int.TryParse(TBCount.Text, out count) || count <= 0)
            {
                MessageBox.Show("Введите количество пар: целое число больше нуля.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // если такой размер уже в заказе, количества складываются
            var line = Manager.Cart.FirstOrDefault(l => l.Stock == stock);
            int already = line == null ? 0 : line.Count;
            if (already + count > stock.Count)
            {
                MessageBox.Show("На складе осталось пар этого размера: " + stock.Count + ", в заказе уже: " + already
                    + ". Уменьшите количество.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (line == null)
                Manager.Cart.Add(new CartLine { Stock = stock, Count = count });
            else
                line.Count += count;

            MessageBox.Show("Товар добавлен в заказ. Оформить заказ можно в корзине.", "Заказ", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
