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
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();

            // категории берём из БД, первый пункт сбрасывает фильтр
            try
            {
                var categories = new List<string> { "Все категории" };
                categories.AddRange(MazinaShoesEntities.GetContext().Category.Select(c => c.CategoryName).ToList());
                ComboCategory.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить категории: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ComboCategory.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;

            BtnCart.Visibility = Manager.IsAuthorized ? Visibility.Visible : Visibility.Collapsed;
            BtnOrders.Visibility = Manager.IsStaff ? Visibility.Visible : Visibility.Collapsed;
            BtnAnalytics.Visibility = Manager.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadProduct()
        {
            try
            {
                // подгруз данных
                var query = MazinaShoesEntities.GetContext().Product
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Subcategory.Category)
                    .AsQueryable();

                // фильтр по категории
                string selectCategory = ComboCategory.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectCategory) && selectCategory != "Все категории")
                {
                    query = query.Where(p => p.Subcategory.Category.CategoryName == selectCategory);
                }

                // поиск по наименованию и описанию
                string searchText = TBSearch.Text.Trim().ToLower();
                if (searchText != "")
                {
                    query = query.Where(p => p.ProductName.ToLower().Contains(searchText)
                                          || p.Description.ToLower().Contains(searchText));
                }

                var products = query.ToList();

                // скидка считается в C#, поэтому сортируем уже в памяти, по цене со скидкой
                if (ComboSort.SelectedIndex == 1)
                    products = products.OrderBy(p => p.PriceWithDiscount).ToList();
                else if (ComboSort.SelectedIndex == 2)
                    products = products.OrderByDescending(p => p.PriceWithDiscount).ToList();

                ProductPageListView.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // при возврате на страницу (после заказа) список перечитывается
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                // сообщение об ошибке нельзя показывать прямо в этот момент: окно ещё перестраивается
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadProduct();
                    BtnCart.Content = "Корзина (" + Manager.Cart.Count + ")";
                }));
            }
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProduct();
        }

        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProduct();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProduct();
        }

        private void ProductPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var product = ProductPageListView.SelectedItem as Product;
            if (product == null)
                return;

            // снимаем выделение, чтобы повторный клик по тому же товару снова сработал
            ProductPageListView.SelectedItem = null;

            if (!Manager.IsAuthorized)
            {
                MessageBox.Show("Заказывать товары могут только авторизованные пользователи. Вернитесь назад и войдите в систему.",
                    "Требуется вход", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Manager.MainFrame.Navigate(new ProductDetailsPage(product));
        }

        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new CartPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new OrdersPage());
        }

        private void BtnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AnalyticsPage());
        }
    }
}
