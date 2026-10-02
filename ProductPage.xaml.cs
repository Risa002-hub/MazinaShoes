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
        private List<Product> currentProduct;
        public ProductPage()
        {
            InitializeComponent();
            LoadProduct();
            //var currentProduct = MazinaShoesEntities.GetContext().Product.ToList();
           
            ProductPageListView.ItemsSource = currentProduct;
            ComboCategory.SelectedIndex = 0;
            System.Diagnostics.Debug.WriteLine($"Загружено товаров: {currentProduct.Count}");
        }
        private void LoadProduct()
        {
            try
            {
                var query = MazinaShoesEntities.GetContext().Product
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Subcategory)
                    .Include(p => p.Subcategory.Category)
                    .AsQueryable();

                string selectCategory = ComboCategory.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(selectCategory) && selectCategory != "Все")
                {
                    // Идем по цепочке: Товар -> Подкатегория -> Категория -> ИмяКатегории
                    // Обязательно проверяем на null на каждом шаге, чтобы программа не упала!
                    query = query.Where(p => p.Subcategory != null &&
                                             p.Subcategory.Category != null &&
                                             p.Subcategory.Category.CategoryName == selectCategory);
                    // ^^^ Замени CategoryName на точное имя поля из твоей таблицы Category!
                }

                currentProduct = query.ToList();
                ProductPageListView.ItemsSource = currentProduct.ToList();

                currentProduct = currentProduct.Where(p => p.ProductName.ToLower().Contains(TBSearch.Text.ToLower())).ToList();

                if (RButtonUp.IsChecked.Value) { ProductPageListView.ItemsSource = currentProduct.OrderBy(p => p.Price).ToList(); }
                if (RButtonDown.IsChecked.Value) { ProductPageListView.ItemsSource = currentProduct.OrderByDescending(p => p.Price).ToList(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            LoadProduct();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            LoadProduct();
        }
    }
}
